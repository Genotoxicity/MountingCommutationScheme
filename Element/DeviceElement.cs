using System.Collections.Generic;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceElement : Element
    {
        public DeviceElement(ProjectObjects projectObjects, DeviceOutline outline, Orientation orientation, ComponentManager componentManager, HashSet<int> electricSchemeSheetIds) : base(projectObjects, outline, orientation, electricSchemeSheetIds)
        {
            Device device = projectObjects.Device;
            device.Id = outline.DeviceId;
            name = device.Location+device.Name;
            component = componentManager.GetDeviceComponent(device.ComponentName, name, orientation, firstPinsGroup, secondPinsGroup);
        }


    }
}
