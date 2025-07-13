using Engine;
using Game;

namespace AdaptCommandBlockForGigavolt
{
    public class Commandblock : Game.CommandBlock, IGVElectricElementBlock
    {
        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectric, int value, int x, int y, int z, uint subterrainId)
        {
            return new CommandGVElectricElement(subsystemGVElectric, new Point3(x, y, z), subterrainId);
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain system, int value, int face, int connectorFace, int x, int y, int z, Terrain subterrain)
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
