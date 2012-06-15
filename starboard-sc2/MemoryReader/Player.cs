using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Starboard.Model;

namespace Starboard.MemoryReader
{
    sealed class Player
    {
        private int index;
        private uint offset;

        private Player(int index)
        {
            this.index = index;
            offset = (uint)index * Offsets.PlayerLength + Offsets.PlayerTable;
        }

        /// <summary>
        /// Gets a list of the current players, excluding the Neutral player.
        /// </summary>
        /// <returns></returns>
        public static List<Player> GetCurrentPlayers()
        {
            var result = new List<Player>(2);
            for (int i = 0; i < 16; i++)
            {
                var player = new Player(i);
                if (player.Valid &&
                   (player.Type == PlayerType.Computer ||
                    player.Type == PlayerType.Player))
                    result.Add(player);
            }
            return result;
        }

        public int Index { get { return index; } }

        public string Name
        {
            get
            {
                return Process.ReadUTFString(offset + 0x48, Process.ReadInt(offset + 0x40));
            }
        }

        // Too lazy to find Random.  It's pretty deep, methinks,
        // not being at all relevant to the game state.
        public Race Race
        {
            get
            {
                uint sPtr = Process.ReadUInt(offset + 0x98);
                if (sPtr != 0)
                {
                    string result;
                    sPtr = Process.ReadUInt(sPtr + 4);
                    int length = Process.ReadInt(sPtr);
                    if ((Process.ReadByte(sPtr + 4) & 4) == 0)
                        result = Process.ReadASCIIString(sPtr + 8, length);
                    else
                        result = Process.ReadASCIIString(Process.ReadUInt(sPtr + 8), length);
                    switch (result)
                    {
                        case "Terr":
                            return Race.Terran;
                        case "Prot":
                            return Race.Protoss;
                        case "Zerg":
                            return Race.Zerg;
                    }
                }
                return Race.Unknown;
            }
        }

        public PlayerColor Color
        {
            get
            {
                if (index == 0) // invalid for Neutral
                    return PlayerColor.Unknown;
                uint offset = Offsets.PlayerInfoLength * (uint)(index - 1) + Offsets.PlayerInfoTable;
                var argbColor = Process.ReadUInt(offset + 0x70C);
                switch (argbColor)
                {
                    case 0xff0042ff:
                        return PlayerColor.Blue;
                    case 0xffb4141e:
                        return PlayerColor.Red;
                    case 0xff1ca7ea:
                        return PlayerColor.Teal;
                    case 0xff540081:
                        return PlayerColor.Purple;
                    case 0xffebe129:
                        return PlayerColor.Yellow;
                    case 0xfffe8a0e:
                        return PlayerColor.Orange;
                    case 0xff168000:
                        return PlayerColor.Green;
                    case 0xffcca6fc:
                        return PlayerColor.LightPink;
                    default:
                        return PlayerColor.Unknown;
                }
            }
        }

        public bool Valid
        {
            get
            {
                return Process.ReadInt(offset) != 0;
            }
        }

        public PlayerType Type
        {
            get
            {
                return (PlayerType)Process.ReadByte(offset + 0x1d);
            }
        }

        public Status Status
        {
            get
            {
                return (Status)Process.ReadByte(offset + 0x1e);
            }
        }
    }

    public enum Status
    {
        Undecided = 0,
        Victory,
        Defeat
    }

    public enum PlayerType
    {
        Unknown = 0,
        Player,
        Computer,
        Referee = 5,
        Spectator = 6
    }
}
