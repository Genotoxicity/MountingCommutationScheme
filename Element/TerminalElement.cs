using System.Collections.Generic;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class TerminalElement : Element
    {
        public string TerminalName { get; private set; }

        public TerminalElement(ProjectObjects projectObjects, DeviceOutline outline, Orientation orientation, ComponentManager componentManager, HashSet<int> electricSchemeSheetIds) : base(projectObjects, outline, orientation, electricSchemeSheetIds)
        { 
            Device device = projectObjects.Device;
            DevicePin pin = projectObjects.Pin;
            device.Id = outline.DeviceId;
            List<int> pinIds = device.PinIds;
            pin.Id = pinIds.First();
            name = device.Location + device.Name;
            TerminalName = name + ":" + pin.Name;
            component = componentManager.GetTerminalComponent(orientation, firstPinsGroup, secondPinsGroup);
        }
    }
}
