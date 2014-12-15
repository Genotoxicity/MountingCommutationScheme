using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class TerminalHorizontalComponent : HorizontalComponent
    {
        public TerminalHorizontalComponent(ProjectObjects projectObjects, Settings settings, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
            : base(projectObjects, settings, firstPinsGroup, secondPinsGroup)
        {
            Name = "TerminalHorizontalComponent";
        }

        protected override void CalculateHorizontal()
        {
            outlineHeight = settings.TerminalMinSize;
            outlineWidth = settings.TerminalMaxSize;
            double offset = outlineWidth / 2;
            leftPins.First().SetOffset(offset);
            rightPins.First().SetOffset(offset);
        }

        protected override void PlaceHorizontalElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element)
        {
            Graphic graph = projectObjects.Graphic;
            List<int> graphIds = new List<int>();
            graphIds.Add(CreateOutline(graph, sheet, sheetId, position));
            TerminalElement terminalElement = element as TerminalElement;
            graphIds.Add(CreateNameHorizontalText(sheet, sheetId, terminalElement.TerminalName, position, settings.SmallFont));
            double outlineLeft = sheet.MoveLeft(position.X, outlineWidth / 2);
            double outlineRight = sheet.MoveRight(position.X, outlineWidth / 2);
            graphIds.AddRange(CreateHorizontalSignalAndAddresses(sheet, sheetId, graph, element.FirstPinsGroup.First(), element.SignalLineLength, outlineLeft, position.Y, Position.Left));
            graphIds.AddRange(CreateHorizontalSignalAndAddresses(sheet, sheetId, graph, element.SecondPinsGroup.First(), element.SignalLineLength, outlineRight, position.Y, Position.Right));
            Group group = projectObjects.Group;
            group.CreateGroup(graphIds);
        }

    }
}
