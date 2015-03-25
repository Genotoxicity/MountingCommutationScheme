using System.Collections.Generic;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public static class Settings
    {
        public static double JumperHeight { get; private set; }
        public static E3Font SmallFont { get; private set; }
        public static E3Font Font { get; private set; }
        public static E3Font SheetTitleFont { get; private set; }
        public static double GridStep { get; private set; }
        public static double HalfGridStep { get; private set; }
        public static double MinSignalLineLength { get; private set; }
        public static double MinPinHeight { get; private set; }
        public static double SignalOffsetFromLine { get; private set; }
        public static double SignalOffsetFromOutline { get; private set; }
        public static double SignalOffsetAfterText { get; private set; }
        public static double AdressOffset { get; private set; }
        public static double PinMinSize { get; private set; }
        public static SheetFormat A4First { get; private set; }
        public static SheetFormat A4Subsequent { get; private set; }
        public static SheetFormat A3First { get; private set; }
        public static SheetFormat A3Subsequent { get; private set; }
        public static double SheetTitleUnderlineOffset { get; private set; }
        public static double TerminalMinSize { get; private set; }
        public static double TerminalMaxSize { get; private set; }
        public static string FunctionAttribute { get; private set; }
        public static string SheetMarkAttribute { get; private set; }
        public static string SubProjectAttribute { get; private set; }
        public static string SheetCountAttribute { get; private set; }
        public static string TerminalComponent { get; private set; }
        public static string DrawingName { get; private set; }
        public static int ObjectNameTextType { get; private set; }
        public static int ProjectTextType { get; private set; }
        public static int DrawingNameTextType { get; private set; }
        public static Dictionary<string, SideType> SideTypeByFunction { get; private set; }
        public static int ElectricSchemeTypeCode { get; private set; }

        static Settings()
        {
            JumperHeight = 2.2;
            SmallFont = new E3Font(height: 2.5);
            Font = new E3Font(height: 3.5);
            SheetTitleFont = new E3Font(height: 5, style: Styles.Italic | Styles.Underline);
            GridStep = 4;
            HalfGridStep = GridStep / 2;
            MinSignalLineLength = 8;
            SignalOffsetFromLine = 1;
            SignalOffsetFromOutline = 3;
            SignalOffsetAfterText = 1;
            MinPinHeight = 4;
            AdressOffset = 1;
            PinMinSize = GridStep;
            TerminalMinSize = 8;
            TerminalMaxSize = 25;
            A4First = new SheetFormat("Формат А4 лист 1", 185, 232, 20, 5, 8, 12);
            A4Subsequent = new SheetFormat("Формат А4 послед. листы", 185, 272, 20, 5, 8, 12);
            A3First = new SheetFormat("Формат А3 лист 1", 395, 232, 20, 5, 16, 12);
            A3Subsequent = new SheetFormat("Формат А3 послед. листы", 395, 272, 20, 5, 16, 12);
            SheetTitleUnderlineOffset = 1;
            FunctionAttribute = "Function";
            SheetMarkAttribute = "marka2";
            SubProjectAttribute = "SubProj";
            SheetCountAttribute = "Всего листов";
            TerminalComponent = "TerminalComponent";
            DrawingName = "Монтажно - коммутационная схема";
            ObjectNameTextType = 505;
            ProjectTextType = 108;
            DrawingNameTextType = 497;
            SideTypeByFunction = new Dictionary<string, SideType>() { { "Корпуса", SideType.Panel }, { "Боковые стенки", SideType.Sidewall }, {"Двери", SideType.Door}};
            ElectricSchemeTypeCode = 0;
        }

        public static SideType GetSideTypeByFunction(string function)
        {
            if (SideTypeByFunction.ContainsKey(function))
                return SideTypeByFunction[function];
            return SideType.None;
        }
    }
}
