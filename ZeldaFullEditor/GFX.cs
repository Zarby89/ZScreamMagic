using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZeldaFullEditor
{
    public static class GFX
    {

        public static Graphics graphictilebuffer;

        public static byte[] gfxdata;

        public static byte[,] imgdata = new byte[128, 32];
        public static byte[] singledata = new byte[128 * 800];


        public static int[] positions = new int[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
        public static int superpos = 0;


        public static byte[] bpp3snestoindexed(byte[] data, int index)
        {
            //3BPP
            //[r0, bp1], [r0, bp2], [r1, bp1], [r1, bp2], [r2, bp1], [r2, bp2], [r3, bp1], [r3, bp2]
            //[r4, bp1], [r4, bp2], [r5, bp1], [r5, bp2], [r6, bp1], [r6, bp2], [r7, bp1], [r7, bp2]
            //[r0, bp3], [r1, bp3], [r2, bp3], [r3, bp3], [r4, bp3], [r5, bp3], [r6, bp3], [r7, bp3]
            //2BPP
            //[r0, bp1], [r0, bp2], [r1, bp1], [r1, bp2], [r2, bp1], [r2, bp2], [r3, bp1], [r3, bp2]
            //[r4, bp1], [r4, bp2], [r5, bp1], [r5, bp2], [r6, bp1], [r6, bp2], [r7, bp1], [r7, bp2]

            byte[] buffer = new byte[128 * 32];
            byte[,] imgdata = new byte[128, 32];
            int yy = 0;
            int xx = 0;
            int pos = 0;

            for (int i = 0; i < index; i++)
            {
                if (Compression.bpp[i] == 3)
                {
                    pos += 64;
                }
                else
                {
                    pos += 128;
                }
            }

            if (Compression.bpp[index] == 3)
            {
                int ypos = 0; 
                for (int i = 0; i < 64; i++) //for each tiles //16 per lines
                {
                    for (int y = 0; y < 8; y++)//for each lines
                    {
                        //[0] + [1] + [16]
                        for (int x = 0; x < 8; x++)
                        {
                            byte[] bitmask = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
                            byte b1 = (byte)((data[(y * 2) + (24 * pos)] & (bitmask[x])));
                            byte b2 = (byte)(data[((y * 2) + (24 * pos)) + 1] & (bitmask[x]));
                            byte b3 = (byte)(data[(16 + y) + (24 * pos)] & (bitmask[x]));
                            byte b = 0;
                            if (b1 != 0) { b |= 1; };
                            if (b2 != 0) { b |= 2; };
                            if (b3 != 0) { b |= 4; };
                            imgdata[x + xx, y + (yy * 8)] = b;
                        }

                    }
                    pos++;
                    ypos++;
                    xx += 8;
                    if (ypos >= 16)
                    {
                        yy++;
                        xx = 0;
                        ypos = 0;

                    }

                }
                int n = 0;
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 128; x++)
                    {

                        buffer[n] = imgdata[x, y];
                        n++;

                    }
                }
            }
            else  //this is not working !
            {
                //[r0, bp1], [r0, bp2], [r1, bp1], [r1, bp2], [r2, bp1], [r2, bp2], [r3, bp1], [r3, bp2]
                //[r4, bp1], [r4, bp2], [r5, bp1], [r5, bp2], [r6, bp1], [r6, bp2], [r7, bp1], [r7, bp2]

                //pos -= 64;
                if (index == 113)
                {
                    pos = 0x02A600;
                }
                if (index == 114)
                {
                    pos = 0x02B200;
                }
                if (index == 218)
                {
                    pos = 0x052800;
                }
                if (index == 219)
                {
                    pos = 0x053400;
                }
                if (index == 220)
                {
                    pos = 0x054000;
                }
                if (index == 221)
                {
                    pos = 0x054C00;
                }
                if (index == 222)
                {
                    pos = 0x055800;
                }

                imgdata = new byte[128, 64]; //ok it 64 to not screw up the indexing, last 32 are just empty data
                buffer = new byte[128 * 64];
                int ypos = 0;
                for (int i = 0; i < 128; i++) //for each tiles //16 per lines
                {
                    for (int y = 0; y < 8; y++)//for each lines
                    {
                        //[0] + [1] + [16]
                        for (int x = 0; x < 8; x++)
                        {
                            byte[] bitmask = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
                            byte b1 = (byte)((data[(y * 2) + pos] & (bitmask[x])));
                            byte b2 = (byte)(data[((y * 2) + pos) + 1] & (bitmask[x]));
                            byte b = 0;
                            if (b1 != 0) { b |= 1; };
                            if (b2 != 0) { b |= 2; };

                            imgdata[x + xx, y + (yy * 8)] = b;
                        }

                    }
                    pos+=16;
                    ypos++;
                    xx += 8;
                    if (ypos >= 16)
                    {
                        yy++;
                        xx = 0;
                        ypos = 0;

                    }

                }

                int n = 0;
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 128; x++)
                    {

                        buffer[n] = imgdata[x, y];
                        n++;

                    }
                }
            }

            return buffer;//buffer.ToArray();
        }


        public static byte[] blocksetData;

        public static unsafe byte* currentData;
        public static IntPtr currentPtr;
        public static BitmapData currentbmpData;
        public static int currentWidth;
        public static int currentHeight;
        public static unsafe void begin_draw(Bitmap b, int width = 512, int height = 512)
        {
            currentbmpData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            currentData = (byte*)currentbmpData.Scan0.ToPointer();
            currentWidth = width;
            currentHeight = height;
        }

        public static unsafe void begin_draw3bpp(Bitmap b, int width = 512, int height = 512)
        {
            currentbmpData = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format4bppIndexed);
            currentData = (byte*)currentbmpData.Scan0.ToPointer();
            currentWidth = width;
            currentHeight = height;
        }

        public static void end_draw(Bitmap b)
        {
            b.UnlockBits(currentbmpData);
        }

  
        public static Bitmap singleGrayscaletobmp(int index)
        {
            byte[] data = bpp3snestoindexed(gfxdata, index);
            Bitmap b = new Bitmap(128, 32,PixelFormat.Format4bppIndexed);
            if (data.Length == (128*32))
            {
                begin_draw3bpp(b, 128, 32);
                unsafe
                {
                     for (int y = 0; y < 32; y++)
                     {
                         for (int x = 0; x < 64; x++)
                         {
                             byte cb = (byte)(((data[(x*2) + (y * 128) ] << 4 & 0xF0)) | (data[(x*2) + (y * 128)+1] & 0x0F));
                             currentData[(x) + (y * 64)] = cb;
                         }
                     }
                }

                end_draw(b);
                ColorPalette p = b.Palette;
                for (int i = 0; i < 8; i++)
                {
                    p.Entries[i] = Color.FromArgb(24 * i, 24 * i, 24 * i);
                }
                for (int i = 8; i < 16; i++)
                {
                    p.Entries[i] = Color.FromArgb(255, 0, 0);
                }
                b.Palette = p;
            }
            else
            {
                b = new Bitmap(128, 64);
                begin_draw(b, 128, 64);
                unsafe
                {
                    for (int x = 0; x < 128; x++)
                    {
                        for (int y = 0; y < 64; y++)
                        {
                            int dest = (x + (y * 128)) * 4;
                            currentData[dest] = (byte)(data[(dest / 4)] * 24);
                            currentData[dest + 1] = (byte)(data[(dest / 4)] * 24);
                            currentData[dest + 2] = (byte)(data[(dest / 4)] * 24);
                            currentData[dest + 3] = 255;
                        }
                    }
                }
                end_draw(b);
            }
             //128 = 4


            return b;
        }

        public static Bitmap single2bppGrayscaletobmp(int index)
        {
            byte[] data = bpp3snestoindexed(gfxdata, index);
            Bitmap b = new Bitmap(128, 32); //128 = 4

            begin_draw(b, 128, 32);
            unsafe
            {
                for (int x = 0; x < 128; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        int dest = (x + (y * 128)) * 4;
                        currentData[dest] = (byte)(data[(dest / 4)] * 32);
                        currentData[dest + 1] = (byte)(data[(dest / 4)] * 32);
                        currentData[dest + 2] = (byte)(data[(dest / 4)] * 32);
                        currentData[dest + 3] = 255;
                    }
                }
            }
            end_draw(b);
            return b;
        }



 


        public static Color getColor(short c)
        {
            return Color.FromArgb(((c & 0x1F) * 8), ((c & 0x3E0) >> 5) * 8, ((c & 0x7C00) >> 10) * 8);
        }
        

 




    }
}
