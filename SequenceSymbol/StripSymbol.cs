using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class StripSymbol  : SequenceSymbol
    {
        private List<TerminalElement> elements;
        private Dictionary<int, List<Position>> jumperPositionsByElementIndex;
        private Orientation orientation;

        public override IEnumerable<Element> Elements
        {
            get
            {
                return elements;
            }
        }

        public StripSymbol(List<TerminalElement> elements, Orientation orientation, E3Text text)
        {
            this.elements = elements;
            this.orientation = orientation;
            jumperPositionsByElementIndex = GetJumperPositionsByElementIndex();
            Calculate(text);
        }

        private Dictionary<int, List<Position>> GetJumperPositionsByElementIndex()
        {
            Dictionary<int, List<Position>> jumperPositionsByElementIndex = new Dictionary<int, List<Position>>(); 
            for (int i = 0; i < elements.Count-1; i++)
            {
                TerminalElement first = elements[i];
                TerminalElement second = elements[i + 1];
                string firstAddress = first.TerminalName;
                string secondAddress = second.TerminalName;
                Position firstGroupPosition = (orientation == Orientation.Horizontal) ? Position.Top : Position.Left;
                Position secondGroupPosition = (orientation == Orientation.Horizontal) ? Position.Bottom : Position.Right;
                bool firstGroupHasSecondAddress = first.FirstPinsGroup.Any(p => p.Addresses.Contains(secondAddress));
                bool secondGroupHasFirstAddress = second.FirstPinsGroup.Any(p => p.Addresses.Contains(firstAddress));
                if (firstGroupHasSecondAddress && secondGroupHasFirstAddress)
                {
                    if (!jumperPositionsByElementIndex.ContainsKey(i))
                        jumperPositionsByElementIndex.Add(i, new List<Position>());
                    jumperPositionsByElementIndex[i].Add(firstGroupPosition);
                }
                firstGroupHasSecondAddress = first.SecondPinsGroup.Exists(p => p.Addresses.Contains(secondAddress));
                secondGroupHasFirstAddress = second.SecondPinsGroup.Exists(p => p.Addresses.Contains(firstAddress));
                if (firstGroupHasSecondAddress && secondGroupHasFirstAddress)
                {
                    if (!jumperPositionsByElementIndex.ContainsKey(i))
                        jumperPositionsByElementIndex.Add(i, new List<Position>());
                    jumperPositionsByElementIndex[i].Add(secondGroupPosition);
                }
            }
            return jumperPositionsByElementIndex;
        }

        private void RemoveJumperAddressesAndSetPinIsJumpered(IEnumerable<int> jumperPositions)
        {
            foreach (int index in jumperPositions)
            {
                TerminalElement first = elements[index];
                TerminalElement second = elements[index + 1];
                string firstAddress = first.TerminalName;
                string secondAddress = second.TerminalName;
                List<Position> positions = jumperPositionsByElementIndex[index];
                foreach (Position position in positions)
                {
                    List<ElementPin> firstPins, secondPins;
                    if (position == Position.Top || position == Position.Left)
                    {
                        firstPins = first.FirstPinsGroup;
                        secondPins = second.FirstPinsGroup;
                    }
                    else
                    {
                        firstPins = first.SecondPinsGroup;
                        secondPins = second.SecondPinsGroup;
                    }
                    firstPins.ForEach(p => p.SetJumpered(secondAddress));
                    secondPins.ForEach(p => p.SetJumpered(firstAddress));
                }
            }
        }

        private void Calculate(E3Text text)
        {
            RemoveJumperAddressesAndSetPinIsJumpered(jumperPositionsByElementIndex.Keys);
            elements.ForEach(e => e.Calculate(text));
            IEnumerable<Margins> margins = elements.Select(e => e.Margins);
            if (orientation == Orientation.Horizontal)
            {
                Width = margins.Sum(m => m.Left + m.Right);
                double topOffset = margins.Max(m => m.Top);
                double bottomOffset = margins.Max(m => m.Bottom);
                Height = topOffset + bottomOffset;
                Margins = new Margins(Width / 2, Width / 2, topOffset, bottomOffset);
            }
            else
            {
                Width = margins.Max(m => m.Left) + margins.Max(m => m.Right);
                Height = margins.Sum(m => m.Top + m.Bottom);
                Margins = new Margins(Width / 2, Width / 2, Height/2, Height/2);
            }

        }

        public override void Place(ProjectObjects projectObjects, Sheet sheet, int sheetId, Point position)
        {
            if (orientation == Orientation.Horizontal)
            {
                double elementWidth = elements.First().OutlineWidth;
                double x = sheet.MoveLeft(position.X, (Width - elementWidth) / 2);
                for (int i = 0; i < elements.Count; i++ )
                {
                    Element element = elements[i];
                    element.Place(projectObjects, sheet, sheetId, new Point(x, position.Y));
                    PlaceJumper(projectObjects, sheet, sheetId, i, x, position.Y);
                    x = sheet.MoveRight(x, elementWidth);
                }
            }
            else
            {
                double elementHeight = elements.First().OutlineHeight;
                double y = sheet.MoveUp(position.Y, (Height - elementHeight) / 2);
                for (int i = 0; i < elements.Count; i++ )
                {
                    Element element = elements[i];
                    element.Place(projectObjects, sheet, sheetId, new Point(position.X, y));
                    PlaceJumper(projectObjects, sheet, sheetId, i, position.X, y);
                    y = sheet.MoveDown(y, elementHeight);
                }
            }
        }

        private void PlaceJumper(ProjectObjects projectObjects, Sheet sheet, int sheetId, int index, double x, double y)
        {
            if (!jumperPositionsByElementIndex.ContainsKey(index))
                return;
            Graphic graph = projectObjects.Graphic;
            double offset = Settings.TerminalMaxSize / 2;
            double step1 = 3;
            double step2 = Settings.TerminalMinSize / 4;
            foreach (Position jumperPosition in jumperPositionsByElementIndex[index])
            {
                List<Point> points = new List<Point>(6);
                double arcX, arcY;
                switch (jumperPosition)
                {
                    case Position.Left:
                        arcX = sheet.MoveLeft(x, offset);
                        arcY = y;
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveLeft(arcX, step1);
                        arcY = sheet.MoveDown(arcY, step2);
                        points.Add(new Point(arcX, arcY));
                        arcY = sheet.MoveDown(arcY, step2*2);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step1);
                        arcY = sheet.MoveDown(arcY, step2);
                        points.Add(new Point(arcX, arcY));
                        break;
                    case Position.Right:
                        arcX = sheet.MoveRight(x, offset);
                        arcY = y;
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step1);
                        arcY = sheet.MoveDown(arcY, step2);
                        points.Add(new Point(arcX, arcY));
                        arcY = sheet.MoveDown(arcY, step2*2);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveLeft(arcX, step1);
                        arcY = sheet.MoveDown(arcY, step2);
                        points.Add(new Point(arcX, arcY));
                        break;
                    case Position.Top:
                        arcX = x;
                        arcY = sheet.MoveUp(y, offset);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step2);
                        arcY = sheet.MoveUp(arcY, step1);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step2*2);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step2);
                        arcY = sheet.MoveDown(arcY, step1);
                        points.Add(new Point(arcX, arcY));
                        break;
                    case Position.Bottom:
                        arcX = x;
                        arcY = sheet.MoveDown(y, offset);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step2);
                        arcY = sheet.MoveDown(arcY, step1);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step2*2);
                        points.Add(new Point(arcX, arcY));
                        arcX = sheet.MoveRight(arcX, step2);
                        arcY = sheet.MoveUp(arcY, step1);
                        points.Add(new Point(arcX, arcY));
                        break;
                }
                graph.CreateCurve(sheetId, points);
            }
        }
    }
}
