using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GangsDrive
{
    interface IGangsDriver
    {
        string DriverName { get; }
        string MountPoint { get; }
        bool IsMounted { get; }
        event EventHandler<connector.MountChangedArgs> OnMountChangedEvent;

        void Mount();
        void ClearMountPoint();
    }
}
