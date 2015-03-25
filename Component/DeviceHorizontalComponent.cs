using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class DeviceHorizontalComponent : HorizontalComponent
    {
        protected double pinWidth;
        protected double pinHeight;

        public DeviceHorizontalComponent(ProjectObjects projectObjects, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup, string name)
            : base(projectObjects, firstPinsGroup, secondPinsGroup)
        {
            Name = name;
        }

        protected override void CalculateHorizontal()
        {
            pinWidth = GetMaxPinSize(leftPins, rightPins);
            pinHeight = Settings.PinMinSize;
            outlineWidth = pinWidth * 2 + nameLength;
            double pinsHeight = Math.Max(leftPins.Count, rightPins.Count) * (pinHeight + Settings.GridStep);
            outlineHeight = Math.Max(nameHeight, pinsHeight);
            SetPinsOffset(leftPins);
            SetPinsOffset(rightPins);
        }

        private void SetPinsOffset(List<ComponentPin> pins)
        {
            int pinsCount = pins.Count;
            if (pinsCount >0)
            {
                double totalGap = outlineHeight - (pinsCount * pinHeight);
                double gap = totalGap / pinsCount;
                double offset = (outlineHeight - gap) / 2;
                pins.First().SetOffset(offset);
                for (int i = 1; i < pinsCount; i++)
                {
                    offset -= (pinHeight + gap);
                    pins[i].SetOffset(offset);
                }
            }
        }

        protected override void PlaceHorizontalElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element)
        {
            Graphic graph = projectObjects.Graphic;
            List<int> graphIds = new List<int>();
            graphIds.Add(CreateOutline(graph, sheet, sheetId, position));
            graphIds.Add(CreateNameHorizontalText(sheet, sheetId, element.Name, position, Settings.Font));
            double outlineLeft = sheet.MoveLeft(position.X, outlineWidth / 2);
            double leftPinRight = sheet.MoveRight(outlineLeft, pinWidth);
            graphIds.AddRange(CreatePins(sheet, sheetId, position, graph, leftPins, element.FirstPinsGroup, outlineLeft, leftPinRight, element.SignalLineLength, Position.Left));
            double outlineRight = sheet.MoveRight(position.X, outlineWidth / 2);
            double rightPinLeft = sheet.MoveLeft(outlineRight, pinWidth);
            graphIds.AddRange(CreatePins(sheet, sheetId, position, graph, rightPins, element.SecondPinsGroup, rightPinLeft, outlineRight, element.SignalLineLength, Position.Right));
            Group group = projectObjects.Group;
            group.CreateGroup(graphIds);
        }

        private List<int> CreatePins(Sheet sheet, int sheetId, Point position, Graphic graph, List<ComponentPin> componentPins, List<ElementPin> elementPins, double pinLeft, double pinRight, double signalLineLength, Position pinsPosition)
        {
            List<int> graphIds = new List<int>();
            double nameX = (pinLeft + pinRight) / 2;
            for (int i = 0; i < componentPins.Count; i++)
            {
                ComponentPin componentPin = componentPins[i];
                ElementPin elementPin = elementPins[i];
                double pinTop = sheet.MoveUp(position.Y, componentPin.Offset);
                double pinBottom = sheet.MoveDown(pinTop, pinHeight);
                double pinCenterY = (pinTop + pinBottom) / 2;
                double nameY = sheet.MoveDown(pinCenterY, Settings.SmallFont.height / 2);
                graphIds.Add(text.CreateText(sheetId, componentPin.Name, nameX, nameY, Settings.SmallFont));
                graphIds.Add(graph.CreateRectangle(sheetId, pinLeft, pinTop, pinRight, pinBottom));
                graphIds.AddRange(CreateHorizontalSignalAndAddresses(sheet, sheetId, graph, elementPin, signalLineLength, (pinsPosition == Position.Left) ? pinLeft : pinRight, pinCenterY, pinsPosition));
            }
            return graphIds;
        }

    }
}
