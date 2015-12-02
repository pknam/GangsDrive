using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GangsDrive
{
    abstract class GangsDriver
    {
        protected readonly string _mountPoint;
        protected readonly string _driverName;
        protected bool _isMounted;
        public event EventHandler<connector.MountChangedArgs> OnMountChangedEvent;

        public GangsDriver(string _mountPoint, string _driverName)
        {
            this._mountPoint = _mountPoint;
            this._driverName = _driverName;
            this._isMounted = false;
        }

        public string MountPoint
        {
            get { return _mountPoint; }
        }

        public bool IsMounted
        {
            get { return _isMounted; }
        }

        public string DriverName
        {
            get { return _driverName; }
        }

        public virtual void Mount()
        {
            if (IsMounted)
                return;

            this._isMounted = true;
            OnMountChanged(new connector.MountChangedArgs(_isMounted));
            (this as IDokanOperations).Mount(this.MountPoint, DokanOptions.DebugMode, 5);
        }
        
        public virtual void ClearMountPoint()
        {
            if (!IsMounted)
                return;

            Dokan.RemoveMountPoint(this.MountPoint);
            this._isMounted = false;
            OnMountChanged(new connector.MountChangedArgs(_isMounted));
        }

        protected virtual void OnMountChanged(connector.MountChangedArgs e)
        {
            EventHandler<connector.MountChangedArgs> handler = this.OnMountChangedEvent;

            if (handler != null)
                handler(this, e);
        }
    }
}
