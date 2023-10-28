using Engine;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptCommandBlockForGigavolt
{
    public class CommandBlock : Game.CommandBlock, IGVElectricElementBlock
    {
        public const int Index = 333;
        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectric, int value, int x, int y, int z)
        {
            return new CommandGVElectricElement(subsystemGVElectric, new Point3(x, y, z));
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            WorkingMode workingMode = GetWorkingMode(value);
            if (workingMode == WorkingMode.Condition)
            {
                return GVElectricConnectorType.Output;
            }
            else if (workingMode == WorkingMode.Variable && connectorFace == 4)
            {
                return GVElectricConnectorType.Output;
            }
            return GVElectricConnectorType.Input;
        }
    }
}
