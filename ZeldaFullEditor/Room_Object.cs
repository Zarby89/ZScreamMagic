using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ZeldaFullEditor
{
    public struct Room_Object
    {
        public byte x, y; //position of the object in the room (*8 for draw)
        public byte size; //size of the object
        public short id;
        public byte layer;

        public Room_Object(short id, byte x, byte y, byte size, byte layer = 0)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.id = id;
            this.layer = layer;
        }
    }

    public struct Room_Blocks
    {
        public byte x, y; //position of the object in the room (*8 for draw)
        public byte size; //size of the object
        public short id;
        public byte layer;

        public Room_Blocks(short id, byte x, byte y, byte size, byte layer = 0)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.id = id;
            this.layer = layer;
        }
    }

    public struct Room_Torches
    {
        public byte x, y; //position of the object in the room (*8 for draw)
        public byte size; //size of the object
        public short id;
        public byte layer;

        public Room_Torches(short id, byte x, byte y, byte size, byte layer = 0)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.id = id;
            this.layer = layer;
        }
    }

    public struct DoorSave
    {
        public byte door_pos;
        public byte door_dir;
        public byte door_type;

        public DoorSave(short id)
        {
            this.door_pos = (byte)((id & 0xF0) >> 3);//*2
            this.door_dir = (byte)((id & 0x03));
            this.door_type = (byte)((id >> 8) & 0xFF);
        }
    }
}



