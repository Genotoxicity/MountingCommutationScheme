using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceRow
    {
        //private List<DeviceSymbol> deviceSymbols;
        private List<RowSymbol> rowSymbols;
        private double topMargin, bottomMargin;

        public int Number { get; private set; }

        public double Height { get; private set; }

        public double TopMargin
        {
            get
            {
                return topMargin;
            }
        }

        public double BottomMargin
        {
            get
            {
                return bottomMargin;
            }
        }

        public List<RowSymbol> RowSymbols
        {
            get
            {
                return rowSymbols;
            }
        }

        public DeviceRow(int rowNumber, List<RowSymbol> rowSymbols)
        {
            Number = rowNumber;
            this.rowSymbols = rowSymbols;  
        }

        public void CalculateLayout( Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName)
        {
            /*double symbolsHeight = deviceSymbols.Max(ds => (componentLayoutByName[ds.Component].Height));
            double maxTopAddressesHeight = deviceSymbols.Max(ds => (ds.TopPinSymbols.Count > 0) ? ds.TopPinSymbols.Max(ts => ts.AddressesSize.Height) : 0);
            maxTopAddressesHeight += (maxTopAddressesHeight > 0) ? settings.AdressesVerticalOffset : 0;
            double maxBottomAddressesHeight = deviceSymbols.Max(ds => (ds.BottomPinSymbols.Count > 0) ? ds.BottomPinSymbols.Max(bs => bs.AddressesSize.Height) : 0);
            maxBottomAddressesHeight += (maxBottomAddressesHeight > 0) ? settings.AdressesVerticalOffset : 0;
            Height = symbolsHeight + maxTopAddressesHeight + maxBottomAddressesHeight;*/
            topMargin = 0;
            bottomMargin = 0;
            rowSymbols.ForEach(rs => rs.CalculateWidth(settings, componentLayoutByName));
            IEnumerable<DeviceSymbol> deviceSymbols = rowSymbols.SelectMany(r=>r.DeviceSymbols);
            foreach (DeviceSymbol deviceSymbol in deviceSymbols)
            {
                double componentTopMargin = componentLayoutByName[deviceSymbol.Component].TopMargin;
                double topAddressesHeight = (deviceSymbol.TopPinSymbols.Count> 0 ) ? deviceSymbol.TopPinSymbols.Max(ts => ts.AddressesSize.Height) + settings.AdressesVerticalOffset : 0;
                topMargin = Math.Max(topMargin, componentTopMargin + topAddressesHeight);
                double componentBottomMargin = componentLayoutByName[deviceSymbol.Component].BottomMargin;
                double bottomAddressesHeight = (deviceSymbol.BottomPinSymbols.Count > 0 ) ? deviceSymbol.BottomPinSymbols.Max(bs => bs.AddressesSize.Height) + settings.AdressesVerticalOffset : 0;
                bottomMargin = Math.Max(bottomMargin, componentBottomMargin + bottomAddressesHeight);
            }
            Height = bottomMargin + topMargin;
        }

    }
}
