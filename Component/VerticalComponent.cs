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

        protected VerticalComponent(ProjectObjects projectObjects, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
            : base(projectObjects.Text)
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
                    double turnX = sheet.MoveRight(signalLineX, Settings.HalfGridStep);
                    double turnY = (position == Position.Top) ? sheet.MoveUp(signalLineStartY, Settings.GridStep) : sheet.MoveDown(signalLineStartY, Settings.GridStep );
                    graphIds.Add(graph.CreateLine(sheetId, signalLineX, signalLineStartY, turnX, turnY));
                    graphIds.Add(graph.CreateLine(sheetId, turnX, turnY, turnX, signalLineEndY));
                }
            }
            if (hasSignal)
            {
                double signalTextX = sheet.MoveLeft(signalLineX, Settings.SignalOffsetFromLine);
                double lengthWithoutOffset = signalLineLength - Settings.SignalOffsetFromOutline;
                double offset = Settings.SignalOffsetFromOutline + (lengthWithoutOffset - Settings.SignalOffsetAfterText) / 2; 
                double signalTextY = (position == Position.Top) ? sheet.MoveUp(signalLineStartY, offset) : sheet.MoveDown(signalLineStartY, offset);
                graphIds.Add(text.CreateVerticalText(sheetId, pin.Signal, signalTextX, signalTextY, Settings.SmallFont));
            }
            if (addressesCount > 0)
            {
                string address = GetAddressString(pin.Addresses, addressesCount);
                Size size = text.GetTextBoxSize(address, Settings.SmallFont, 90);
                double addressY = (position == Position.Top) ? sheet.MoveUp(signalLineEndY, Settings.AdressOffset) : sheet.MoveDown(signalLineEndY, Settings.AdressOffset);
                E3Font font = new E3Font();
                font.height = Settings.SmallFont.height;
                font.alignment = (position == Position.Top) ? Alignment.Left : Alignment.Right;
                double addressX = text.GetTextAbsciss(signalLineX, size.Width, font, sheet);
                graphIds.Add(text.CreateVerticalText(sheetId, address, addressX, addressY, font));
            }
            return graphIds;
        }

    }
}
