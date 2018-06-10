using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaFullEditor
{
    public struct EntranceOW
    {
        public short mapPos;
        public short mapId;
        public byte entranceId;
        public EntranceOW(short mapId, short mapPos, byte entranceId)
        {
            this.mapPos = mapPos;
            this.mapId = mapId;
            this.entranceId = entranceId;
           
        }

    }
}
