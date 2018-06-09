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
    [Serializable]
    public class Room_Object
    {
        public byte x, y; //position of the object in the room (*8 for draw)
        public byte nx, ny;
        public byte ox, oy;
        public byte size; //size of the object
        public List<Tile> tiles = new List<Tile>();
        public short id;
        public string name; //name of the object will be shown on the form
        public byte layer = 0;

        public Room_Object(short id, byte x, byte y, byte size, byte layer = 0)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.id = id;
            this.layer = layer;
            this.nx = x;
            this.ny = y;
            this.ox = x;
            this.oy = y;

        }
        
    }

}



