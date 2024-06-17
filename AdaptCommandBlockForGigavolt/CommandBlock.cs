using Engine;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptCommandBlockForGigavolt
{
    public class 命令方块 : Game.命令方块, IGVElectricElementBlock
    {
        public const int Index = 333;
        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectric, int value, int x, int y, int z, uint subterrainId)
        {
            return new CommandGVElectricElement(subsystemGVElectric, new Point3(x, y, z), subterrainId);
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain system, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId)
        {
            WorkingMode workingMode = GetWorkingMode(value);
            if (workingMode == WorkingMode.条件)
            {
                return GVElectricConnectorType.Output;
            }
            else if (workingMode == WorkingMode.变量 && connectorFace == 4)
            {
                return GVElectricConnectorType.Output;
            }
            return GVElectricConnectorType.Input;
        }
    }
}
