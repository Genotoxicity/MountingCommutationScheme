using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public abstract class Element
    {
        protected string name;
        protected Orientation orientation;
        protected List<ElementPin> firstPinsGroup;  // top / left
        protected List<ElementPin> secondPinsGroup; // bottom / right
        protected Component component;

        public List<ElementPin> FirstPinsGroup
        {
            get
            {
                return firstPinsGroup;
            }
        }

        public List<ElementPin> SecondPinsGroup
        {
            get
            {
                return secondPinsGroup;
            }
        }

        public double OutlineWidth
        {
            get
            {
                return component.OutlineWidth;
            }
        }

        public double OutlineHeight
        {
            get
            {
                return component.OutlineHeight;
            }
        }

        public double SignalLineLength { get; private set; }

        public List<int> ElectricSchemePinIds
        {
            get
            {
                List<int> pinIds = new List<int>(firstPinsGroup.SelectMany(fp => fp.ElectricPinIds));
                pinIds.AddRange(secondPinsGroup.SelectMany(sp => sp.ElectricPinIds));
                return pinIds;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
        }

        protected Element(ProjectObjects projectObjects, DeviceOutline outline, Orientation orientation, HashSet<int> electricSchemeSheetIds)
        {
            this.orientation = orientation;
            firstPinsGroup = new List<ElementPin>();
            secondPinsGroup = new List<ElementPin>();
            NormalDevice device = projectObjects.Device;
            device.Id = outline.DeviceId;
            List<int> pinIds = device.PinIds;
            DevicePin pin = projectObjects.Pin;
            Dictionary<int, Point> panelPositionById;
            List<ElementPin> pins;
            GetPanelPinsAndPositions(pin, pinIds, electricSchemeSheetIds, out pins, out panelPositionById);
            SplitPinsByGroups(outline, orientation, panelPositionById, pins);
            PinComparer comparer = new PinComparer(panelPositionById);
            firstPinsGroup.Sort(comparer);
            secondPinsGroup.Sort(comparer);
        }

        private void SplitPinsByGroups(DeviceOutline outline, Orientation orientation, Dictionary<int, Point> panelPositionById, List<ElementPin> pins)
        {
            if (orientation == Orientation.Vertical)
            {
                double centerY = outline.Center.Y;
                foreach (ElementPin pin in pins)
                {
                    Point panelPosition = panelPositionById[pin.PanelPinId];
                    if (centerY <= panelPosition.Y)
                        firstPinsGroup.Add(pin);
                    else
                        secondPinsGroup.Add(pin);
                }
            }
            else
            {
                double centerX = outline.Center.X;
                foreach (ElementPin pin in pins)
                {
                    Point panelPosition = panelPositionById[pin.PanelPinId];
                    if (centerX <= panelPosition.X)
                        firstPinsGroup.Add(pin);
                    else
                        secondPinsGroup.Add(pin);
                }
            }
        }

        private static void GetPanelPinsAndPositions(DevicePin pin, List<int> pinIds, HashSet<int> electricSchemeSheetIds,  out List<ElementPin> pins, out Dictionary<int, Point> panelPositionById)
        {
            Dictionary<int, int> panelPinPhysicalIdById = new Dictionary<int, int>();
            Dictionary<int, int> electricSchemePinIdsByPhysicalId = new Dictionary<int, int>();
            Dictionary<int, ElementPin> elementPinByPanelId = new Dictionary<int, ElementPin>();
            panelPositionById = new Dictionary<int, Point>();
            foreach (int pinId in pinIds)
            {
                pin.Id = pinId;
                if (pin.IsOnPanel)
                {
                    panelPinPhysicalIdById.Add(pinId, pin.PhysicalId);
                    PinPanelLocation location = pin.PanelLocation;
                    panelPositionById.Add(pinId, new Point(location.X, location.Y));
                    elementPinByPanelId.Add(pinId, new ElementPin(pinId, pin.SignalName));
                    continue;
                }
                int sheetId = pin.SheetId;
                if (electricSchemeSheetIds.Contains(sheetId))
                {
                    int physicalId = pin.PhysicalId;
                    if (!electricSchemePinIdsByPhysicalId.ContainsKey(physicalId))
                        electricSchemePinIdsByPhysicalId.Add(physicalId, pinId);
                }
            }
            foreach (int panelPinId in panelPinPhysicalIdById.Keys)
            {
                int physicalId = panelPinPhysicalIdById[panelPinId];
                if (electricSchemePinIdsByPhysicalId.ContainsKey(physicalId))
                {
                    int electricSchemePinId = electricSchemePinIdsByPhysicalId[physicalId];
                    elementPinByPanelId[panelPinId].SetEquivalentElectricSchemePinIds(electricSchemePinId);
                }
            }
            pins = elementPinByPanelId.Values.ToList();
        }

        public void SetAddresses(MateAddresses mateAddresses)
        {
            firstPinsGroup.ForEach(fp => fp.SetMateAdresses(mateAddresses));
            secondPinsGroup.ForEach(sp => sp.SetMateAdresses(mateAddresses));
        }

        public ElementSizes GetSizesAndSetSignalLineLength()
        {
            ElementSizes sizes = component.GetElementSizes(firstPinsGroup, secondPinsGroup);
            SignalLineLength = sizes.SignalLineLength;
            return sizes;
        }

        public void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position)
        {
            component.PlaceElement(projectObjects, sheet, sheetId, position, this);
        }

        private class PinComparer : IComparer<ElementPin>
        {
            Dictionary<int, Point> panelPositionById;

            public PinComparer(Dictionary<int, Point> panelPositionById)
            {
                this.panelPositionById = panelPositionById;
            }

            public int Compare(ElementPin a, ElementPin b)
            {
                Point aPosition = panelPositionById[a.PanelPinId];
                Point bPosition = panelPositionById[b.PanelPinId];
                int result = (int)(aPosition.X - bPosition.X);
                if (result != 0)
                    return result;
                return (int)(bPosition.Y - aPosition.Y);
            }
        }
    }
}
