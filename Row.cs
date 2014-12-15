using System.Collections.Generic;
using System.Linq;

namespace MountingCommutationScheme
{
    public class Row
    {
        private List<RowSymbol> symbols;

        public int Number { get; private set; }

        public List<RowSymbol> Symbols
        {
            get
            {
                return symbols;
            }
        }

        public double TopMargin { get; private set; }

        public double BottomMargin { get; private set; }

        public double Height { get; private set; }
            
        public Row(int number, List<RowSymbol> rowSymbols)
        {
            symbols = rowSymbols;
            Number = number;
        }

        public void Calculate()
        {
            double topMargin = 0;
            double bottomMargin = 0;
            IEnumerable<Margins> margins = symbols.Select(s => s.CalculateAndGetMargins());
            topMargin = margins.Max(m => m.Top);
            bottomMargin = margins.Max(m => m.Bottom);
            TopMargin = topMargin;
            BottomMargin = bottomMargin;
            Height = bottomMargin + topMargin;
        }
    }
}
