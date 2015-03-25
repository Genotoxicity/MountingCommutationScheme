using System.Collections.Generic;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceOutlineHorizontalComparer : IComparer<DeviceOutline>
    {
        public int Compare(DeviceOutline a, DeviceOutline b)
        {
            if (a.Center.X == b.Center.X)
                return 0;
            if (a.Center.X < b.Center.X)
                return -1;
            else
                return 1;
        }
    }
}
