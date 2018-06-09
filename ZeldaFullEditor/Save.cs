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
        MapSave[] all_maps;
        Entrance[] entrances;
        string[] texts;
        string debugstring = "";
        public Save(Room[] all_rooms, MapSave[] all_maps, Entrance[] entrances, string[] texts)
        {
            this.all_rooms = all_rooms;
            this.all_maps = all_maps;
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

        }

        public void writeOverworldGroups(string path)
        {
            if (!Directory.Exists(path + "Overworld"))
            {
                Directory.CreateDirectory(path + "Overworld");
            }

            byte[] owblocksetgroups = new byte[80 * 4];
            for (int i = 0; i < 80 * 4; i++)
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
            for (int i = 0; i < 0xA6; i++)
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
            for (int i = 0; i < 158; i++)
            {
                
                File.WriteAllText(path + "Overworld//Maps//Map" + i.ToString("D3") + ".json", JsonConvert.SerializeObject(all_maps[i]));
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
                GFX.singleGrayscaletobmp(i).Save(path + "Graphics//Tilesets 3bpp//blockset" + i.ToString("D3") + ".png");
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
                writePalette(0xDD734 + (i * 180), 15, 6, path, "Dungeon Palette", "Dungeon " + i.ToString("D2"));
            }
            //DE544-DE603 Map bg palette (15*6)
            writePalette(0xDE544, 15, 6, path, "Dungeon Map Palette", "Map Bg");

            //DE604-DE6C7 overworld Aux Palettes (7*14)
            writePalette(0xDE604, 7, 14, path, "Overworld Palette", "Overworld Animated");

            //DE6C8 Main Overworld Palettes (7*5) * 6
            for (int i = 0; i < 6; i++)
            {
                writePalette(0xDE6C8 + (i * 70), 7, 5, path, "Overworld Palette", "Main Overworld " + i.ToString("D2"));
            }

            //DE86C Overworld Aux Palettes (7*3) * 20
            for (int i = 0; i < 20; i++)
            {
                writePalette(0xDE86C + (i * 42), 7, 3, path, "Overworld Palette", "Overworld Aux2 " + i.ToString("D2"));
            }


            //save them in .png format 8x8 for each colors



        }

        public void writePalette(int palettePos, int w, int h, string path, string dir, string name)
        {

            if (!Directory.Exists(path + "Palettes//" + dir))
            {
                Directory.CreateDirectory(path + "Palettes//" + dir);
            }


            //Bitmap paletteBitmap = new Bitmap(w * 8, h * 8);
            Color[] palettes = new Color[h * w];
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

        public void writeRooms(string path)
        {
            if (!Directory.Exists(path + "Rooms"))
            {
                Directory.CreateDirectory(path + "Rooms");
            }
            for (int i = 0; i < 296; i++)
            {
                roomSave rs = new roomSave((short)i, all_rooms);
                File.WriteAllText(path + "Rooms//Room " + i.ToString("D3") + ".json", JsonConvert.SerializeObject(rs, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

            }
        }

        public int getLongPointerSnestoPc(int pos)
        {
            int p = (ROM.DATA[pos + 2] << 16) + (ROM.DATA[pos + 1] << 8) + (ROM.DATA[pos]);
            return (Addresses.snestopc(p));
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
        public byte bg2;
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
