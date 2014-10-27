using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class DeviceOutlineOnSheetHorizontalComparer : IComparer<DeviceOutline>
    {
        private Sheet sheet;

        public DeviceOutlineOnSheetHorizontalComparer(Sheet sheet)
        {
            this.sheet = sheet;
        }

        public int Compare(DeviceOutline a, DeviceOutline b)
        {
            if (a.Center.X == b.Center.X)
                return 0;
            if (sheet.IsLeftOfTarget(b.Center.X, a.Center.X))
                return -1;
            else
                return 1;
        }
    }
}
