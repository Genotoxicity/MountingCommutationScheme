using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class SidePreview
    {
        private string header;
        private double width;
        private double height;
        private SheetFormat[][] formatLayout;

        public double Width
        {
            get
            {
                return width;
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
        }

        /*public int SheetCount
        {
            get
            {
                return formatLayout.Sum(r => r.Count());
            }
        }*/

        public SidePreview(Settings settings, CabinetSide side)
        {
            header = side.Name;
            formatLayout = side.GetFormatLayout();
            width = 0;
            height = 0;
            foreach (SheetFormat[] row in formatLayout)
            {
                double rowWidth = row.Sum(r => r.PreviewWidth);
                width = Math.Max(width, rowWidth);
                height += row.First().PreviewHeight;
            }
            double titleGap = settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset + settings.GridStep;
            height += titleGap;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, int sheetId, Point position, ref int sheetNumber)
        {
            Sheet sheet = projectObjects.Sheet;
            E3Text text = projectObjects.Text;
            Graphic graphic = projectObjects.Graphic;
            Group group = projectObjects.Group;
            sheet.Id = sheetId;
            double top = sheet.MoveUp(position.Y, height / 2);
            double left = sheet.MoveLeft(position.X, width / 2);
            double titleGap = settings.SheetTitleFont.height + settings.SheetTitleUnderlineOffset + settings.GridStep;
            E3Font headerFont = settings.SheetTitleFont;
            E3Font numberFont = new E3Font(height: 5, alignment: Alignment.Centered, style: Styles.Italic);
            double textOrdinate = sheet.MoveDown(top, headerFont.height / 2);
            double textWidth = text.GetTextLength(header, headerFont);
            double textAbsciss = position.X;
            int sheetCount = formatLayout.Sum(r => r.Count());
            List<int> ids = new List<int>(sheetCount * 2 + 1);
            ids.Add(text.CreateText(sheetId, header, textAbsciss, textOrdinate, headerFont));
            double y1 = sheet.MoveDown(top, titleGap);
            foreach (SheetFormat[] row in formatLayout)
            {
                double x1 = left;
                double y2 = sheet.MoveDown(y1, row.First().PreviewHeight);
                foreach (SheetFormat sheetFormat in row)
                {
                    double x2 = sheet.MoveRight(x1, sheetFormat.PreviewWidth);
                    ids.Add(graphic.CreateRectangle(sheetId, x1, y1, x2, y2));
                    double ordinate = sheet.MoveDown((y1 + y2) / 2, numberFont.height / 2);
                    ids.Add(text.CreateText(sheetId, (sheetNumber++).ToString(), (x1 + x2) / 2, ordinate, numberFont));
                    x1 = x2;
                }
                y1 = y2;
            }
            group.CreateGroup(ids);
        }

    }
}
