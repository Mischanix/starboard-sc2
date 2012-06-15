using System;
using System.Collections.Generic;
using System.Text;

namespace Starboard.MemoryReader
{
    static class Offsets
    {
        public static bool Supports(int version)
        {
            if (version == 21029)
                return true;
            return false;
        }

        public static void Init(int version)
        {
        }


        public static uint PlayerTable = 0x26A8668;
        public static uint PlayerLength = 0x910;
        public static uint PlayerInfoTable = 0x02A349EC;
        public static uint PlayerInfoLength = 0xF98;
        public static uint GameTick = 0x1D76494;
        public static uint LocalPlayerFlag = 0x16F57D0;
        public static uint LocalPlayerByte = 0x15B3F2C; // also, +1
    }
}
