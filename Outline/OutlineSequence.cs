using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MountingCommutationScheme
{
    public class OutlineSequence
    {
        private List<DeviceOutline> outlines;
        private double top, bottom, right, left;

        public Orientation Orientation { get; private set; }

        public List<DeviceOutline> Outlines
        {
            get
            {
                return outlines;
            }
        }

        /*public Point Center
        {
            get
            {
                return new Point((left + right) / 2, (top + bottom) / 2);
            }
        }*/

        public double Top
        {
            get
            {
                return top;
            }
        }

        public double Bottom
        {
            get
            {
                return bottom;
            }
        }

        public double Left
        {
            get
            {
                return left;
            }
        }

        public double Right
        {
            get
            {
                return right;
            }
        }

        public int Number { get; private set; }

        public OutlineSequence(Orientation orientation)
        {
            outlines = new List<DeviceOutline>();
            Orientation = orientation;
            top = double.MinValue;
            bottom = double.MaxValue; ;
            left = double.MinValue;
            right = double.MaxValue;
        }

        public OutlineSequence(Orientation orientation, DeviceOutline outline) : this(orientation)
        {
            AddOutline(outline);
        }

        public void AddOutline(DeviceOutline outline)
        {
            outlines.Add(outline);
            top = Math.Max(top, outline.Top);
            bottom = Math.Min(bottom, outline.Bottom);
            left = Math.Max(left, outline.Left);
            right = Math.Min(right, outline.Right);
        }

        public void Sort()
        {
            if (Orientation == Orientation.Horizontal)
                outlines.Sort(new DeviceOutlineHorizontalComparer());
            else
                outlines.Sort(new DeviceOutlineVerticalComparer());
        }
    }
}
