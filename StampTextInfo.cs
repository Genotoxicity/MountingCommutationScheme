using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class StampTextInfo
    {
        private int textType;
        private E3Font font;
        private Point locationFromRightBottom;
        private Size size;
        private string strText;

        public StampTextInfo(Sheet sheet, E3Text text, int textType)
        {
            this.textType = textType;
            List<int> projectTextIds = sheet.GetTextIds(textType);
            if (projectTextIds.Count == 1)
            {
                text.Id = projectTextIds.First();
                font = text.GetFont();
                Point point = text.GetLocation();
                locationFromRightBottom = new Point(Math.Abs(sheet.DrawingArea.Right - point.X), Math.Abs(sheet.DrawingArea.Bottom - point.Y));
                size = text.GetBox();
                strText = text.GetText();
            }
        }

        public StampTextInfo(Sheet sheet, E3Text text, int textType, string name)
        {
            this.textType = textType;
            List<int> projectTextIds = sheet.GetTextIds(textType);
            if (projectTextIds.Count == 1)
            {
                text.Id = projectTextIds.First();
                font = text.GetFont();
                strText = name;
            }
        }

        public void SetTextPropertiesOnSheet(Sheet sheet, E3Text text)
        {
            List<int> projectTextIds = sheet.GetTextIds(textType);
            if (projectTextIds.Count == 1)
            {
                text.Id = projectTextIds.First();
                text.SetFont(font);
                Point location = new Point(sheet.MoveLeft(sheet.DrawingArea.Right, locationFromRightBottom.X), sheet.MoveUp(sheet.DrawingArea.Bottom, locationFromRightBottom.Y));
                text.SetLocation(location);
                text.SetBox(size);
                text.SetText(strText);
            }
        }

        public void SetTextAndFontOnSheet(Sheet sheet, E3Text text)
        {
            List<int> projectTextIds = sheet.GetTextIds(textType);
            if (projectTextIds.Count == 1)
            {
                text.Id = projectTextIds.First();
                text.SetFont(font);
                text.SetText(strText);
            }
        }
    }
}
