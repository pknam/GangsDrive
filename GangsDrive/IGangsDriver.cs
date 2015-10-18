using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GangsDrive
{
    interface IGangsDriver
    {
        string MountPoint { get; }
        bool IsMounted { get; }

        void Mount();
        void ClearMountPoint();
    }
}
