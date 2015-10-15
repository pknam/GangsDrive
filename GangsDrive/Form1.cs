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
        bool mounted;
        Thread thread;
        Mirror mirror;

        public Form1()
        {
            mounted = false;
            thread = null;
            mirror = null;
            InitializeComponent();
        }

        private void createMirrorDrive()
        {
            try
            {
                mirror = new Mirror("C:");
                mounted = true;
                mirror.Mount("n:\\", DokanOptions.DebugMode, 5);

                MessageBox.Show("end");
            }
            catch (DokanException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (mounted)
                return;

            thread = new Thread(createMirrorDrive);
            thread.Start();
            MessageBox.Show("thread create");
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (mounted)
            {
                //Dokan.Unmount('N');
                Dokan.RemoveMountPoint("n:\\");
                MessageBox.Show("unmounted");
                mounted = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FileStream isoStream = File.Open(@"D:\Tools\Forensic\deok9\dban.iso", FileMode.Open))
            {
                CDReader cd = new CDReader(isoStream, true);
                string[] files = cd.GetFiles(@"");
                
                Stream st = cd.OpenFile(@"ABOUT.TXT", FileMode.Open);
                byte[] buf = new byte[100];
                st.Seek(100, SeekOrigin.Begin);
                st.Read(buf, 0, 50);
                st.Close();

                string str = System.Text.Encoding.Default.GetString(buf);

                MessageBox.Show(str);

            }
        }
    }
}
