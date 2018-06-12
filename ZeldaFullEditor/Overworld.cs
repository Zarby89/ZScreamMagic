using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaFullEditor
{
    public class Overworld
    {



        public Overworld()
        {



        }

        public List<Tile16> tiles16 = new List<Tile16>();
        public List<Tile32> tiles32 = new List<Tile32>();
        public void AssembleMap16Tiles(bool fromJson = false)
        {
            if (fromJson)
            {
                tiles16 = JsonConvert.DeserializeObject<Tile16[]>(File.ReadAllText("ProjectDirectory//Overworld//Tiles16.json")).ToList();
                return;
            }
            int tpos = Constants.map16Tiles;
            for (int i = 0; i < 3760; i += 1)
            {
                TileInfo t0 = gettilesinfo(BitConverter.ToInt16(ROM.DATA, (tpos)));
                tpos += 2;
                TileInfo t1 = gettilesinfo(BitConverter.ToInt16(ROM.DATA, (tpos)));
                tpos += 2;
                TileInfo t2 = gettilesinfo(BitConverter.ToInt16(ROM.DATA, (tpos)));
                tpos += 2;
                TileInfo t3 = gettilesinfo(BitConverter.ToInt16(ROM.DATA, (tpos)));
                tpos += 2;
                tiles16.Add(new Tile16(t0, t1, t2, t3));
            }
        }






        public TileInfo gettilesinfo(short tile)
        {
            //vhopppcc cccccccc
            bool o = false;
            bool v = false;
            bool h = false;
            short tid = (short)(tile & 0x3FF);
            byte p = (byte)((tile >> 10) & 0x07);
            if ((tile & 0x2000) == 0x2000)
            {
                o = true;
            }
            if ((tile & 0x4000) == 0x4000)
            {
                h = true;
            }
            if ((tile & 0x8000) == 0x8000)
            {
                v = true;
            }
            return new TileInfo(tid, p, v, h, o);

        }

        public void AssembleMap32Tiles()
        {

            for (int i = 0; i < 0x33F0; i += 6)
            {
                ushort tl = (ushort)(ROM.DATA[Constants.map32TilesTL + (i)] + (((ROM.DATA[Constants.map32TilesTL + (i) + 4] >> 4) & 0x0f) * 256));
                ushort tr = (ushort)(ROM.DATA[Constants.map32TilesTR + (i)] + (((ROM.DATA[Constants.map32TilesTR + (i) + 4] >> 4) & 0x0f) * 256));
                ushort bl = (ushort)(ROM.DATA[Constants.map32TilesBL + (i)] + (((ROM.DATA[Constants.map32TilesBL + (i) + 4] >> 4) & 0x0f) * 256));
                ushort br = (ushort)(ROM.DATA[Constants.map32TilesBR + (i)] + (((ROM.DATA[Constants.map32TilesBR + (i) + 4] >> 4) & 0x0f) * 256));
                tiles32.Add(new Tile32(tl, tr, bl, br));


                tl = (ushort)(ROM.DATA[Constants.map32TilesTL + 1 + (i)] + (((ROM.DATA[Constants.map32TilesTL + (i) + 4]) & 0x0f) * 256));
                tr = (ushort)(ROM.DATA[Constants.map32TilesTR + 1 + (i)] + (((ROM.DATA[Constants.map32TilesTR + (i) + 4]) & 0x0f) * 256));
                bl = (ushort)(ROM.DATA[Constants.map32TilesBL + 1 + (i)] + (((ROM.DATA[Constants.map32TilesBL + (i) + 4]) & 0x0f) * 256));
                br = (ushort)(ROM.DATA[Constants.map32TilesBR + 1 + (i)] + (((ROM.DATA[Constants.map32TilesBR + (i) + 4]) & 0x0f) * 256));
                tiles32.Add(new Tile32(tl, tr, bl, br));

                tl = (ushort)(ROM.DATA[Constants.map32TilesTL + 2 + (i)] + (((ROM.DATA[Constants.map32TilesTL + (i) + 5] >> 4) & 0x0f) * 256));
                tr = (ushort)(ROM.DATA[Constants.map32TilesTR + 2 + (i)] + (((ROM.DATA[Constants.map32TilesTR + (i) + 5] >> 4) & 0x0f) * 256));
                bl = (ushort)(ROM.DATA[Constants.map32TilesBL + 2 + (i)] + (((ROM.DATA[Constants.map32TilesBL + (i) + 5] >> 4) & 0x0f) * 256));
                br = (ushort)(ROM.DATA[Constants.map32TilesBR + 2 + (i)] + (((ROM.DATA[Constants.map32TilesBR + (i) + 5] >> 4) & 0x0f) * 256));
                tiles32.Add(new Tile32(tl, tr, bl, br));


                tl = (ushort)(ROM.DATA[Constants.map32TilesTL + 3 + (i)] + (((ROM.DATA[Constants.map32TilesTL + (i) + 5]) & 0x0f) * 256));
                tr = (ushort)(ROM.DATA[Constants.map32TilesTR + 3 + (i)] + (((ROM.DATA[Constants.map32TilesTR + (i) + 5]) & 0x0f) * 256));
                bl = (ushort)(ROM.DATA[Constants.map32TilesBL + 3 + (i)] + (((ROM.DATA[Constants.map32TilesBL + (i) + 5]) & 0x0f) * 256));
                br = (ushort)(ROM.DATA[Constants.map32TilesBR + 3 + (i)] + (((ROM.DATA[Constants.map32TilesBR + (i) + 5]) & 0x0f) * 256));
                tiles32.Add(new Tile32(tl, tr, bl, br));
            }

        }


        public Tile32[] map16tiles = new Tile32[40960];
        public List<Size> posSize = new List<Size>();
        public void DecompressAllMapTiles()
        {
            int npos = 0;
            for (int i = 0; i < 160; i++)
            {

                int p1 =
                (ROM.DATA[(Constants.compressedAllMap32PointersHigh) + 2 + (int)(3 * i)] << 16) +
                (ROM.DATA[(Constants.compressedAllMap32PointersHigh) + 1 + (int)(3 * i)] << 8) +
                (ROM.DATA[(Constants.compressedAllMap32PointersHigh + (int)(3 * i))]);
                p1 = Addresses.snestopc(p1);

                int p2 =
                (ROM.DATA[(Constants.compressedAllMap32PointersLow) + 2 + (int)(3 * i)] << 16) +
                (ROM.DATA[(Constants.compressedAllMap32PointersLow) + 1 + (int)(3 * i)] << 8) +
                (ROM.DATA[(Constants.compressedAllMap32PointersLow + (int)(3 * i))]);
                p2 = Addresses.snestopc(p2);

                int ttpos = 0;
                int compressedSize1 = 0;
                int compressedSize2 = 0;


                byte[] bytes = ZCompressLibrary.Decompress.ALTTPDecompressOverworld(ROM.DATA, p2, 1000, ref compressedSize1);
                byte[] bytes2 = ZCompressLibrary.Decompress.ALTTPDecompressOverworld(ROM.DATA, p1, 1000, ref compressedSize2);

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        ushort tidD = (ushort)((bytes2[ttpos] << 8) + bytes[ttpos]);

                        int tpos = tidD;
                        if (tpos < tiles32.Count)
                        {
                            map16tiles[npos] = new Tile32(tiles32[tpos].tile0, tiles32[tpos].tile1, tiles32[tpos].tile2, tiles32[tpos].tile3);

                        }
                        else
                        {
                            Console.WriteLine("Found 0,0,0,0");
                            map16tiles[npos] = new Tile32(0, 0, 0, 0);
                        }
                        npos++;
                        ttpos += 1;

                    }
                }
            }


        }




        public Tile32[] t32Unique = new Tile32[10000];
        public List<ushort> t32 = new List<ushort>();
        public int tiles32count = 0;
        public void createMap32TilesFrom16()
        {
            t32.Clear();
            t32Unique = new Tile32[10000];
            tiles32count = 0;
            //40960 = numbers of 32x32 tiles 

            for (int i = 0; i < 40960; i++)
            {
                short foundIndex = -1;
                for (int j = 0; j < tiles32count; j++)
                {
                    if (t32Unique[j].tile0 == map16tiles[i].tile0)
                    {
                        if (t32Unique[j].tile1 == map16tiles[i].tile1)
                        {
                            if (t32Unique[j].tile2 == map16tiles[i].tile2)
                            {
                                if (t32Unique[j].tile3 == map16tiles[i].tile3)
                                {
                                    foundIndex = (short)j;
                                    break;
                                }
                            }
                        }
                    }


                }

                if (foundIndex == -1)
                {
                    t32Unique[tiles32count] = new Tile32(map16tiles[i].tile0, map16tiles[i].tile1, map16tiles[i].tile2, map16tiles[i].tile3);
                    t32.Add((ushort)tiles32count);
                    tiles32count++;
                }
                else
                {
                    t32.Add((ushort)foundIndex);
                }


            }

            Console.WriteLine("Nbr of tiles32 = " + tiles32count);

        }



        //UNUSED CODE

        public void AllMapTilesFromMap(int mapid, ushort[,] tiles, bool large = false)
        {

            string s = "";
            int tpos = mapid * 256;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    map16tiles[tpos] = new Tile32(tiles[(x * 2), (y * 2)], tiles[(x * 2) + 1, (y * 2)], tiles[(x * 2), (y * 2) + 1], tiles[(x * 2) + 1, (y * 2) + 1]);
                    s += "[" + map16tiles[tpos].tile0.ToString("D4")+","+ map16tiles[tpos].tile1.ToString("D4") + "," + map16tiles[tpos].tile2.ToString("D4") + "," + map16tiles[tpos].tile3.ToString("D4") + "] ";
                    tpos++;
                    
                }
                s += "\r\n";
            }
            File.WriteAllText("TileDebug.txt", s);

        }


        public void Save32Tiles()
        {
            int index = 0;
            int c = tiles32count;
            for (int i = 0; i < c - 4; i += 6)
            {
                if (i >= 0x33F0)
                {
                    Console.WriteLine("Too Many Unique Tiles !");
                    break;
                }

                //Top Left
                ROM.DATA[Constants.map32TilesTL + (i)] = (byte)(t32Unique[index].tile0 & 0xFF);
                ROM.DATA[Constants.map32TilesTL + (i + 1)] = (byte)(t32Unique[index + 1].tile0 & 0xFF);
                ROM.DATA[Constants.map32TilesTL + (i + 2)] = (byte)(t32Unique[index + 2].tile0 & 0xFF);
                ROM.DATA[Constants.map32TilesTL + (i + 3)] = (byte)(t32Unique[index + 3].tile0 & 0xFF);

                ROM.DATA[Constants.map32TilesTL + (i + 4)] = (byte)(((t32Unique[index].tile0 >> 4) & 0xF0) + ((t32Unique[index + 1].tile0 >> 8) & 0x0F));
                ROM.DATA[Constants.map32TilesTL + (i + 5)] = (byte)(((t32Unique[index + 2].tile0 >> 4) & 0xF0) + ((t32Unique[index + 3].tile0 >> 8) & 0x0F));

                //Top Right
                ROM.DATA[Constants.map32TilesTR + (i)] = (byte)(t32Unique[index].tile1 & 0xFF);
                ROM.DATA[Constants.map32TilesTR + (i + 1)] = (byte)(t32Unique[index + 1].tile1 & 0xFF);
                ROM.DATA[Constants.map32TilesTR + (i + 2)] = (byte)(t32Unique[index + 2].tile1 & 0xFF);
                ROM.DATA[Constants.map32TilesTR + (i + 3)] = (byte)(t32Unique[index + 3].tile1 & 0xFF);

                ROM.DATA[Constants.map32TilesTR + (i + 4)] = (byte)(((t32Unique[index].tile1 >> 4) & 0xF0) | ((t32Unique[index + 1].tile1 >> 8) & 0x0F));
                ROM.DATA[Constants.map32TilesTR + (i + 5)] = (byte)(((t32Unique[index + 2].tile1 >> 4) & 0xF0) | ((t32Unique[index + 3].tile1 >> 8) & 0x0F));

                //Bottom Left
                ROM.DATA[Constants.map32TilesBL + (i)] = (byte)(t32Unique[index].tile2 & 0xFF);
                ROM.DATA[Constants.map32TilesBL + (i + 1)] = (byte)(t32Unique[index + 1].tile2 & 0xFF);
                ROM.DATA[Constants.map32TilesBL + (i + 2)] = (byte)(t32Unique[index + 2].tile2 & 0xFF);
                ROM.DATA[Constants.map32TilesBL + (i + 3)] = (byte)(t32Unique[index + 3].tile2 & 0xFF);

                ROM.DATA[Constants.map32TilesBL + (i + 4)] = (byte)(((t32Unique[index].tile2 >> 4) & 0xF0) | ((t32Unique[index + 1].tile2 >> 8) & 0x0F));
                ROM.DATA[Constants.map32TilesBL + (i + 5)] = (byte)(((t32Unique[index + 2].tile2 >> 4) & 0xF0) | ((t32Unique[index + 3].tile2 >> 8) & 0x0F));

                //Bottom Right
                ROM.DATA[Constants.map32TilesBR + (i)] = (byte)(t32Unique[index].tile3 & 0xFF);
                ROM.DATA[Constants.map32TilesBR + (i + 1)] = (byte)(t32Unique[index + 1].tile3 & 0xFF);
                ROM.DATA[Constants.map32TilesBR + (i + 2)] = (byte)(t32Unique[index + 2].tile3 & 0xFF);
                ROM.DATA[Constants.map32TilesBR + (i + 3)] = (byte)(t32Unique[index + 3].tile3 & 0xFF);

                ROM.DATA[Constants.map32TilesBR + (i + 4)] = (byte)(((t32Unique[index].tile3 >> 4) & 0xF0) | ((t32Unique[index + 1].tile3 >> 8) & 0x0F));
                ROM.DATA[Constants.map32TilesBR + (i + 5)] = (byte)(((t32Unique[index + 2].tile3 >> 4) & 0xF0) | ((t32Unique[index + 3].tile3 >> 8) & 0x0F));


                index += 4;
                c += 2;
            }

        }





        public void savemapstorom()
        {
            int pos = 0x1A0000;
            for (int i = 0; i < 160; i++)
            {
                int npos = 0;
                byte[] singlemap1 = new byte[256];
                byte[] singlemap2 = new byte[256];
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        singlemap1[npos] = (byte)(t32[npos + (i * 256)] & 0xFF);
                        singlemap2[npos] = (byte)((t32[npos + (i * 256)] >> 8) & 0xFF);
                        npos++;
                    }
                }

                int snesPos = Addresses.pctosnes(pos);
                ROM.DATA[(Constants.compressedAllMap32PointersHigh) + 0 + (int)(3 * i)] = (byte)(snesPos & 0xFF);
                ROM.DATA[(Constants.compressedAllMap32PointersHigh) + 1 + (int)(3 * i)] = (byte)((snesPos >> 8) & 0xFF);
                ROM.DATA[(Constants.compressedAllMap32PointersHigh) + 2 + (int)(3 * i)] = (byte)((snesPos >> 16) & 0xFF);

                ROM.DATA[pos] = 0xE0;
                ROM.DATA[pos + 1] = 0xFF;
                pos += 2;
                for (int j = 0; j < 256; j++)
                {
                    ROM.DATA[pos] = singlemap2[j];
                    pos += 1;
                }
                ROM.DATA[pos] = 0xFF;
                pos += 1;
                snesPos = Addresses.pctosnes(pos);
                ROM.DATA[(Constants.compressedAllMap32PointersLow) + 0 + (int)(3 * i)] = (byte)(snesPos & 0xFF);
                ROM.DATA[(Constants.compressedAllMap32PointersLow) + 1 + (int)(3 * i)] = (byte)((snesPos >> 8) & 0xFF);
                ROM.DATA[(Constants.compressedAllMap32PointersLow) + 2 + (int)(3 * i)] = (byte)((snesPos >> 16) & 0xFF);

                ROM.DATA[pos] = 0xE0;
                ROM.DATA[pos + 1] = 0xFF;
                pos += 2;
                for (int j = 0; j < 256; j++)
                {
                    ROM.DATA[pos] = singlemap1[j];
                    pos += 1;
                }
                ROM.DATA[pos] = 0xFF;
                pos += 1;

            }
            Save32Tiles();
        }
    }
}
