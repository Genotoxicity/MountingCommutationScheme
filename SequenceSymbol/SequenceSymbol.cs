using System.Collections.Generic;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public abstract class SequenceSymbol
    {
        public double Width { get; protected set; }

        public double Height { get; protected set; }

        public Margins Margins { get; protected set; }

        public abstract IEnumerable<Element> Elements{ get; }

        public abstract void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position);
    }
}
