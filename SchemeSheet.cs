using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class SchemeSheet
    {
        private SheetFormat format;

        public SheetFormat Format
        {
            get
            {
                return format;
            }
        }

        private Dictionary<int, SheetSymbolRow> sheetSymbolRowByRowNumber;

        public SchemeSheet()
        {
            sheetSymbolRowByRowNumber = new Dictionary<int, SheetSymbolRow>();
        }

        private void AddSymbolInSheet(RowSymbolInSheet symbolInSheet)
        {
            if (!sheetSymbolRowByRowNumber.ContainsKey(symbolInSheet.RowNumber))
                sheetSymbolRowByRowNumber.Add(symbolInSheet.RowNumber, new SheetSymbolRow());
            sheetSymbolRowByRowNumber[symbolInSheet.RowNumber].AddSymbol(symbolInSheet.Symbol);
        }

        private void SetRowsVerticalOffsetFromTop(Dictionary<int, double> offsetFromTopByRowNumber)
        {
            foreach (int rowNumber in sheetSymbolRowByRowNumber.Keys)
                sheetSymbolRowByRowNumber[rowNumber].VerticalOffsetFromTop = offsetFromTopByRowNumber[rowNumber];
        }

        private void CalculateRowsMinimalWidth(double gridStep)
        {
            foreach (SheetSymbolRow row in sheetSymbolRowByRowNumber.Values)
                row.CalculateMinimalWidth(gridStep);
        }

        private void SelectFormat(Settings settings)
        {
            double maxMinWidth = sheetSymbolRowByRowNumber.Values.Max(sr => sr.MinimalWidth);
            format = maxMinWidth <= settings.A4Subsequent.AvailableWidth ? settings.A4First : settings.A3First;
        }

        private void SelectFormat(Settings settings, bool isFirst)
        {
            double maxMinWidth = sheetSymbolRowByRowNumber.Values.Max(sr => sr.MinimalWidth);
            if (isFirst)
                format = maxMinWidth <= settings.A4Subsequent.AvailableWidth ? settings.A4First : settings.A3First;
            else
                format = maxMinWidth <= settings.A4Subsequent.AvailableWidth ? settings.A4Subsequent : settings.A3Subsequent;
        }

        public static SchemeSheet[][] GetSheetsLayout(List<Row> rows, Settings settings, bool isFirst)
        {
            double titleGap = settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset + settings.HalfGridStep;
            double firstVerticalFreeSpace = isFirst ? settings.A3First.AvailableHeight : settings.A3Subsequent.AvailableHeight;
            firstVerticalFreeSpace -= titleGap;
            List<RowSymbolInSheet> symbolsInSheet = GetRowSymbolsInSheet(rows, settings, titleGap, firstVerticalFreeSpace);
            Dictionary<int, double> offsetFromTopByRowNumber = GetOffsetFromTopByRowNumber(symbolsInSheet, rows, settings, titleGap, firstVerticalFreeSpace);
            Dictionary<Tuple<int, int>, SchemeSheet> schemeSheetByPosition = new Dictionary<Tuple<int, int>, SchemeSheet>();
            Dictionary<int, int> maxHorizontalSheetNumberByVerticalNumber = new Dictionary<int, int>();
            foreach (RowSymbolInSheet symbolInSheet in symbolsInSheet)
            {
                Tuple<int, int> position = new Tuple<int, int>(symbolInSheet.VerticalSheetNumber, symbolInSheet.HorizontalSheetNumber);
                if (!schemeSheetByPosition.ContainsKey(position))
                    schemeSheetByPosition.Add(position, new SchemeSheet());
                schemeSheetByPosition[position].AddSymbolInSheet(symbolInSheet);
                if (!maxHorizontalSheetNumberByVerticalNumber.ContainsKey(symbolInSheet.VerticalSheetNumber))
                    maxHorizontalSheetNumberByVerticalNumber.Add(symbolInSheet.VerticalSheetNumber, 0);
                maxHorizontalSheetNumberByVerticalNumber[symbolInSheet.VerticalSheetNumber] = Math.Max(maxHorizontalSheetNumberByVerticalNumber[symbolInSheet.VerticalSheetNumber], symbolInSheet.HorizontalSheetNumber);
            }
            foreach (SchemeSheet schemeSheet in schemeSheetByPosition.Values)
            {
                schemeSheet.SetRowsVerticalOffsetFromTop(offsetFromTopByRowNumber);
                schemeSheet.CalculateRowsMinimalWidth(settings.GridStep);
                schemeSheet.SelectFormat(settings, false);
            }
            Tuple<int, int> firstPosition = new Tuple<int, int>(0, 0);
            schemeSheetByPosition[firstPosition].SelectFormat(settings, isFirst);
            SchemeSheet[][] sheetsLayout = Array.CreateInstance(typeof(SchemeSheet[]), maxHorizontalSheetNumberByVerticalNumber.Keys.Count) as SchemeSheet[][];
            foreach (int verticalNumber in maxHorizontalSheetNumberByVerticalNumber.Keys)
                sheetsLayout[verticalNumber] = Array.CreateInstance(typeof(SchemeSheet), maxHorizontalSheetNumberByVerticalNumber[verticalNumber] + 1) as SchemeSheet[];
            foreach (Tuple<int, int> position in schemeSheetByPosition.Keys)
                sheetsLayout[position.Item1][position.Item2] = schemeSheetByPosition[position];
            return sheetsLayout;
        }

        private static Dictionary<int, double> GetOffsetFromTopByRowNumber(List<RowSymbolInSheet> symbolsInSheet, List<Row> rows, Settings settings, double titleGap, double firstVerticalFreeSpace)
        {
            Dictionary<int, double> offsetFromTopByRowNumber = new Dictionary<int, double>(rows.Count);
            Dictionary<int, List<int>> rowNumbersByVerticalSheetNumber = GetRowNumbersByVerticalSheetNumber(symbolsInSheet);
            double verticalFreeSpace = firstVerticalFreeSpace;
            foreach (List<int> rowNumbers in rowNumbersByVerticalSheetNumber.Values)
            {
                double rowsHeight = rowNumbers.Sum(rn => rows[rn].Height);
                double totalVerticalGap = verticalFreeSpace - rowsHeight;
                double verticalGap = totalVerticalGap / rowNumbers.Count;
                double offset = settings.A3Subsequent.TopBorder + verticalGap / 2 + titleGap;
                foreach (int rowNumber in rowNumbers)
                {
                    Row row = rows[rowNumber];
                    offset += row.TopMargin;
                    offsetFromTopByRowNumber.Add(rowNumber, offset);
                    offset += (row.BottomMargin + verticalGap);
                }
            }
            return offsetFromTopByRowNumber;
        }

        private static Dictionary<int, List<int>> GetRowNumbersByVerticalSheetNumber(List<RowSymbolInSheet> symbolsInSheet)
        {
            Dictionary<int, List<int>> rowNumbersByVerticalSheetNumber = new Dictionary<int, List<int>>();
            foreach (RowSymbolInSheet symbolInSheet in symbolsInSheet)
            {
                int verticalSheetNumber = symbolInSheet.VerticalSheetNumber;
                if (!rowNumbersByVerticalSheetNumber.ContainsKey(verticalSheetNumber))
                    rowNumbersByVerticalSheetNumber.Add(verticalSheetNumber, new List<int>());
                if (!rowNumbersByVerticalSheetNumber[verticalSheetNumber].Contains(symbolInSheet.RowNumber))
                    rowNumbersByVerticalSheetNumber[verticalSheetNumber].Add(symbolInSheet.RowNumber);
            }
            return rowNumbersByVerticalSheetNumber;
        }

        private static List<RowSymbolInSheet> GetRowSymbolsInSheet(List<Row> rows, Settings settings, double titleGap, double firstVerticalFreeSpace)
        {
            List<RowSymbolInSheet> rowSymbolsInSheet = new List<RowSymbolInSheet>(rows.Sum(r => r.Symbols.Count));
            double verticalFreeSpace = firstVerticalFreeSpace;
            int verticalSheetNumber = 0;
            foreach (Row row in rows)
            {
                verticalFreeSpace -= (row.Height + settings.GridStep);
                if (verticalFreeSpace < 0)
                {
                    verticalSheetNumber++;
                    verticalFreeSpace = settings.A3Subsequent.AvailableHeight - (row.Height + titleGap + settings.GridStep);
                }
                int horizontalSheetNumber = 0;
                double horizontalFreeSpace = settings.A3Subsequent.AvailableWidth;
                foreach (RowSymbol symbol in row.Symbols)
                {
                    horizontalFreeSpace -= symbol.Width + settings.GridStep;
                    if (horizontalFreeSpace < 0)
                    {
                        horizontalSheetNumber++;
                        horizontalFreeSpace = settings.A3Subsequent.AvailableWidth - (symbol.Width + settings.GridStep);
                    }
                    rowSymbolsInSheet.Add(new RowSymbolInSheet(row.Number, symbol, verticalSheetNumber, horizontalSheetNumber));
                }
            }
            return rowSymbolsInSheet;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, string cabinetSideName, int sheetNumber, StampAttributes sheetAttributes)
        {
            E3Text text = projectObjects.Text;
            Sheet sheet = projectObjects.Sheet;
            int sheetId = sheet.Create(sheetNumber.ToString(), format.Name);
            sheetAttributes.SetAttributes(sheet, text, sheetNumber);
            double titleOrdinate = sheet.MoveDown(sheet.DrawingArea.Top, format.TopBorder + settings.SheetTitleFont.height + settings.HalfGridStep);
            double titleAbsciss = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + format.AvailableWidth / 2);
            text.CreateText(sheetId, cabinetSideName, titleAbsciss, titleOrdinate, settings.SheetTitleFont);
            foreach (SheetSymbolRow row in sheetSymbolRowByRowNumber.Values)
            {
                double ordinate = sheet.MoveDown(sheet.DrawingArea.Top, row.VerticalOffsetFromTop);
                double totalHorizontalGap = format.AvailableWidth - row.RowSymbols.Sum(s => s.Width);
                double horizontalGap = totalHorizontalGap / row.RowSymbols.Count;
                double offset = format.LeftBorder + horizontalGap / 2;
                foreach (RowSymbol symbol in row.RowSymbols)
                {
                    double halfWidth = symbol.Width / 2;
                    offset += halfWidth;
                    double absciss = sheet.MoveRight(sheet.DrawingArea.Left, offset);
                    symbol.Place(projectObjects, sheet, sheetId, new Point(absciss, ordinate));
                    offset += (halfWidth + horizontalGap);
                }
            }
        }

        private class RowSymbolInSheet
        {
            public int RowNumber { get; private set; }
            public RowSymbol Symbol { get; private set; }
            public int VerticalSheetNumber { get; private set; }
            public int HorizontalSheetNumber { get; private set; }

            public RowSymbolInSheet(int rowNumber, RowSymbol symbol, int verticalSheetNumber, int horizontalSheetNumber)
            {
                RowNumber = rowNumber;
                Symbol = symbol;
                VerticalSheetNumber = verticalSheetNumber;
                HorizontalSheetNumber = horizontalSheetNumber;
            }

        }
        
        private class SheetSymbolRow
        {
            public double VerticalOffsetFromTop { get; set; }
            public double MinimalWidth { get; private set; }

            private List<RowSymbol> rowSymbols;

            public List<RowSymbol> RowSymbols
            {
                get
                {
                    return rowSymbols;
                }
            }

            public SheetSymbolRow()
            {
                rowSymbols = new List<RowSymbol>();
            }

            public void AddSymbol(RowSymbol symbol)
            {
                rowSymbols.Add(symbol);
            }

            public void CalculateMinimalWidth(double gridStep)
            { 
                MinimalWidth = rowSymbols.Sum(rs=>rs.Width)+rowSymbols.Count * gridStep;
            }
        }

    }
}
