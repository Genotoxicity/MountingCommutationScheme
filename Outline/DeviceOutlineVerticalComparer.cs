using System.Collections.Generic;

namespace MountingCommutationScheme
{
    public class DeviceOutlineVerticalComparer : IComparer<DeviceOutline>
    {

        public int Compare(DeviceOutline a, DeviceOutline b)
        {
            if (a.Center.Y == b.Center.Y)
                return 0;
            if (a.Center.Y > b.Center.Y)
                return -1;
            else
                return 1;
        }
    }
}
