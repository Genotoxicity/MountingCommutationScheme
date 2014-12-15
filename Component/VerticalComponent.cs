using System;
using System.Collections.Generic;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    abstract class VerticalComponent : Component
    {
        protected List<ComponentPin> topPins;
        protected List<ComponentPin> bottomPins;

        protected VerticalComponent(ProjectObjects projectObjects, Settings settings, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
            : base(projectObjects.Text, settings)
        {
            topPins = new List<ComponentPin>(firstPinsGroup.Count);
            bottomPins = new List<ComponentPin>(secondPinsGroup.Count);
            DevicePin pin = projectObjects.Pin;
            foreach (ElementPin elementPin in firstPinsGroup)
            {
                pin.Id = elementPin.PanelPinId;
                topPins.Add(new ComponentPin(pin.Name));
            }
            foreach (ElementPin elementPin in secondPinsGroup)
            {
                pin.Id = elementPin.PanelPinId;
                bottomPins.Add(new ComponentPin(pin.Name));
            }
        }

        public override void Calculate()
        {
            CalculateVertical();
        }

        protected abstract void CalculateVertical();

        public override ElementSizes GetElementSizes(List<ElementPin> topElementPins, List<ElementPin> bottomElementPins)
        {
            double rightMargin = outlineWidth / 2;
            double leftMargin = rightMargin;
            double signalLineLength = GetSignalLineLength(topElementPins, bottomElementPins);
            double topAdrressesLength = GetAddressesLength(topElementPins);
            double bottomAdrressesLength = GetAddressesLength(bottomElementPins);
            double topMargin = outlineHeight / 2 + signalLineLength + topAdrressesLength + settings.AdressOffset;
            double bottomMargin = outlineHeight / 2 + signalLineLength + bottomAdrressesLength + settings.AdressOffset;
            return new ElementSizes(new Margins(leftMargin, rightMargin, topMargin, bottomMargin), signalLineLength);
        }

        public override void PlaceElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element)
        {
            PlaceVerticalElement(projectObjects, sheet, sheetId, position, element);
        }

        public abstract void PlaceVerticalElement(ProjectObjects projectObjects, Sheet sheet, int sheetId,Point position, Element element);

        protected List<int> CreateVerticalSignalAndAddresses(Sheet sheet, int sheetId, Graphic graph, ElementPin pin, double signalLineLength, double signalLineStartY, double signalLineX, Position position)
        {
            List<int> graphIds = new List<int>();
            int addressesCount = pin.Addresses.Count;
            bool hasSignal = !String.IsNullOrEmpty(pin.Signal);
            double signalLineEndY = (position == Position.Top) ? sheet.MoveUp(signalLineStartY, signalLineLength) : sheet.MoveDown(signalLineStartY, signalLineLength);
            if (addressesCount > 0 || hasSignal)
            {
                graphIds.Add(graph.CreateLine(sheetId, signalLineX, signalLineStartY, signalLineX, signalLineEndY));
                if (addressesCount > 1)
                {
                    double turnX = sheet.MoveRight(signalLineX, settings.HalfGridStep);
                    double turnY = (position == Position.Top) ? sheet.MoveUp(signalLineStartY, settings.GridStep) : sheet.MoveDown(signalLineStartY, settings.GridStep );
                    graphIds.Add(graph.CreateLine(sheetId, signalLineX, signalLineStartY, turnX, turnY));
                    graphIds.Add(graph.CreateLine(sheetId, turnX, turnY, turnX, signalLineEndY));
                }
            }
            if (hasSignal)
            {
                double signalTextX = sheet.MoveLeft(signalLineX, settings.SignalOffsetFromLine);
                double signalTextY = (signalLineEndY + signalLineStartY) / 2;
                graphIds.Add(text.CreateVerticalText(sheetId, pin.Signal, signalTextX, signalTextY, settings.SmallFont));
            }
            if (addressesCount > 0)
            {
                string address = GetAddressString(pin.Addresses, addressesCount);
                Size size = text.GetTextBoxSize(address, settings.SmallFont, 90);
                double addressY = (position == Position.Top) ? sheet.MoveUp(signalLineEndY, settings.AdressOffset) : sheet.MoveDown(signalLineEndY, settings.AdressOffset);
                E3Font font = new E3Font();
                font.height = settings.SmallFont.height;
                font.alignment = (position == Position.Top) ? Alignment.Left : Alignment.Right;
                double addressX = text.GetTextAbsciss(signalLineX, size.Width, font, sheet);
                graphIds.Add(text.CreateVerticalText(sheetId, address, addressX, addressY, font));
            }
            return graphIds;
        }

    }
}
