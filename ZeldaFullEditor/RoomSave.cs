using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZeldaFullEditor.Properties;

namespace ZeldaFullEditor
{

    public class RoomSave
    {
        private int header_location;
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
        public byte tag1;
        public byte tag2;
        public byte holewarp;
        public byte holewarp_plane;
        public byte[] staircase_rooms;
        public byte[] staircase_plane;
        public bool light;
        public short messageid;
        public bool damagepit;
        public List<Room_Blocks> blocks;
        public List<Room_Torches> torches;
        public List<DoorSave> doors;
        public List<ChestData> chest_list;
        public List<Room_Object> tilesObjects;
        public List<Room_Sprite> sprites;
        public List<roomPotSave> pot_items;
        public bool sortSprites;
        public string name;
        public RoomSave(short roomId)
        {
            staircase_rooms = new byte[4];
            staircase_plane = new byte[4];
            blocks = new List<Room_Blocks>();
            torches = new List<Room_Torches>();
            doors = new List<DoorSave>();
            chest_list = new List<ChestData>();
            tilesObjects = new List<Room_Object>();
            sprites = new List<Room_Sprite>();
            pot_items = new List<roomPotSave>();
            sortSprites = false;
            index = roomId;
            this.name = ROMStructure.roomsNames[index];
            messageid = (short)((ROM.DATA[Constants.messages_id_dungeon + (index * 2) + 1] << 8) + ROM.DATA[Constants.messages_id_dungeon + (index * 2)]);

            
            loadHeader();
            loadTilesObjects();
            addSprites();
            addBlocks();
            addTorches();
            addPotsItems();
            isdamagePit();

        }


        public void isdamagePit()
        {
            int pitCount = (ROM.DATA[Constants.pit_count] / 2);
            int pitPointer = (ROM.DATA[Constants.pit_pointer + 2] << 16) + (ROM.DATA[Constants.pit_pointer + 1] << 8) + (ROM.DATA[Constants.pit_pointer]);
            pitPointer = Addresses.snestopc(pitPointer);
            for (int i = 0; i < pitCount; i++)
            {
                if (((ROM.DATA[pitPointer + 1 + (i * 2)] << 8) + (ROM.DATA[pitPointer + (i * 2)])) == index)
                {
                    damagepit = true;
                    return;
                }
            }
        }

        public void addSprites()
        {
            int spritePointer = (04 << 16) + (ROM.DATA[Constants.rooms_sprite_pointer + 1] << 8) + (ROM.DATA[Constants.rooms_sprite_pointer]);
            //09 bank ? Need to check if HM change that
            int sprite_address_snes = (09 << 16) +
            (ROM.DATA[spritePointer + (index * 2) + 1] << 8) +
            ROM.DATA[spritePointer + (index * 2)];
            int sprite_address = Addresses.snestopc(sprite_address_snes);
            sortSprites = ROM.DATA[sprite_address] == 1 ? true : false;
            sprite_address += 1;
            while (true)
            {
                byte b1 = ROM.DATA[sprite_address];
                byte b2 = ROM.DATA[sprite_address + 1];
                byte b3 = ROM.DATA[sprite_address + 2];


                if (b1 == 0xFF) { break; }
                Room_Sprite s = new Room_Sprite(b3, (byte)(b2 & 0x1F), (byte)(b1 & 0x1F), Sprites_Names.name[b3], (byte)((b2 & 0xE0) >> 5), (byte)((b1 & 0x60) >> 5), (byte)((b1 & 0x80) >> 7), 0);
                sprites.Add(s);

                if (sprites.Count > 1)
                {
                    Room_Sprite spr = sprites[sprites.Count - 1];
                    Room_Sprite prevSprite = sprites[sprites.Count - 2];
                    if (spr.id == 0xE4 && spr.x == 0x00 && spr.y == 0x1E && spr.layer == 1 && ((spr.subtype << 3) + spr.overlord) == 0x18)
                    {
                        if (prevSprite != null)
                        {

                            prevSprite.keyDrop = 1;
                            sprites.RemoveAt(sprites.Count - 1);
                        }
                    }
                    if (spr.id == 0xE4 && spr.x == 0x00 && spr.y == 0x1D && spr.layer == 1 && ((spr.subtype << 3) + spr.overlord) == 0x18)
                    {
                        if (prevSprite != null)
                        {

                            prevSprite.keyDrop = 2;
                            sprites.RemoveAt(sprites.Count - 1);
                        }
                    }
                }
                sprite_address += 3;
            }
        }


