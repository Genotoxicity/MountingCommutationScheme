using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class ComponentLayout
    {
        private List<DeviceSymbol> deviceSymbols;
        private List<double> topPinsHorizontalOffsets, bottomPinsHorizontalOffsets;
        private double outlineHeight, outlineWidth;
        private double pinHeight;
        private double signalLineLength;
        //private bool isTerminal;
        
        public double OutlineHeight
        {
            get
            {
                return outlineHeight;
            }
        }

        public double OutlineWidth
        {
            get
            {
                return outlineWidth;
            }
        }

        public double TopMargin { get; private set; }

        public double BottomMargin { get; private set; }

        public double PinHeight
        {
            get
            {
                return pinHeight;
            }
        }

        public double SignalLineLength
        {
            get
            {
                return signalLineLength;
            }
        }

        public List<double> TopPinsHorizontalOffsets
        {
            get
            {
                return topPinsHorizontalOffsets;
            }
        }

        public List<double> BottomPinsHorizontalOffsets
        {
            get
            {
                return bottomPinsHorizontalOffsets;
            }
        }

        public double NameVerticalOffset { get; private set; }

        public string Component { get; private set; }

        public ComponentLayout(string componentName)
        {
            Component = componentName;
            deviceSymbols = new List<DeviceSymbol>();
        }

        public void AddDeviceSymbol(DeviceSymbol deviceSymbol)
        {
            deviceSymbols.Add(deviceSymbol);
        }

        public void Calculate(ProjectObjects projectObjects, Settings settings)
        {
            if (Component.Equals(settings.TerminalComponent))
                CalculateTerminal(projectObjects, settings);
            else
                CalculateDevice(projectObjects, settings);

        }

        private void CalculateTerminal(ProjectObjects projectObjects, Settings settings)
        {
            outlineHeight = settings.TerminalHeight;
            outlineWidth = settings.TerminalWidth;
            List<PinSymbol> pinSymbols = deviceSymbols.SelectMany(ds => ds.PanelPinSymbols).ToList();
            if (pinSymbols.Count > 0)
            {
                E3Text text = projectObjects.Text;
                pinSymbols.ForEach(ps => ps.CalculateTextSizes(text, settings));
                signalLineLength = GetSignalLength(settings, pinSymbols);
            }
            else
                signalLineLength = 0;
            TopMargin = settings.TerminalHeight / 2 + signalLineLength;
            BottomMargin = TopMargin;
            NameVerticalOffset = 0;
            pinHeight = 0;
            topPinsHorizontalOffsets = new List<double>() {0};
            bottomPinsHorizontalOffsets = new List<double>() {0};
        }

        private void CalculateDevice(ProjectObjects projectObjects, Settings settings)
        {
            E3Text text = projectObjects.Text;
            deviceSymbols.ForEach(ds => ds.CalculateNameSize(text, settings));
            double maxNameHeight = deviceSymbols.Max(ds => ds.NameSize.Height);
            outlineHeight = GetJustificatedLength(maxNameHeight) + settings.GridStep;
            double maxNameWidth = deviceSymbols.Max(ds => ds.NameSize.Width);
            outlineWidth = GetJustificatedLength(maxNameWidth);
            pinHeight = 0;
            signalLineLength = 0;
            NameVerticalOffset = -settings.Font.height / 2;
            TopMargin = outlineHeight / 2;
            BottomMargin = outlineHeight / 2;
            topPinsHorizontalOffsets = new List<double>();
            bottomPinsHorizontalOffsets = new List<double>();
            List<PinSymbol> pinSymbols = deviceSymbols.SelectMany(ds => ds.PanelPinSymbols).ToList();
            if (pinSymbols.Count > 0)
            {
                pinSymbols.ForEach(ps => ps.CalculateTextSizes(text, settings));
                pinHeight = GetJustificatedLength(pinSymbols.Max(ps => ps.NameLength));
                pinHeight = Math.Max(settings.MinPinHeight, pinHeight);
                signalLineLength = GetSignalLength(settings, pinSymbols);
                double halfAddressesWidth = pinSymbols.Max(ps => ps.AddressesSize.Width) / 2;
                double signalOffset = settings.SmallFont.height + settings.SignalHorizontalOffset;
                double pinLeftMargin = Math.Max(settings.HalfGridStep, Math.Max(signalOffset, halfAddressesWidth));
                double pinRightMargin = Math.Max(settings.HalfGridStep, halfAddressesWidth);
                double offsetBetweenPins = Math.Max(pinLeftMargin + pinRightMargin + settings.HalfGridStep, settings.GridStep * 2);
                double offsetBeforeFirstPin = pinLeftMargin + settings.HalfGridStep;
                double offsetAfterLastPin = pinRightMargin + settings.HalfGridStep;
                int topPinsCount = deviceSymbols.First().TopPinSymbols.Count;
                int bottomPinsCount = deviceSymbols.First().BottomPinSymbols.Count;
                double topPinsWidth = offsetBeforeFirstPin + offsetBetweenPins * (topPinsCount - 1) + offsetAfterLastPin;
                double bottomPinsWidth = offsetBeforeFirstPin + offsetBetweenPins * (bottomPinsCount - 1) + offsetAfterLastPin;
                double pinsWidth = Math.Max(topPinsWidth, bottomPinsWidth);
                outlineWidth = Math.Max(outlineWidth, pinsWidth);
                outlineHeight += (topPinsCount > 0) ? pinHeight : 0;
                outlineHeight += (bottomPinsCount > 0) ? pinHeight : 0;
                TopMargin = outlineHeight / 2 + ((topPinsCount > 0) ? signalLineLength : 0);
                BottomMargin = outlineHeight / 2 + ((bottomPinsCount > 0) ? signalLineLength : 0);
                double topAdditionalOffset = pinsWidth - topPinsWidth;
                double bottomAdditionalOffset = pinsWidth - bottomPinsWidth;
                topPinsHorizontalOffsets = GetPinsHorizontalOffsets(offsetBetweenPins, topAdditionalOffset, topPinsCount);
                bottomPinsHorizontalOffsets = GetPinsHorizontalOffsets(offsetBetweenPins, bottomAdditionalOffset, bottomPinsCount);
                if (bottomPinsCount > 0 && topPinsCount == 0)
                    NameVerticalOffset += pinHeight / 2;
                if (topPinsCount > 0 && bottomPinsCount == 0)
                    NameVerticalOffset -= pinHeight / 2;
            }
        }

        private double GetSignalLength(Settings settings, List<PinSymbol> pinSymbols)
        {
            double lineLength = GetJustificatedLength(pinSymbols.Max(ps => ps.SignalLength)) + settings.SignalVerticalOffset;
            return Math.Max(settings.MinSignalLineLength, lineLength);
        }

        private List<double> GetPinsHorizontalOffsets(double offsetBetweenPins, double additionalOffset, int pinsCount)
        {
            List<double> pinsOffsets = new List<Double>(pinsCount);
            double addition = ((additionalOffset > 0) ? additionalOffset : 0) / (pinsCount);
            if (pinsCount > 0)
            {
                offsetBetweenPins += addition;
                double halfDistanceBetweenPins = offsetBetweenPins * (pinsCount - 1) / 2;
                double horizontalOffset = - halfDistanceBetweenPins;
                for (int i = 0; i < pinsCount; i++)
                {
                    pinsOffsets.Add(horizontalOffset);
                    horizontalOffset += offsetBetweenPins;
                }
            }
            return pinsOffsets;
        }

        private int GetJustificatedLength(double length)
        {
            return (int)(length + 3);
        }
    }
}
