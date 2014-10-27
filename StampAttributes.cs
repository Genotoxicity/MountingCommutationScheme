using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class StampAttributes
    {
        private Settings settings;
        private int sheetCount;
        private string drawingName;
        private string sheetMark;
        private string subProjectMark;
        private StampTextInfo projectTextInfo;
        private StampTextInfo objectTextInfo;

        public StampAttributes(ProjectObjects projectObjects, Settings settings)
        {
            this.settings = settings;
            Sheet sheet = projectObjects.Sheet;
            sheet.Id = sheet.ParentSheetId;
            subProjectMark = sheet.GetAttributeValue(settings.SubProjectAttribute);
            drawingName = "Монтажно - коммутационная схема";
            Regex regex = new Regex(@"(\d+)$");
            sheetMark = String.Empty;
            string mark = sheet.GetAttributeValue(settings.SheetMarkAttribute);
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
            E3Text text = projectObjects.Text;
            projectTextInfo = new StampTextInfo(sheet, text, settings.ProjectTextType);
            objectTextInfo = new StampTextInfo(sheet, text, settings.ObjectNameTextType);
        }

        public void SetSheetCount(int sheetCount)
        {
            this.sheetCount = sheetCount;
        }

        public void SetAttributes(Sheet sheet, E3Text text, int sheetNumber)
        {
            if (!String.IsNullOrEmpty(sheetMark))
                sheet.SetAttribute(settings.SheetMarkAttribute, sheetMark);
            if (!String.IsNullOrEmpty(subProjectMark))
                sheet.SetAttribute(settings.SubProjectAttribute, subProjectMark);
            if (sheetNumber == 1)
                SetFirstPageAttributes(sheet, text);
        }

        private void SetFirstPageAttributes(Sheet sheet, E3Text text)
        {
            if (sheetCount > 0)
                sheet.SetAttribute(settings.SheetCountAttribute, sheetCount.ToString());
            if (!String.IsNullOrEmpty(drawingName))
                sheet.SetAttribute(settings.DrawingNameAttribute, drawingName);
            projectTextInfo.SetTextPropertiesOnSheet(sheet, text);
            objectTextInfo.SetTextPropertiesOnSheet(sheet, text);
        }

    }
}
