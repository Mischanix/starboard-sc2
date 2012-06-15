using System;
using System.Collections.Generic;
using System.Text;

namespace Starboard.Automation
{
    static class Offsets
    {
        public static bool Supports(int version)
        {
            switch (version)
            {
                case 21029:
                    return true;
                default:
                    return false;
            }
        }

        public static void Init(int version)
        {
            switch (version)
            {
                case 21029:
                    PlayerTable = 0x26A8668;
                    PlayerLength = 0x910;
                    PlayerInfoTable = 0x02A349EC;
                    PlayerInfoLength = 0xF98;
                    GameTick = 0x1D76494;
                    LocalPlayerFlag = 0x16F57D0;
                    LocalPlayerByte = 0x15B3F2C;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported version");
            }
        }

        public static uint PlayerTable { get; private set; }
        public static uint PlayerLength { get; private set; }
        public static uint PlayerInfoTable { get; private set; }
        public static uint PlayerInfoLength { get; private set; }
        public static uint GameTick { get; private set; }
        public static uint LocalPlayerFlag { get; private set; }
        public static uint LocalPlayerByte { get; private set; }
    }
}
