using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace ZeldaFullEditor
{
    class Save
    {

        //ROM.DATA is a base rom loaded to get basic information it can either be JP1.0 or US1.2
        //can still use it for load but must not be used 
        public int newHeaderPos = 0x122000;
        Room[] all_rooms;
        Entrance[] entrances;
        string[] texts;
        string debugstring = "";
        public Save(Room[] all_rooms, Entrance[] entrances, string[] texts)
        {
            this.all_rooms = all_rooms;
            this.entrances = entrances;
            this.texts = texts;
            //TODO : Change Header location to be dynamic instead of static

            if (!Directory.Exists("ProjectDirectory"))
            {
                Directory.CreateDirectory("ProjectDirectory");
            }
            //ZipArchive zipfile = new ZipArchive(new FileStream("PROJECTFILE.zip", FileMode.Open), ZipArchiveMode.Create);
            File.WriteAllText("ProjectDirectory//Main.cfg", writeProjectConfig());
            writeRooms("ProjectDirectory//");
            writeText("ProjectDirectory//");
            writePalettes("ProjectDirectory//");
            writeGfx("ProjectDirectory//");
            writeOverworldTiles16("ProjectDirectory//");
            writeOverworldMaps("ProjectDirectory//");
            writeOverworldConfig("ProjectDirectory//");
            writeOverworldGroups("ProjectDirectory//");
            //GFX 
            //GFX.singleGrayscaletobmp()
            

            //*****Starting Equipment******
            //save the array of byte


            //Remaining stuff to save : Gfx, Starting Equipment, Misc


        }

        public void writeOverworldGroups(string path)
        {
            if (!Directory.Exists(path + "Overworld"))
            {
                Directory.CreateDirectory(path + "Overworld");
            }

            byte[] owblocksetgroups = new byte[80*4];
            for(int i = 0;i<80*4;i++)
            {
                owblocksetgroups[i] = ROM.DATA[Constants.overworldgfxGroups + i];
            }
            File.WriteAllText(path + "Overworld//BlocksetGroups.json", JsonConvert.SerializeObject(owblocksetgroups));
        }

        public void writeOverworldConfig(string path)
        {
            if (!Directory.Exists(path + "Overworld"))
            {
                Directory.CreateDirectory(path + "Overworld");
            }
            OverworldConfig c = new OverworldConfig();
            File.WriteAllText(path + "Overworld//Config.json", JsonConvert.SerializeObject(c));



            byte[] owpalettesgroups = new byte[0xA6];
            for(int i = 0; i< 0xA6;i++)
            {
                owpalettesgroups[i] = ROM.DATA[Constants.overworldMapPaletteGroup + i];
            }

            File.WriteAllText(path + "Overworld//PalettesGroups.json", JsonConvert.SerializeObject(owpalettesgroups));
        }

        public void writeOverworldTiles16(string path)
        {
            if (!Directory.Exists(path + "Overworld"))
            {
                Directory.CreateDirectory(path + "Overworld");
            }
            File.WriteAllText(path + "Overworld//Tiles16.json", JsonConvert.SerializeObject(GFX.tiles16));
        }
        public void writeOverworldMaps(string path)
        {
            if (!Directory.Exists(path + "Overworld"))
            {
                Directory.CreateDirectory(path + "Overworld");
            }
            if (!Directory.Exists(path + "Overworld//Maps"))
            {
                Directory.CreateDirectory(path + "Overworld//Maps");
            }
            for (int i = 0; i < 160; i++)
            {
                MapSave ms = new MapSave((short)i);
                File.WriteAllText(path + "Overworld//Maps//Map" + i.ToString("D3") + ".json", JsonConvert.SerializeObject(ms));
            }
        }

        public void writeGfx(string path)
        {
            if (!Directory.Exists(path + "Graphics"))
            {
                Directory.CreateDirectory(path + "Graphics");
            }
            if (!Directory.Exists(path + "Graphics//Tilesets 3bpp"))
            {
                Directory.CreateDirectory(path + "Graphics//Tilesets 3bpp");
            }
            if (!Directory.Exists(path + "Graphics//Hud 2bpp"))
            {
                Directory.CreateDirectory(path + "Graphics//Hud 2bpp");
            }
            if (!Directory.Exists(path + "Graphics//Sprites 3bpp"))
            {
                Directory.CreateDirectory(path + "Graphics//Sprites 3bpp");
            }
            for (int i = 0; i < 113; i++)
            {
                GFX.singleGrayscaletobmp(i).Save(path + "Graphics//Tilesets 3bpp//blockset"+i.ToString("D3")+".png");
            }
            for (int i = 113; i < 115; i++)
            {
                GFX.singleGrayscaletobmp(i).Save(path + "Graphics//Hud 2bpp//blockset" + i.ToString("D3") + ".png");
            }
            for (int i = 115; i < 218; i++)
            {
                GFX.singleGrayscaletobmp(i).Save(path + "Graphics//Sprites 3bpp//blockset" + i.ToString("D3") + ".png");
            }
            for (int i = 218; i < 223; i++)
            {
                GFX.singleGrayscaletobmp(i).Save(path + "Graphics//Hud 2bpp//blockset" + i.ToString("D3") + ".png");
            }
        }

        public void writeText(string path)
        {
            if (!Directory.Exists(path + "Texts"))
            {
                Directory.CreateDirectory(path + "Texts");
            }

            TextSave ts = new TextSave(TextData.messages);
            File.WriteAllText(path + "Texts//AllTexts.json", JsonConvert.SerializeObject(ts));
        }



        public void writePalettes(string path)
        {
            //save them into yy-chr format

            //:thinking:


            //Separating palettes

            //DD218-DD290 lightworld sprites palettes (15*4)

            writePalette(0xDD218, 15, 4, path, "Sprites Palettes", "Lightworld Sprites");

            //DD291-DD308 darkworld sprites palettes (15*4)

            writePalette(0xDD290, 15, 4, path, "Sprites Palettes", "Darkworld Sprites");

            //DD309-DD39D Armors Palettes (15*5)
            writePalette(0xDD308, 15, 5, path, "Link Palettes", "Mails");

            //DD39E-DD445 Spr Aux Palettes? (7*12)
            writePalette(0xDD39E, 7, 12, path, "Sprites Palettes", "Aux Sprites1");

            //DD446-DD4DF Spr Aux2 Palettes? (7*11)
            writePalette(0xDD446, 7, 11, path, "Sprites Palettes", "Aux Sprites2");

            //DD4E0-DD62F Spr Aux Palettes? (7*24)
            writePalette(0xDD4E0, 7, 24, path, "Sprites Palettes", "Aux Sprites3");

            //DD630-DD647 Sword Palettes (3*4)
            writePalette(0xDD39E, 3, 4, path, "Link Palettes", "Sword Sprites");

            //DD648-DD65F Shield Palettes (4*3)
            writePalette(0xDD648, 4, 3, path, "Link Palettes", "Shield Sprites");

            //DD660-DD69F Hud Palettes (4*8)
            writePalette(0xDD660, 4, 8, path, "Hud Palettes", "Hud1");

            //DD6A0-DD6DF Hud Palettes2 (4*8)
            writePalette(0xDD6A0, 4, 8, path, "Hud Palettes", "Hud2");

            //DD6E0-DD709 Unused Palettes (7*3) ?
            writePalette(0xDD6E0, 7, 3, path, "Unused Palettes", "Unused");

            //DD70A-DD733 Map Sprites Palettes (7*3)
            writePalette(0xDD70A, 7, 3, path, "Dungeon Map Palette", "Map Sprite");

            //DD734 Dungeons Palettes :scream: (15*6) * 19
            for (int i = 0; i < 19; i++)
            {
                writePalette(0xDD734 + (i * 180), 15, 6, path, "Dungeon Palette", "Dungeon "+i.ToString("D2"));
            }
            //DE544-DE603 Map bg palette (15*6)
            writePalette(0xDE544, 15, 6, path, "Dungeon Map Palette", "Map Bg");

            //DE604-DE6C7 overworld Aux Palettes (7*14)
            writePalette(0xDE604, 7, 14, path, "Overworld Palette", "Overworld Animated");

            //DE6C8 Main Overworld Palettes (7*5) * 6
            for (int i = 0; i < 6; i++)
            {
                writePalette(0xDE6C8+ (i * 70), 7, 5, path, "Overworld Palette", "Main Overworld " + i.ToString("D2"));
            }

            //DE86C Overworld Aux Palettes (7*3) * 20
            for (int i = 0; i < 20; i++)
            {
                writePalette(0xDE86C + (i * 42), 7, 3, path, "Overworld Palette", "Overworld Aux2 " + i.ToString("D2"));
            }


            //save them in .png format 8x8 for each colors



        }

        public void writePalette(int palettePos,int w, int h, string path, string dir,string name)
        {

            if (!Directory.Exists(path+ "Palettes//" + dir))
            {
                Directory.CreateDirectory(path + "Palettes//" + dir);
            }


            //Bitmap paletteBitmap = new Bitmap(w * 8, h * 8);
            Color[] palettes = new Color[h*w];
            int pos = palettePos;
            int ppos = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {

                    palettes[ppos] = GFX.getColor((short)((ROM.DATA[pos + 1] << 8) + ROM.DATA[pos]));
                    //Graphics g = Graphics.FromImage(paletteBitmap);
                    //g.FillRectangle(new SolidBrush(c), new Rectangle(x * 8, y * 8, 8, 8));
                    pos += 2;
                    ppos++;
                }
            }
            File.WriteAllText(path + "Palettes//" + dir + "//" + name + ".json", JsonConvert.SerializeObject(palettes));
            /*
            //path = ProjectDirectory//
            paletteBitmap.Save(path + "Palettes//" + dir + "//" + name + ".png");

            paletteBitmap.Dispose();*/


        }

        public string writeProjectConfig()
        {
            configSave cs = new configSave();
            return JsonConvert.SerializeObject(cs);
        }

        public void writeEntrances(ZipArchive zipfile)
        {

            for (int i = 0; i < 0x84; i++) // Entrances
            {
                ZipArchiveEntry entry = zipfile.CreateEntry("Entrances\\Entrance" + i.ToString("D3") + ".zen");
                using (BinaryWriter bw = new BinaryWriter(entry.Open()))
                {
                    bw.Write(entrances[i].Room); //short room id
                    bw.Write(entrances[i].YPosition);//short Y Position *NOT FINAL*
                    bw.Write(entrances[i].XPosition);//short X Position *NOT FINAL*
                    bw.Write(entrances[i].XScroll);//short X Scroll *NOT FINAL*
                    bw.Write(entrances[i].YScroll);//short Y Scroll *NOT FINAL*
                    bw.Write(entrances[i].XCamera);//short XCAM Position *NOT FINAL*
                    bw.Write(entrances[i].YCamera);//short YCAM Position *NOT FINAL*
                    bw.Write(entrances[i].Blockset);//byte blockset
                    bw.Write(entrances[i].Music);//byte music
                    bw.Write(entrances[i].Dungeon);//byte dungeon id
                    //bw.Write(entrances[i].door);//MISSING DATA?
                    bw.Write(entrances[i].Floor);//byte Floor
                    bw.Write(entrances[i].Ladderbg);//byte ladderbg
                    bw.Write(entrances[i].Scrolling);//byte scrolling (bitwise stuff)
                    bw.Write(entrances[i].Scrollquadrant);//byte scrollingquadrant
                    //TODO : **IMPORTANT** ADD MISSING STUFF
                    //**MISSING LOTS OF STUFF**//

                    bw.Close();
                }
            }
        }

        public void writeRooms(string path)
        {
            if (!Directory.Exists(path+"Rooms"))
            {
                Directory.CreateDirectory(path + "Rooms");
            }
            for (int i = 0; i < 296; i++)
            {
                roomSave rs = new roomSave((short)i, all_rooms);
                File.WriteAllText(path + "Rooms//Room "+i.ToString("D3")+".json", JsonConvert.SerializeObject(rs,Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

            }
        }



        public void writeTiles(BinaryWriter bw, int i)
        {
            debugstring += "----------------------------------------------------------------\n";
            debugstring += "TILES OBJECTS                                                   \n";
            debugstring += "----------------------------------------------------------------\n";
            bw.Write((short)all_rooms[i].tilesObjects.Count);
            debugstring += "Object Count : " + all_rooms[i].tilesObjects.Count.ToString() + "\n";

            for (int j = 0; j < all_rooms[i].tilesObjects.Count; j++)
            {
                //<Tiles Objects Data>
                //short ID ,byte X, byte Y, byte Layer

                bw.Write(all_rooms[i].tilesObjects[j].id);
                bw.Write(all_rooms[i].tilesObjects[j].x);
                bw.Write(all_rooms[i].tilesObjects[j].y);
                bw.Write(all_rooms[i].tilesObjects[j].size);
                bw.Write(all_rooms[i].tilesObjects[j].layer);
                bw.Write((byte)all_rooms[i].tilesObjects[j].options);

                debugstring += "ID: " + all_rooms[i].tilesObjects[j].id.ToString("X2") + ", X:" + all_rooms[i].tilesObjects[j].x.ToString() +
                ",Y:" + all_rooms[i].tilesObjects[j].y.ToString() + ",Size:" + all_rooms[i].tilesObjects[j].size + ",Layer:" + all_rooms[i].tilesObjects[j].layer + ",Options:" + (byte)all_rooms[i].tilesObjects[j].options + "\n";

            }
        }

        public void writeSprites(BinaryWriter bw, int i)
        {
            debugstring += "----------------------------------------------------------------\n";
            debugstring += "SPRITES OBJECTS                                                 \n";
            debugstring += "----------------------------------------------------------------\n";
            bw.Write((short)all_rooms[i].sprites.Count);
            debugstring += "Sprites Count : " + all_rooms[i].sprites.Count.ToString() + "\n";
            for (int j = 0; j < all_rooms[i].sprites.Count; j++)
            {
                //<Sprites Data>
                //byte ID ,byte X, byte Y, byte Layer, byte KeyDrop, byte overlord, byte subtype
                bw.Write(all_rooms[i].sprites[j].id);
                bw.Write(all_rooms[i].sprites[j].x);
                bw.Write(all_rooms[i].sprites[j].y);
                bw.Write(all_rooms[i].sprites[j].layer);
                bw.Write(all_rooms[i].sprites[j].keyDrop);
                bw.Write(all_rooms[i].sprites[j].overlord);
                bw.Write(all_rooms[i].sprites[j].subtype);
                debugstring += "ID: " + all_rooms[i].sprites[j].id.ToString() + ", X:" + all_rooms[i].sprites[j].x.ToString() +
    ",Y:" + all_rooms[i].sprites[j].y.ToString() + "Layer:" + all_rooms[i].sprites[j].layer + "Key:" + all_rooms[i].sprites[j].keyDrop +
    "Overlord:" + all_rooms[i].sprites[j].overlord + "Subtype:" + all_rooms[i].sprites[j].subtype + "\n";

            }
        }

        public void writeItems(BinaryWriter bw, int i)
        {
            debugstring += "----------------------------------------------------------------\n";
            debugstring += "ITEMS OBJECTS                                                   \n";
            debugstring += "----------------------------------------------------------------\n";
            bw.Write((short)all_rooms[i].pot_items.Count);
            debugstring += "Items Count : " + all_rooms[i].pot_items.Count.ToString() + "\n";
            for (int j = 0; j < all_rooms[i].pot_items.Count; j++)
            {
                //<Items Data>
                //byte ID ,byte X, byte Y, byte Layer
                bw.Write(all_rooms[i].pot_items[j].id);
                bw.Write(all_rooms[i].pot_items[j].x);
                bw.Write(all_rooms[i].pot_items[j].y);
                bw.Write(all_rooms[i].pot_items[j].bg2);
                debugstring += "ID: " + all_rooms[i].pot_items[j].id.ToString("X2") + ", X:" + all_rooms[i].pot_items[j].x.ToString() +
                ",Y:" + all_rooms[i].pot_items[j].y.ToString() + "BG2:" + all_rooms[i].pot_items[j].bg2 + "\n";

            }
        }

        public void writeChests(BinaryWriter bw, int i)
        {
            debugstring += "----------------------------------------------------------------\n";
            debugstring += "CHEST OBJECTS                                                   \n";
            debugstring += "----------------------------------------------------------------\n";
            bw.Write((short)all_rooms[i].chest_list.Count);
            debugstring += "Chest Count : " + all_rooms[i].chest_list.Count.ToString() + "\n";
            for (int j = 0; j < all_rooms[i].chest_list.Count; j++)
            {
                //<Items Data>
                //byte Item ID, bool isBigChest
                bw.Write(all_rooms[i].chest_list[j].item);
                bw.Write(all_rooms[i].chest_list[j].bigChest);
                debugstring += "ID: " + all_rooms[i].chest_list[j].item.ToString() + ", BigChest?:" + all_rooms[i].chest_list[j].bigChest.ToString() + "\n";
            }
        }

        public void writeHeader(BinaryWriter bw, int i)
        {
            debugstring += "----------------------------------------------------------------\n";
            debugstring += "ROOM HEADER                                                     \n";
            debugstring += "----------------------------------------------------------------\n";
            bw.Write((byte)((((byte)all_rooms[i].bg2 & 0x07) << 5) + (all_rooms[i].collision << 2) + (all_rooms[i].light == true ? 1 : 0)));
            debugstring += "BG2:" + ((byte)all_rooms[i].bg2 & 0x07).ToString() + ", Collision:" + (all_rooms[i].collision).ToString() + ", Light:" + (all_rooms[i].light == true ? 1 : 0).ToString() + "\n";
            bw.Write((byte)all_rooms[i].palette);
            debugstring += "Palette:" + ((byte)all_rooms[i].palette).ToString() + "\n";
            bw.Write((byte)all_rooms[i].blockset);
            debugstring += "Blockset:" + ((byte)all_rooms[i].blockset).ToString() + "\n";
            bw.Write((byte)all_rooms[i].spriteset);
            debugstring += "Spriteset:" + ((byte)all_rooms[i].spriteset).ToString() + "\n";
            bw.Write((byte)all_rooms[i].effect);
            debugstring += "Effect:" + ((byte)all_rooms[i].effect).ToString() + "\n";
            bw.Write((byte)all_rooms[i].tag1);
            debugstring += "Tag1:" + ((byte)all_rooms[i].tag1).ToString() + "\n";
            bw.Write((byte)all_rooms[i].tag2);
            debugstring += "Tag2:" + ((byte)all_rooms[i].tag2).ToString() + "\n";
            bw.Write((byte)((all_rooms[i].holewarp_plane) + (all_rooms[i].staircase_plane[0] << 2) + (all_rooms[i].staircase_plane[1] << 4) + (all_rooms[i].staircase_plane[2] << 6)));
            debugstring += "Planes: (Hole)" + all_rooms[i].holewarp_plane.ToString() + ",(Stairs):" + (all_rooms[i].staircase_plane[0].ToString()) + "," + (all_rooms[i].staircase_plane[1].ToString()) + "," + (all_rooms[i].staircase_plane[2].ToString()) + "," + (all_rooms[i].staircase_plane[3]) + "\n";
            bw.Write((byte)all_rooms[i].staircase_plane[3]);
            bw.Write((byte)all_rooms[i].holewarp);
            bw.Write((byte)(all_rooms[i].staircase_rooms[0]));
            bw.Write((byte)(all_rooms[i].staircase_rooms[1]));
            bw.Write((byte)(all_rooms[i].staircase_rooms[2]));
            bw.Write((byte)(all_rooms[i].staircase_rooms[3]));
            //missing 1byte?
            debugstring += "WarpRoom: (Hole)" + (all_rooms[i].holewarp).ToString() + ",(Stairs)" + (all_rooms[i].staircase_rooms[0]) + "," + (all_rooms[i].staircase_rooms[1]) + "," + (all_rooms[i].staircase_rooms[2]) + "," + (all_rooms[i].staircase_rooms[3]);
        }


        public bool saveEntrances(Entrance[] entrances)
        {
            for (int i = 0; i < 0x84; i++)
            {
                entrances[i].save(i);
            }

            return false;
        }

        public bool saveTexts(string[] texts, Charactertable table)
        {
            int pos = 0xE0000;
            for (int i = 0; i < 395; i++)
            {
                byte[] b = table.textToHex(texts[i]);
                for (int j = 0; j < b.Length; j++)
                {
                    ROM.DATA[pos] = b[j];
                    pos++;
                }
            }

            if (pos > 0xE7355)
            {
                return true;
            }
            return false;
        }

        public bool saveOWExits()
        {
            for (int i = 0; i < OverworldGlobal.exits.Count; i++)
            {
                ROM.DATA[Constants.OWExitXPlayer + (i * 2)] = (byte)(OverworldGlobal.exits[i].playerX & 0xFF);
                ROM.DATA[Constants.OWExitXPlayer + (i * 2) + 1] = (byte)((OverworldGlobal.exits[i].playerX >> 8) & 0xFF);
                ROM.DATA[Constants.OWExitYPlayer + (i * 2)] = (byte)(OverworldGlobal.exits[i].playerY & 0xFF);
                ROM.DATA[Constants.OWExitYPlayer + (i * 2) + 1] = (byte)((OverworldGlobal.exits[i].playerY >> 8) & 0xFF);


                ROM.DATA[Constants.OWExitXCamera + (i * 2)] = (byte)((OverworldGlobal.exits[i].playerX + 7) & 0xFF);
                ROM.DATA[Constants.OWExitXCamera + (i * 2) + 1] = (byte)(((OverworldGlobal.exits[i].playerX + 7) >> 8) & 0x3F);
                ROM.DATA[Constants.OWExitYCamera + (i * 2)] = (byte)((OverworldGlobal.exits[i].playerY + 31) & 0xFF);
                ROM.DATA[Constants.OWExitYCamera + (i * 2) + 1] = (byte)(((OverworldGlobal.exits[i].playerY + 31) >> 8) & 0x3F);

                ROM.DATA[Constants.OWExitXScroll + (i * 2)] = (byte)((OverworldGlobal.exits[i].playerX - 134) & 0xFF);
                ROM.DATA[Constants.OWExitXScroll + (i * 2) + 1] = (byte)(((OverworldGlobal.exits[i].playerX - 134) >> 8) & 0x3F);
                ROM.DATA[Constants.OWExitYScroll + (i * 2)] = (byte)((OverworldGlobal.exits[i].playerY - 78) & 0xFF);
                ROM.DATA[Constants.OWExitYScroll + (i * 2) + 1] = (byte)(((OverworldGlobal.exits[i].playerY - 78) >> 8) & 0x3F);

            }
            return false;
        }

        public bool saveRoomsHeaders()
        {
            //long??
            int headerPointer = (ROM.DATA[Constants.room_header_pointer + 2] << 16) + (ROM.DATA[Constants.room_header_pointer + 1] << 8) + (ROM.DATA[Constants.room_header_pointer]);
            headerPointer = Addresses.snestopc(headerPointer);
            //headerPointer = 04F1E2 topc -> 0271E2
            if (Constants.Rando)
            {
                //24
                ROM.DATA[Constants.room_header_pointers_bank] = 0x24;
                for (int i = 0; i < 296; i++)
                {
                    ROM.DATA[headerPointer + (i * 2)] = (byte)((Addresses.pctosnes(newHeaderPos + (i * 14)) & 0xFF));
                    ROM.DATA[headerPointer + (i * 2) + 1] = (byte)((Addresses.pctosnes(newHeaderPos + (i * 14)) >> 8) & 0xFF);
                    saveHeader(newHeaderPos, i);
                    //savetext
                    //(short)((ROM.DATA[Constants.messages_id_dungeon + (index * 2) + 1] << 8) + ROM.DATA[Constants.messages_id_dungeon + (index * 2)])
                    ROM.DATA[Constants.messages_id_dungeon + (i * 2)] = (byte)(all_rooms[i].messageid & 0xFF);
                    ROM.DATA[Constants.messages_id_dungeon + (i * 2) + 1] = (byte)((all_rooms[i].messageid >> 8) & 0xFF);

                }
            }
            else
            {
                ROM.DATA[Constants.room_header_pointers_bank] = 0x22;
                for (int i = 0; i < 296; i++)
                {
                    ROM.DATA[headerPointer + (i * 2)] = (byte)((Addresses.pctosnes(0x110000 + (i * 14)) & 0xFF));
                    ROM.DATA[headerPointer + (i * 2) + 1] = (byte)((Addresses.pctosnes(0x110000 + (i * 14)) >> 8) & 0xFF);
                    saveHeader(0x110000, i);
                }
            }
            return false; // False = no error

        }
        public int getLongPointerSnestoPc(int pos)
        {
            int p = (ROM.DATA[pos + 2] << 16) + (ROM.DATA[pos + 1] << 8) + (ROM.DATA[pos]);
            return (Addresses.snestopc(p));
        }

        public bool saveBlocks()
        {
            //if we reach 0x80 size jump to pointer2 etc...
            int[] region = new int[4] { Constants.blocks_pointer1, Constants.blocks_pointer2, Constants.blocks_pointer3, Constants.blocks_pointer4 };
            int blockCount = 0;
            int r = 0;
            int pos = getLongPointerSnestoPc(region[r]);
            int count = 0;
            for (int i = 0; i < 296; i++)
            {
                foreach (Room_Object o in all_rooms[i].tilesObjects)
                {
                    if ((o.options & ObjectOption.Block) == ObjectOption.Block) //if we find a block save it
                    {
                        ROM.DATA[pos] = (byte)((i & 0xFF));//b1
                        pos++;
                        ROM.DATA[pos] = (byte)(((i >> 8) & 0xFF));//b2
                        pos++;
                        int xy = (((o.y * 64) + o.x) << 1);
                        ROM.DATA[pos] = (byte)(xy & 0xFF);//b3
                        pos++;
                        ROM.DATA[pos] = (byte)((byte)(((xy >> 8) & 0x1F) + (o.layer * 0x20)));//b4
                        //((b4 & 0x20) >> 5)
                        pos++;

                        count += 4;
                        if (count >= 0x80)
                        {
                            r++;
                            pos = getLongPointerSnestoPc(region[r]);
                            count = 0;
                        }
                        blockCount++;
                    }

                }
            }
            if (blockCount > 99)
            {
                return true; // False = no error
            }
            /*if (b3 == 0xFF && b4 == 0xFF) { break; }
            int address = ((b4 & 0x1F) << 8 | b3) >> 1;
            int px = address % 64;
            int py = address >> 6;
            Room_Object r = addObject(0x0E00, (byte)(px), (byte)(py), 0, (byte)((b4 & 0x20) >> 5));*/
            return false; // False = no error
        }


        public bool saveTorches()
        {
            int bytes_count = (ROM.DATA[Constants.torches_length_pointer + 1] << 8) + ROM.DATA[Constants.torches_length_pointer];
            int pos = Constants.torch_data;

            for (int i = 0; i < 296; i++)
            {
                bool room = false;
                foreach (Room_Object o in all_rooms[i].tilesObjects)
                {
                    if ((o.options & ObjectOption.Torch) == ObjectOption.Torch) //if we find a torch
                    {
                        //if we find a torch then store room if it not stored

                        if (room == false)
                        {
                            ROM.DATA[pos] = (byte)((i & 0xFF));
                            pos++;
                            ROM.DATA[pos] = (byte)(((i >> 8) & 0xFF));
                            pos++;
                            room = true;
                        }

                        int xy = (((o.y * 64) + o.x) << 1);
                        ROM.DATA[pos] = (byte)(xy & 0xFF);
                        pos++;
                        ROM.DATA[pos] = (byte)((byte)(((xy >> 8) & 0xFF) + (o.layer * 0x80)));
                        pos++;

                    }
                }
                if (room == true)
                {
                    ROM.DATA[pos] = (byte)(0xFF);
                    pos++;
                    ROM.DATA[pos] = (byte)(0xFF);
                    pos++;
                }
            }

            if ((pos - Constants.torch_data) > bytes_count)
            {
                return true;
            }
            else
            {
                //(ROM.DATA[Constants.torches_length_pointer + 1] << 8) + ROM.DATA[Constants.torches_length_pointer]
                short npos = (short)(pos - Constants.torch_data);
                ROM.DATA[Constants.torches_length_pointer] = (byte)(npos & 0xFF);
                ROM.DATA[Constants.torches_length_pointer + 1] = (byte)((npos >> 8) & 0xFF);
                /*for(int i = (pos - Constants.torch_data);i < bytes_count;i++)
                {
                    ROM.DATA[i] = 0xFF;
                }*/
            }
            return false; // False = no error
        }


        public void saveHeader(int pos, int i)
        {
            ROM.DATA[pos + 0 + (i * 14)] = (byte)((((byte)all_rooms[i].bg2 & 0x07) << 5) + (all_rooms[i].collision << 2) + (all_rooms[i].light == true ? 1 : 0));
            ROM.DATA[pos + 1 + (i * 14)] = ((byte)all_rooms[i].palette);
            ROM.DATA[pos + 2 + (i * 14)] = ((byte)all_rooms[i].blockset);
            ROM.DATA[pos + 3 + (i * 14)] = ((byte)all_rooms[i].spriteset);
            ROM.DATA[pos + 4 + (i * 14)] = ((byte)all_rooms[i].effect);
            ROM.DATA[pos + 5 + (i * 14)] = ((byte)all_rooms[i].tag1);
            ROM.DATA[pos + 6 + (i * 14)] = ((byte)all_rooms[i].tag2);
            ROM.DATA[pos + 7 + (i * 14)] = (byte)((all_rooms[i].holewarp_plane) + (all_rooms[i].staircase_plane[0] << 2) + (all_rooms[i].staircase_plane[1] << 4) + (all_rooms[i].staircase_plane[2] << 6));
            ROM.DATA[pos + 8 + (i * 14)] = (byte)(all_rooms[i].staircase_plane[3]);
            ROM.DATA[pos + 9 + (i * 14)] = (byte)(all_rooms[i].holewarp);
            ROM.DATA[pos + 10 + (i * 14)] = (byte)(all_rooms[i].staircase_rooms[0]);
            ROM.DATA[pos + 11 + (i * 14)] = (byte)(all_rooms[i].staircase_rooms[1]);
            ROM.DATA[pos + 12 + (i * 14)] = (byte)(all_rooms[i].staircase_rooms[2]);
            ROM.DATA[pos + 13 + (i * 14)] = (byte)(all_rooms[i].staircase_rooms[3]);
        }


        public bool saveAllPits()
        {
            int pitCount = (ROM.DATA[Constants.pit_count] / 2);
            int pitPointer = (ROM.DATA[Constants.pit_pointer + 2] << 16) + (ROM.DATA[Constants.pit_pointer + 1] << 8) + (ROM.DATA[Constants.pit_pointer]);
            pitPointer = Addresses.snestopc(pitPointer);
            int pitCountNew = 0;
            for (int i = 0; i < 296; i++)
            {
                if (all_rooms[i].damagepit)
                {
                    ROM.DATA[pitPointer + 1] = (byte)(all_rooms[i].index >> 8);
                    ROM.DATA[pitPointer] = (byte)(all_rooms[i].index);
                    pitPointer += 2;
                    pitCountNew++;
                }
            }
            if (pitCountNew > pitCount)
            {
                return true;
            }
            return false;
        }



        int saddr = 0;
        public bool saveAllObjects()
        {
            var section1Index = Constants.room_objects_section1; //0x50004 to 0x5374F  //ON US 0x50000 to 0x53730
            var section2Index = Constants.room_objects_section2; //0xF878A to 0xFFFF7 //ON US 0xF8780 to 0xFFFFF
            var section3Index = Constants.room_objects_section3; //0x1EB90 to 0x1FFFF //ON US 0x1EB06 to 0x1FFFF
                                                                 // var section4Index = 0x121210; // 0x121210 to ????? expanded region. need to find max safe for rando roms

            //reorder room from bigger to lower

            for (int i = 0; i < 296; i++)
            {


                var roomBytes = all_rooms[i].getTilesBytes();
                int doorPos = roomBytes.Length - 2;



                if (roomBytes.Length < 10)
                {
                    saveObjectBytes(all_rooms[i].index, 0x50000, roomBytes, doorPos); //empty room pointer
                    continue;
                }
                while (true)
                {

                    if (doorPos >= 04)
                    {
                        if (roomBytes[doorPos] == 0xF0 && roomBytes[doorPos + 1] == 0xFF)
                        {
                            doorPos += 2;
                            break;
                        }
                        doorPos -= 2;
                    }
                    else
                    {
                        break;
                    }
                }
                

                if (section1Index + roomBytes.Length <= Constants.room_objects_section1_max) //0x50000 to 0x5374F
                {
                    // write the room
                    saveObjectBytes(all_rooms[i].index, section1Index, roomBytes, doorPos);
                    section1Index += roomBytes.Length;
                    continue;
                }
                else if (section2Index + roomBytes.Length <= Constants.room_objects_section2_max) //0xF878A to 0xFFFF7
                {
                    // write the room
                    saveObjectBytes(all_rooms[i].index, section2Index, roomBytes, doorPos);
                    section2Index += roomBytes.Length;
                    continue;
                }
                else if (section3Index + roomBytes.Length <= Constants.room_objects_section3_max) //0x1EB90 to 0x1FFFF
                {
                    // write the room
                    saveObjectBytes(all_rooms[i].index, section3Index, roomBytes, doorPos);
                    section3Index += roomBytes.Length;
                    continue;
                }
                else
                {
                    // ran out of space
                    // write the room
                    //saveObjectBytes(i, section4Index, roomBytes);
                    //section4Index += roomBytes.Length;

                    return true;

                    //move to EXPANDED region
                    //Console.WriteLine("Room " + i + " no more space jump to 0x121210");
                    //currentPos = 0x121210;
                    //MessageBox.Show("We are running out space in the original portion of the ROM next data will be writed to : 0x121210");
                }
            }
            return false; // False = no error
        }

        void saveObjectBytes(int roomId, int position, byte[] bytes, int doorOffset)
        {
            int objectPointer = (ROM.DATA[Constants.room_object_pointer + 2] << 16) + (ROM.DATA[Constants.room_object_pointer + 1] << 8) + (ROM.DATA[Constants.room_object_pointer]);
            objectPointer = Addresses.snestopc(objectPointer);
            saddr = Addresses.pctosnes(position);
            int daddr = Addresses.pctosnes(position + doorOffset);
            // update the index
            ROM.DATA[objectPointer + (roomId * 3)] = (byte)(saddr & 0xFF);
            ROM.DATA[objectPointer + (roomId * 3) + 1] = (byte)((saddr >> 8) & 0xFF);
            ROM.DATA[objectPointer + (roomId * 3) + 2] = (byte)((saddr >> 16) & 0xFF);

            ROM.DATA[Constants.doorPointers + (roomId * 3)] = (byte)(daddr & 0xFF);
            ROM.DATA[Constants.doorPointers + (roomId * 3) + 1] = (byte)((daddr >> 8) & 0xFF);
            ROM.DATA[Constants.doorPointers + (roomId * 3) + 2] = (byte)((daddr >> 16) & 0xFF);

            Array.Copy(bytes, 0, ROM.DATA, position, bytes.Length);
        }

        public void savePalettes()//room settings floor1, floor2, blockset, spriteset, palette
        {

        }

        public bool saveallChests()
        {
            int cpos = (ROM.DATA[Constants.chests_data_pointer1 + 2] << 16) + (ROM.DATA[Constants.chests_data_pointer1 + 1] << 8) + (ROM.DATA[Constants.chests_data_pointer1]);
            cpos = Addresses.snestopc(cpos);
            int chestCount = 0;

            for (int i = 0; i < 296; i++)
            {
                //number of possible chests
                foreach (Chest c in all_rooms[i].chest_list)
                {
                    ushort room_index = (ushort)i;
                    if (c.bigChest == true)
                    {
                        room_index += 0x8000;
                    }
                    ROM.DATA[cpos] = (byte)(room_index & 0xFF);
                    ROM.DATA[cpos + 1] = (byte)((room_index >> 8) & 0xFF);
                    ROM.DATA[cpos + 2] = (byte)(c.item);
                    cpos += 3;
                    chestCount++;
                }
            }
            Console.WriteLine("Nbr of chests : " + chestCount);
            if (chestCount > 168)
            {
                return true; // False = no error
            }
            return false; // False = no error
        }

        public bool saveallPots()
        {
            int pos = Constants.items_data_start + 2; //skip 2 FF FF that are empty pointer
            for (int i = 0; i < 296; i++)
            {
                if (all_rooms[i].pot_items.Count == 0)
                {
                    ROM.DATA[Constants.room_items_pointers + (i * 2)] = (byte)((Addresses.pctosnes(Constants.items_data_start) & 0xFF));
                    ROM.DATA[Constants.room_items_pointers + (i * 2) + 1] = (byte)((Addresses.pctosnes(Constants.items_data_start) >> 8) & 0xFF);
                    continue;
                }
                //pointer
                ROM.DATA[Constants.room_items_pointers + (i * 2)] = (byte)((Addresses.pctosnes(pos) & 0xFF));
                ROM.DATA[Constants.room_items_pointers + (i * 2) + 1] = (byte)((Addresses.pctosnes(pos) >> 8) & 0xFF);
                for (int j = 0; j < all_rooms[i].pot_items.Count; j++)
                {
                    if (all_rooms[i].pot_items[j].layer == 0)
                    {
                        all_rooms[i].pot_items[j].bg2 = false;
                    }
                    else
                    {
                        all_rooms[i].pot_items[j].bg2 = true;
                    }

                    int xy = (((all_rooms[i].pot_items[j].y * 64) + all_rooms[i].pot_items[j].x) << 1);
                    ROM.DATA[pos] = (byte)(xy & 0xFF);
                    pos++;
                    ROM.DATA[pos] = (byte)(((xy >> 8) & 0xFF) + (all_rooms[i].pot_items[j].bg2 == true ? 0x20 : 0x00));
                    pos++;
                    ROM.DATA[pos] = all_rooms[i].pot_items[j].id;
                    pos++;
                }
                ROM.DATA[pos] = 0xFF;
                pos++;
                ROM.DATA[pos] = 0xFF;
                pos++;
                if (pos > Constants.items_data_end)
                {
                    return true;
                }
            }
            return false; // False = no error

        }


        public bool saveallSprites()
        {

            byte[] sprites_buffer = new byte[0x1670];
            //empty room data = 0x250
            //start of data = 0x252
            try
            {
                int pos = 0x252;
                //set empty room
                sprites_buffer[0x250] = 0x00;
                sprites_buffer[0x251] = 0xFF;

                for (int i = 0; i < 296; i++)
                {
                    if (all_rooms[i].sprites.Count <= 0)
                    {
                        sprites_buffer[(i * 2)] = (byte)((Addresses.pctosnes(Constants.sprites_data_empty_room) & 0xFF));
                        sprites_buffer[(i * 2) + 1] = (byte)((Addresses.pctosnes(Constants.sprites_data_empty_room) >> 8) & 0xFF);
                    }
                    else
                    {
                        //pointer : 
                        sprites_buffer[(i * 2)] = (byte)((Addresses.pctosnes(Constants.sprites_data + (pos - 0x252)) & 0xFF));
                        sprites_buffer[(i * 2) + 1] = (byte)((Addresses.pctosnes(Constants.sprites_data + (pos - 0x252)) >> 8) & 0xFF);
                        //ROM.DATA[sprite_address] == 1 ? true : false;
                        sprites_buffer[pos] = (byte)(all_rooms[i].sortSprites == true ? 0x01 : 0x00);//Unknown byte??
                        pos++;
                        foreach (Sprite spr in all_rooms[i].sprites) //3bytes
                        {
                            byte b1 = (byte)((spr.layer << 7) + (spr.subtype << 5) + spr.y);
                            byte b2 = (byte)((spr.overlord << 5) + spr.x);
                            byte b3 = (byte)((spr.id));

                            sprites_buffer[pos] = b1;
                            pos++;
                            sprites_buffer[pos] = b2;
                            pos++;
                            sprites_buffer[pos] = b3;
                            pos++;

                            //if current sprite hold a key then save it before 
                            if (spr.keyDrop == 1)
                            {
                                byte bb1 = (byte)(0xFE);
                                byte bb2 = (byte)(0x00);
                                byte bb3 = (byte)(0xE4);

                                sprites_buffer[pos] = bb1;
                                pos++;
                                sprites_buffer[pos] = bb2;
                                pos++;
                                sprites_buffer[pos] = bb3;
                                pos++;
                            }
                            if (spr.keyDrop == 2)
                            {
                                byte bb1 = (byte)(0xFD);
                                byte bb2 = (byte)(0x00);
                                byte bb3 = (byte)(0xE4);

                                sprites_buffer[pos] = bb1;
                                pos++;
                                sprites_buffer[pos] = bb2;
                                pos++;
                                sprites_buffer[pos] = bb3;
                                pos++;
                            }
                        }
                        sprites_buffer[pos] = 0xFF;//End of sprites
                        pos++;
                    }
                }
                int spritePointer = (04 << 16) + (ROM.DATA[Constants.rooms_sprite_pointer + 1] << 8) + (ROM.DATA[Constants.rooms_sprite_pointer]);
                sprites_buffer.CopyTo(ROM.DATA, spritePointer);
            }
            catch (Exception e)
            {
                return true;
            }
            return false; // False = no error
        }


    }


    public class roomSpriteSave
    {
        public byte x, y, id;
        public byte layer = 0;
        public byte subtype = 0;
        public byte overlord = 0;
        public string name;
        public byte keyDrop = 0;

        public roomSpriteSave(Sprite o)
        {
            x = o.x;
            y = o.y;
            id = o.id;
            layer = o.layer;
            subtype = o.subtype;
            overlord = o.overlord;
            name = o.name;
            keyDrop = o.keyDrop;
        }

    }

    public class TextSave
    {
        public string[] all_texts;
        public TextSave(string[] all_texts)
        {
            this.all_texts = all_texts;
        }
    }



    //Rooms, Pots, Chests, Sprites, Headers, Done !
    public class roomPotSave
    {
        public byte x, y, id;
        public bool bg2 = false;

        public roomPotSave(PotItem o)
        {
            this.x = o.x;
            this.y = o.y;
            this.id = o.id;
            this.bg2 = o.bg2;
        }
    }

    public class roomObjectSave
    {
        public byte x, y;
        public byte size;
        public byte layer = 0;
        public short id;

        public roomObjectSave(Room_Object o)
        {
            this.x = o.x;
            this.y = o.y;
            this.size = o.size;
            this.id = o.id;
            this.layer = o.layer;
        }

    }

    public class doorSave
    {
        public byte door_pos = 0;
        public byte door_dir = 0;
        public byte door_type = 0;

        public doorSave(byte pos, byte dir, byte type)
        {
            this.door_pos = pos;
            this.door_dir = dir;
            this.door_type = type;
        }
    }


    public class roomSave
    {
        public int index;
        public byte layout;
        public byte floor1;
        public byte floor2;
        public byte blockset;
        public byte spriteset;
        public byte palette;
        public byte collision; //Need a better name for that
        public Background2 bg2;
        public byte effect;
        public TagKey tag1;
        public TagKey tag2;
        public byte holewarp;
        public byte holewarp_plane;
        public byte[] staircase_rooms = new byte[4];
        public byte[] staircase_plane = new byte[4];
        public bool light;
        public short messageid;
        public bool damagepit;
        public List<roomObjectSave> blocks = new List<roomObjectSave>();//BLOCKS THIS IS NOT THE SAME AS ROOM 
        public List<roomObjectSave> torches = new List<roomObjectSave>();
        public List<doorSave> doors = new List<doorSave>();
        public List<Chest> chest_list = new List<Chest>();
        public List<roomObjectSave> tilesObjects = new List<roomObjectSave>();
        public List<roomSpriteSave> sprites = new List<roomSpriteSave>();
        public List<roomPotSave> pot_items = new List<roomPotSave>();
        public bool sortSprites = false;

        public roomSave(short roomId, Room[] allrooms)
        {
            index = allrooms[roomId].index;
            layout = allrooms[roomId].layout;
            floor1 = allrooms[roomId].floor1;
            floor2 = allrooms[roomId].floor2;
            blockset = allrooms[roomId].blockset;
            spriteset = allrooms[roomId].spriteset;
            palette = allrooms[roomId].palette;
            collision = allrooms[roomId].collision;
            bg2 = allrooms[roomId].bg2;
            effect = allrooms[roomId].effect;
            tag1 = allrooms[roomId].tag1;
            tag2 = allrooms[roomId].tag2;
            holewarp = allrooms[roomId].holewarp;
            holewarp_plane = allrooms[roomId].holewarp_plane;
            staircase_rooms = allrooms[roomId].staircase_rooms;
            staircase_plane = allrooms[roomId].staircase_plane;
            light = allrooms[roomId].light;
            messageid = allrooms[roomId].messageid;
            damagepit = allrooms[roomId].damagepit;
            chest_list = allrooms[roomId].chest_list;
            foreach (Room_Object o in allrooms[roomId].tilesObjects)
            {

                if (o is object_door)
                {
                    doors.Add(new doorSave((o as object_door).door_pos, (o as object_door).door_dir, (o as object_door).door_type));
                }
                else if (o.id == 0xE00) //block
                {
                    blocks.Add(new roomObjectSave(o));
                }
                else if (o.id == 0x150)//torches
                {
                    torches.Add(new roomObjectSave(o));
                }
                else
                {
                    tilesObjects.Add(new roomObjectSave(o));
                }
            }
            foreach (PotItem o in allrooms[roomId].pot_items)
            {
                pot_items.Add(new roomPotSave(o));
            }
            foreach (Sprite o in allrooms[roomId].sprites)
            {
                sprites.Add(new roomSpriteSave(o));
            }
            sortSprites = allrooms[roomId].sortSprites;
    }

}

    public class configSave
    {
        public string ProjectName = "";
        public string ProjectVersion = "";
        public string[] allDungeons = new string[17];
        public DataRoom[] allrooms = new DataRoom[296];
        public string[] allMapsNames = new string[160];

        public configSave()
        {
            ProjectName = "Test Name";
            ProjectVersion = "V1.0";

            allDungeons = ROMStructure.dungeonsNames;
            DataRoom[] dr = ROMStructure.dungeonsRoomList
            .Where(x => x != null)
            .OrderBy(x => x.id)
            .Select(x => x) //?
            .ToArray();
            allrooms = dr;
        }


    }

    public class MapSave
    {
        public ushort[,] tiles = new ushort[32, 32]; //all map tiles (short values) 0 to 1024 from left to right
        public bool largeMap = false;
        public byte spriteset = 0;
        public short index = 0;
        public byte palette = 0;
        public byte sprite_palette = 0;
        public byte blockset = 0;
        public string name = "";

        public MapSave(short id)
        {
            this.index = id;
            this.palette = (byte)(ROM.DATA[Constants.overworldMapPalette + index] << 2);
            this.blockset = ROM.DATA[Constants.mapGfx + index];
            this.sprite_palette = (byte)(ROM.DATA[Constants.overworldSpritePalette + index]);
            if (index != 0x80)
            {
                if (index <= 150)
                {
                    if (ROM.DATA[Constants.overworldMapSize + (index & 0x3F)] != 0)
                    {
                        largeMap = true;
                    }
                }
            }
            this.spriteset = ROM.DATA[Constants.overworldSpriteset + index];
            this.name = ROMStructure.mapsNames[index];
            int t = index * 256;

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    tiles[(x * 2), (y * 2)] = Compression.map16tiles[t].tile0;
                    tiles[(x * 2) + 1, (y * 2)] = Compression.map16tiles[t].tile1;
                    tiles[(x * 2), (y * 2) + 1] = Compression.map16tiles[t].tile2;
                    tiles[(x * 2) + 1, (y * 2) + 1] = Compression.map16tiles[t].tile3;
                    t++;
                }
            }



        }
    }



    public class OverworldConfig
    {
        public Color hardCodedDWGrass;
        public Color hardCodedLWGrass;
        public Color hardCodedDMGrass;

        public OverworldConfig()
        {
            hardCodedDWGrass = GFX.getColor((short)((ROM.DATA[Constants.hardcodedGrassDW + 1] << 8) + ROM.DATA[Constants.hardcodedGrassDW]));
            hardCodedLWGrass = GFX.getColor((short)((ROM.DATA[Constants.hardcodedGrassLW + 1] << 8) + ROM.DATA[Constants.hardcodedGrassLW]));
            hardCodedDMGrass = GFX.getColor((short)((ROM.DATA[Constants.hardcodedGrassSpecial + 1] << 8) + ROM.DATA[Constants.hardcodedGrassSpecial]));
        }

    }

    public class PaletteConfig
    {

        byte[] owpalgroup1 = new byte[0xA6];

    }

    /*

    
*/
}
