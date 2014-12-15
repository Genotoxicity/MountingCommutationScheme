using System.Collections.Generic;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public abstract class RowSymbol
    {
        public double Width { get; protected set; }

        public abstract IEnumerable<Element> Elements{ get; }

        public abstract Margins CalculateAndGetMargins();

        public abstract void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position);
    }
}
