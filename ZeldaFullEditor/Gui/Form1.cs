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
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using ZeldaFullEditor.Properties;
using Microsoft.VisualBasic;
using System.IO.Compression;
using System.Runtime.InteropServices;


namespace ZeldaFullEditor
{

    public partial class zscreamForm : Form
    {
        public zscreamForm()
        {
            InitializeComponent();

        }

        public Room[] all_rooms = new Room[296];
        public MapSave[] all_maps = new MapSave[158];
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(of.FileName, FileMode.Open, FileAccess.Read);
                ROM.DATA = new byte[fs.Length];
                fs.Read(ROM.DATA, 0, (int)fs.Length);
                fs.Close();
            }
            //Scan ROM to get the version
            //Check if the rom is headered
            if ((ROM.DATA.Length & 0x200) == 0x200)
            {
                //Rom is headered, remove header
                byte[] temp = new byte[ROM.DATA.Length];
                ROM.DATA.CopyTo(temp,0);
                ROM.DATA = new byte[temp.Length - 0x200];
                for(int i = 0x200; i < temp.Length; i++)
                {
                    ROM.DATA[i - 0x200] = temp[i];
                }
                temp = null;
                writeLog("Header ROM detected", Color.Orange);
            }
            progressBar1.Value++;//1
            ROMStructure.loadDefaultProject();
            //simple title check

            //Search for "THE LEGEND OF ZELDA" - US 1.2
            if (compareBytes(0x7FC0, new byte[] { 0x54, 0x48, 0x45, 0x20, 0x4C, 0x45, 0x47, 0x45, 0x4E, 0x44, 0x20, 0x4F, 0x46, 0x20, 0x5A, 0x45, 0x4C, 0x44, 0x41 }))
            {
                //US 1.2 detected
                writeLog("Version Detected : US 1.2", Color.Green);
            }
            //Search for "ZELDANODENSETSU" - JP 1.0
            else if (compareBytes(0x7FC0, new byte[] { 0x5A, 0x45, 0x4C, 0x44, 0x41, 0x4E, 0x4F, 0x44, 0x45, 0x4E, 0x53, 0x45, 0x54, 0x53, 0x55 }))
            {
                //JP 1.0 detected
                writeLog("Version Detected : JP 1.0", Color.Green);
                Constants.Init_Jp(); //use JP Constants
            }
            else
            {
                //Unknown Title
                writeLog("Unknown Game Title : Using US 1.2 as default", Color.Orange);
            }
            progressBar1.Value++;//2

            //Extracting Room Data
            if (createAllRooms() == true)
            {
                writeLog("Critical Error happened when loading room data make sure your ROM version is supported!", Color.Red,FontStyle.Bold);
                return; //break the loading here useless to go further
            }

            GFX.gfxdata = Compression.DecompressTiles();
            GFX.AssembleMap16Tiles();
            GFX.AssembleMap32Tiles();
            progressBar1.Value++;//3

            if (createAllMaps() == true)
            {
                writeLog("Critical Error happened when loading maps data make sure your ROM version is supported!", Color.Red, FontStyle.Bold);
                return; //break the loading here useless to go further
            }
            progressBar1.Value++;//4

            try
            {
                OverworldGlobal.loadEntrances();
            }
            catch(Exception er)
            {
                writeLog("Error : " + er.Message.ToString(), Color.Red);
            }
            progressBar1.Value++;//5

            try
            {
                OverworldGlobal.loadExits();
            }
            catch (Exception er)
            {
                writeLog("Error : " + er.Message.ToString(), Color.Red);
            }
            progressBar1.Value++;//6

            try
            {
                OverworldGlobal.loadHoles();
            }
            catch (Exception er)
            {
                writeLog("Error : " + er.Message.ToString(), Color.Red);
            }
            progressBar1.Value++;//7

            try
            {
                OverworldGlobal.loadHoles();
            }
            catch (Exception er)
            {
                writeLog("Error : " + er.Message.ToString(), Color.Red);
            }
            progressBar1.Value++;//7



            if (createAllText() == true)
            {
                writeLog("Critical Error happened when loading texts data make sure your ROM version is supported!", Color.Red, FontStyle.Bold);
                return; //break the loading here useless to go further
            }
            progressBar1.Value++;

            Save s = new Save(all_rooms, all_maps, null, TextData.messages);
            progressBar1.Value++;

        }

        public bool createAllText()
        {
            bool error = false;
            try
            {
                TextData.readAllText();
            }
            catch (Exception e)
            {
                writeLog("Error : " + e.Message.ToString(), Color.Red);
                error = true;
            }
            if (error == false)
            {
                writeLog("All texts data loaded properly", Color.Green);
            }
            return error;
        }

        public bool createAllRooms()
        {
            bool error = false;
            for (int i = 0; i < 296; i++)
            {
                try
                {
                    all_rooms[i] = new Room(i);
                }
                catch(Exception e)
                {
                    writeLog("Error : "+e.Message.ToString(), Color.Red);
                    error = true;
                }
            }
            if (error == false)
            {
                writeLog("Underworld rooms data loaded properly", Color.Green);
            }
            return error;
        }

        public bool createAllMaps()
        {
            bool error = false;
            try
            {
                Compression.DecompressAllMapTiles();
                Compression.createMap32TilesFrom16();
                
            }
            catch (Exception e)
            {
                writeLog("Error : " + e.Message.ToString(), Color.Red);
                error = true;
                return true;
            }
            writeLog("Overworld tiles data loaded properly", Color.Green);

            for (int i = 0; i < 158; i++)
            {
                try
                {
                    all_maps[i] = new MapSave((short)i);
                }
                catch (Exception e)
                {
                    writeLog("Error : " + e.Message.ToString(), Color.Red);
                    error = true;
                }
            }
            if (error == false)
            {
                writeLog("Overworld maps data loaded properly", Color.Green);
            }
            return error;
        }

        public bool compareBytes(int location,byte[] array)
        {
            for(int i = 0;i<array.Length;i++)
            {
                if (ROM.DATA[location+i] != array[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void writeLog(string line, Color col,FontStyle fs = FontStyle.Regular)
        {
            Font f = new Font(richTextBox1.Font, fs);
            string text = line + "\r\n";
            richTextBox1.AppendText(text);
            richTextBox1.Select((richTextBox1.Text.Length - text.Length) + 1, text.Length);
            richTextBox1.SelectionColor = col;
            richTextBox1.SelectionFont = f;
            richTextBox1.Refresh();
        }

        
    }

    public class dataObject
    {
        public short id;
        public string Name { get; set; }
        
        public dataObject(short id, string name)
        {
            this.Name = name;
            this.id = id;
        }


    }

    [Serializable]
    public class object_door : Room_Object
    {
        public byte door_pos = 0;
        public byte door_dir = 0;
        public byte door_type = 0;
        public object_door(short id, byte x, byte y, byte size, byte layer) : base(id, x, y, size, layer)
        {
            door_pos = (byte)((id & 0xF0) >> 3);//*2
            door_dir = (byte)((id & 0x03));
            door_type = (byte)((id >> 8) & 0xFF);
            name = "Door";

        }

        public void updateId()
        {
            byte b1 = (byte)((door_pos << 3) + door_dir);
            byte b2 = door_type;
            id = (short)((b2 << 8) + b1);

        }
    }


    }
