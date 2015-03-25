using System.Collections.Generic;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceElement : Element
    {
        public DeviceElement(ProjectObjects projectObjects, DeviceOutline outline, ComponentManager componentManager, Orientation orientation) : base(projectObjects, outline, orientation)
        {
            Device device = projectObjects.Device;
            device.Id = outline.DeviceId;
            name = device.Location+device.Name;
            component = componentManager.GetDeviceComponent(device.ComponentName, name, orientation, firstPinsGroup, secondPinsGroup);
        }


    }
}
