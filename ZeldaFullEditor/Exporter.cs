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
    public class Exporter
    {
        RichTextBox logTextbox;
        ProgressBar progressBar;
        Overworld overworld = new Overworld();
        byte[] romData;
        public Exporter(byte[] romData, ProgressBar progressBar, RichTextBox logTextbox)
        {
            this.logTextbox = logTextbox;
            this.progressBar = progressBar;
            this.romData = romData;
            ROM.DATA = romData;
            ROMStructure.loadDefaultProject();
            Export();
        }


        public RoomSave[] all_rooms = new RoomSave[296];
        public MapSave[] all_maps = new MapSave[158];
        public void Export()
        {
            CheckGameTitle();
            LoadDungeonsRooms();
            LoadOverworldTiles();
            LoadOverworldMaps();

            SaveJson s = new SaveJson(all_rooms, all_maps, null, TextData.messages,overworld);
        }

        public void LoadDungeonsRooms()
        {

            for (int i = 0; i < 296; i++)
            {
                try
                {
                    all_rooms[i] = new RoomSave((short)i);
                }
                catch (Exception e)
                {
                    writeLog("Error : " + e.Message.ToString(), Color.Red);
                    return;
                }
            }
            writeLog("All dungeon rooms data loaded properly : ", Color.Green);
        }

        public void LoadOverworldTiles()
        {
            

            try
            {
                GFX.gfxdata = Compression.DecompressTiles();
                overworld.AssembleMap16Tiles();
                overworld.AssembleMap32Tiles();

                overworld.DecompressAllMapTiles(); //need to change
                overworld.createMap32TilesFrom16(); //need to change
            }
            catch (Exception e)
            {
                writeLog("Error : " + e.Message.ToString(), Color.Red);
                return;
            }
            writeLog("Overworld tiles data loaded properly", Color.Green);
        }

        public void LoadOverworldMaps()
        {
            for (int i = 0; i < 158; i++)
            {
                try
                {
                    all_maps[i] = new MapSave((short)i,overworld);
                }
                catch (Exception e)
                {
                    writeLog("Error : " + e.Message.ToString(), Color.Red);
                    return;
                }
            }
            writeLog("Overworld maps data loaded properly", Color.Green);
        }



        public void CheckGameTitle()
        {
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
            progressBar.Value++;
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

        public bool compareBytes(int location, byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (romData[location + i] != array[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}
