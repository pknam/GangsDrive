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
        Thread thread;
        IGangsDriver isoDriver;

        public Form1()
        {
            thread = null;
            isoDriver = null;
            InitializeComponent();
        }
        private void createISODrrive()
        {
            try
            {
                isoDriver = new GangsISODriver(@"D:\GangsBox_WebDAV\Installer\AcrobatPro11.iso", "i:\\");
                isoDriver.Mount();

                MessageBox.Show("iso end");
            }
            catch(DokanException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isoDriver != null && isoDriver.IsMounted)
                return;

            thread = new Thread(createISODrrive);
            thread.Start();
            MessageBox.Show("thread create");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isoDriver.IsMounted)
            {
                isoDriver.ClearMountPoint();
                MessageBox.Show("unmounted");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isoDriver.IsMounted)
            {
                isoDriver.ClearMountPoint();
                MessageBox.Show("unmounted");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FileStream isoFileStream = File.Open(@"D:\GangsBox_WebDAV\Installer\AcrobatPro11.iso", FileMode.Open, System.IO.FileAccess.Read, FileShare.None);
            CDReader isoReader = new CDReader(isoFileStream, true);
            Stream tt = isoReader.OpenFile("ReadMe.htm", FileMode.Open);
            byte[] buf = new byte[100];
            tt.Position = 1;
            tt.Read(buf, 0, 90);
            MessageBox.Show(Encoding.Default.GetString(buf));

            tt.Close();
            isoReader.Dispose();
            isoFileStream.Close();
        }


    }
}
