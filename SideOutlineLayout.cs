using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class SideOutlineLayout
    {
        private List<OutlineSequence> rows;
        private List<OutlineSequence> columns;
        private DeviceOutline sideOutline;

        public string Name { get; private set; }

        public List<OutlineSequence> Rows
        {
            get
            {
                return rows;
            }
        }

        public List<OutlineSequence> Columns
        {
            get
            {
                return columns;
            }
        }

        public double Height { get; private set; }

        public double Left { get; private set; }

        public double Right { get; private set; }

        public double Top { get; private set; }

        public double Bottom { get; private set; }

        public double Width { get; private set; }

        private SideOutlineLayout(DeviceOutline sideOutline, IEnumerable<DeviceOutline> deviceOutlines)
        {
            this.sideOutline = sideOutline;
            SetRowsAndColumns(deviceOutlines);
            SetSizes();
        }

        private void SetRowsAndColumns(IEnumerable<DeviceOutline> deviceOutlines)
        {
            Dictionary<int, Orientation> orientationByMountId = GetMountOrientationById(deviceOutlines);
            Dictionary<int, int> mountIdByCarrierId = GetMountIdByCarrierId(deviceOutlines);
            IEnumerable<DeviceOutline> placedOutlines = deviceOutlines.Where(o => o.HasPlacedSymbols);
            Dictionary<int, OutlineSequence> sequenceByMountId = new Dictionary<int, OutlineSequence>();
            List<DeviceOutline> notMountedOutlines = new List<DeviceOutline>();
            foreach (DeviceOutline placedOutline in placedOutlines)
            {
                int carrierId = placedOutline.CarrierId;
                if (carrierId != 0 && mountIdByCarrierId.ContainsKey(carrierId))
                {
                    int mountId = mountIdByCarrierId[carrierId];
                    if (!sequenceByMountId.ContainsKey(mountId))
                        sequenceByMountId.Add(mountId, new OutlineSequence(orientationByMountId[mountId]));
                    sequenceByMountId[mountId].AddOutline(placedOutline);
                }
                else
                    notMountedOutlines.Add(placedOutline);
            }
            rows = new List<OutlineSequence>();
            columns = new List<OutlineSequence>();
            foreach (OutlineSequence sequence in sequenceByMountId.Values)
                if (sequence.Orientation == Orientation.Horizontal)
                    rows.Add(sequence);
                else
                    columns.Add(sequence);
            List<OutlineSequence> notMountedRows = GetSequencesFromNotMountedOutlinesByOrientation(notMountedOutlines, Orientation.Horizontal);
            notMountedRows.ForEach(r => rows.Add(r));
            IEnumerable<int> notMountedRowIds = notMountedRows.SelectMany(r => r.Outlines).Select(o => o.DeviceId);
            notMountedOutlines.RemoveAll(o => notMountedRowIds.Contains(o.DeviceId));
            List<OutlineSequence> notMountedColumns = GetSequencesFromNotMountedOutlinesByOrientation(notMountedOutlines, Orientation.Vertical);
            notMountedColumns.ForEach(c => columns.Add(c));
            IEnumerable<int> notMountedColumnIds = notMountedColumns.SelectMany(r => r.Outlines).Select(o => o.DeviceId);
            notMountedOutlines.RemoveAll(o => notMountedColumnIds.Contains(o.DeviceId));
            notMountedOutlines.ForEach(o => rows.Add(new OutlineSequence(Orientation.Horizontal, o)));
            rows.Sort(new OutlineSequenceComparer(Orientation.Horizontal));
            rows.ForEach(r => r.Sort());
            columns.Sort(new OutlineSequenceComparer(Orientation.Vertical));
            columns.ForEach(c => c.Sort());
        }

        public void SetSizes()
        {
            if (sideOutline != null)
            {
                Left = sideOutline.Left;
                Right = sideOutline.Right;
                Top = sideOutline.Top;
                Bottom = sideOutline.Bottom;
                Height = sideOutline.Height;
                
            }
            else
            {
                if (columns.Count == 0 && rows.Count > 0 )
                {
                    Left = rows.Min(r => r.Left);
                    Right = rows.Max(r => r.Right);
                    Top = rows.Max(r => r.Top);
                    Bottom = rows.Min(r => r.Bottom);
                }
                if (rows.Count == 0 && columns.Count>0)
                {
                    Left = columns.Min(r => r.Left);
                    Right = columns.Max(r => r.Right);
                    Top = columns.Max(r => r.Top);
                    Bottom = columns.Min(r => r.Bottom);
                }
                if (columns.Count > 0 && rows.Count > 0)
                {
                    Left = Math.Min(rows.Min(r => r.Left), columns.Min(c => c.Left));
                    Right = Math.Max(rows.Max(r => r.Right), columns.Max(c => c.Right));
                    Top = Math.Max(rows.Max(r => r.Top), columns.Max(c => c.Top));
                    Bottom = Math.Min(rows.Min(r => r.Bottom), columns.Min(c => c.Bottom));
                }
            }
            Height = Top - Bottom;
            Width = Right - Left;
        }

        private static bool IsOutlineInRow(DeviceOutline outline, OutlineSequence row)
        {
            return outline.Center.Y >= row.Bottom && outline.Center.Y <= row.Top;
        }

        private static bool IsOutlineInColumn(DeviceOutline outline, OutlineSequence column)
        {
            return outline.Center.X >= column.Left && outline.Center.X <= column.Right;
        }

        private static List<OutlineSequence> GetSequencesFromNotMountedOutlinesByOrientation(List<DeviceOutline> notMountedOutlines, Orientation orientation)
        {
            if (notMountedOutlines.Count == 0)
                return new List<OutlineSequence>(0);
            if (orientation==Orientation.Horizontal)
                notMountedOutlines.Sort(new DeviceOutlineVerticalComparer());
            else
                notMountedOutlines.Sort(new DeviceOutlineHorizontalComparer());
            OutlineSequence sequence = (orientation == Orientation.Horizontal) ? new OutlineSequence(Orientation.Horizontal) : new OutlineSequence(Orientation.Vertical);
            sequence.AddOutline(notMountedOutlines[0]);
            List<OutlineSequence> sequences = new List<OutlineSequence>() { sequence };
            for (int i = 1; i < notMountedOutlines.Count; i++)
            {
                DeviceOutline outline = notMountedOutlines[i];
                if ((orientation == Orientation.Horizontal && IsOutlineInRow(outline, sequence)) || (orientation == Orientation.Vertical && IsOutlineInColumn(outline, sequence)))
                    sequence.AddOutline(outline);
                else
                {
                    sequence = new OutlineSequence(Orientation.Horizontal);
                    sequence.AddOutline(outline);
                    sequences.Add(sequence);
                }
            }
            sequences.RemoveAll(s => s.Outlines.Count < 2);
            return sequences;
        }

        public static List<SideOutlineLayout> GetSideOutlinesLayout(ProjectObjects projectObjects, int panelSheetId)
        {
            List<DeviceOutline> deviceOutlines = GetDeviceOutlines(projectObjects, panelSheetId);
            Dictionary<int, Orientation> mountOrientationById = GetMountOrientationById(deviceOutlines); 
            Dictionary<DeviceOutline, List<DeviceOutline>> outlinesBySide = GetOutlinesBySide(deviceOutlines);
            HashSet<int> sideOutlineIds = new HashSet<int>();
            List<SideOutlineLayout> sideLayouts = new List<SideOutlineLayout>(outlinesBySide.Count);
            foreach(DeviceOutline sideOutline in outlinesBySide.Keys)
            {
                List<DeviceOutline> outlines = outlinesBySide[sideOutline];
                if (outlines.Count() > 0)
                    sideLayouts.Add(new SideOutlineLayout(sideOutline, outlines));
                sideOutlineIds.Add(sideOutline.DeviceId);
                outlines.ForEach(o => sideOutlineIds.Add(o.DeviceId));
            }
            IEnumerable<DeviceOutline> notSideOutlines = deviceOutlines.Where(o => !sideOutlineIds.Contains(o.DeviceId));
            if (notSideOutlines.Count() > 0)
                sideLayouts.Add(new SideOutlineLayout(null, notSideOutlines));
            Sort(sideLayouts);
            SetSideNames(projectObjects, sideLayouts);
            return sideLayouts;
        }

        private static Dictionary<int, int> GetMountIdByCarrierId(IEnumerable<DeviceOutline> deviceOutlines)
        {
            Dictionary<int, DeviceOutline> outlineByIds = new Dictionary<int, DeviceOutline>();
            foreach(DeviceOutline outline in deviceOutlines)
                outlineByIds.Add(outline.DeviceId, outline);
            Dictionary<int, int> mountIdByCarrierId = new Dictionary<int, int>();
            foreach (DeviceOutline outline in deviceOutlines)
            {
                int carrierId = outline.CarrierId;
                if (carrierId == 0 || mountIdByCarrierId.ContainsKey(carrierId))
                    continue;
                if (outline.IsMount)
                {
                    mountIdByCarrierId.Add(carrierId, carrierId);
                    continue;
                }
                int endMountId = GetEndMountId(carrierId, outlineByIds);
                if (endMountId != 0)
                    mountIdByCarrierId.Add(carrierId, endMountId);
            }
            return mountIdByCarrierId;
        }

        private static int GetEndMountId(int carrierId, Dictionary<int, DeviceOutline> outlineByIds)
        {
            int endMountId = 0;
            while (true)
                if (outlineByIds.ContainsKey(carrierId))
                {
                    DeviceOutline carrierOutline = outlineByIds[carrierId];
                    if (carrierOutline.IsMount)
                    {
                        endMountId = carrierId;
                        break;
                    }
                    else
                        carrierId = carrierOutline.CarrierId;
                }
                else
                    break;
            return endMountId;
        }

        private static Dictionary<int, Orientation> GetMountOrientationById(IEnumerable<DeviceOutline> deviceOutlines)
        {
            Dictionary<int, Orientation> mountOrientationById = new Dictionary<int, Orientation>();
            Dictionary<int, int> carrierIdById = new Dictionary<int, int>();
            foreach (DeviceOutline deviceOutline in deviceOutlines)
                if (deviceOutline.IsMount)
                {
                    Orientation orientation = (deviceOutline.Right - deviceOutline.Left) > (deviceOutline.Top - deviceOutline.Bottom) ? Orientation.Horizontal : Orientation.Vertical;
                    mountOrientationById.Add(deviceOutline.DeviceId, orientation);
                }
            return mountOrientationById;
        }

        private static List<DeviceOutline> GetDeviceOutlines(ProjectObjects projectObjects, int panelSheetId)
        {
            NormalDevice device = projectObjects.Device;
            Outline outline = projectObjects.Outline;
            Symbol symbol = projectObjects.Symbol;
            Sheet sheet = projectObjects.Sheet;
            sheet.Id = panelSheetId;
            List <DeviceOutline> deviceOutlines = new List<DeviceOutline>();
            foreach (int symbolId in sheet.SymbolIds)
            {
                device.Id = symbolId;
                deviceOutlines.Add(new DeviceOutline(device, outline, symbol, projectObjects.ElectricSheetIds));
            }
            return deviceOutlines;
        }

        private static Dictionary<DeviceOutline, List<DeviceOutline>> GetOutlinesBySide(List<DeviceOutline> deviceOutlines)
        {
            deviceOutlines.Sort((do1, do2) => -do1.Area.CompareTo(do2.Area));   // сортируем по площади, от больших к меньшим
            HashSet<int> includedIndexes = new HashSet<int>();
            Dictionary<DeviceOutline, List<DeviceOutline>> outlinesBySide = new Dictionary<DeviceOutline, List<DeviceOutline>>();
            int outlinesCount = deviceOutlines.Count;
            for (int i = 0; i < outlinesCount - 1; i++)
            {
                if (includedIndexes.Contains(i))
                    continue;
                DeviceOutline first = deviceOutlines[i];
                for (int j = i + 1; j < outlinesCount; j++) // меньшие площади по определению не включают в себя большие
                {
                    if (includedIndexes.Contains(j))
                        continue;
                    DeviceOutline second = deviceOutlines[j];
                    if (first.Contains(second))
                    {
                        if (!outlinesBySide.ContainsKey(first))
                            outlinesBySide.Add(first, new List<DeviceOutline>());
                        outlinesBySide[first].Add(second);
                        includedIndexes.Add(j);
                    }
                }
            }
            return outlinesBySide;
        }

        private static void Sort(List<SideOutlineLayout> sideLayouts)
        {
            sideLayouts.Sort((s1, s2) => { if (s1.Left < s2.Left) return -1; else return 1; }); // cортировка по горизонтали
        }

        private static void SetSideNames(ProjectObjects projectObjects, List<SideOutlineLayout> sideLayouts)
        {
            Device device = projectObjects.Device;
            List<int> doorIndexes = new List<int>();
            List<int> sidewallIndexes = new List<int>();
            for (int i=0; i<sideLayouts.Count; i++)
            {
                SideOutlineLayout side = sideLayouts[i];
                if (side.sideOutline == null)
                    side.Name = "Не определено";
                else
                {
                    device.Id = side.sideOutline.DeviceId;
                    string function = device.GetAttributeValue(Settings.FunctionAttribute);
                    SideType sideType = Settings.GetSideTypeByFunction(function);
                    side.Name = device.Name;
                    if (sideType == SideType.Door)
                    {
                        doorIndexes.Add(i);
                        side.Name = "Дверь " + side.Name;
                        continue;
                    }
                    if (sideType == SideType.Sidewall)
                    {
                        sidewallIndexes.Add(i);
                        side.Name = "Боковая стенка " + side.Name;
                        continue;
                    }
                    if (sideType == SideType.Panel)
                        side.Name = "Монтажная панель "+side.Name;
                }
            }
            if (doorIndexes.Count >= 2)
            {
                SideOutlineLayout side = sideLayouts[doorIndexes.First()];
                side.Name = "Левая д"+side.Name.Substring(1);
                side = sideLayouts[doorIndexes.Last()];
                side.Name = "Правая д" + side.Name.Substring(1);   
            }
            if (sidewallIndexes.Count >= 2)
            {
                SideOutlineLayout side = sideLayouts[sidewallIndexes.First()];
                side.Name = "Левая б" + side.Name.Substring(1);
                side = sideLayouts[sidewallIndexes.Last()];
                side.Name = "Правая б" + side.Name.Substring(1);
            }
        }

        private class OutlineSequenceComparer : IComparer<OutlineSequence>
        {
            private IComparer<DeviceOutline> outlineComparer;

            public OutlineSequenceComparer(Orientation orientation)
            { 
                if (orientation == Orientation.Horizontal)
                    outlineComparer = new DeviceOutlineVerticalComparer();
                else
                    outlineComparer = new DeviceOutlineHorizontalComparer();
            }

            public int Compare(OutlineSequence a, OutlineSequence b)
            {
                return outlineComparer.Compare(a.Outlines.First(), b.Outlines.First());
            }
        }

    }

}
