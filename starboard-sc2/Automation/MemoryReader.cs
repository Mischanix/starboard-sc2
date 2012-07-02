using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Timers;
using System.Text;
using System.Linq;

using Starboard.ViewModel;
using Starboard.MVVM;

namespace Starboard.Automation
{
    class MemoryReader
    {
        /// <summary>
        /// The current state of the relationship between Starboard and SC2.
        /// </summary>
        State state;

        ScoreboardControlViewModel scoreboard;

        Database db;

        PropertyObserver<Model.Player> player1Observer;
        PropertyObserver<Model.Player> player2Observer;

        public MemoryReader()
        {
            state = State.NoSC2;
            scoreboard = MainWindowViewModel.DisplayWindow.Scoreboard;
            db = new Database("players.db");

            var t = new DispatcherTimer();
            t.Interval = TimeSpan.FromMilliseconds(500);
            t.Tick += new EventHandler(Tick);
            t.Start();

            player1Observer = new PropertyObserver<Model.Player>(scoreboard.Player1);
            player1Observer.RegisterHandler(n => n.Name, OnNameChanged);
            player1Observer.RegisterHandler(n => n.Race, OnRaceChanged);

            player2Observer = new PropertyObserver<Model.Player>(scoreboard.Player2);
            player2Observer.RegisterHandler(n => n.Name, OnNameChanged);
            player2Observer.RegisterHandler(n => n.Race, OnRaceChanged);
        }

        public void Exit()
        {
            state = State.Exiting;
        }

        void Tick(object s, EventArgs e)
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
            catch (InvalidOperationException f)
            {
                state = State.NoSC2;
            }
        }

        void OnNameChanged(Model.Player p)
        {
            if (state == State.InGame)
            {
                try
                {
                    if (p.Name != String.Empty && GameState.IsObserver())
                    {
                        var players = Player.GetCurrentPlayers();
                        if (players.TrueForAll(n => db.DisplayName(n.Name) != p.Name))
                        {
                            db.UpdateDisplayName(
                                players.First(n => n.Color == p.Color).Name,
                                p.Name);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    state = State.NoSC2;
                }
            }
        }

        void OnRaceChanged(Model.Player p)
        {
            if (state == State.InGame)
            {
                try
                {
                    if (p.Race != Model.Race.Unknown && GameState.IsObserver())
                    {
                        var players = Player.GetCurrentPlayers();
                        if (players.TrueForAll(n => db.DisplayName(n.Name) != p.Name))
                        {
                            db.UpdateDisplayRace(
                                players.First(n => n.Color == p.Color).Name,
                                p.Race);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    state = State.NoSC2;
                }
            }
        }

        /// <summary>
        /// Fill scoreboard with information.
        /// </summary>
        void UpdateScoreboard()
        {
            var playerList = Player.GetCurrentPlayers();
            if (playerList.Count != 2) return; // 2v2, 1v0, etc.
            var player1 = playerList[0];
            var player2 = playerList[1];
            var player1Name = player1.Name;
            var player2Name = player2.Name;

            Model.Player p1 = scoreboard.Player1;
            Model.Player p2 = scoreboard.Player2;

            p1.Name = db.DisplayName(player1Name) ?? player1Name;
            p1.Race = db.DisplayRace(player1Name) ?? player1.Race;
            p1.Color = player1.Color;

            p2.Name = db.DisplayName(player2Name) ?? player2Name;
            p2.Race = db.DisplayRace(player2Name) ?? player2.Race;
            p2.Color = player2.Color;
        }

        /// <summary>
        /// Check for victory/defeat, update accordingly.
        /// </summary>
        void CheckGameStatus()
        {
            var playerList = Player.GetCurrentPlayers();
            if (playerList.Count > 2 || playerList.Count == 0) return;

            var player1 = playerList[0];
            if (player1.Status == Status.Undecided) return;

            state = State.EndGame;

            if (player1.Status == Status.Victory)
            {
                if (scoreboard.Player1.Color == player1.Color)
                    scoreboard.Player1.Score++;
                else scoreboard.Player2.Score++;
            }
            else
            {
                if (scoreboard.Player1.Color == player1.Color)
                    scoreboard.Player2.Score++;
                else scoreboard.Player1.Score++;
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
