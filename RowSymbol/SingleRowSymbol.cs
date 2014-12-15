using System.Collections.Generic;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class SingleRowSymbol : RowSymbol
    {
        private Element element;

        public override IEnumerable<Element> Elements
        {
            get
            {
                return new List<Element>(1){element};
            }
        }

        public SingleRowSymbol(Element element)
        {
            this.element = element;
        }

        public override Margins CalculateAndGetMargins()
        {
            ElementSizes sizes = element.GetSizesAndSetSignalLineLength();
            Width = sizes.Margins.Left + sizes.Margins.Right;
            return sizes.Margins;
        }

        public override void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position)
        {
            element.Place(projectObjects, sheet, sheetId, position);
        }
    }
}
