using System.Collections.Generic;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class SingleSymbol : SequenceSymbol
    {
        private Element element;

        public override IEnumerable<Element> Elements
        {
            get
            {
                return new List<Element>(1){element};
            }
        }

        public SingleSymbol(Element element, E3Text text)
        {
            this.element = element;
            element.Calculate(text);
            Width = element.Margins.Left + element.Margins.Right;
            Height = element.Margins.Top + element.Margins.Bottom;
            Margins = element.Margins;
        }


        public override void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position)
        {
            element.Place(projectObjects, sheet, sheetId, position);
        }
    }
}
