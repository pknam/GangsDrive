using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DokanNet;
using System.Threading;
using DiscUtils;
using System.IO;
using DiscUtils.Iso9660;

namespace GangsDrive
{
    public partial class Form1 : Form
    {
        GangsDriveManager manager;
        int isoIndex;
        int sftpIndex;

        public Form1()
        {
            manager = GangsDriveManager.Instance;
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            manager.UnmountAllDriver();
        }

        private void btnIsoStart_Click(object sender, EventArgs e)
        {
            isoIndex = manager.AddDriver(new GangsISODriver(tbIsoPath.Text, "i:\\"));
            manager.MountDriver(isoIndex);
        }

        private void btnIsoStop_Click(object sender, EventArgs e)
        {
            manager.UnmountDriver(isoIndex);
        }

        private void btnSftpStart_Click(object sender, EventArgs e)
        {
            try
            {
                sftpIndex = manager.AddDriver(new GangsSFTPDriver(
                    tbSftpHost.Text,
                    Convert.ToInt32(tbSftpPort.Text),
                    tbSftpUsername.Text,
                    tbSftpPasswd.Text,
                    "s:\\"));
            }
            catch(FormatException)
            {
                MessageBox.Show("Invalid value : Port number");
                return;
            }
            manager.MountDriver(sftpIndex);
        }

        private void btnSftpStop_Click(object sender, EventArgs e)
        {
            manager.UnmountDriver(sftpIndex);
        }


    }
}
