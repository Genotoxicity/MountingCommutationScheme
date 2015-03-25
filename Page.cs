using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class Page
    {
        private SheetFormat format;
        private List<ElementSequence> pageElementSequences;
        private double minSymbolGap;
        private double minLineGap;
        private double minSequenceGap;
        private double availableWidth;
        private double availableHeight;
        private string sideName;
        private ProjectObjects projectObjects;
        private List<SymbolLines> symbolLines;
        private E3Font titleFont;

        public bool IsSequenceCarry { get; private set; }

        public List<ElementSequence> Sequences
        {
            get
            {
                return pageElementSequences;
            }
        }

        public int LastSymbol { get; private set; }

        public Page(ProjectObjects projectObjects, SheetFormat format, List<ElementSequence> elementSequences, int startSymbol, string sideName)
        {
            this.format = format;
            this.sideName = sideName;
            this.projectObjects = projectObjects;
            titleFont = Settings.SheetTitleFont;
            minSymbolGap = Settings.HalfGridStep;
            minLineGap = Settings.GridStep;
            minSequenceGap = minLineGap * 2;
            availableWidth = format.AvailableWidth;
            availableHeight = format.AvailableHeight - (Settings.SheetTitleFont.height + minSymbolGap+Settings.SheetTitleUnderlineOffset);
            pageElementSequences = GetPageSequences(elementSequences, startSymbol);
        }

        private List<ElementSequence> GetPageSequences( List<ElementSequence> elementSequences, int startSymbol)
        {
            List<ElementSequence> pageSequences = new List<ElementSequence>();
            symbolLines = new List<SymbolLines>();
            double totalWidth = 0;
            double totalHeight = 0;
            for (int i = 0; i < elementSequences.Count; i++ )
            {
                ElementSequence sequence = elementSequences[i];
                int start = i == 0 ? startSymbol : 0;
                List<Line> lines = GetSequencesLine(sequence, start);
                Orientation orientation = sequence.Orientation;
                int linesAdded = GetLinesAddedCount(lines, orientation, ref totalWidth, ref totalHeight);
                if (linesAdded == lines.Count)
                {
                    pageSequences.Add(sequence);
                    symbolLines.Add(new SymbolLines(lines, orientation, minLineGap));
                }
                else
                {
                    if (pageSequences.Count == 0)
                    {
                        pageSequences.Add(sequence);
                        IsSequenceCarry = true;
                        List<Line> addedLines = lines.GetRange(0, linesAdded);
                        symbolLines.Add(new SymbolLines(addedLines, orientation, minLineGap));
                        int symbolCount = addedLines.Sum(l => l.SymbolsCount);
                        LastSymbol = startSymbol + symbolCount - 1;
                    }
                    break;
                }
            }
            return pageSequences;
        }

        private int GetLinesAddedCount(List<Line> lines, Orientation orientation, ref double totalWidth, ref double totalHeight)
        {
            int linesAdded = 0;
            if (orientation == Orientation.Horizontal)
            {
                totalHeight += minSequenceGap;
                foreach (Line line in lines)
                {
                    totalHeight += line.Height;
                    if (totalHeight > availableHeight)
                        break;
                    else
                    {
                        totalHeight += minLineGap;
                        linesAdded++;
                    }
                }
            }
            else
            {
                totalWidth += minSequenceGap;
                foreach (Line line in lines)
                {
                    totalWidth += line.Width;
                    if (totalWidth > availableWidth)
                        break;
                    else
                    {
                        totalWidth += minLineGap;
                        linesAdded++;
                    }
                }
            }
            return linesAdded;
        }

        public void Place(int sheetNumber, StampAttributes stampAttributes)
        {
            Sheet sheet = projectObjects.Sheet;
            int sheetId = sheet.Create(sheetNumber.ToString(), format.Name);
            E3Text text = projectObjects.Text;
            stampAttributes.SetAttributes(sheet, text, sheetNumber);
            double x = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + format.AvailableWidth / 2);
            double y = sheet.MoveDown(sheet.DrawingArea.Top, format.TopBorder + Settings.SheetTitleFont.height + minSymbolGap);
            text.CreateText(sheetId, sideName, x, y, Settings.SheetTitleFont);
            IEnumerable<SymbolLines> rows = symbolLines.Where(l => l.Orientation == Orientation.Horizontal);
            IEnumerable<SymbolLines> columns = symbolLines.Where(l => l.Orientation == Orientation.Vertical);
            int columnsCount = columns.Count();
            double rowsHeight = (rows.Count()== 0) ? 0 :rows.Sum(r => r.Size.Height);
            double columnsHeight = (columnsCount == 0) ? 0 : columns.Max(c => c.Size.Height);
            int verticalGapCount = rows.Count() + (columnsCount == 0 ? 0 : 1);
            double verticalGap = (availableHeight - (rowsHeight + columnsHeight)) / verticalGapCount;
            y = sheet.MoveDown(y, Settings.SheetTitleUnderlineOffset+verticalGap / 2);
            x = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder);
            foreach (SymbolLines lines in rows)
            {
                lines.Place(projectObjects, sheet, sheetId, x,y);
                y = sheet.MoveDown(y, lines.Size.Height+ verticalGap);
            }
            if (columnsCount > 0)
            {
                double columnsWidth = columns.Sum(c => c.Size.Width);
                double horizontalGap = (availableWidth - columnsWidth) / columnsCount;
                x = sheet.MoveRight(x, horizontalGap / 2);
                foreach (SymbolLines lines in columns)
                {
                    lines.Place(projectObjects, sheet, sheetId, x, y);
                    x = sheet.MoveRight(x, lines.Size.Width + horizontalGap);
                }
            }
        }

        private List<Line> GetSequencesLine(ElementSequence sequence, int startSymbol)
        {
            List<Line> lines = new List<Line>();
            Line line = new Line(projectObjects, sequence.Orientation, sequence.Orientation == Orientation.Horizontal ? availableWidth : availableHeight, minSymbolGap, sequence.Number);
            lines.Add(line);
            for (int i = startSymbol; i < sequence.Symbols.Count; i++)
            {
                SequenceSymbol symbol = sequence.Symbols[i];
                if (!line.TryAdd(symbol))
                {
                    line = new Line(projectObjects, sequence.Orientation, sequence.Orientation == Orientation.Horizontal ? availableWidth : availableHeight, minSymbolGap, sequence.Number);
                    line.TryAdd(symbol);
                    lines.Add(line);
                }
            }
            return lines;
        }

        private class SymbolLines
        {
            private List<Line> lines;
            private Orientation orientation;
            private double lineGap;

            public Orientation Orientation
            {
                get
                {
                    return orientation;
                }
            }

            public Size Size { get; private set; }

            public SymbolLines(List<Line> lines, Orientation orientation, double lineGap)
            {
                this.lines = lines;
                this.orientation = orientation;
                this.lineGap = lineGap;
                Size = GetSize();
            }

            private Size GetSize()
            {
                double height, width;
                if (orientation == Orientation.Horizontal)
                {
                    height = lines.Sum(l => l.Height) + (lines.Count - 1) * lineGap;
                    width = lines.Max(l => l.Width);
                }
                else
                {
                    width = lines.Sum(l => l.Width) + (lines.Count - 1) * lineGap;
                    height = lines.Max(l => l.Height);
                }
                return new Size(width, height);
            }

            public void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, double x, double y)
            {
                if (orientation == Orientation.Horizontal)
                {
                    foreach (Line line in lines)
                    {
                        line.PlaceHorizontally(projectObjects, sheet, sheetId, x, y);
                        y = sheet.MoveDown(y, line.Height + lineGap);
                    }
                }
                else
                {
                    foreach (Line line in lines)
                    {
                        line.PlaceVertically(projectObjects, sheet, sheetId, x, y);
                        x = sheet.MoveRight(x, line.Width + lineGap);
                    }
                }
            }

        }

        private class Line
        {
            private bool isEmpty;
            private double totalHeight;
            private double totalWidth;
            private double symbolGap;
            private double availableSpace;
            private double descriptionWidth;
            private string description;
            private Orientation orientation;
            private List<SequenceSymbol> symbols;
            private E3Font font;

            public double Height
            {
                get
                {
                    return totalHeight;
                }
            }

            public double Width
            {
                get
                {
                    return totalWidth;
                }
            }

            public int SymbolsCount
            {
                get
                {
                    return symbols.Count;
                }
            }

            public Line(ProjectObjects projectObjects, Orientation orientation, double availableSpace, double symbolGap, int number)
            {
                this.orientation = orientation;
                this.symbolGap = symbolGap;
                this.availableSpace = availableSpace;
                description = number.ToString() + (orientation == Orientation.Horizontal ? " ряд." : " кол.");
                font = new E3Font(height: 5);
                descriptionWidth = projectObjects.Text.GetTextLength(description, font);
                symbols = new List<SequenceSymbol>();
                isEmpty = true;
            }

            public bool TryAdd(SequenceSymbol symbol)
            {
                return orientation == Orientation.Horizontal ? TryAddHorizontal(symbol) : TryAddVertical(symbol);
            }

            private bool TryAddHorizontal(SequenceSymbol symbol)
            {
                if (isEmpty)
                {
                    isEmpty = false;
                    symbols.Add(symbol);
                    totalHeight = Math.Max(symbol.Height, font.height);
                    totalWidth = descriptionWidth + symbol.Width + 2 * symbolGap;
                    return true;
                }
                else
                {
                    double width = symbol.Width + symbolGap;
                    if ((totalWidth + width) > availableSpace)
                        return false;
                    symbols.Add(symbol);
                    totalWidth += width;
                    totalHeight = Math.Max(totalHeight, symbol.Height);
                    return true;
                }
            }

            private bool TryAddVertical(SequenceSymbol symbol)
            {
                if (isEmpty)
                {
                    isEmpty = false;
                    symbols.Add(symbol);
                    totalHeight = font.height + symbol.Height + 2 * symbolGap;
                    totalWidth = Math.Max(symbol.Width, descriptionWidth);
                    return true;
                }
                else
                {
                    double height = symbol.Height + symbolGap;
                    if ((totalHeight + height) > availableSpace)
                        return false;
                    symbols.Add(symbol);
                    totalHeight += height;
                    totalWidth = Math.Max(totalWidth, symbol.Width);
                    return true;
                }
            }

            public void PlaceHorizontally(ProjectObjects projectObjects, Sheet sheet, int sheetId, double x, double y)
            {
                double topMargin = symbols.Max(s => s.Margins.Top);
                topMargin = Math.Max(topMargin, font.height / 2);
                y = sheet.MoveDown(y, topMargin);
                E3Text text = projectObjects.Text;
                double yText = sheet.MoveDown(y, font.height / 2);
                x = sheet.MoveRight(x, descriptionWidth/2 + symbolGap);
                text.CreateText(sheetId, description, x, yText, font);
                x = sheet.MoveRight(x, descriptionWidth / 2);
                foreach (SequenceSymbol symbol in symbols)
                {
                    x = sheet.MoveRight(x, symbol.Margins.Left + symbolGap);
                    symbol.Place(projectObjects, sheet, sheetId, new Point(x, y));
                    x = sheet.MoveRight(x, symbol.Margins.Right);
                }
            }

            public void PlaceVertically(ProjectObjects projectObjects, Sheet sheet, int sheetId, double x, double y)
            {
                double leftMargin = symbols.Max(s => s.Margins.Left);
                leftMargin = Math.Max(leftMargin, descriptionWidth / 2);
                x = sheet.MoveRight(x, leftMargin);
                y = sheet.MoveDown(y, font.height + symbolGap);
                E3Text text = projectObjects.Text;
                text.CreateText(sheetId, description, x, y, font);
                foreach (SequenceSymbol symbol in symbols)
                {
                    y = sheet.MoveDown(y, symbol.Margins.Top + symbolGap);
                    symbol.Place(projectObjects, sheet, sheetId, new Point(x, y));
                    y = sheet.MoveDown(y, symbol.Margins.Bottom);
                }
            }

        }
    }
}