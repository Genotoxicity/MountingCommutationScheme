using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class RowSymbol
    {
        private List<DeviceSymbol> deviceSymbols;
        private bool isTerminalStrip;

        public List<DeviceSymbol> DeviceSymbols
        {
            get
            {
                return deviceSymbols;
            }
        }

        public double Width { get; private set; }

        public RowSymbol(DeviceSymbol deviceSymbol)
        {
            deviceSymbols = new List<DeviceSymbol>() { deviceSymbol };
        }

        public RowSymbol(List<DeviceSymbol> deviceSymbols)
        {
            this.deviceSymbols = deviceSymbols;
            isTerminalStrip = true;
        }

        public void CalculateWidth(Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName)
        {
            if (isTerminalStrip)
                Width = deviceSymbols.Count * settings.TerminalWidth;
            else
                Width = componentLayoutByName[deviceSymbols.First().Component].OutlineWidth;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, Sheet sheet, Point position, Dictionary<string, ComponentLayout> componentLayoutByName)
        {
            if (isTerminalStrip)
            {
                double absciss = sheet.MoveLeft(position.X, Width / 2 - settings.TerminalWidth / 2);
                foreach (DeviceSymbol deviceSymbol in deviceSymbols)
                {
                    deviceSymbol.Place(projectObjects, settings, sheet, new Point(absciss, position.Y), componentLayoutByName);
                    absciss = sheet.MoveRight(absciss, settings.TerminalWidth);
                }
            }
            else
                deviceSymbols.First().Place(projectObjects, settings, sheet, position, componentLayoutByName);
        }

    }
}
