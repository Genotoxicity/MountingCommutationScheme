using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public abstract class Component
    {
        protected double nameLength;
        protected double nameHeight;
        protected double outlineHeight;
        protected double outlineWidth;
        protected Settings settings;
        protected E3Text text;

        public double OutlineWidth
        {
            get
            {
                return outlineWidth;
            }
        }

        public double OutlineHeight
        {
            get
            {
                return outlineHeight;
            }
        }

        public string Name { get; protected set; }

        protected Component(E3Text text, Settings settings)
        {
            this.settings = settings;
            this.text = text;
            nameHeight = GetJustificatedLength(settings.Font.height);
        }

        public void AdjustNameLength(string name)
        {
            nameLength = Math.Max(GetJustificatedLength(text.GetTextLength(name, settings.Font)), nameLength);
        }

        public abstract void Calculate();

        public abstract ElementSizes GetElementSizes(List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup);

        public abstract void PlaceElement(ProjectObjects projectObjects, Sheet sheet, int sheetId,  Point position, Element element);

        protected int CreateOutline(Graphic graph, Sheet sheet, int sheetId, Point position)
        {
            double halfWidth = outlineWidth / 2;
            double halfHeight = outlineHeight / 2;
            double xLeft = sheet.MoveLeft(position.X, halfWidth);
            double xRight = sheet.MoveRight(position.X, halfWidth);
            double yTop = sheet.MoveUp(position.Y, halfHeight);
            double yBottom = sheet.MoveDown(position.Y, halfHeight);
            return graph.CreateRectangle(sheetId, xLeft, yTop, xRight, yBottom);
        }

        protected double GetSignalLineLength(List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
        {
            double firstMax = (firstPinsGroup.Count > 0) ? (firstPinsGroup.Max(fp => text.GetTextLength(fp.Signal, settings.SmallFont)) + settings.SignalOffsetFromOutline * 2) : 0;
            double secondMax = (secondPinsGroup.Count > 0) ? (secondPinsGroup.Max(sp => text.GetTextLength(sp.Signal, settings.SmallFont)) + settings.SignalOffsetFromOutline * 2) : 0;
            return Math.Max(firstMax, secondMax);
        }

        protected double GetAddressesLength(List<ElementPin> pins)
        {
            double addressesLength = 0;
            if (pins.Count > 0)
            {
                IEnumerable<string> addresses = pins.SelectMany(p => p.Addresses);
                if (addresses.Count() > 0)
                    addressesLength = addresses.Max(a => text.GetTextLength(a, settings.SmallFont)) + settings.AdressOffset;
            }
            return addressesLength;
        }

        protected double GetMaxPinSize(List<ComponentPin> firstPins, List<ComponentPin> secondPins)
        { 
            
            double maxFirstPinNameSize = (firstPins.Count==0) ? 0 : firstPins.Max(fp => text.GetTextLength(fp.Name, settings.SmallFont));
            double maxSecondPinNameSize = (secondPins.Count==0) ? 0 : secondPins.Max(sp => text.GetTextLength(sp.Name, settings.SmallFont));
            return GetJustificatedLength(Math.Max(maxFirstPinNameSize, maxSecondPinNameSize));
        }

        protected static int GetJustificatedLength(double length)
        {
            return (int)(length + 3);
        }

        protected int CreateNameVerticalText(Sheet sheet, int sheetId, string name, Point position, E3Font font)
        {
            double x = sheet.MoveRight(position.X, font.height / 2);
            return text.CreateVerticalText(sheetId, name, x, position.Y, font);
        }

        protected int CreateNameHorizontalText(Sheet sheet, int sheetId, string name, Point position, E3Font font)
        {
            double y = sheet.MoveDown(position.Y, font.height / 2);
            return text.CreateText(sheetId, name, position.X, y, font);
        }

        protected static string GetAddressString(List<string> addresses, int addressesCount)
        {
            string address = String.Empty;
            if (addressesCount == 1)
                address = addresses.First();
            else
            {
                addresses.ForEach(a => address += a + Environment.NewLine);
                address = address.TrimEnd(Environment.NewLine.ToCharArray());
            }
            return address;
        }

    }
}
