using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class DeviceVerticalComponent : VerticalComponent
    {
        protected double pinWidth;
        protected double pinHeight;

        public DeviceVerticalComponent(ProjectObjects projectObjects, Settings settings, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup, string name)
            : base(projectObjects, settings, firstPinsGroup, secondPinsGroup)
        {
            Name = name;
        }

        protected override void CalculateVertical()
        {
            pinHeight = GetMaxPinSize(topPins, bottomPins);
            pinWidth = settings.PinMinSize;
            outlineHeight = pinHeight * 2 + nameHeight;
            double pinsWidth = Math.Max(topPins.Count, bottomPins.Count) * (pinWidth + settings.GridStep);
            outlineWidth = Math.Max(nameLength, pinsWidth);
            SetPinsOffset(topPins);
            SetPinsOffset(bottomPins);
        }

        private void SetPinsOffset(List<ComponentPin> pins)
        {
            int pinsCount = pins.Count;
            if (pinsCount > 0)
            {
                double totalGap = outlineWidth - (pinsCount * pinWidth);
                double gap = totalGap / pinsCount;
                double offset = (outlineWidth - gap) / 2;
                pins.First().SetOffset(offset);
                for (int i = 1; i < pinsCount; i++)
                {
                    offset -= (pinWidth + gap);
                    pins[i].SetOffset(offset);
                }
            }
        }

        public override void PlaceVerticalElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element)
        {
            Graphic graph = projectObjects.Graphic;
            List<int> graphIds = new List<int>();
            graphIds.Add(CreateOutline(graph, sheet, sheetId, position));
            graphIds.Add(CreateNameHorizontalText(sheet, sheetId, element.Name, position, settings.Font));
            double outlineTop = sheet.MoveUp(position.Y, outlineHeight / 2);
            double topPinBottom = sheet.MoveDown(outlineTop, pinHeight);
            graphIds.AddRange(CreatePins(sheet, sheetId, position, graph, topPins, element.FirstPinsGroup, outlineTop, topPinBottom, element.SignalLineLength, Position.Top));
            double outlineBottom = sheet.MoveDown(position.Y, outlineHeight / 2);
            double bottomPinTop = sheet.MoveUp(outlineBottom, pinHeight);
            graphIds.AddRange(CreatePins(sheet, sheetId, position, graph, bottomPins, element.SecondPinsGroup, bottomPinTop, outlineBottom, element.SignalLineLength, Position.Bottom));
            Group group = projectObjects.Group;
            group.CreateGroup(graphIds);
        }

        private List<int> CreatePins(Sheet sheet, int sheetId, Point position, Graphic graph, List<ComponentPin> componentPins, List<ElementPin> elementPins, double pinTop, double pinBottom, double signalLineLength, Position pinsPosition)
        {
            List<int> graphIds = new List<int>();
            double nameY = (pinTop + pinBottom) / 2;
            for (int i=0; i<componentPins.Count; i++)
            {
                ComponentPin componentPin = componentPins[i];
                ElementPin elementPin = elementPins[i];
                double pinLeft = sheet.MoveLeft(position.X, componentPin.Offset);
                double pinRight = sheet.MoveRight(pinLeft, pinWidth);
                double pinCenterX = (pinLeft + pinRight) / 2;
                double nameX = sheet.MoveRight(pinCenterX, settings.SmallFont.height / 2);
                graphIds.Add(text.CreateVerticalText(sheetId, componentPin.Name, nameX, nameY, settings.SmallFont));
                graphIds.Add(graph.CreateRectangle(sheetId, pinLeft, pinTop, pinRight, pinBottom));
                graphIds.AddRange(CreateVerticalSignalAndAddresses(sheet, sheetId, graph, elementPin, signalLineLength, (pinsPosition == Position.Top) ? pinTop : pinBottom , pinCenterX, pinsPosition));
            }
            return graphIds;
        }
    }
}
