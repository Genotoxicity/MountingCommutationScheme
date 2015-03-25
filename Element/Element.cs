using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;
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

        public int Id { get; private set; }

        public Margins Margins { get; private set; }

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

        protected Element(ProjectObjects projectObjects, DeviceOutline outline, Orientation orientation)
        {
            Id = outline.DeviceId;
            this.orientation = orientation;
            firstPinsGroup = new List<ElementPin>();
            secondPinsGroup = new List<ElementPin>();
            NormalDevice device = projectObjects.Device;
            device.Id = outline.DeviceId;
            List<int> pinIds = device.PinIds;
            DevicePin pin = projectObjects.Pin;
            Dictionary<int, Point> panelPositionById;
            List<ElementPin> pins;
            GetPanelPinsAndPositions(pin, pinIds, projectObjects.ElectricSheetIds, out pins, out panelPositionById);
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

        public void Calculate(E3Text text)
        {
            double leftMargin = component.OutlineWidth / 2;
            double rightMargin = leftMargin;
            double topMargin = component.OutlineHeight / 2;
            double bottomMargin = topMargin;
            PinGroupInfo firstPinGroupInfo = new PinGroupInfo(firstPinsGroup, text);
            PinGroupInfo secondPinGroupInfo = new PinGroupInfo(secondPinsGroup, text);
            SignalLineLength = Math.Max(firstPinGroupInfo.SignalLineLength, secondPinGroupInfo.SignalLineLength);
            double additional = 0;
            if (firstPinGroupInfo.AdressesLength > 0)
                additional = SignalLineLength + firstPinGroupInfo.AdressesLength + Settings.AdressOffset;
            else
                if (firstPinGroupInfo.HasSignal)
                    additional = SignalLineLength;
            if (firstPinGroupInfo.HasJumper)
                additional = Math.Max(additional, Settings.JumperHeight);
            if (orientation == Orientation.Horizontal)
                leftMargin += additional;
            else
                topMargin += additional;
            additional = 0;
            if (secondPinGroupInfo.AdressesLength > 0)
                additional = SignalLineLength + secondPinGroupInfo.AdressesLength + Settings.AdressOffset;
            else
                if (secondPinGroupInfo.HasSignal)
                    additional = SignalLineLength;
            if (secondPinGroupInfo.HasJumper)
                additional = Math.Max(additional, Settings.JumperHeight);
            if (orientation == Orientation.Horizontal)
                rightMargin += additional;
            else
                bottomMargin += additional;
            Margins = new Margins(leftMargin, rightMargin, topMargin, bottomMargin);
        }

        public void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position)
        {
            component.PlaceElement(projectObjects, sheet, sheetId, position, this);
        }

        private class PinGroupInfo
        {
            public bool HasSignal { get; private set; }
            public double AdressesLength { get; private set; }
            public double SignalLineLength { get; private set; }
            public bool HasJumper { get; private set; }

            public PinGroupInfo(List<ElementPin> pins, E3Text text)
            {
                if (pins.Count == 0)
                {
                    HasSignal = false;
                    AdressesLength = 0;
                    SignalLineLength = 0;
                    HasJumper = false;
                }
                else
                {
                    HasJumper = pins.Any(p => p.IsJumpered);
                    IEnumerable<string> addresses = pins.SelectMany(p => p.Addresses);
                    if (addresses.Count() > 0)
                        AdressesLength = addresses.Max(a => text.GetTextLength(a, Settings.SmallFont)) + Settings.AdressOffset;
                    else
                        AdressesLength = 0;
                    HasSignal = pins.TrueForAll(p => !String.IsNullOrEmpty(p.Signal));
                    SignalLineLength = pins.Max(p => text.GetTextLength(p.Signal, Settings.SmallFont)) + Settings.SignalOffsetFromOutline + Settings.SignalOffsetAfterText;
                }
            }
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
