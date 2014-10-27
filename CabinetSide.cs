using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class CabinetSide
    {
        private string name;
        //private List<List<DeviceRow>> rowsBySheet;
        //private Dictionary<int, List<List<RowSymbol>>> rowSymbolsBySheetsByRowNumber;
        private SchemeSheet[][] schemeSheetLayout;
        private List<DeviceRow> deviceRows;
        
        public List<DeviceRow> DeviceRows
        {
            get
            {
                return deviceRows;
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

        //public string Cabinet { get; private set; }

        public CabinetSide(ProjectObjects projectObjects, Settings settings, string cabinet, string name, int rootId, List<DeviceOutline> outlines, Sheet sheet, HashSet<int> electricSchemeSheetIds)
        {
            if (rootId > 0)
            {
                NormalDevice device = projectObjects.Device;
                device.Id = rootId;
                this.name = name + " " + device.Name;
            }
            else
                this.name = name;
            //Cabinet = cabinet;
            deviceRows = GetRows(projectObjects, settings, outlines, sheet, electricSchemeSheetIds);
        }

        private static List<DeviceRow> GetRows(ProjectObjects projectObjects, Settings settings, List<DeviceOutline> outlines, Sheet sheet, HashSet<int> electricSchemeSheetIds)
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
            List<DeviceRow> localDeviceRows = new List<DeviceRow>(outlineRows.Count);
            int rowNumber = 0;
            foreach (List<DeviceOutline> deviceOutlines in outlineRows)
            {
                deviceOutlines.Sort(new DeviceOutlineOnSheetHorizontalComparer(sheet));
                List<DeviceSymbol> terminalSymbols = new List<DeviceSymbol>();
                List<RowSymbol> rowSymbols = new List<RowSymbol>();
                foreach (DeviceOutline deviceOutline in deviceOutlines)
                {
                    int deviceId = deviceOutline.DeviceId;
                    device.Id = deviceId;
                    DeviceSymbol deviceSymbol = new DeviceSymbol(projectObjects, settings, deviceId, electricSchemeSheetIds, (ordinate) => { return sheet.IsAboveTarget(deviceOutline.Center.Y, ordinate); });
                    if (deviceSymbol.IsTerminal)
                    {
                        if (terminalSymbols.Count > 0 && !terminalSymbols.Last().Name.Equals(deviceSymbol.Name))
                        {
                            rowSymbols.Add(new RowSymbol(terminalSymbols));
                            terminalSymbols = new List<DeviceSymbol>();
                        }
                        terminalSymbols.Add(deviceSymbol);
                    }
                    else
                    {
                        if (terminalSymbols.Count > 0)
                        {
                            rowSymbols.Add(new RowSymbol(terminalSymbols));
                            terminalSymbols = new List<DeviceSymbol>();
                        }
                        rowSymbols.Add(new RowSymbol(deviceSymbol));
                    }
                }
                if (terminalSymbols.Count > 0)
                    rowSymbols.Add(new RowSymbol(terminalSymbols));
                localDeviceRows.Add(new DeviceRow(rowNumber++, rowSymbols));
            }
            return localDeviceRows;
        }

        public void CalculateRows(Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName)
        {
            deviceRows.ForEach(dr => dr.CalculateLayout(settings, componentLayoutByName));
        }

        public bool IsFitIntoOneSheet(Settings settings, bool isFirstCabinet)
        {
            double availableHeight = isFirstCabinet ? settings.A3First.AvailableHeight : settings.A3Subsequent.AvailableHeight;
            double totalHeight = deviceRows.Sum(dr => dr.Height) + settings.GridStep * deviceRows.Count;
            if (totalHeight >= availableHeight)
                return false;
            double totalWidth = deviceRows.Max(dr => dr.RowSymbols.Sum(rs => rs.Width) + dr.RowSymbols.Count * settings.GridStep);
            return settings.A3Subsequent.AvailableWidth >= totalWidth;
        }

        public void SetSheetLayout(Settings settings, bool isFirst)
        {
            schemeSheetLayout = SchemeSheet.GetSheetsLayout(deviceRows, settings, isFirst);
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

        public void Place(ProjectObjects projectObjects, Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName, ref int sheetNumber, StampAttributes sheetAttributes)
        {
            int verticalRowCount = schemeSheetLayout.Count();
            for (int verticalIndex = 0; verticalIndex < verticalRowCount; verticalIndex++)
            {
                int sheetInRowCount = schemeSheetLayout[verticalIndex].Count();
                for (int horizontalIndex = 0; horizontalIndex < sheetInRowCount; horizontalIndex++)
                {
                    SchemeSheet schemeSheet = schemeSheetLayout[verticalIndex][horizontalIndex];
                    schemeSheet.Place(projectObjects, settings, componentLayoutByName, name, sheetNumber++, sheetAttributes);
                }
            }
        }

        /*
        public List<List<int>> GetGroupedSheetNumbers(Settings settings, bool isFirstCabinet)
        {
            rowsBySheet = GetRowsBySheet(settings, isFirstCabinet);
            rowSymbolsBySheetsByRowNumber = GetSymbolsBySheetsByRow(settings);
            List<List<int>> groupedSheetNumbers = new List<List<int>>(rowsBySheet.Count);
            int sheetNumber = 1;
            foreach (List<DeviceRow> rows in rowsBySheet)
            {
                List<int> sheetNumbers = new List<int>();
                sheetNumbers.Add(sheetNumber++);
                foreach (DeviceRow row in rows)
                {
                    List<List<RowSymbol>> symbolsBySheet = rowSymbolsBySheetsByRowNumber[row.Number];
                    for (int i = 0; i < symbolsBySheet.Count; i++)
                        if (i >= sheetNumbers.Count)
                            sheetNumbers.Add(sheetNumber++);
                }
                groupedSheetNumbers.Add(sheetNumbers);
            }
            return groupedSheetNumbers;
        }

        /*
        public void Place(ProjectObjects projectObjects, Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName, ref int sheetNumber, string sheetMark, string subProjectMark, bool isFirstCabinet)
        {
            E3Text text = projectObjects.Text;
            //deviceRows.ForEach(dr => dr.CalculateLayout(settings, componentLayoutByName));
            //rowsBySheet = GetRowsBySheet(settings);
            //rowSymbolsBySheetsByRowNumber = GetSymbolsBySheetsByRow(settings);
            Sheet sheet = projectObjects.Sheet;
            for (int i = 0; i < rowsBySheet.Count; i++ )
            {
                List<DeviceRow> rows = rowsBySheet[i];
                List<int> sheetIds = new List<int>();
                SheetFormat format = (isFirstCabinet && i == 0 && sheetNumber == 1) ? settings.A3First : settings.A3Subsequent;
                sheetIds.Add(sheet.Create((sheetNumber++).ToString(), format.Name));
                if (!String.IsNullOrEmpty(sheetMark))
                    sheet.SetAttribute(settings.SheetMarkAttribute, sheetMark);
                if (!String.IsNullOrEmpty(subProjectMark))
                    sheet.SetAttribute(settings.SubProjectAttribute, subProjectMark);
                double titleOrdinate = sheet.MoveDown(sheet.DrawingArea.Top, format.TopBorder + settings.SheetTitleFont.height + settings.HalfGridStep);
                double titleAbsciss = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + format.AvailableWidth / 2);
                text.CreateText(sheetIds.First(), Name, titleAbsciss, titleOrdinate, settings.SheetTitleFont);
                double rowsHeight = rows.Sum(r => r.Height);
                double totalVerticalGap = format.AvailableHeight - (rowsHeight + settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset + settings.HalfGridStep);
                double verticalGap = totalVerticalGap / rows.Count;
                double ordinate = sheet.MoveDown(sheet.DrawingArea.Top, format.TopBorder + settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset + settings.HalfGridStep + verticalGap / 2);
                foreach (DeviceRow row in rows)
                {
                    ordinate = sheet.MoveDown(ordinate, row.TopMargin);
                    List<List<RowSymbol>> symbolsBySheet = rowSymbolsBySheetsByRowNumber[row.Number];
                    for (int j = 0; j < symbolsBySheet.Count; j++)
                    {
                        List<RowSymbol> rowSymbols = symbolsBySheet[j];
                        if (j >= sheetIds.Count)
                        {
                            sheetIds.Add(sheet.Create((sheetNumber++).ToString(), format.Name));
                            if (!String.IsNullOrEmpty(sheetMark))
                                sheet.SetAttribute(settings.SheetMarkAttribute, sheetMark);
                            if (!String.IsNullOrEmpty(subProjectMark))
                                sheet.SetAttribute(settings.SubProjectAttribute, subProjectMark);
                            text.CreateText(sheetIds.Last(), Name, titleAbsciss, titleOrdinate, settings.SheetTitleFont);
                        }
                        sheet.Id = sheetIds[j];
                        double symbolsWidth = rowSymbols.Sum(rs => rs.Width);
                        double totalHorizontalGap = format.AvailableWidth - symbolsWidth;
                        double horizontalGap = totalHorizontalGap / rowSymbols.Count;
                        double absciss = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + horizontalGap / 2);
                        foreach (RowSymbol rowSymbol in rowSymbols)
                        {
                            double halfWidth = rowSymbol.Width / 2;
                            absciss = sheet.MoveRight(absciss, halfWidth);
                            rowSymbol.Place(projectObjects, settings, sheet, new Point(absciss, ordinate), componentLayoutByName);
                            absciss = sheet.MoveRight(absciss, halfWidth + horizontalGap);
                        }
                    }
                    ordinate = sheet.MoveDown(ordinate, row.BottomMargin + verticalGap);
                }
            }
        }
        */

        private List<List<DeviceRow>> GetRowsBySheet(Settings settings, bool isFirstCabinet)
        {
            List<List<DeviceRow>> rowsBySheet = new List<List<DeviceRow>>();
            List<DeviceRow> rows = new List<DeviceRow>();
            double availableHeight = (isFirstCabinet ? settings.A3First.AvailableHeight : settings.A3Subsequent.AvailableHeight) - (settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset);
            double totalHeight = settings.GridStep;
            SheetFormat format = settings.A3Subsequent;
            foreach (DeviceRow row in deviceRows)
            {
                double rowHeight = row.Height;
                if ((rowHeight + settings.GridStep * 2) > availableHeight)
                {
                    if (rows.Count > 0)
                    {
                        rowsBySheet.Add(rows);
                        rows = new List<DeviceRow>();
                    }
                    rowsBySheet.Add(new List<DeviceRow> { row });
                    totalHeight = settings.GridStep;
                    availableHeight = format.AvailableHeight;
                }
                else
                {
                    totalHeight += row.Height + settings.GridStep;
                    if (totalHeight >= availableHeight)
                    {
                        rowsBySheet.Add(rows);
                        rows = new List<DeviceRow> { row };
                        totalHeight = settings.GridStep + rowHeight;
                        availableHeight = format.AvailableHeight;
                    }
                    else
                        rows.Add(row);
                }
            }
            if (rows.Count > 0)
                rowsBySheet.Add(rows);
            return rowsBySheet;
        }

        private Dictionary<int, List<List<RowSymbol>>> GetSymbolsBySheetsByRow(Settings settings)
        {
            Dictionary<int, List<List<RowSymbol>>> rowSymbolsBySheetsByRowNumber = new Dictionary<int, List<List<RowSymbol>>>(deviceRows.Count);
            double availableWidth = settings.A3Subsequent.AvailableWidth;
            foreach (DeviceRow deviceRow in deviceRows)
            {
                List<List<RowSymbol>> symbolsBySheetsByRow = new List<List<RowSymbol>>();
                List<RowSymbol> symbols = new List<RowSymbol>();
                double width = settings.GridStep;
                foreach (RowSymbol rowSymbol in deviceRow.RowSymbols)
                {
                    double symbolWidth = rowSymbol.Width;
                    if ((symbolWidth + settings.GridStep * 2) > availableWidth)
                    {
                        if (symbols.Count > 0)
                        {
                            symbolsBySheetsByRow.Add(symbols);
                            symbols = new List<RowSymbol>();
                        }
                        symbolsBySheetsByRow.Add(new List<RowSymbol> { rowSymbol });
                        width = settings.GridStep;
                    }
                    else
                    {
                        width += symbolWidth + settings.GridStep;
                        if (width >= availableWidth)
                        {
                            symbolsBySheetsByRow.Add(symbols);
                            symbols = new List<RowSymbol> { rowSymbol };
                            width = settings.GridStep + symbolWidth;
                        }
                        else
                            symbols.Add(rowSymbol);
                    }
                }
                if (symbols.Count > 0)
                    symbolsBySheetsByRow.Add(symbols);
                rowSymbolsBySheetsByRowNumber.Add(deviceRow.Number, symbolsBySheetsByRow);
            }
            return rowSymbolsBySheetsByRowNumber;
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
