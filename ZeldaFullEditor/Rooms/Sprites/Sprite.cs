using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaFullEditor
{
    [Serializable]
    public class Sprite
    {
        public byte x, y, id;
        public byte nx, ny;
        public byte layer = 0;
        public byte subtype = 0;
        public byte overlord = 0;
        public string name;
        public byte keyDrop = 0;
        public int sizeMap = 512;
        Room room;
        public Rectangle boundingbox;
        bool picker = false;
        public bool selected = false;
        public Sprite(Room room,byte id, byte x, byte y, string name, byte overlord, byte subtype, byte layer)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.name = name;
            this.room = room;
            this.overlord = overlord;
            this.subtype = subtype;
            this.layer = layer;
            this.nx = x;
            this.ny = y;
        }

        public void updateBBox()
        {
            lowerX = 1;
            lowerY = 1;
            higherX = 15;
            higherY = 15;
        }
        
        public void DrawKey(bool bigKey = false)
        {
            if (bigKey == false)
            {
                int dx = (boundingbox.X + boundingbox.Width / 2) - 4;
                int dy = boundingbox.Y - 10;
                drawSpriteTile(dx, dy, 14, 2, 11, false, false, 1, 2, true);
            }
            else
            {
                int dx = (boundingbox.X + boundingbox.Width / 2) - 4;
                int dy = boundingbox.Y - 10;
                drawSpriteTile(dx, dy, 14, 6, 11, false, false, 2, 2, true);
            }
        }


        public void update()
        {

        }

        int lowerX = 32;
        int lowerY = 32;
        int higherX = 0;
        int higherY = 0;
        int width = 16;
        int height = 16;
        public void drawSpriteTile(int x, int y, int srcx, int srcy, int pal, bool mirror_x = false, bool mirror_y = false, int sx = 2, int sy = 2, bool iskey = false)
        {



            int zx = x - (this.x * 16);
            int zy = y - (this.y * 16);
            if (iskey == false)
            {

                if (lowerX > zx)
                {
                    lowerX = zx;
                }

                if (lowerY > zy)
                {
                    lowerY = zy;
                }

                if (higherX < zx + (sx * 8))
                {
                    higherX = zx + (sx * 8);
                }

                if (higherY < zy + (sy * 8))
                {
                    higherY = zy + (sy * 8);
                }

                width = higherX - lowerX;
                height = higherY - lowerY;
                if (picker)
                {
                    x += 8;
                    y += 8;
                }

                if (nx != this.x || ny != this.y)
                {
                    x -= (this.x * 16);
                    y -= (this.y * 16);
                    x += (this.nx * 16);
                    y += (this.ny * 16);
                }
            }
            int ty = srcy + 32; 
            if (iskey)
            {
                ty = srcy;
            }
            int tx = srcx;
            int mx = 0;
            int my = 0;
            pal = pal - 2;
            if (mirror_x == true)
            {
                mx = sx * 8;
            }

            for (int xx = 0; xx < (sx * 8); xx++)
            {
                if (mx > 0)
                {
                    mx--;
                }
                if (mirror_y == true)
                {
                    my = sy * 8;
                }
                for (int yy = 0; yy < (sy * 8); yy++)
                {
                    if (my > 0)
                    {
                        my--;
                    }


                       
                    int x_dest = ((x) + (xx)) * 4;
                    int y_dest = (((y) + (yy)) * GFX.currentWidth) * 4;
                    if (picker)
                    {
                        y_dest = (((y) + (yy)) * GFX.currentWidth) * 4;
                    }
                    int dest = x_dest + y_dest;

                    int x_src = ((tx * 8) + (xx));
                    if (mirror_x)
                    {
                        x_src = ((tx * 8) + mx);
                    }
                    int y_src = (((ty * 8) + (yy)) * 128);
                    if (mirror_y)
                    {
                        y_src = (((ty * 8) + my) * 128);
                    }

                    int src = x_src + y_src;
                    unsafe
                    {
                        if (dest < (GFX.currentWidth * GFX.currentHeight * 4))
                        {
                            if (dest > 0)
                            {
                                if (!iskey)
                                {
                                    if (GFX.singledata[(src)] != 0)
                                    {
                                        GFX.currentData[dest] = (GFX.spritesPalettes[GFX.singledata[(src)], pal].B);
                                        GFX.currentData[dest + 1] = (GFX.spritesPalettes[GFX.singledata[(src)], pal].G);
                                        GFX.currentData[dest + 2] = (GFX.spritesPalettes[GFX.singledata[(src)], pal].R);
                                        GFX.currentData[dest + 3] = 255;
                                    }
                                }
                                else
                                {
                                    if (GFX.itemsdataEDITOR[(src)] != 0)
                                    {
                                        GFX.currentData[dest] = (GFX.spritesPalettes[GFX.itemsdataEDITOR[(src)], pal].B);
                                        GFX.currentData[dest + 1] = (GFX.spritesPalettes[GFX.itemsdataEDITOR[(src)], pal].G);
                                        GFX.currentData[dest + 2] = (GFX.spritesPalettes[GFX.itemsdataEDITOR[(src)], pal].R);
                                        GFX.currentData[dest + 3] = 255;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}