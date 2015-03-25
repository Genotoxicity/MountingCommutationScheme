using System;
using System.Collections.Generic;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    abstract class HorizontalComponent : Component
    {
        protected List<ComponentPin> leftPins;
        protected List<ComponentPin> rightPins;

        protected HorizontalComponent(ProjectObjects projectObjects, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
            : base(projectObjects.Text)
        {
            leftPins = new List<ComponentPin>(firstPinsGroup.Count);
            rightPins = new List<ComponentPin>(secondPinsGroup.Count);
            DevicePin pin = projectObjects.Pin;
            foreach (ElementPin elementPin in firstPinsGroup)
            {
                pin.Id = elementPin.PanelPinId;
                leftPins.Add(new ComponentPin(pin.Name));
            }
            foreach (ElementPin elementPin in secondPinsGroup)
            {
                pin.Id = elementPin.PanelPinId;
                rightPins.Add(new ComponentPin(pin.Name));
            }
        }

        public override void Calculate()
        {
            CalculateHorizontal();
        }

        protected abstract void CalculateHorizontal();

        public override void PlaceElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element)
        {
            PlaceHorizontalElement(projectObjects, sheet, sheetId, position, element);
        }

        protected abstract void PlaceHorizontalElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element);

        protected List<int> CreateHorizontalSignalAndAddresses(Sheet sheet, int sheetId, Graphic graph, ElementPin pin, double signalLineLength, double signalLineStartX, double signalLineY, Position position)
        {
            List<int> graphIds = new List<int>();
            int addressesCount = pin.Addresses.Count;
            bool hasSignal = !String.IsNullOrEmpty(pin.Signal);
            double signalLineEndX = (position == Position.Left) ? sheet.MoveLeft(signalLineStartX, signalLineLength) : sheet.MoveRight(signalLineStartX, signalLineLength);
            if (addressesCount > 0 || hasSignal)
            {
                graphIds.Add(graph.CreateLine(sheetId, signalLineStartX, signalLineY, signalLineEndX, signalLineY));
                if (addressesCount > 1)
                {
                    double turnY = sheet.MoveDown(signalLineY, Settings.HalfGridStep);
                    double turnX = (position == Position.Left) ? sheet.MoveLeft(signalLineStartX, Settings.GridStep) : sheet.MoveRight(signalLineStartX, Settings.GridStep);
                    graphIds.Add(graph.CreateLine(sheetId, signalLineStartX, signalLineY, turnX, turnY));
                    graphIds.Add(graph.CreateLine(sheetId, turnX, turnY, signalLineEndX, turnY));
                }
            }
            if (hasSignal)
            {
                double signalTextY = sheet.MoveUp(signalLineY, Settings.SignalOffsetFromLine);
                double lengthWithoutOffset = signalLineLength - Settings.SignalOffsetFromOutline;
                double offset = Settings.SignalOffsetFromOutline + (lengthWithoutOffset - Settings.SignalOffsetAfterText) / 2; 
                double signalTextX = (position == Position.Left) ? sheet.MoveLeft(signalLineStartX, offset) : sheet.MoveRight(signalLineStartX, offset);
                graphIds.Add(text.CreateText(sheetId, pin.Signal, signalTextX, signalTextY, Settings.SmallFont));
            }
            if (addressesCount > 0)
            {
                string address = GetAddressString(pin.Addresses, addressesCount);
                Size size = text.GetTextBoxSize(address, Settings.SmallFont, 0);
                double addressX = (position == Position.Left) ? sheet.MoveLeft(signalLineEndX, Settings.AdressOffset) : sheet.MoveRight(signalLineEndX, Settings.AdressOffset);
                E3Font font = new E3Font();
                font.height = Settings.SmallFont.height;
                font.alignment = (position == Position.Left) ? Alignment.Right : Alignment.Left;
                double addressY = text.GetTextOrdinate(signalLineY, size.Height, font, sheet);
                graphIds.Add(text.CreateText(sheetId, address, addressX, addressY, font));
            }
            return graphIds;
        }
    }
}
