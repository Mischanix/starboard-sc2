using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Starboard.MemoryReader
{
    static class GameState
    {
        public static bool IsInGame()
        {
            // 0.5s is a reasonably safe margin
            return Offsets.Supports(Process.Version) &&
                Process.ReadInt(Offsets.GameTick) > 0x800;
        }

        public static bool IsObserver()
        {
            var flag = Process.ReadByte(Offsets.LocalPlayerFlag);
            var byte1 = Process.ReadByte(Offsets.LocalPlayerByte);
            var byte2 = Process.ReadByte(Offsets.LocalPlayerByte + 1);
            return (flag == 0 && byte1 == 0x10 && byte2 == 0x10) || // same as Tick == 0?
                flag == 1;
        }
    }
}
