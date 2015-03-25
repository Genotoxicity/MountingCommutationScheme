using System.Collections.Generic;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class CabinetPreview
    {
        private double margin = 4;
        private double availableHeight;
        private double scaleFactor;
        private Dictionary<int, List<SidePreview>> sidePreviewsBySheet;

        public CabinetPreview(List<SideOutlineLayout> sideLayouts)
        {
            availableHeight = Settings.A3First.AvailableHeight - margin * 7;
            scaleFactor = GetScaleFactor(sideLayouts, availableHeight);
            sidePreviewsBySheet = GetSidePreviewsBySheet(sideLayouts, Settings.A3First.AvailableWidth);
        }

        private static double GetScaleFactor(List<SideOutlineLayout> sideLayouts, double previewHeight)
        {
            double sidesHeight = sideLayouts.Max(s => s.Height);
            double scaleFactor = previewHeight / sidesHeight;
            return scaleFactor;
        }

        private Dictionary<int, List<SidePreview>> GetSidePreviewsBySheet(List<SideOutlineLayout> sideLayouts, double availableWidth)
        {
            List<SidePreview> previews = GetSidePreviews(sideLayouts);
            Dictionary<int, List<SidePreview>> sidePreviewsBySheet = new Dictionary<int, List<SidePreview>>();
            double gap = margin * 2;
            double width = availableWidth - previews[0].Width - gap;
            int sheet = 0;
            sidePreviewsBySheet.Add(sheet, new List<SidePreview>() { previews[0]});
            for (int i = 1; i < previews.Count; i++)
            {
                SidePreview preview = previews[i];
                width -= preview.Width;
                if (width <= 0)
                {
                    width = availableWidth - preview.Width - gap;
                    sidePreviewsBySheet.Add(++sheet, new List<SidePreview>() { preview });
                }
                else
                    sidePreviewsBySheet[sheet].Add(preview);
            }
            return sidePreviewsBySheet;
        }

        private List<SidePreview> GetSidePreviews(List<SideOutlineLayout> sideLayouts)
        {
            List<SidePreview> sidePreviews = new List<SidePreview>(sideLayouts.Count);
            sideLayouts.ForEach(s => sidePreviews.Add(new SidePreview(s, scaleFactor, margin)));
            return sidePreviews;
        }

        public void Place(ProjectObjects projectObjects, StampAttributes stampAttributes, int sheetCount)
        {
            Sheet sheet = projectObjects.Sheet;
            foreach (int sheetIndex in sidePreviewsBySheet.Keys)
            {
                int sheetNumber = sheetIndex+1; 
                List<SidePreview> previews = sidePreviewsBySheet[sheetIndex];
                int previewCount = previews.Count;
                double previewsWidth = previews.Sum(p => p.Width);
                double minimumWidth = previewsWidth + (previewCount + 1) * margin;
                SheetFormat format = GetFormat(sheetNumber, minimumWidth);
                int sheetId = sheet.Create((sheetIndex+1).ToString(), format.Name);
                stampAttributes.SetSheetCount(sheetCount);
                stampAttributes.SetAttributes(sheet, projectObjects.Text, sheetNumber);
                double gap = (format.AvailableWidth - previewsWidth) / previewCount;
                double yBottom = sheet.MoveDown(sheet.DrawingArea.Top, format.TopBorder + format.AvailableHeight - 4);
                double xLeft = sheet.MoveRight(sheet.DrawingArea.Left, format.LeftBorder + gap/2);
                foreach (SidePreview preview in previews)
                {
                    preview.Place(projectObjects, sheet, sheetId, xLeft, yBottom, scaleFactor);
                    xLeft += (preview.Width + gap);
                }
            }
        }

        private SheetFormat GetFormat(int sheetNumber, double previewsWidth)
        {
            SheetFormat format;
            if (previewsWidth < Settings.A4First.AvailableWidth)
                format = (sheetNumber == 1) ? Settings.A4First : Settings.A4Subsequent;
            else
                format = (sheetNumber == 1) ? Settings.A3First : Settings.A3Subsequent;
            return format;
        }

    }
}
