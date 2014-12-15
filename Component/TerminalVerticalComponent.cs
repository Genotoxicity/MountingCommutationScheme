using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class TerminalVerticalComponent : VerticalComponent
    {
        public TerminalVerticalComponent(ProjectObjects projectObjects, Settings settings, List<ElementPin> firstPinsGroup, List<ElementPin> secondPinsGroup)
            : base(projectObjects, settings, firstPinsGroup, secondPinsGroup)
        {
            Name = "TerminalVerticalComponent";
        }

        protected override void CalculateVertical()
        {
            outlineHeight = settings.TerminalMaxSize;
            outlineWidth = settings.TerminalMinSize;
            double offset = outlineHeight / 2;
            topPins.First().SetOffset(offset);
            bottomPins.First().SetOffset(offset);
        }

        public override void PlaceVerticalElement(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position, Element element)
        {
            Graphic graph = projectObjects.Graphic;
            List<int> graphIds = new List<int>();
            graphIds.Add(CreateOutline(graph, sheet, sheetId, position));
            TerminalElement terminalElement = element as TerminalElement;
            graphIds.Add(CreateNameVerticalText(sheet, sheetId, terminalElement.TerminalName, position, settings.SmallFont));
            double outlineTop = sheet.MoveUp(position.Y, outlineHeight / 2);
            double outlineBottom = sheet.MoveDown(position.Y, outlineHeight / 2);
            graphIds.AddRange(CreateVerticalSignalAndAddresses(sheet, sheetId, graph, element.FirstPinsGroup.First(), element.SignalLineLength, outlineTop, position.X, Position.Top));
            graphIds.AddRange(CreateVerticalSignalAndAddresses(sheet, sheetId, graph, element.SecondPinsGroup.First(), element.SignalLineLength, outlineBottom, position.X, Position.Bottom));
            Group group = projectObjects.Group;
            group.CreateGroup(graphIds);
        }

    }
}
