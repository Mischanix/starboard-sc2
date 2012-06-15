using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using Starboard.ViewModel;

namespace Starboard.MemoryReader
{
    class MemoryReader
    {
        private static State state = State.NoSC2;

        public static void Tick(object s, EventArgs e)
        {
            if (state == State.Exiting) return;
            if (state != State.NoSC2 && !Process.IsSC2Open())
            {
                state = State.NoSC2;
                return;
            }

            if (state == State.NoSC2 || state == State.UnsupportedVersion)
            {
                if (Process.IsSC2Open())
                {
                    Process.Init();
                    if (Offsets.Supports(Process.Version))
                    {
                        Offsets.Init(Process.Version);
                        state = State.NoGame;
                    } else
                        state = State.UnsupportedVersion;
                } else
                    state = State.NoSC2;
            }

            if (state > State.NoSC2 && !Offsets.Supports(Process.Version))
                state = State.UnsupportedVersion;

            try
            {
                if (state == State.NoGame && GameState.IsInGame())
                {
                    if (GameState.IsObserver())
                        UpdateScoreboard();
                    state = State.InGame;
                }
                else if ((state == State.InGame || state == State.EndGame) && !GameState.IsInGame())
                {
                    state = State.NoGame;
                }

                if (state == State.InGame)
                {
                    if (GameState.IsObserver())
                        CheckGameStatus();
                }
            }
            catch (InvalidOperationException)
            {
                state = State.NoSC2;
            }
        }

        public static void Exit()
        {
            state = State.Exiting;
        }

        /// <summary>
        /// Fill scoreboard with information.
        /// </summary>
        static void UpdateScoreboard()
        {
            var playerList = Player.GetCurrentPlayers();
            if (playerList.Count != 2) return; // 2v2, 1v0, etc.
            var player1 = playerList[0];
            var player2 = playerList[1];
            var player1Name = player1.Name;
            var player2Name = player2.Name;
            var sb = MainWindowViewModel.DisplayWindow.Scoreboard;

            Model.Player p1 = sb.Player1;
            Model.Player p2 = sb.Player2;
            if (String.Compare(player1Name, player2Name, true) < 0)
            {
                p1.Name = player1Name;
                p1.Race = player1.Race;
                p1.Color = player1.Color;

                p2.Name = player2Name;
                p2.Race = player2.Race;
                p2.Color = player2.Color;
            }
            else
            {
                p1.Name = player2Name;
                p1.Race = player2.Race;
                p1.Color = player2.Color;

                p2.Name = player1Name;
                p2.Race = player1.Race;
                p2.Color = player1.Color;
            }
        }

        /// <summary>
        /// Check for victory/defeat, update accordingly.
        /// </summary>
        static void CheckGameStatus()
        {
            var playerList = Player.GetCurrentPlayers();
            if (playerList.Count > 2 || playerList.Count == 0) return;

            var player1 = playerList[0];
            if (player1.Status == Status.Undecided) return;

            var sb = MainWindowViewModel.DisplayWindow.Scoreboard;
            state = State.EndGame;

            if (player1.Status == Status.Victory)
            {
                if (sb.Player1.Color == player1.Color)
                    sb.Player1.Score++;
                else sb.Player2.Score++;
            }
            else
            {
                if (sb.Player1.Color == player1.Color)
                    sb.Player2.Score++;
                else sb.Player1.Score++;
            }
        }

        enum State
        {
            Exiting,
            UnsupportedVersion,
            NoSC2,
            NoGame,
            InGame,
            EndGame
        }
    }
}
