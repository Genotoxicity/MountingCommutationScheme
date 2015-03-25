using System;
using System.Collections.Generic;

namespace MountingCommutationScheme
{
    public class ComponentManager
    {
        private Dictionary<string, Component> componentByName;
        private ProjectObjects projectObjects;

        public ComponentManager(ProjectObjects projectObjects)
        {
            componentByName = new Dictionary<string, Component>();
            this.projectObjects = projectObjects;
        }

        public Component GetDeviceComponent(string componentName, string deviceName, Orientation orientation, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
        { 
            componentName = String.Format("{0}{1}", componentName, (orientation == Orientation.Vertical) ? "Vertical" : "Horizontal");
            if (!componentByName.ContainsKey(componentName))
            {
                Component component;
                if (orientation == Orientation.Vertical)
                    component = new DeviceVerticalComponent(projectObjects, firstPinsGroup, secondPinsGroup, componentName);
                else
                    component = new DeviceHorizontalComponent(projectObjects, firstPinsGroup, secondPinsGroup, componentName);
                componentByName.Add(componentName, component);
            }
            componentByName[componentName].AdjustNameLength(deviceName);
            return componentByName[componentName];
        }

        public Component GetTerminalComponent(Orientation orientation, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
        {
            string componentName = String.Format("{0}{1}", "Terminal" ,(orientation == Orientation.Vertical) ? "Vertical" : "Horizontal");
            if (!componentByName.ContainsKey(componentName))
            {
                Component component;
                if (orientation == Orientation.Vertical)
                    component = new TerminalVerticalComponent(projectObjects, firstPinsGroup, secondPinsGroup);
                else
                    component = new TerminalHorizontalComponent(projectObjects, firstPinsGroup, secondPinsGroup);
                componentByName.Add(componentName, component);
            }
            return componentByName[componentName];
        }

        public void CalculateComponents()
        {
            foreach (Component component in componentByName.Values)
                component.Calculate();
        }
    }
}
