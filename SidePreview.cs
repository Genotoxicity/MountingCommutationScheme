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
        private double width;
        private double height;
        private SideOutlineLayout sideLayout;

        public double Width
        {
            get
            {
                return width;
            }
        }
        
        public SidePreview(SideOutlineLayout sideLayout, double scaleFactor, double margin)
        {
            width = sideLayout.Width * scaleFactor;
            height = sideLayout.Height * scaleFactor;
            this.sideLayout = sideLayout;
        }

        public void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, double startX, double startY, double scaleFactor)
        {
            Graphic graph = projectObjects.Graphic;
            E3Text text = projectObjects.Text;
            PreviewPointConverter converter = new PreviewPointConverter(sideLayout.Left, sideLayout.Bottom, startX, startY, scaleFactor);
            double outlineLeft = converter.GetX(sideLayout.Left);
            double outlineRight = converter.GetX(sideLayout.Right);
            double outlineTop = converter.GetY(sideLayout.Top);
            double outlineBottom = converter.GetY(sideLayout.Bottom);
            graph.CreateRectangle(sheetId, outlineLeft, outlineBottom, outlineRight, outlineTop);
            double textY = sheet.MoveUp(outlineTop, Settings.GridStep);
            E3Font font = new E3Font(Settings.Font);
            font.alignment = Alignment.Centered;
            text.CreateText(sheetId, sideLayout.Name, (outlineLeft + outlineRight) / 2, textY, Settings.Font);
            foreach (OutlineSequence row in sideLayout.Rows)
            {
                double left = converter.GetX(row.Left);
                double right = converter.GetX(row.Right);
                double top = converter.GetY(row.Top);
                double bottom = converter.GetY(row.Bottom);
                graph.CreateRectangle(sheetId, left, bottom, right, top);
            }
            foreach (OutlineSequence column in sideLayout.Columns)
            {
                double left = converter.GetX(column.Left);
                double right = converter.GetX(column.Right);
                double top = converter.GetY(column.Top);
                double bottom = converter.GetY(column.Bottom);
                graph.CreateRectangle(sheetId, left, bottom, right, top);
            }
        }

        private class PreviewPointConverter
        {
            private double scaleFactor;
            private double outlineLeft;
            private double outlineBottom;
            private double startLeft;
            private double startBottom;

            public PreviewPointConverter(double sideLayoutLeft, double sideLayoutBottom, double startX, double startY, double scaleFactor)
            {
                this.scaleFactor = scaleFactor;
                outlineLeft = sideLayoutLeft;
                outlineBottom = sideLayoutBottom;
                startLeft = startX;
                startBottom = startY;
            }

            public Point GetSheetPoint(double outlineX, double outlineY)
            {
                double x = outlineX - outlineLeft;
                double y = outlineY - outlineBottom;
                x *= scaleFactor;
                y *= scaleFactor;
                x += startLeft;
                y += startBottom;
                return new Point(x, y);
            }

            public double GetX(double outlineX)
            {
                double x = outlineX - outlineLeft;
                x *= scaleFactor;
                x += startLeft;
                return x;
            }

            public double GetY(double outlineY)
            {
                double y = outlineY - outlineBottom;
                y *= scaleFactor;
                y += startBottom;
                return y;
            }
        }
    }
}
