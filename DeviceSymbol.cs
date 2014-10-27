using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceSymbol
    {
        private string name;
        private string terminalName;
        //private Rect outline;
        private Size nameSize;
        //private List<int> panelPinIds;
        private List<int> electricSchemeEquivalentPinIds;
        private List<PinSymbol> panelPinSymbols, topPinSymbols, bottomPinSymbols;
        private bool isTerminal;
        //private bool isTerminalStrip;

        public bool IsTerminal
        {
            get
            {
                return isTerminal;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Component { get; private set; }

        public Size NameSize
        {
            get
            {
                return nameSize;
            }
        }

        public List<int> PanelPinAndEquivalenceIds
        {
            get
            {
                return electricSchemeEquivalentPinIds;
            }
        }

        public List<PinSymbol> PanelPinSymbols
        {
            get
            {
                return panelPinSymbols;
            }
        }

        public List<PinSymbol> TopPinSymbols
        {
            get
            {
                return topPinSymbols;
            }
        }

        public List<PinSymbol> BottomPinSymbols
        {
            get
            {
                return bottomPinSymbols;
            }
        }

        public DeviceSymbol(ProjectObjects projectObjects, Settings settings, int deviceId, HashSet<int> electricSchemeSheetIds, Func<double, bool> IsAboveOutlineCenter)
        {
            NormalDevice device = projectObjects.Device;
            DevicePin pin = projectObjects.Pin;
            device.Id = deviceId;
            isTerminal = device.IsTerminal();
            name = device.Location + device.Name;
            GetPanelPinSymbolsAndEquivalentElectricSchemePinIds(device, pin, electricSchemeSheetIds, out panelPinSymbols, out electricSchemeEquivalentPinIds);
            topPinSymbols = new List<PinSymbol>();
            bottomPinSymbols = new List<PinSymbol>();
            panelPinSymbols.ForEach(ps => { if (IsAboveOutlineCenter(ps.PanelY)) topPinSymbols.Add(ps); else bottomPinSymbols.Add(ps); });
            PinSymbolComparer comparer = new PinSymbolComparer();
            topPinSymbols.Sort(comparer);
            bottomPinSymbols.Sort(comparer);
            if (isTerminal)
            {
                Component = settings.TerminalComponent;
                terminalName = name + ":" + panelPinSymbols.First().Name;
            }
            else
                Component = device.ComponentName;
        }

        private static void GetPanelPinSymbolsAndEquivalentElectricSchemePinIds(NormalDevice device, DevicePin pin, HashSet<int> electricSchemeSheetIds, out List<PinSymbol> panelPinSymbols, out List<int> electricSchemeEquivalentPinIds)
        {
            //List<int> devicePanelPinIds = new List<int>();
            //Dictionary<int, Tuple<int, int>> panelPinsEquivalenceById = new Dictionary<int, Tuple<int, int>>();
            //Dictionary<Tuple<int, int>, List<int>> electricSchemePinIdsByEquivalence = new Dictionary<Tuple<int, int>, List<int>>();
            Dictionary<int, int> panelPinPhysicalIdById = new Dictionary<int, int>();
            Dictionary<int, int> electricSchemePinIdsByPhysicalId = new Dictionary<int, int>();
            foreach (int pinId in device.PinIds)
            {
                pin.Id = pinId;
                if (pin.IsOnPanel)
                {
                    //devicePanelPinIds.Add(pinId);
                    //panelPinsEquivalenceById.Add(pinId, new Tuple<int, int>(pin.LogicalEquivalence, pin.NameEquivalence));
                    panelPinPhysicalIdById.Add(pinId, pin.PhysicalId);
                    continue;
                }
                int sheetId = pin.SheetId;
                if (electricSchemeSheetIds.Contains(sheetId))
                {
                    //Tuple<int, int> equivalences = new Tuple<int, int>(pin.LogicalEquivalence, pin.NameEquivalence);
                    //if (!electricSchemePinIdsByEquivalence.ContainsKey(equivalences))
                    //    electricSchemePinIdsByEquivalence.Add(equivalences, new List<int>());
                    //electricSchemePinIdsByEquivalence[equivalences].Add(pinId);
                    int physicalId = pin.PhysicalId;
                    if (!electricSchemePinIdsByPhysicalId.ContainsKey(physicalId))
                        electricSchemePinIdsByPhysicalId.Add(physicalId, pinId);
                }
            }
            Dictionary<int, int> equivalentElectricSchemePinIdByPinId = new Dictionary<int, int>(panelPinPhysicalIdById.Count);
            List<PinSymbol> localPanelPinSymbols = new List<PinSymbol>(panelPinPhysicalIdById.Count);
            foreach (int pinId in panelPinPhysicalIdById.Keys)
            {
                //List<int> equivalentElectricSchemePinIds = electricSchemePinIdsByEquivalences.Values.Where(il => il.Contains(pinId)).First();
                //equivalentElectricSchemePinIds.Remove(pinId);
                //equivalentElectricSchemePinIdsByPinId.Add(pinId, equivalentElectricSchemePinIds);
                //Tuple<int, int> equivalence = panelPinsEquivalenceById[pinId];
                int physicalId = panelPinPhysicalIdById[pinId];
                if (electricSchemePinIdsByPhysicalId.ContainsKey(physicalId))
                {
                    int electricSchemePinId = electricSchemePinIdsByPhysicalId[physicalId];
                    equivalentElectricSchemePinIdByPinId.Add(pinId, electricSchemePinId);
                    localPanelPinSymbols.Add(new PinSymbol(pin, pinId, electricSchemePinId));
                }
                else
                    localPanelPinSymbols.Add(new PinSymbol(pin, pinId, 0));
            }
            List<int> localElectricSchemeEquivalentPinIds = new List<int>(panelPinPhysicalIdById.Keys);
            equivalentElectricSchemePinIdByPinId.Values.ToList().ForEach(l => localElectricSchemeEquivalentPinIds.Add(l));
            panelPinSymbols = localPanelPinSymbols;
            electricSchemeEquivalentPinIds = localElectricSchemeEquivalentPinIds;
        }

        /*private void GetPanelPinIdsAndEquivalentPinIds(List<int> pinIds, DevicePin pin, out List<int> panelPinIds, out Dictionary<int, List<int>> equivalentPinIdsByPinId)
        {
            panelPinIds = new List<int>();
            Dictionary<Tuple<int, int>, List<int>> pinIdsByEquivalences = new Dictionary<Tuple<int, int>, List<int>>();
            foreach (int pinId in pinIds)
            {
                pin.Id = pinId;
                Tuple<int, int> equivalences = new Tuple<int, int>(pin.LogicalEquivalence, pin.NameEquivalence);
                if (!pinIdsByEquivalences.ContainsKey(equivalences))
                    pinIdsByEquivalences.Add(equivalences, new List<int>());
                pinIdsByEquivalences[equivalences].Add(pinId);
                if (pin.IsOnPanel)
                    panelPinIds.Add(pinId);
            }
            equivalentPinIdsByPinId = new Dictionary<int, List<int>>(panelPinIds.Count);
            foreach (int pinId in panelPinIds)
            {
                List<int> equivalentPinIds = pinIdsByEquivalences.Values.Where(il => il.Contains(pinId)).First();
                equivalentPinIds.Remove(pinId);
                equivalentPinIdsByPinId.Add(pinId, equivalentPinIds);
            }
        }

        private void GetPanelUtmostCoordinates(out double xLeft, out double xRight, out double yTop, out double yBottom, Outline outline, List<int> outlineIds)
        {
            foreach (int outlineId in outlineIds)
            {
                outline.Id = outlineId;
                if (outline.Type == OutlineType.NormalOutline)
                    break;
            }
            List<Point> points = outline.GetPath();
            xLeft = points.Min(p => p.X);
            xRight = points.Max(p => p.X);
            yBottom = points.Min(p => p.Y);
            yTop = points.Max(p => p.Y);
            return new Rect(xLeft, yTop, xRight - xLeft, yTop - yBottom);
        }*/

        public void CalculateNameSize(E3Text text, Settings settings)
        {
            nameSize = text.GetTextBoxSize(name, settings.Font, 0);
            //topAddressesLength = (topPinSymbols.Count > 0) ? topPinSymbols.Max(ts => ts.AddressesSize.Height) : 0;
            //bottomAddressesLength = (bottomPinSymbols.Count > 0) ? bottomPinSymbols.Max(bs => bs.AddressesSize.Height) : 0;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, Sheet sheet, Point position, Dictionary<string, ComponentLayout> componentLayoutByName)
        {
            ComponentLayout componentLayout = componentLayoutByName[Component];
            List<int> graphicIds = new List<int>(isTerminal ? 8 : 2 + panelPinSymbols.Count * 5); // общее количество элементов графики
            double xLeft = sheet.MoveLeft(position.X, componentLayout.OutlineWidth / 2);
            double xRight = sheet.MoveRight(xLeft, componentLayout.OutlineWidth);
            double yBottom = sheet.MoveDown(position.Y, componentLayout.OutlineHeight / 2);
            double yTop = sheet.MoveUp(yBottom, componentLayout.OutlineHeight);
            int sheetId = sheet.Id;
            Graphic graphic = projectObjects.Graphic;
            E3Text text = projectObjects.Text;
            Group group = projectObjects.Group;
            graphicIds.Add(graphic.CreateRectangle(sheetId, xLeft, yBottom, xRight, yTop));
            if (isTerminal)
                graphicIds.Add(text.CreateVerticalText(sheetId, terminalName, sheet.MoveRight(position.X, settings.SmallFont.height / 2 ), position.Y, settings.SmallFont));
            else
                graphicIds.Add(text.CreateText(sheetId, name, position.X, sheet.MoveUp(position.Y, componentLayout.NameVerticalOffset), settings.Font));
            graphicIds.AddRange(PlacePins(projectObjects, settings, sheet, position, componentLayout, sheetId, Level.Top, yTop));
            graphicIds.AddRange(PlacePins(projectObjects, settings, sheet, position, componentLayout, sheetId, Level.Bottom, yBottom));
            group.CreateGroup(graphicIds);
        }

        private List<int> PlacePins(ProjectObjects projectObjects, Settings settings, Sheet sheet, Point position, ComponentLayout componentLayout, int sheetId, Level level, double edgeY)
        {
            List<double> horizontalOffsets;
            List<PinSymbol> pins;
            double pinsY;
            sheet.Id = sheetId;
            if (level == Level.Top)
            {
                horizontalOffsets = componentLayout.TopPinsHorizontalOffsets;
                pins = topPinSymbols;
                pinsY = sheet.MoveDown(edgeY, componentLayout.PinHeight / 2);
            }
            else
            {
                horizontalOffsets = componentLayout.BottomPinsHorizontalOffsets;
                pins = bottomPinSymbols;
                pinsY = sheet.MoveUp(edgeY, componentLayout.PinHeight / 2);
            }
            List<int> graphicIds = new List<int>(horizontalOffsets.Count * 5);
            for (int i = 0; i < horizontalOffsets.Count; i++)
            {
                double pinX = sheet.MoveRight(position.X, horizontalOffsets[i]);
                graphicIds.AddRange(pins[i].Place(projectObjects, settings, sheet, new Point(pinX, pinsY), componentLayout, level, isTerminal, sheetId)); 
            }
            return graphicIds;
        }

        private class PinSymbolComparer : IComparer<PinSymbol>
        {
            public int Compare(PinSymbol a, PinSymbol b)
            {
                int result = -(int)(a.PanelY - b.PanelY);
                if (result != 0)
                    return result;
                return (int)(a.PanelX - b.PanelX);

            }
        }
    }
}
