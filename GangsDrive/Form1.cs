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

        Mirror mirrorDriver;
        bool mirrorMounted;

        GangsISODriver isoDriver;
        bool isoMounted;

        public Form1()
        {
            mirrorMounted = false;
            isoMounted = false;

            thread = null;
            mirrorDriver = null;
            InitializeComponent();
        }

        private void createMirrorDrive()
        {
            try
            {
                mirrorDriver = new Mirror("C:");
                mirrorMounted = true;
                mirrorDriver.Mount("n:\\", DokanOptions.DebugMode, 5);

                MessageBox.Show("end");
            }
            catch (DokanException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void createISODrrive()
        {
            try
            {
                isoDriver = new GangsISODriver(@"D:\GangsBox_WebDAV\Installer\AcrobatPro11.iso");
                isoMounted = true;
                isoDriver.Mount("i:\\", DokanOptions.DebugMode, 5);

                MessageBox.Show("iso end");
            }
            catch(DokanException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (mirrorMounted)
                return;

            thread = new Thread(createMirrorDrive);
            thread.Start();
            MessageBox.Show("thread create");
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (mirrorMounted)
            {
                Dokan.RemoveMountPoint("n:\\");
                MessageBox.Show("unmounted");
                mirrorMounted = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isoMounted)
                return;

            thread = new Thread(createISODrrive);
            thread.Start();
            MessageBox.Show("thread create");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isoMounted)
            {
                Dokan.RemoveMountPoint("i:\\");
                MessageBox.Show("unmounted");
                isoMounted = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isoMounted)
            {
                Dokan.RemoveMountPoint("i:\\");
                MessageBox.Show("unmounted");
                isoMounted = false;
            }

            if (mirrorMounted)
            {
                Dokan.RemoveMountPoint("n:\\");
                MessageBox.Show("unmounted");
                mirrorMounted = false;
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
