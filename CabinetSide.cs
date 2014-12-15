using System;
using System.Collections.Generic;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class CabinetSide
    {
        private string name;
        private SchemeSheet[][] schemeSheetLayout;
        private List<Row> rows;

        public IEnumerable<Element> Elements
        {
            get
            {
                return rows.SelectMany(r=>r.Symbols.SelectMany(s=>s.Elements));
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public int SheetCount
        {
            get
            {
                return schemeSheetLayout.Sum(r => r.Count());
            }
        }

        public CabinetSide(ProjectObjects projectObjects, string name, int rootId, List<DeviceOutline> outlines, Sheet sheet, HashSet<int> electricSchemeSheetIds, ComponentManager componentManager, List<int> verticalMountIds)
        {
            if (rootId > 0)
            {
                NormalDevice device = projectObjects.Device;
                device.Id = rootId;
                this.name = name + " " + device.Name;
            }
            else
                this.name = name;
            rows = GetRows(projectObjects, outlines, sheet, electricSchemeSheetIds, componentManager, verticalMountIds, out rows);
        }

        private static List<Row> GetRows(ProjectObjects projectObjects, List<DeviceOutline> outlines, Sheet sheet, HashSet<int> electricSchemeSheetIds, ComponentManager componentManager, List<int> verticalMountIds, out List<Row> rows)
        {
            outlines.Sort(new DeviceOutlineOnSheetVerticalComparer(sheet));
            double previousBottom = outlines.First().GetBottom(sheet);
            List<List<DeviceOutline>> outlineRows = new List<List<DeviceOutline>>();
            outlineRows.Add(new List<DeviceOutline>());
            foreach (DeviceOutline outline in outlines)
            {
                double top = outline.GetTop(sheet);
                if (sheet.IsAboveTarget(top, previousBottom))
                    outlineRows.Add(new List<DeviceOutline>());
                outlineRows.Last().Add(outline);
                previousBottom = outline.GetBottom(sheet);
            }
            NormalDevice device = projectObjects.Device;
            rows = new List<Row>(outlineRows.Count);
            int rowNumber = 0;
            foreach (List<DeviceOutline> deviceOutlines in outlineRows)
            {
                deviceOutlines.Sort(new DeviceOutlineOnSheetHorizontalComparer(sheet));
                List<TerminalElement> terminalElements = new List<TerminalElement>();
                List<RowSymbol> rowSymbols = new List<RowSymbol>();
                foreach (DeviceOutline deviceOutline in deviceOutlines)
                {
                    int deviceId = deviceOutline.DeviceId;
                    device.Id = deviceId;
                    Element element = GetElement(projectObjects, componentManager, verticalMountIds, device, deviceOutline, electricSchemeSheetIds);
                    TerminalElement terminalElement = element as TerminalElement;
                    if (terminalElement != null)
                    {
                        if (terminalElements.Count > 0 && !terminalElements.Last().Name.Equals(terminalElement.Name))
                        {
                            rowSymbols.Add(new TerminalStripRowSymbol(terminalElements));
                            terminalElements = new List<TerminalElement>();
                        }
                        terminalElements.Add(terminalElement);
                    }
                    else
                    {
                        if (terminalElements.Count > 0)
                        {
                            rowSymbols.Add(new TerminalStripRowSymbol(terminalElements));
                            terminalElements = new List<TerminalElement>();
                        }
                        rowSymbols.Add(new SingleRowSymbol(element));
                    }
                }
                if (terminalElements.Count > 0)
                    rowSymbols.Add(new TerminalStripRowSymbol(terminalElements));
                rows.Add(new Row(rowNumber++, rowSymbols));
            }
            return rows;
        }

        private static Element GetElement(ProjectObjects projectObjects, ComponentManager componentManager, List<int> verticalMountIds, NormalDevice device, DeviceOutline deviceOutline, HashSet<int> electricSchemeSheetIds)
        {
            Orientation orientation = GetOrientation(verticalMountIds, device);
            Element element;
            if (device.IsTerminal())
                element = new TerminalElement(projectObjects, deviceOutline, orientation, componentManager, electricSchemeSheetIds);
            else
                element = new DeviceElement(projectObjects, deviceOutline, orientation, componentManager, electricSchemeSheetIds);
            return element;
        }

        private static Orientation GetOrientation(List<int> verticalMountIds, NormalDevice device)
        {
            int mountId = device.CarrierId;
            Orientation orientation = Orientation.Vertical;
            if (mountId > 0 && verticalMountIds.Contains(mountId))
                orientation = Orientation.Horizontal;
            return orientation;
        }

        public void CalculateRows()
        {
            rows.ForEach(r => r.Calculate());
        }

        public bool IsFitIntoOneSheet(Settings settings, bool isFirstCabinet)
        {
            double availableHeight = isFirstCabinet ? settings.A3First.AvailableHeight : settings.A3Subsequent.AvailableHeight;
            double totalHeight = rows.Sum(r => r.Height) + settings.GridStep * rows.Count;
            if (totalHeight >= availableHeight)
                return false;
            double totalWidth = rows.Max(r => r.Symbols.Sum(s => s.Width) + r.Symbols.Count * settings.GridStep); 
            return settings.A3Subsequent.AvailableWidth >= totalWidth;
        }

        public void SetSheetLayout(Settings settings, bool isFirst)
        {
            schemeSheetLayout = SchemeSheet.GetSheetsLayout(rows, settings, isFirst);
        }

        public SheetFormat[][] GetFormatLayout()
        {
            int rowCount = schemeSheetLayout.Count();
            SheetFormat[][] formatLayout = Array.CreateInstance(typeof(SheetFormat[]), rowCount) as SheetFormat[][];
            for (int i = 0; i < rowCount; i++ )
            {
                int sheetCount = schemeSheetLayout[i].Count();
                formatLayout[i] = Array.CreateInstance(typeof(SheetFormat), sheetCount) as SheetFormat[];
                for (int j = 0; j < sheetCount; j++)
                    formatLayout[i][j] = schemeSheetLayout[i][j].Format;
            }
            return formatLayout;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, ref int sheetNumber, StampAttributes sheetAttributes)
        {
            int verticalRowCount = schemeSheetLayout.Count();
            for (int verticalIndex = 0; verticalIndex < verticalRowCount; verticalIndex++)
            {
                int sheetInRowCount = schemeSheetLayout[verticalIndex].Count();
                for (int horizontalIndex = 0; horizontalIndex < sheetInRowCount; horizontalIndex++)
                {
                    SchemeSheet schemeSheet = schemeSheetLayout[verticalIndex][horizontalIndex];
                    schemeSheet.Place(projectObjects, settings, name, sheetNumber++, sheetAttributes);
                }
            }
        }

        private class DeviceOutlineOnSheetVerticalComparer : IComparer<DeviceOutline>
        {
            private Sheet sheet;

            public DeviceOutlineOnSheetVerticalComparer(Sheet sheet)
            {
                this.sheet = sheet;
            }

            public int Compare(DeviceOutline a, DeviceOutline b)
            {
                if (a.Center.Y == b.Center.Y)
                    return 0;
                if (sheet.IsAboveTarget(b.Center.Y, a.Center.Y))
                    return -1;
                else
                    return 1;
            }
        }
    }
}
