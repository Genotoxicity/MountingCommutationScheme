using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class PreviewAllocator
    {
        private List<PreviewSheet> previewSheets;

        public int PreviewSheetCount
        {
            get
            {
                return previewSheets.Count;
            }
        }
        
        public PreviewAllocator(List<SidePreview> previews, Settings settings)
        {
            previewSheets = GetPreviewSheets(previews, settings);
        }

        private List<PreviewSheet> GetPreviewSheets(List<SidePreview> previews, Settings settings)
        {
            previewSheets = new List<PreviewSheet>();
            previewSheets.Add(new PreviewSheet(settings.A3First, settings));
            foreach (SidePreview preview in previews)
                if (previewSheets.Last().TryAddPreview(preview))
                    continue;
                else
                {
                    previewSheets.Add(new PreviewSheet(settings.A3Subsequent, settings));
                    previewSheets.Last().TryAddPreview(preview);
                }
            for (int i = 0; i < previewSheets.Count; i++)
                if (previewSheets[i].MinWidth < settings.A4First.AvailableWidth)
                    previewSheets[i].SetFormat(i == 0 ? settings.A4First : settings.A4Subsequent);
            return previewSheets;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, StampAttributes sheetAttributes)
        {
            int sheetCount = PreviewSheetCount;
            int previewSheetNumber = sheetCount;
            previewSheetNumber++;
            for (int i = 0; i < sheetCount; i++)
                previewSheets[i].Place(projectObjects, settings, i+1, ref previewSheetNumber, sheetAttributes);
        }

        private class PreviewSheet
        {
            private SheetFormat format;
            private double horizontalFreeSpace;
            private double verticalFreeSpace;
            private double maxColumnWidth;
            private Settings settings;
            private List<PreviewColumn> columns;

            public double MinWidth
            {
                get
                {
                    return columns.Sum(c => c.Width) + columns.Count * settings.GridStep;
                }
            }

            public PreviewSheet(SheetFormat format, Settings settings)
            {
                this.format = format;
                columns = new List<PreviewColumn>();
                horizontalFreeSpace = format.AvailableWidth;
                double titleGap = settings.SheetTitleFont.height + settings.HalfGridStep + settings.SheetTitleUnderlineOffset;
                verticalFreeSpace = format.AvailableHeight - titleGap;
                this.settings = settings;
            }

            public bool TryAddPreview(SidePreview preview)
            {
                int columnCount = columns.Count;
                maxColumnWidth = Math.Max(preview.Width, maxColumnWidth);
                if (columnCount == 0)
                    return AddPreviewToNewColumn(preview);
                if (horizontalFreeSpace - (maxColumnWidth + settings.GridStep) > 0)
                    if (columns.First().TryAddPreview(preview))
                        return true;
                    else
                    {
                        horizontalFreeSpace -= MinWidth;
                        maxColumnWidth = preview.Width;
                        if (horizontalFreeSpace - (maxColumnWidth + settings.GridStep) > 0)
                            return AddPreviewToNewColumn(preview);
                        else
                            return false;
                    }
                else
                {
                    horizontalFreeSpace -= (maxColumnWidth+settings.GridStep);
                    if (columnCount == 1)
                        return columns.First().TryAddPreview(preview);
                }
                return false;
            }

            private bool AddPreviewToNewColumn(SidePreview preview)
            {
                PreviewColumn column = new PreviewColumn(verticalFreeSpace, settings);
                columns.Add(column);
                return column.TryAddPreview(preview);
            }

            public void SetFormat(SheetFormat format)
            {
                this.format = format;
            }

            public void Place(ProjectObjects projectObjects, Settings settings, int sheetNumber, ref int previewSheetNumber, StampAttributes sheetAttributes)
            {
                E3Text text = projectObjects.Text;
                Sheet sheet = projectObjects.Sheet;
                int sheetId = sheet.Create(sheetNumber.ToString(), format.Name);
                sheetAttributes.SetAttributes(sheet, text, sheetNumber);
                double titleOrdinate = sheet.MoveDown(sheet.DrawingArea.Top, format.TopBorder + settings.SheetTitleFont.height + settings.HalfGridStep);
                double titleAbsciss = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + format.AvailableWidth / 2);
                text.CreateText(sheetId, "Навигация по листам", titleAbsciss, titleOrdinate, settings.SheetTitleFont);
                double titleGap = settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset + settings.HalfGridStep;
                double columnTop = sheet.MoveDown(sheet.DrawingArea.Top, titleGap + format.TopBorder);
                double totalHorizontalGap = format.AvailableWidth - columns.Sum(s => s.Width);
                double horizontalGap = totalHorizontalGap / columns.Count;
                double absciss = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + horizontalGap / 2);
                foreach (PreviewColumn column in columns)
                {
                    double halfWidth = column.Width / 2;
                    absciss = sheet.MoveRight(absciss, halfWidth);
                    column.Place(projectObjects, settings, sheetId, new Point(absciss, columnTop), ref previewSheetNumber);
                    absciss = sheet.MoveRight(absciss, halfWidth + horizontalGap);
                }
            }

            private class PreviewColumn
            {
                private double availableVerticalSpace;
                private double verticalFreeSpace;
                private double width;
                private double verticalGap;
                private List<SidePreview> previews;

                public double Width
                {
                    get
                    {
                        return width;
                    }
                }

                public PreviewColumn(double verticalFreeSpace, Settings settings)
                {
                    this.verticalFreeSpace = verticalFreeSpace;
                    availableVerticalSpace = verticalFreeSpace;
                    verticalGap = settings.GridStep;
                    previews = new List<SidePreview>();
                }

                public bool TryAddPreview(SidePreview preview)
                {
                    availableVerticalSpace -= (preview.Height + verticalGap);
                    if (availableVerticalSpace >= 0 || previews.Count == 0)
                    {
                        previews.Add(preview);
                        width = Math.Max(width, preview.Width);
                        return true;
                    }
                    return false;
                }

                public void Place(ProjectObjects projectObjects, Settings settings, int sheetId, Point topCenter, ref int previewSheetNumber)
                {
                    Sheet sheet = projectObjects.Sheet;
                    sheet.Id = sheetId;
                    double totalVerticalGap = verticalFreeSpace - previews.Sum(p => p.Height);
                    double verticalGap = totalVerticalGap / previews.Count;
                    double absciss = topCenter.X;
                    double ordinate = sheet.MoveDown(topCenter.Y, verticalGap/2);
                    foreach (SidePreview preview in previews)
                    {
                        double halfHeight = preview.Height / 2;
                        ordinate = sheet.MoveDown(ordinate, halfHeight);
                        preview.Place(projectObjects, settings, sheetId, new Point(absciss, ordinate), ref previewSheetNumber);
                        ordinate = sheet.MoveDown(ordinate, verticalGap + halfHeight);
                    }
                }

            }
        }

    }
}