        public void addlistBlock(ref byte[] blocksdata, int maxCount)
        {
            int pos1 = (ROM.DATA[Constants.blocks_pointer1 + 2] << 16) + (ROM.DATA[Constants.blocks_pointer1 + 1] << 8) + (ROM.DATA[Constants.blocks_pointer1]);
            pos1 = Addresses.snestopc(pos1);
            int pos2 = (ROM.DATA[Constants.blocks_pointer2 + 2] << 16) + (ROM.DATA[Constants.blocks_pointer2 + 1] << 8) + (ROM.DATA[Constants.blocks_pointer2]);
            pos2 = Addresses.snestopc(pos2);
            int pos3 = (ROM.DATA[Constants.blocks_pointer3 + 2] << 16) + (ROM.DATA[Constants.blocks_pointer3 + 1] << 8) + (ROM.DATA[Constants.blocks_pointer3]);
            pos3 = Addresses.snestopc(pos3);
            int pos4 = (ROM.DATA[Constants.blocks_pointer4 + 2] << 16) + (ROM.DATA[Constants.blocks_pointer4 + 1] << 8) + (ROM.DATA[Constants.blocks_pointer4]);
            pos4 = Addresses.snestopc(pos4);
            for (int i = 0; i < 0x80; i += 1)
            {
                blocksdata[i] = (ROM.DATA[i + pos1]);
                blocksdata[i + 0x80] = (ROM.DATA[i + pos2]);
                if (i + 0x100 < maxCount)
                {
                    blocksdata[i + 0x100] = (ROM.DATA[i + pos3]);
                }
                if (i + 0x180 < maxCount)
                {
                    blocksdata[i + 0x180] = (ROM.DATA[i + pos4]);
                }
            }
        }

        public void addBlocks()
        {
            //288

            int blocksCount = (short)((ROM.DATA[Constants.blocks_length + 1] << 8) + ROM.DATA[Constants.blocks_length]);
            byte[] blocksdata = new byte[blocksCount];
            //int blocksCount = (short)((ROM.DATA[Constants.blocks_length + 1] << 8) + ROM.DATA[Constants.blocks_length]);
            addlistBlock(ref blocksdata, blocksCount);
            for (int i = 0; i < blocksCount; i += 4)
            {
                byte b1 = blocksdata[i];
                byte b2 = blocksdata[i + 1];
                byte b3 = blocksdata[i + 2];
                byte b4 = blocksdata[i + 3];
                if (((b2 << 8) + b1) == index)
                {
                    if (b3 == 0xFF && b4 == 0xFF) { break; }
                    int address = ((b4 & 0x1F) << 8 | b3) >> 1;
                    int px = address % 64;
                    int py = address >> 6;
                    blocks.Add(new Room_Blocks(0x0E00, (byte)(px), (byte)(py), 0, (byte)((b4 & 0x20) >> 5)));
                }
            }
        }
        public void addTorches()
        {
            int bytes_count = (ROM.DATA[Constants.torches_length_pointer + 1] << 8) + ROM.DATA[Constants.torches_length_pointer];

            for (int i = 0; i < bytes_count; i += 2)
            {
                byte b1 = ROM.DATA[Constants.torch_data + i];
                byte b2 = ROM.DATA[Constants.torch_data + i + 1];
                if (b1 == 0xFF && b2 == 0xFF) { continue; }
                if (((b2 << 8) + b1) == index) // if roomindex = indexread
                {
                    i += 2;
                    while (true)
                    {

                        b1 = ROM.DATA[Constants.torch_data + i];
                        b2 = ROM.DATA[Constants.torch_data + i + 1];

                        if (b1 == 0xFF && b2 == 0xFF) { break; }
                        int address = ((b2 & 0x1F) << 8 | b1) >> 1;
                        int px = address % 64;
                        int py = address >> 6;


                        torches.Add(new Room_Torches(0x150, (byte)px, (byte)py, 0, (byte)((b2 & 0x20) >> 5)));
                        //tilesObjects[tilesObjects.Count - 1].is_torch = true;
                        i += 2;
                    }
                }
                else
                {
                    while (true)
                    {
                        b1 = ROM.DATA[Constants.torch_data + i];
                        b2 = ROM.DATA[Constants.torch_data + i + 1];
                        if (b1 == 0xFF && b2 == 0xFF) { break; }
                        i += 2;
                    }
                }
            }
        }


