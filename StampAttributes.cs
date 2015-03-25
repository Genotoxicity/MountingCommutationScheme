using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class StampAttributes
    {
        private int sheetCount;
        private string sheetMark;
        private string subProjectMark;
        private StampTextInfo projectTextInfo;
        private StampTextInfo objectTextInfo;
        private StampTextInfo drawingTextInfo;

        public StampAttributes(ProjectObjects projectObjects, int sheetId)
        {
            Sheet sheet = projectObjects.Sheet;
            sheet.Id = sheetId;
            subProjectMark = sheet.GetAttributeValue(Settings.SubProjectAttribute);
            sheetMark = GetSheetMark(sheet);
            E3Text text = projectObjects.Text;
            projectTextInfo = new StampTextInfo(sheet, text, Settings.ProjectTextType);
            objectTextInfo = new StampTextInfo(sheet, text, Settings.ObjectNameTextType);
            drawingTextInfo = new StampTextInfo(sheet, text, Settings.DrawingNameTextType, Settings.DrawingName);
        }


        private string GetSheetMark(Sheet sheet)
        {
            string sheetMark = String.Empty;
            Regex regex = new Regex(@"(\d+)$");
            string mark = sheet.GetAttributeValue(Settings.SheetMarkAttribute);
            if (!String.IsNullOrEmpty(mark))
            {
                MatchCollection matches = regex.Matches(mark);
                if (matches.Count > 0)
                {
                    int index = Int16.Parse(matches[0].Value);
                    index++;
                    sheetMark = regex.Replace(mark, index.ToString());
                }
            }
            return sheetMark;
        }

        public void SetSheetCount(int sheetCount)
        {
            this.sheetCount = sheetCount;
        }

        public void SetAttributes(Sheet sheet, E3Text text, int sheetNumber)
        {
            if (!String.IsNullOrEmpty(sheetMark))
                sheet.SetAttribute(Settings.SheetMarkAttribute, sheetMark);
            if (!String.IsNullOrEmpty(subProjectMark))
                sheet.SetAttribute(Settings.SubProjectAttribute, subProjectMark);
            if (sheetNumber == 1)
                SetFirstPageAttributes(sheet, text);
        }

        private void SetFirstPageAttributes(Sheet sheet, E3Text text)
        {
            if (sheetCount > 0)
                sheet.SetAttribute(Settings.SheetCountAttribute, sheetCount.ToString());
            projectTextInfo.SetTextPropertiesOnSheet(sheet, text);
            objectTextInfo.SetTextPropertiesOnSheet(sheet, text);
            drawingTextInfo.SetTextAndFontOnSheet(sheet, text);
        }

    }
}
