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
using Newtonsoft.Json;

namespace ZeldaFullEditor
{
    public class Importer
    {
        RichTextBox logTextbox;
        ProgressBar progressBar;
        Overworld overworld = new Overworld();
        byte[] romData;
        public Importer(byte[] romData, ProgressBar progressBar, RichTextBox logTextbox)
        {
            this.logTextbox = logTextbox;
            this.progressBar = progressBar;
            this.romData = romData;
            ROM.DATA = romData;
            ROMStructure.loadDefaultProject();
            Import();
        }

        public MapSave[] all_maps = new MapSave[160];
        public void Import()
        {
            all_maps = new MapSave[160];
            CheckGameTitle();
            LoadOverworldTiles();
            progressBar.Value = progressBar.Maximum;
            writeLog("All 'Overworld' data saved in ROM successfuly.", Color.Green, FontStyle.Bold);
        }

        public byte[] getLargeMaps()
        {
            List<byte> largemaps = new List<byte>();
            for (int i = 0; i < 64; i++)
            {
                if (i > 0)
                {
                    if (all_maps[i - 1].largeMap)
                    {
                        if (largemaps.Contains((byte)(i - 1)))
                        {
                            continue;
                        }
                    }
                }
                if (i > 7)
                {
                    if (all_maps[i - 8].largeMap)
                    {
                        if (largemaps.Contains((byte)(i - 8)))
                        {
                            continue;
                        }
                    }
                }
                if (i > 8)
                {
                    if (all_maps[i - 9].largeMap)
                    {
                        if (largemaps.Contains((byte)(i - 9)))
                        {
                            continue;
                        }
                    }
                }
                if (all_maps[i].largeMap)
                {
                    largemaps.Add((byte)i);
                }
            }

            return largemaps.ToArray();
        }


        public void LoadOverworldTiles()
        {

            overworld.AssembleMap16Tiles(true);
            
            for (int i = 0; i < 160; i++)
            {
                all_maps[i] = JsonConvert.DeserializeObject<MapSave>(File.ReadAllText("ProjectDirectory//Overworld//Maps//Map" + i.ToString("D3") + ".json"));
                
                overworld.AllMapTilesFromMap(i, all_maps[i].tiles);
                if (i == 159)
                {
                    string s = "";
                    int tpos = 0;
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            Tile32 map16 = new Tile32(all_maps[i].tiles[(x * 2), (y * 2)], all_maps[i].tiles[(x * 2) + 1, (y * 2)], all_maps[i].tiles[(x * 2), (y * 2) + 1], all_maps[i].tiles[(x * 2) + 1, (y * 2) + 1]);
                            s += "[" + map16.tile0.ToString("D4") + "," + map16.tile1.ToString("D4") + "," + map16.tile2.ToString("D4") + "," + map16.tile3.ToString("D4") + "] ";
                            tpos++;
                        }
                        s += "\r\n";
                    }
                    File.WriteAllText("TileDebug2.txt", s);
                }

            }
            byte[] largemaps = getLargeMaps();
            overworld.createMap32TilesFrom16();
            overworld.savemapstorom();

            try
            {



                //GFX.gfxdata = Compression.DecompressTiles();
                SaveFileDialog sf = new SaveFileDialog();
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sf.FileName, FileMode.OpenOrCreate,FileAccess.Write);
                    fs.Write(ROM.DATA, 0, ROM.DATA.Length);
                    fs.Close();
                }

            }
            catch (Exception e)
            {
                writeLog("Error : " + e.Message.ToString(), Color.Red);
                return;
            }
            writeLog("Overworld tiles data loaded properly", Color.Green);
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