        public void addPotsItems()
        {
            int item_address_snes = (01 << 16) +
            (ROM.DATA[Constants.room_items_pointers + (index * 2) + 1] << 8) +
            ROM.DATA[Constants.room_items_pointers + (index * 2)];
            int item_address = Addresses.snestopc(item_address_snes);

            while (true)
            {
                byte b1 = ROM.DATA[item_address];
                byte b2 = ROM.DATA[item_address + 1];
                byte b3 = ROM.DATA[item_address + 2];
                //0x20 = bg2

                if (b1 == 0xFF && b2 == 0xFF) { break; }
                int address = ((b2 & 0x1F) << 8 | b1) >> 1;
                int px = address % 64;
                int py = address >> 6;
                roomPotSave p = new roomPotSave(b3, (byte)((px)), (byte)((py)), (b2 & 0x20) == 0x20 ? true : false);
                pot_items.Add(p);


                //bit 7 is set if the object is a special object holes, switches
                //after 0x16 it goes to 0x80

                item_address += 3;
            }
        }

        public void loadChests(ref List<ChestData> chests_in_room)
        {
            int cpos = (ROM.DATA[Constants.chests_data_pointer1 + 2] << 16) + (ROM.DATA[Constants.chests_data_pointer1 + 1] << 8) + (ROM.DATA[Constants.chests_data_pointer1]);
            cpos = Addresses.snestopc(cpos);
            int clength = (ROM.DATA[Constants.chests_length_pointer + 1] << 8) + (ROM.DATA[Constants.chests_length_pointer + 1]);

            for (int i = 0; i < clength; i++)
            {
                if ((((ROM.DATA[cpos + (i * 3) + 1] << 8) + (ROM.DATA[cpos + (i * 3)])) & 0x7FFF) == index)
                {
                    //there's a chest in that room !
                    bool big = false;
                    if ((((ROM.DATA[cpos + (i * 3) + 1] << 8) + (ROM.DATA[cpos + (i * 3)])) & 0x8000) == 0x8000) //????? 
                    {
                        big = true;
                    }
                    chests_in_room.Add(new ChestData(ROM.DATA[cpos + (i * 3) + 2], big));
                    //
                }
            }
        }



        public void loadTilesObjects(bool floor = true)
        {
            //adddress of the room objects
            int objectPointer = (ROM.DATA[Constants.room_object_pointer + 2] << 16) + (ROM.DATA[Constants.room_object_pointer + 1] << 8) + (ROM.DATA[Constants.room_object_pointer]);
            objectPointer = Addresses.snestopc(objectPointer);
            int room_address = objectPointer + (index * 3);
            int tile_address = (ROM.DATA[room_address + 2] << 16) +
                (ROM.DATA[room_address + 1] << 8) +
                ROM.DATA[room_address];

            int objects_location = Addresses.snestopc(tile_address);

            if (floor)
            {
                floor1 = (byte)(ROM.DATA[objects_location] & 0x0F);
                floor2 = (byte)((ROM.DATA[objects_location] >> 4) & 0x0F);
            }
            layout = (byte)((ROM.DATA[objects_location + 1] >> 2) & 0x07);

            List<ChestData> chests_in_room = new List<ChestData>();
            loadChests(ref chests_in_room);

            int pos = objects_location + 2;
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            byte posX = 0;
            byte posY = 0;
            byte sizeX = 0;
            byte sizeY = 0;
            byte sizeXY = 0;
            short oid = 0;
            int layer = 0;
            bool door = false;
            bool endRead = false;
            while (endRead == false)
            {

                b1 = ROM.DATA[pos];
                b2 = ROM.DATA[pos + 1];
                if (b1 == 0xFF && b2 == 0xFF)
                {
                    pos += 2; //we jump to layer2
                    layer++;
                    door = false;
                    if (layer == 3)
                    {
                        endRead = true;
                        break;
                    }
                    continue;
                }

                if (b1 == 0xF0 && b2 == 0xFF)
                {
                    pos += 2; //we jump to layer2
                    door = true;
                    continue;
                }
                b3 = ROM.DATA[pos + 2];
                if (door)
                {
                    pos += 2;

                }
                else
                {
                    pos += 3;
                }

                if (door == false)
                {
                    if (b3 >= 0xF8)
                    {
                        oid = (short)((b3 << 4) | 0x80 + (((b2 & 0x03) << 2) + ((b1 & 0x03))));
                        posX = (byte)((b1 & 0xFC) >> 2);
                        posY = (byte)((b2 & 0xFC) >> 2);
                        sizeXY = (byte)((((b1 & 0x03) << 2) + (b2 & 0x03)));
                    }
                    else //subtype1
                    {
                        oid = b3;
                        posX = (byte)((b1 & 0xFC) >> 2);
                        posY = (byte)((b2 & 0xFC) >> 2);
                        sizeX = (byte)((b1 & 0x03));
                        sizeY = (byte)((b2 & 0x03));
                        sizeXY = (byte)(((sizeX << 2) + sizeY));
                    }
                    if (b1 >= 0xFC) //subtype2 (not scalable? )
                    {
                        oid = (short)((b3 & 0x3F) + 0x100);
                        posX = (byte)(((b2 & 0xF0) >> 4) + ((b1 & 0x3) << 4));
                        posY = (byte)(((b2 & 0x0F) << 2) + ((b3 & 0xC0) >> 6));
                        sizeXY = 0;
                    }

                    tilesObjects.Add(new Room_Object(oid, posX, posY, sizeXY, (byte)layer));

                    //IF Object is a chest loaded and there's object in the list chest
                    if (oid == 0xF99)
                    {
                        if (chests_in_room.Count > 0)
                        {
                            chest_list.Add(new ChestData(chests_in_room[0].itemIn, false));
                            chests_in_room.RemoveAt(0);
                        }
                    }
                    else if (oid == 0xFB1)
                    {
                        if (chests_in_room.Count > 0)
                        {
                            chest_list.Add(new ChestData(chests_in_room[0].itemIn, true));
                            chests_in_room.RemoveAt(0);
                        }
                    }


                }
                else
                {
                    doors.Add(new DoorSave((short)((b2 << 8) + b1)));
                    continue;
                }



            }
        }


