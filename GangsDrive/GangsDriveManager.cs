using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GangsDrive
{
    class GangsDriverSet
    {
        GangsDriver driver;
        Thread thread;

        public GangsDriverSet(GangsDriver driver)
        {
            this.driver = driver;
            this.thread = null;
        }

        public void Mount()
        {
            if (!driver.IsMounted)
            {
                this.thread = new Thread(DriverThreadMethod);
                this.thread.Start();
            }
        }

        public void Unmount()
        {
            if(driver.IsMounted)
                this.driver.ClearMountPoint();
        }

        private void DriverThreadMethod()
        {
            try
            {
                driver.Mount();
            }
            catch(DokanException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }
    }

    class GangsDriveManager
    {
        private List<GangsDriverSet> driverSet;
        private int maxDriverSize;

        private static GangsDriveManager _instance;
        public static GangsDriveManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GangsDriveManager(5);

                return _instance;
            }
        }

        private GangsDriveManager(int maxDriverSize)
        {
            this.driverSet = new List<GangsDriverSet>();
            this.maxDriverSize = maxDriverSize;
        }

        public int AddDriver(GangsDriver driver)
        {
            if (this.driverSet.Count >= maxDriverSize)
                throw new IndexOutOfRangeException();

            GangsDriverSet tmp = new GangsDriverSet(driver);
            this.driverSet.Add(tmp);

            return this.driverSet.IndexOf(tmp);
        }

        public void RemoveDriver(int index)
        {
            if (index >= driverSet.Count)
                return;

            driverSet.RemoveAt(index);
        }

        public void MountDriver(int index)
        {
            if (index >= driverSet.Count)
                return;

            driverSet[index].Mount();
        }

        public void UnmountDriver(int index)
        {
            if (index >= driverSet.Count)
                return;

            driverSet[index].Unmount();
        }

        public void UnmountAllDriver()
        {
            foreach(var set in driverSet)
            {
                set.Unmount();
            }
        }
    }
}
