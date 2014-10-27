using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceOutline
    {
        private double xMin, xMax, yMin, yMax;
        private int deviceId;

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

        public DeviceOutline(NormalDevice device, Outline outline, int deviceId, Func<List<int>, bool> hasPlacedSymbols)
        {
            this.deviceId = deviceId;
            if (deviceId == 400082)
                deviceId.ToString();
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
            HasPlacedSymbols = hasPlacedSymbols(device.GetSymbolIds(SymbolReturnParameter.Placed));
            List<Point> points = outline.GetPath();
            xMin = points.Min(p => p.X);
            xMax = points.Max(p => p.X);
            yMin = points.Min(p => p.Y);
            yMax = points.Max(p => p.Y);
            Center = new Point((xMin+xMax) / 2, (yMin + yMax) / 2);
            Area = (xMax - xMin) * (yMax - yMin);
        }

        public bool Contains(DeviceOutline outline)
        { 
            return xMin <= outline.xMin && xMax >= outline.xMax && yMin <= outline.yMin && yMax >= outline.yMax;
        }

        public double GetTop(Sheet sheet)
        {
            return sheet.IsAboveOrEqualTarget(yMin, yMax) ? yMax : yMin; 
        }

        public double GetBottom(Sheet sheet)
        {
            return sheet.IsUnderOrEqualTarget(yMin, yMax) ? yMax : yMin;
        }

        public override int GetHashCode()
        {
            return deviceId;
        }
    }
}
