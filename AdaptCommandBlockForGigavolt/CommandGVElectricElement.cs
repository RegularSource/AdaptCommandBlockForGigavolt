using Engine;
using System.Collections.Generic;
using System;

namespace Game {
    public class CommandGVElectricElement : GVElectricElement
    {
        public SubsystemCommand m_subsystemCommand;

        public CommandData m_commandData;

        public uint m_voltage;

        public bool clockAllowed = true;

        public SubmitResult m_submitResult;

        public CommandGVElectricElement(SubsystemGVElectricity subsystemElectricity, Point3 position, uint subterrainId)
            : base(subsystemElectricity, new List<GVCellFace>
            {
                    new(position.X, position.Y, position.Z, 0),
                    new (position.X, position.Y, position.Z, 1),
                    new (position.X, position.Y, position.Z, 2),
                    new (position.X, position.Y, position.Z, 3),
                    new (position.X, position.Y, position.Z, 4),
                    new (position.X, position.Y, position.Z, 5)
            },subterrainId)
        {
            m_commandData = subsystemElectricity.Project.FindSubsystem<SubsystemCommandBlockBehavior>(true).GetCommandData(position);
            m_subsystemCommand = subsystemElectricity.Project.FindSubsystem<SubsystemCommand>(true);
            m_submitResult = SubmitResult.Fail;
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override bool Simulate()
        {
            try
            {
                if (m_commandData == null) return false;
                if (m_commandData.Mode == WorkingMode.执行)
                {
                    if (CalculateHighInputsCount() > 0)
                    {
                        m_submitResult = m_subsystemCommand.Submit(m_commandData.Name, m_commandData, false);
                        m_subsystemCommand.ShowSubmitTips(string.Empty, true, m_submitResult, m_commandData);
                        return m_submitResult == SubmitResult.Success;
                    }
                }
                else if (m_commandData.Mode == WorkingMode.条件)
                {
                    SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
                    if (m_submitResult != SubmitResult.Success && m_submitResult != SubmitResult.Fail)
                    {
                        return false;
                    }
                    m_submitResult = m_subsystemCommand.Submit(m_commandData.Name, m_commandData, true);
                    if (m_submitResult != SubmitResult.OutRange && m_submitResult != SubmitResult.Invalid)
                    {
                        m_subsystemCommand.ShowSubmitTips(string.Empty, true, m_submitResult, m_commandData);
                    }
                    m_voltage = (m_submitResult == SubmitResult.Success) ? uint.MaxValue : 0;
                    return true;
                }
                else if (m_commandData.Mode == WorkingMode.变量)
                {
                    m_voltage = 0u;
                    int[] signals = GetSignals();
                    int clockSignal = signals[4];
                    if (IsVariableSyncMode())
                    {
                        if ((clockSignal >= 8 || clockSignal < 0) && clockAllowed)
                        {
                            clockAllowed = false;
                            return VariableSubmit(signals);
                        }
                        if (clockSignal is < 8 and >= 0) clockAllowed = true;
                    }
                    else
                    {
                        if (signals[0] != 0 || signals[1] != 0 || signals[2] != 0 || signals[3] != 0)
                        {
                            return VariableSubmit(signals);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("CommandElectricElement:" + e.Message);
            }
            return false;
        }

        public bool VariableSubmit(int[] signals)
        {
            m_commandData = DataHandle.SetVariableData(m_commandData, signals);
            m_submitResult = m_subsystemCommand.Submit(m_commandData.Name, m_commandData, false);
            m_subsystemCommand.ShowSubmitTips(string.Empty, true, m_submitResult, m_commandData);
            if (m_submitResult == SubmitResult.Success)
            {
                m_voltage = uint.MaxValue;
                return true;
            }
            else
            {
                m_voltage = 0u;
                return false;
            }
        }

        public int[] GetSignals()
        {
            int[] signals = new int[6];
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    int face = connection.NeighborConnectorFace;
                    signals[face] = uint2int(connection.NeighborGVElectricElement.GetOutputVoltage(face));
                }
            }
            return signals;
        }

        public bool IsVariableSyncMode()
        {
            bool clockConnection = false;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, 0, connection.ConnectorFace);
                    if (connectorDirection is GVElectricConnectorDirection.Bottom) {
                        clockConnection = true;
                    }
                }
            }
            return clockConnection;
        }
        public static int uint2int(uint value)
        {
            return value >> 31 == 1u ? -(int)(value^0x80000000u) : (int)value;
        }
    }
}