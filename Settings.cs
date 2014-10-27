using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class Settings
    {
        public E3Font SmallFont { get; private set; }
        public E3Font Font { get; private set; }
        public E3Font SheetTitleFont { get; private set; }
        public double GridStep { get; private set; }
        public double HalfGridStep { get; private set; }
        public double MinSignalLineLength { get; private set; }
        public double MinPinHeight { get; private set; }
        public double SignalHorizontalOffset { get; private set; }
        public double SignalVerticalOffset { get; private set; }
        public double AdressesVerticalOffset { get; private set; }
        public double PinWidth { get; private set; }
        public SheetFormat A4First { get; private set; }
        public SheetFormat A4Subsequent { get; private set; }
        public SheetFormat A3First { get; private set; }
        public SheetFormat A3Subsequent { get; private set; }
        public double SheetTitleUnderlineOffset { get; private set; }
        public double TerminalWidth { get; private set; }
        public double TerminalHeight { get; private set; }
        public string FunctionAttribute { get; private set; }
        public string SheetMarkAttribute { get; private set; }
        public string SubProjectAttribute { get; private set; }
        public string TerminalComponent { get; private set; }
        public string SheetCountAttribute { get; private set; }
        public string DrawingNameAttribute { get; private set; }
        public int ObjectNameTextType { get; private set; }
        public int ProjectTextType { get; private set; }
        public Dictionary<string, SideType> SideTypeByFunction { get; private set; }
        public int ElectricSchemeTypeCode { get; private set; }

        public Settings()
        {
            SmallFont = new E3Font(height: 2.5);
            Font = new E3Font(height: 3.5);
            SheetTitleFont = new E3Font(height: 5, style: Styles.Italic | Styles.Underline);
            GridStep = 4;
            HalfGridStep = GridStep / 2;
            MinSignalLineLength = 8;
            SignalHorizontalOffset = 1;
            SignalVerticalOffset = 2;
            MinPinHeight = 4;
            AdressesVerticalOffset = 1;
            PinWidth = GridStep;
            TerminalWidth = 8;
            TerminalHeight = 25;
            A4First = new SheetFormat("Формат А4 лист 1", 185, 232, 20, 5, 8, 12);
            A4Subsequent = new SheetFormat("Формат А4 послед. листы", 185, 272, 20, 5, 8, 12);
            A3First = new SheetFormat("Формат А3 лист 1", 395, 232, 20, 5, 16, 12);
            A3Subsequent = new SheetFormat("Формат А3 послед. листы", 395, 272, 20, 5, 16, 12);
            SheetTitleUnderlineOffset = 1;
            FunctionAttribute = "Function";
            SheetMarkAttribute = "marka2";
            SubProjectAttribute = "SubProj";
            SheetCountAttribute = "Всего листов";
            DrawingNameAttribute = "Название четрежа"; 
            TerminalComponent = "TerminalComponent";
            ObjectNameTextType = 505;
            ProjectTextType = 108;
            SideTypeByFunction = new Dictionary<string, SideType>() { { "Корпуса", SideType.Panel }, { "Боковые стенки", SideType.Sidewall }};
            ElectricSchemeTypeCode = 0;
        }
    }
}
