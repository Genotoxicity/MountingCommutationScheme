using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class PinSymbol
    {
        private string name;
        private string signal;
        private double nameLength;
        private double signalLength;
        private Size addressesSize; 
        private string addresses;
        private int equivalentId;
        private int addressesCount;
        private int id;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public double PanelX { get; private set; }

        public double PanelY { get; private set; }

        public double NameLength
        {
            get
            {
                return nameLength;
            }
        }

        public double SignalLength
        {
            get
            {
                return signalLength;
            }
        }

        public Size AddressesSize
        {
            get
            {
                return addressesSize;
            }
        }

        public PinSymbol(DevicePin pin, int pinId, int equivalentId)
        {
            pin.Id = pinId;
            id = pinId;
            name = pin.Name;
            signal = pin.SignalName;
            double x,y,z;
            pin.GetPanelLocation(out x, out y, out z);
            PanelX = x;
            PanelY = y;
            this.equivalentId = equivalentId;
        }

        public void SetMateAddresses(MateAddresses mateAddresses)
        {
            List<string> mateAddressesStrings = new List<string>();
            mateAddressesStrings.AddRange(mateAddresses.GetMateAdressesByPinId(id));
            mateAddressesStrings.AddRange(mateAddresses.GetMateAdressesByPinId(equivalentId));
            mateAddressesStrings = mateAddressesStrings.Distinct().ToList();
            addresses = String.Empty;
            addressesCount = mateAddressesStrings.Count;
            if (addressesCount > 0)
            {
                mateAddressesStrings.ForEach(a => addresses += a + Environment.NewLine);
                addresses = addresses.TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        public void CalculateTextSizes(E3Text text, Settings settings)
        {
            E3Font font = settings.SmallFont;
            nameLength = text.GetTextLength(name,font);
            signalLength = text.GetTextLength(signal, font);
            addressesSize = new Size(0,0);
            if (!String.IsNullOrEmpty(addresses))
            {
                double rotation = 90;
                addressesSize = text.GetTextBoxSize(addresses, font, rotation);
            }
        }

        public List<int> Place(ProjectObjects projectObjects, Settings settings, Sheet sheet, Point position, ComponentLayout componentLayout, Level level, bool isTerminal, int sheetId)
        {
            if (isTerminal)
                return PlaceTerminalPins(projectObjects, settings, sheet, position, componentLayout, level, sheetId);
            else
                return PlaceNormalPins(projectObjects, settings, sheet, position, componentLayout, level, sheetId);
        }

        private List<int> PlaceNormalPins (ProjectObjects projectObjects, Settings settings, Sheet sheet, Point position, ComponentLayout componentLayout, Level level, int sheetId)
        {
            Graphic graphic = projectObjects.Graphic;
            E3Text text = projectObjects.Text;
            double yTop = sheet.MoveUp(position.Y, componentLayout.PinHeight / 2);
            double yBottom = sheet.MoveDown(yTop, componentLayout.PinHeight);
            double xLeft = sheet.MoveLeft(position.X, settings.PinWidth / 2);
            double xRight = sheet.MoveRight(xLeft, settings.PinWidth);
            List<int> signalAndAddressIds = PlaceSignalAndAddress(projectObjects, sheet, settings, yTop, yBottom, position.X, componentLayout.SignalLineLength, level, sheetId);
            List<int> graphicIds = new List<int>(signalAndAddressIds.Count + 2);
            graphicIds.Add(graphic.CreateRectangle(sheetId, xLeft, yBottom, xRight, yTop));
            double pinNameX = text.GetTextAbsciss(position.X, settings.SmallFont.height, settings.SmallFont, sheet);
            graphicIds.Add(text.CreateVerticalText(sheetId, name, pinNameX, position.Y, settings.SmallFont));
            return graphicIds;
        }

        private List<int> PlaceTerminalPins(ProjectObjects projectObjects, Settings settings, Sheet sheet, Point position, ComponentLayout componentLayout, Level level, int sheetId)
        {
            Graphic graphic = projectObjects.Graphic;
            E3Text text = projectObjects.Text;
            return PlaceSignalAndAddress(projectObjects, sheet, settings, position.Y, position.Y, position.X, componentLayout.SignalLineLength, level, sheetId);
        }

        private List<int> PlaceSignalAndAddress(ProjectObjects projectObjects, Sheet sheet, Settings settings, double top, double bottom, double absciss, double signalLineLength, Level level, int sheetId)
        {
            if (addressesCount == 0)
                return new List<int>(0);
            int graphicCount = addressesCount == 1 ? 2 : 4;
            if (!String.IsNullOrEmpty(signal))
                graphicCount++;
            List<int> graphicIds = new List<int>(graphicCount);
            double signalTop, signalBottom, addressesY;
            Alignment alignment;
            if (level == Level.Top)
            {
                signalBottom = top;
                signalTop = sheet.MoveUp(signalBottom, signalLineLength);
                addressesY = sheet.MoveUp(signalTop, settings.AdressesVerticalOffset);
                alignment = Alignment.Left;
            }
            else
            {
                signalTop = bottom;
                signalBottom = sheet.MoveDown(signalTop, signalLineLength);
                addressesY = sheet.MoveDown(signalBottom, settings.AdressesVerticalOffset);
                alignment = Alignment.Right;
            }
            Graphic graphic = projectObjects.Graphic;
            graphicIds.Add(graphic.CreateLine(sheetId, absciss, signalBottom, absciss, signalTop));
            E3Text text = projectObjects.Text;
            E3Font font = new E3Font(height: settings.SmallFont.height, alignment: alignment);
            double addressesX = text.GetTextAbsciss(absciss, addressesSize.Width, settings.SmallFont, sheet);
            graphicIds.Add(text.CreateVerticalText(sheetId, addresses, addressesX, addressesY, font));
            if (!String.IsNullOrEmpty(signal))
            {
                double signalTextY = (signalBottom + signalTop) / 2;
                double signalTextX = sheet.MoveLeft(absciss, settings.SignalHorizontalOffset);
                graphicIds.Add(text.CreateVerticalText(sheetId, signal, signalTextX, signalTextY, settings.SmallFont));
            }
            if (addressesCount > 1)
            {
                double signalOffshootX = sheet.MoveRight(absciss, settings.HalfGridStep);
                if (level == Level.Top)
                {
                    double signalIntermediate = sheet.MoveUp(signalBottom, settings.GridStep);
                    graphicIds.Add(graphic.CreateLine(sheetId, absciss, signalBottom, signalOffshootX, signalIntermediate));
                    graphicIds.Add(graphic.CreateLine(sheetId, signalOffshootX, signalIntermediate, signalOffshootX, signalTop));
                }
                else
                {
                    double signalIntermediate = sheet.MoveDown(signalTop, settings.GridStep);
                    graphicIds.Add(graphic.CreateLine(sheetId, absciss, signalTop, signalOffshootX, signalIntermediate));
                    graphicIds.Add(graphic.CreateLine(sheetId, signalOffshootX, signalIntermediate, signalOffshootX, signalBottom));
                }
            }
            return graphicIds;
        }
    }
}
