using System;
using System.Collections.Generic;

namespace MountingCommutationScheme
{
    public class ComponentManager
    {
        private Dictionary<string, Component> componentByName;
        private ProjectObjects projectObjects;
        private Settings settings;

        public ComponentManager(ProjectObjects projectObjects, Settings settings)
        {
            componentByName = new Dictionary<string, Component>();
            this.projectObjects = projectObjects;
            this.settings = settings;
        }

        public Component GetDeviceComponent(string componentName, string deviceName, Orientation orientation, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
        { 
            componentName = String.Format("{0}{1}", componentName, (orientation == Orientation.Vertical) ? "Vertical" : "Horizontal");
            if (!componentByName.ContainsKey(componentName))
            {
                Component component;
                if (orientation == Orientation.Vertical)
                    component = new DeviceVerticalComponent(projectObjects, settings, firstPinsGroup, secondPinsGroup, componentName);
                else
                    component = new DeviceHorizontalComponent(projectObjects, settings, firstPinsGroup, secondPinsGroup, componentName);
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
                    component = new TerminalVerticalComponent(projectObjects, settings, firstPinsGroup, secondPinsGroup);
                else
                    component = new TerminalHorizontalComponent(projectObjects, settings, firstPinsGroup, secondPinsGroup);
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
