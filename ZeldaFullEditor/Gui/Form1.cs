using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace ZeldaFullEditor
{

    public partial class zscreamForm : Form
    {
        public zscreamForm()
        {
            InitializeComponent();

        }


        public Exporter exporter;
        public Importer importer;
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        byte[] romData;
        private void openROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(of.FileName, FileMode.Open, FileAccess.Read);
                byte[] temp = new byte[fs.Length];
                fs.Read(temp, 0, (int)fs.Length);
                fs.Close();
                romData = new byte[temp.Length];
                if ((temp.Length & 0x200) == 0x200)
                {
                    //Rom is headered, remove header
                    romData = new byte[temp.Length - 0x200];
                    for (int i = 0x200; i < temp.Length; i++)
                    {
                        romData[i - 0x200] = temp[i];
                    }
                    writeLog("Header ROM detected", Color.Orange);
                }
                else
                {
                    romData = (byte[])temp.Clone();
                }
                temp = null;
            }

            exporter = new Exporter(romData,progressBar1,logTextbox);

        }



        public void writeLog(string line, Color col, FontStyle fs = FontStyle.Regular)
        {
            Font f = new Font(logTextbox.Font, fs);
            string text = line + "\r\n";
            logTextbox.AppendText(text);
            logTextbox.Select((logTextbox.Text.Length - text.Length) + 1, text.Length);
            logTextbox.SelectionColor = col;
            logTextbox.SelectionFont = f;
            logTextbox.Refresh();
        }

        private void loadProjectToROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(of.FileName, FileMode.Open, FileAccess.Read);
                byte[] temp = new byte[fs.Length];
                fs.Read(temp, 0, (int)fs.Length);
                fs.Close();
                romData = new byte[temp.Length];
                if ((temp.Length & 0x200) == 0x200)
                {
                    //Rom is headered, remove header
                    romData = new byte[temp.Length - 0x200];
                    for (int i = 0x200; i < temp.Length; i++)
                    {
                        romData[i - 0x200] = temp[i];
                    }
                    writeLog("Header ROM detected", Color.Orange);
                }
                else
                {
                    romData = (byte[])temp.Clone();
                }
                temp = null;
            }

            importer = new Importer(romData, progressBar1, logTextbox);
        }
    }


 


    }
