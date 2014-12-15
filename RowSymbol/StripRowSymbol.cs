using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class TerminalStripRowSymbol  : RowSymbol
    {
        private List<TerminalElement> elements;
        private Orientation orientation;
        private double height;

        public override IEnumerable<Element> Elements
        {
            get
            {
                return elements;
            }
        }

        public TerminalStripRowSymbol(List<TerminalElement> elements)
        {
            this.elements = elements;
            orientation = elements.First().Orientation;
        }

        public override Margins CalculateAndGetMargins()
        {
            IEnumerable<ElementSizes> elementsSizes = elements.Select(e => e.GetSizesAndSetSignalLineLength());
            if (orientation == Orientation.Vertical)
            {
                Width = elementsSizes.Sum(e => e.Margins.Left + e.Margins.Right);
                double topOffset = elementsSizes.Max(e => e.Margins.Top);
                double bottomOffset = elementsSizes.Max(e => e.Margins.Bottom);
                height = topOffset + bottomOffset;
                return new Margins(Width / 2, Width / 2, topOffset, bottomOffset);
            }
            else
            {
                Width = elementsSizes.Max(e => e.Margins.Left) + elementsSizes.Max(e => e.Margins.Right);
                height = elementsSizes.Sum(e => e.Margins.Top + e.Margins.Bottom);
                return new Margins(Width / 2, Width / 2, height/2, height/2);
            }

        }

        public override void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position)
        {
            if (orientation == Orientation.Vertical)
            {
                double elementWidth = elements.First().OutlineWidth;
                double x = sheet.MoveLeft(position.X, (Width - elementWidth) / 2);
                foreach (Element element in elements)
                {
                    element.Place(projectObjects, sheet, sheetId, new Point(x, position.Y));
                    x = sheet.MoveRight(x, elementWidth);
                }
            }
            else
            {
                double elementHeight = elements.First().OutlineHeight;
                double y = sheet.MoveUp(position.Y, (height - elementHeight) / 2);
                foreach (Element element in elements)
                {
                    element.Place(projectObjects, sheet, sheetId, new Point(position.X, y));
                    y = sheet.MoveDown(y, elementHeight);
                }
            }
        }
    }
}