        public Room_Object addObject(short oid, byte x, byte y, byte size, byte layer)
        {
            return new Room_Object(oid, x, y, size, layer);
        }


        public void loadHeader()
        {
            //address of the room header
            int headerPointer = (ROM.DATA[Constants.room_header_pointer + 2] << 16) + (ROM.DATA[Constants.room_header_pointer + 1] << 8) + (ROM.DATA[Constants.room_header_pointer]);
            headerPointer = Addresses.snestopc(headerPointer);
            int address = (ROM.DATA[Constants.room_header_pointers_bank] << 16) +
                            (ROM.DATA[(headerPointer + 1) + (index * 2)] << 8) +
                            ROM.DATA[(headerPointer) + (index * 2)];

            header_location = Addresses.snestopc(address);

            bg2 = (byte)((ROM.DATA[header_location] >> 5) & 0x07);
            collision = (byte)((ROM.DATA[header_location] >> 2) & 0x07);
            light = (((ROM.DATA[header_location]) & 0x01) == 1 ? true : false);
            if (light)
            {
                bg2 = 00;
            }

            palette = (byte)((ROM.DATA[header_location + 1] & 0x3F));
            blockset = (byte)((ROM.DATA[header_location + 2]));
            spriteset = (byte)((ROM.DATA[header_location + 3]));
            effect = (byte)((ROM.DATA[header_location + 4]));
            tag1 = (byte)((ROM.DATA[header_location + 5]));
            tag2 = (byte)((ROM.DATA[header_location + 6]));

            holewarp_plane = (byte)((ROM.DATA[header_location + 7]) & 0x03);
            staircase_plane[0] = (byte)((ROM.DATA[header_location + 7] >> 2) & 0x03);
            staircase_plane[1] = (byte)((ROM.DATA[header_location + 7] >> 4) & 0x03);
            staircase_plane[2] = (byte)((ROM.DATA[header_location + 7] >> 6) & 0x03);
            staircase_plane[3] = (byte)((ROM.DATA[header_location + 8]) & 0x03);

            holewarp = (byte)((ROM.DATA[header_location + 9]));
            staircase_rooms[0] = (byte)((ROM.DATA[header_location + 10]));
            staircase_rooms[1] = (byte)((ROM.DATA[header_location + 11]));
            staircase_rooms[2] = (byte)((ROM.DATA[header_location + 12]));
            staircase_rooms[3] = (byte)((ROM.DATA[header_location + 13]));
        }


    }




}
