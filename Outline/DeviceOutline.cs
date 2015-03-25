using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceOutline
    {
        private double xMin, xMax, yMin, yMax;
        private int deviceId;
        private bool isMount;
        private int carrierId;

        public bool IsTerminal { get; private set; }

        public double Height
        {
            get
            {
                return yMax - yMin;
            }
        }

        public double Top
        {
            get
            {
                return yMax;
            }
        }

        public double Bottom
        {
            get
            {
                return yMin;
            }
        }

        public double Left
        {
            get
            {
                return xMin;
            }
        }

        public double Right
        {
            get
            {
                return xMax;
            }
        }

        public int CarrierId
        {
            get
            {
                return carrierId;
            }
        }

        public int DeviceId
        {
            get
            {
                return deviceId;
            }
        }

        public Point Center { get; private set; }

        public double Area { get; private set; }

        public bool HasPlacedSymbols { get; private set; }

        public bool IsMount
        {
            get
            {
                return isMount;
            }
        }

        public DeviceOutline(NormalDevice device, Outline outline, Symbol symbol, HashSet<int> electricSchemeSheetIds)
        {
            this.deviceId = device.Id;
            IsTerminal = device.IsTerminal();
            bool hasOutline = false;
            foreach (int outlineId in device.OutlineIds)
            {
                outline.Id = outlineId;
                hasOutline = true;
                if (outline.Type == OutlineType.NormalOutline) 
                    break;
            }
            if (!hasOutline)
                return;
            isMount = device.IsMount();
            carrierId = isMount ? 0 : device.CarrierId;
            HasPlacedSymbols = IsHasPlacedSymbols(device, symbol, electricSchemeSheetIds);
            List<Point> points = outline.GetPath();
            xMin = points.Min(p => p.X);
            xMax = points.Max(p => p.X);
            yMin = points.Min(p => p.Y);
            yMax = points.Max(p => p.Y);
            Center = new Point(Math.Round((xMin+xMax) / 2, 3), Math.Round((yMin + yMax) / 2,3));
            Area = (xMax - xMin) * (yMax - yMin);
        }

        private static bool IsHasPlacedSymbols(NormalDevice device, Symbol symbol, HashSet<int> electricSchemeSheetIds)
        { 
            List<int> symbolIds = device.GetSymbolIds(SymbolReturnParameter.Placed);
            if (symbolIds.Count == 0)
                return false;
            return symbolIds.Any(sId => { symbol.Id = sId; return electricSchemeSheetIds.Contains(symbol.SheetId); });
        }

        public bool Contains(DeviceOutline outline)
        { 
            return xMin <= outline.xMin && xMax >= outline.xMax && yMin <= outline.yMin && yMax >= outline.yMax;
        }

        /*public void SetOrientation(Dictionary<int, Orientation> mountOrientationById)
        {
            if (mountOrientationById.ContainsKey(carrierId))
                Orientation = mountOrientationById[carrierId] == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;  // на вертикальных рейках устроуства расположены горизонтально
            else
                Orientation = Orientation.Vertical; // по умолчанию вертикальное расположение
        }*/

        public override int GetHashCode()
        {
            return deviceId;
        }
    }
}
