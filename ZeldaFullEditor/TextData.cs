using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaFullEditor
{
    public static class TextData
    {

        public static string[] messages = new string[400];
        public static int[] messagesPos = new int[400];
        public static void readAllText()
        {
            int maxMessage = 0;
            int pos = 0xE0000;
            int msgid = 0;

            while (msgid < 400)
            {
                messagesPos[msgid] = pos;
                messages[msgid] = "";
                byte byteRead = ROM.DATA[pos];
                while (byteRead != 0x7F) //7F = end of string
                {

                    if (getTextCharacter(byteRead, msgid))
                    {
                        pos++;
                    }

                    //Commands
                    else if (byteRead == 0x67) // $67[NextPic] command
                    {
                        messages[msgid] += "[PIC]";
                        pos++;
                    }
                    else if (byteRead == 0x68) //$68[Choose] command
                    {
                        messages[msgid] += "[CHS]";
                        pos++;
                    }
                    else if (byteRead == 0x69) //$69[Item] command(for waterfall of wishing)
                    {
                        messages[msgid] += "[ITM]";
                        pos++;
                    }
                    else if (byteRead == 0x6A) //$6A[Name] command(insert's player's name)
                    {
                        messages[msgid] += "[NAM]";
                        pos++;
                    }
                    else if (byteRead == 0x6B) //$6B[Window XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[WND=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2")+"]";
                        pos++;
                    }
                    else if (byteRead == 0x6C) // $6C[Number XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[NBR=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x6D) // $6D[Position XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[POS=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x6E) //$6E[ScrollSpd XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[SSP=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x6F) //$6F[SelChng] command
                    {
                        messages[msgid] += "[SCH]";
                        pos++;
                    }
                    else if (byteRead == 0x70) //$70 (Crash)
                    {
                        messages[msgid] += "[REM]";
                        pos++;
                    }
                    else if (byteRead == 0x71) //$71[Choose2] command
                    {
                        messages[msgid] += "[CH1]";
                        pos++;
                    }
                    else if (byteRead == 0x72) //$71[Choose3] command
                    {
                        messages[msgid] += "[CH2]";
                        pos++;
                    }
                    else if (byteRead == 0x73) //$73[SCROLL] command
                    {
                        messages[msgid] += "[SCL]";
                        pos++;
                    }
                    else if (byteRead == 0x74) //$74[1] command(aka[Line1])
                    {
                        messages[msgid] += "[LN1]";
                        pos++;
                    }
                    else if (byteRead == 0x75) //$75[1] command(aka[Line2])
                    {
                        messages[msgid] += "[LN2]";
                        pos++;
                    }
                    else if (byteRead == 0x76) //$76[1] command(aka[Line3])
                    {
                        messages[msgid] += "[LN3]";
                        pos++;
                    }
                    else if (byteRead == 0x77) //$77[Color XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[COL=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x78) //$78[Wait  XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[WAI=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x79) //$79[Sound XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[SND=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x7A) //$7A[Speed XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[SPD=";
                        pos++;
                        byteRead = ROM.DATA[pos];
                        messages[msgid] += byteRead.ToString("X2") + "]";
                        pos++;
                    }
                    else if (byteRead == 0x7E) //$7A[Speed XX] command(takes next byte as argument)
                    {
                        messages[msgid] += "[WFK]";
                        pos++;
                    }
                    else if (byteRead == 0x80)
                    {
                        pos++;
                        break;
                    }
                    else if (byteRead >= 0x88 && byteRead <= 0xF9)
                    {
                        //Dictionary
                        byte dictionaryIndex = (byte)(byteRead - 0x88);
                        byte a0 = 0x0E;
                        byte a1 = ROM.DATA[0x74703 + (dictionaryIndex * 2) + 1];
                        byte a2 = ROM.DATA[0x74703 + (dictionaryIndex * 2)];
                        int dictionaryPos = Addresses.snestopc(((a0 << 16) | (a1 << 8) | (a2)));
                        
                        a0 = 0x0E;
                        a1 = ROM.DATA[0x74703 + (dictionaryIndex * 2) + 3];
                        a2 = ROM.DATA[0x74703 + (dictionaryIndex * 2) + 2];
                        int dictionaryNextPos = Addresses.snestopc(((a0 << 16) | (a1 << 8) | (a2)));

                        byte length = (byte)(dictionaryNextPos - dictionaryPos);

                        for (int i = 0;i<length;i++)
                        {
                            getTextCharacter(ROM.DATA[dictionaryPos + i], msgid);
                        }
                        pos++;

                    }
                    else
                    {
                        //Console.WriteLine(byteRead.ToString("X2"));
                        break;
                    }
                    byteRead = ROM.DATA[pos];

                    if (pos >= 0xE8000)
                    {
                        maxMessage = msgid;
                        break;
                    }

                    continue;
                }

                if (pos >= 0xE8000)
                {
                    maxMessage = msgid;
                    break;
                }

                pos++;
                msgid++;
                byteRead = ROM.DATA[pos];

            }
        }

        public static bool getTextCharacter(byte byteRead, int msgid)
        {
            if (byteRead >= 0x00 && byteRead <= 0x19) //Caps Letters
            {
                char letter = (char)(byteRead + 0x41);
                messages[msgid] += letter;
            }
            else if (byteRead >= 0x1A && byteRead <= 0x33) //Small Letters
            {
                char letter = (char)((byteRead - 0x1A) + 0x61);
                messages[msgid] += letter;

            }
            else if (byteRead >= 0x34 && byteRead <= 0x3D) //Numbers
            {
                char letter = (char)((byteRead - 0x34) + 0x30);
                messages[msgid] += letter;

            }
            else if (byteRead == 0x3E)
            {
                messages[msgid] += '!';

            }
            else if (byteRead == 0x3F)
            {
                messages[msgid] += '?';

            }
            else if (byteRead == 0x40)
            {
                messages[msgid] += '-';

            }
            else if (byteRead == 0x41)
            {
                messages[msgid] += '.';

            }
            else if (byteRead == 0x42)
            {
                messages[msgid] += ',';

            }
            else if (byteRead == 0x43)
            {
                messages[msgid] += '…';

            }
            else if (byteRead == 0x44)
            {
                messages[msgid] += '►';

            }
            else if (byteRead == 0x45)
            {
                messages[msgid] += '(';

            }
            else if (byteRead == 0x46)
            {
                messages[msgid] += ')';

            }
            else if (byteRead == 0x47)
            {
                messages[msgid] += "[47]";

            }
            else if (byteRead == 0x48)
            {
                messages[msgid] += "[48]";

            }
            else if (byteRead == 0x49)
            {
                messages[msgid] += "[49]";

            }
            else if (byteRead == 0x4A)
            {
                messages[msgid] += "[4A]";

            }
            else if (byteRead == 0x4B)
            {
                messages[msgid] += "[4B]";

            }
            else if (byteRead == 0x4C)
            {
                messages[msgid] += '"';

            }
            else if (byteRead == 0x4D)
            {
                messages[msgid] += '⌃';

            }
            else if (byteRead == 0x4E)
            {
                messages[msgid] += '⌄';

            }
            else if (byteRead == 0x4F)
            {
                messages[msgid] += '<';

            }
            else if (byteRead == 0x50)
            {
                messages[msgid] += '>';

            }
            else if (byteRead == 0x51)
            {
                messages[msgid] += "'";

            }
            else if (byteRead == 0x52)
            {
                messages[msgid] += "[52]";

            }
            else if (byteRead == 0x53)
            {
                messages[msgid] += "[53]";

            }
            else if (byteRead == 0x54)
            {
                messages[msgid] += "[54]";

            }
            else if (byteRead == 0x55)
            {
                messages[msgid] += "[55]";

            }
            else if (byteRead == 0x56)
            {
                messages[msgid] += "[56]";

            }
            else if (byteRead == 0x57)
            {
                messages[msgid] += "[57]";

            }
            else if (byteRead == 0x58)
            {
                messages[msgid] += "[58]";

            }
            else if (byteRead == 0x59)
            {
                messages[msgid] += " ";

            }
            else if (byteRead == 0x5A)
            {
                messages[msgid] += "🢀";

            }
            else if (byteRead == 0x5B)
            {
                messages[msgid] += "Ⓐ";

            }
            else if (byteRead == 0x5C)
            {
                messages[msgid] += "Ⓑ";

            }
            else if (byteRead == 0x5D)
            {
                messages[msgid] += "ⓧ";

            }
            else if (byteRead == 0x5E)
            {
                messages[msgid] += "ⓨ";

            }
            else if (byteRead == 0x5F)
            {
                messages[msgid] += "[5F]";

            }
            else if (byteRead == 0x60)
            {
                messages[msgid] += "[60]";

            }
            else if (byteRead == 0x61)
            {
                messages[msgid] += "[61]";

            }
            else if (byteRead == 0x62)
            {
                messages[msgid] += "[62]";

            }
            else if (byteRead == 0x63)
            {
                messages[msgid] += "[63]";

            }
            else if (byteRead == 0x64)
            {
                messages[msgid] += "[64]";

            }
            else if (byteRead == 0x65)
            {
                messages[msgid] += "[65]";

            }
            else if (byteRead == 0x66)
            {
                messages[msgid] += "[66]";

            }
            else
            {
                return false;
            }
            return true;
        }

    }
}
