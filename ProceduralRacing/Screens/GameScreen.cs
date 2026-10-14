using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProceduralRacing
{
    // Runs a race for 1 player (time trial, endless laps) or 2 players (1v1 split screen, fixed laps).
    // Order of play: track generates -> countdown -> race -> results.
    //
    // Penalties:
    //   1v1        - leaving the track or cutting more than SkipGrace pieces adds 5s to your total time
    //   time trial - either one invalidates the current lap (it won't count as a personal best)

    public class GameScreen : Screen
    {
        private class Player
        {
            public Car Car;
            public Camera Camera;
            public LapTimer Laps = new LapTimer();
            public Vector2 LastPosition;
            public bool WasOnTrack = true;

            public double PenaltySeconds;
            public bool Finished;
            public double FinishTime;  // race time + penalties
            public int Place;

            public string Message = "";  // pop-up text in the centre of the screen
            public Color MessageColor;
            public float MessageTimer;
        }

        private static readonly Keys[][] Controls =
        {
            new[] { Keys.W, Keys.S, Keys.A, Keys.D },
            new[] { Keys.NumPad8, Keys.NumPad5, Keys.NumPad4, Keys.NumPad6 },
        };
        private static readonly Vector2[] GridSlots = { Vector2.Zero, new Vector2(-10, -24) };

        private readonly Game1 game;
        private readonly Grid grid;
        private readonly Track track;
        private readonly Player[] players;
        private readonly int totalLaps;
        private bool IsTimeTrial => players.Length == 1;

        private float countdown = Settings.Race.CountdownDuration;
        private bool racing;
        private double raceTime;
        private bool resultsShown;

        private int fps, fpsFrames;
        private float fpsTimer;

        public GameScreen(Game1 game, int seed, TrackDifficulty difficulty, CarLoadout[] loadouts)
        {
            this.game = game;
            grid = new Grid(0, Settings.Generation.GridSize, 0, Settings.Generation.GridSize, Settings.Generation.TileSize);
            track = new Track(grid, seed, difficulty);
            track.LoadContent(game.Content);
            totalLaps = TrackConfig.FromDifficulty(difficulty).Laps;

            // Spawn just behind the start/finish line, which is the exit of the 5-tile starting grid piece
            int tile = Settings.Generation.TileSize;
            int gridLength = PieceLibrary.StartingPiece.Size.X;
            Vector2 startPos = new Vector2((Settings.Generation.TrackOriginX + gridLength - 1) * tile + 8, Settings.Generation.TrackOriginY * tile + 44);

            players = new Player[loadouts.Length];
            for (int i = 0; i < players.Length; i++)
            {
                Vector2 spawn = startPos + GridSlots[i];
                var car = new Car(spawn, loadouts[i], Controls[i][0], Controls[i][1], Controls[i][2], Controls[i][3]);
                car.LoadContent(game.Content);
                car.Tint = Interface.PlayerColors[i];
                if (players.Length == 2) car.SetAudioPan(i == 0 ? -0.8f : 0.8f); // each player hears their car on their side

                players[i] = new Player { Car = car, Camera = new Camera(spawn), LastPosition = spawn };
            }
        }

        // --- Update ---

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateFps(dt);

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || (resultsShown && Keyboard.GetState().IsKeyDown(Keys.Enter)))
            {
                foreach (var p in players) p.Car.StopAudio();
                game.GoToMenu();
                return;
            }

            // 1. Generate the track (one step per frame so you can watch it build), then place checkpoints
            if (!track.IsComplete)
            {
                track.Update(gameTime);
                if (track.IsComplete)
                    foreach (var p in players) p.Laps.BuildCheckpoints(track.Pieces, Settings.Generation.TileSize);
                return;
            }

            // 2. Countdown
            if (!racing)
            {
                countdown -= dt;
                racing = countdown <= 0f;
            }
            else raceTime += dt;

            // 3. Each player
            foreach (var p in players)
            {
                p.MessageTimer -= dt;
                p.LastPosition = p.Car.Position;

                if (racing && !p.Finished)
                {
                    Car opponent = players.Length == 2 ? players.First(o => o != p).Car : null;
                    p.Car.Update(dt, track.Pieces, opponent);
                    p.Laps.Update(dt, p.Car.Position, p.LastPosition);
                    HandleLapEvents(p);
                    CheckTrackLimits(p);
                }
                p.Camera.Update(dt, p.Car);
            }

            // 4. Car vs car
            if (players.Length == 2 && racing)
                CarCollision.ResolveCarVsCar(players[0].Car, players[1].Car);

            // 5. Results once everyone has finished - ranked by total time including penalties
            if (!IsTimeTrial && !resultsShown && players.All(p => p.Finished))
            {
                var order = players.OrderBy(p => p.FinishTime).ToList();
                for (int i = 0; i < order.Count; i++) order[i].Place = i + 1;
                resultsShown = true;
            }
        }

        private void CheckTrackLimits(Player p)
        { // Penalise once, on the frame the car goes from on-track to off-track
            bool onTrack = p.Car.IsOnTrack;
            if (p.WasOnTrack && !onTrack)
            {
                if (IsTimeTrial)
                {
                    if (p.Laps.CurrentLapValid) ShowMessage(p, "TRACK LIMITS - LAP INVALID", Interface.Bad);
                    p.Laps.InvalidateLap();
                }
                else
                {
                    p.PenaltySeconds += Settings.Race.TrackLimitsPenalty;
                    ShowMessage(p, $"TRACK LIMITS  +{Settings.Race.TrackLimitsPenalty:0}s", Interface.Bad);
                }
            }
            p.WasOnTrack = onTrack;
        }

        private void HandleLapEvents(Player p)
        {
            if (p.Laps.LastEvent == LapEvent.LapInvalidated)
            {
                if (IsTimeTrial)
                    ShowMessage(p, "CUT TRACK - LAP INVALID", Interface.Bad);
                else
                {
                    p.PenaltySeconds += Settings.Race.CutPenalty;
                    ShowMessage(p, $"CUT TRACK  +{Settings.Race.CutPenalty:0}s", Interface.Bad);
                }
            }

            if (p.Laps.LastEvent == LapEvent.LapCompleted || (p.Laps.LastEvent == LapEvent.LapInvalidated && p.Laps.CurrentLapTime == 0))
                ShowLapFeedback(p);

            if (!IsTimeTrial && p.Laps.LapsCompleted >= totalLaps && !p.Finished)
            {
                p.Finished = true;
                p.FinishTime = raceTime + p.PenaltySeconds;
                p.Car.StopAudio();
                ShowMessage(p, "FINISHED", Interface.Text);
            }
        }

        private void ShowLapFeedback(Player p)
        { // Pops up the lap time, coloured: purple = new best, green = faster than best, red = slower
            var laps = p.Laps;
            string time = Interface.FormatTime(laps.LastLapTime);

            if (!laps.LastLapValid) ShowMessage(p, $"{time}  INVALID", Interface.Bad);
            else if (laps.LastLapWasBest) ShowMessage(p, $"{time}  NEW BEST", Interface.Best);
            else ShowMessage(p, $"{time}  {laps.LastLapDelta:+0.000;-0.000}", laps.LastLapDelta < 0 ? Interface.Good : Interface.Bad);
        }

        private static void ShowMessage(Player p, string text, Color color)
        {
            p.Message = text;
            p.MessageColor = color;
            p.MessageTimer = 2.5f;
        }

        private void UpdateFps(float dt)
        {
            fpsFrames++;
            fpsTimer += dt;
            if (fpsTimer >= 0.5f) { fps = (int)(fpsFrames / fpsTimer); fpsFrames = 0; fpsTimer = 0; }
        }

        // --- Draw ---

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var full = new Viewport(0, 0, game.ScreenWidth, game.ScreenHeight);

            for (int i = 0; i < players.Length; i++)
            {
                int width = game.ScreenWidth / players.Length;
                var viewport = new Viewport(i * width, 0, width, game.ScreenHeight);
                game.GraphicsDevice.Viewport = viewport;
                DrawWorld(sb, viewport, i);
                DrawHud(sb, viewport, players[i]);
            }

            game.GraphicsDevice.Viewport = full;
            sb.Begin();
            if (players.Length == 2)
                Interface.DrawRect(sb, new Rectangle(game.ScreenWidth / 2 - 3, 0, 6, game.ScreenHeight), Color.Black);
            if (resultsShown) DrawResults(sb);
            Interface.DrawText(sb, $"{fps} FPS", new Vector2(10, game.ScreenHeight - 30), Interface.SubText * 0.6f, 0.6f, shadow: false);
            sb.End();
        }

        private void DrawWorld(SpriteBatch sb, Viewport viewport, int playerIndex)
        {
            sb.Begin(transformMatrix: players[playerIndex].Camera.GetViewMatrix(viewport), samplerState: SamplerState.PointClamp);
            track.Draw(sb, grid);
            foreach (var p in players) p.Car.Draw(sb);
            sb.End();
        }

        private void DrawHud(SpriteBatch sb, Viewport vp, Player p)
        {
            sb.Begin();
            int w = vp.Width, h = vp.Height;
            var laps = p.Laps;

            if (!track.IsComplete)
            {
                Interface.DrawText(sb, "GENERATING TRACK...", new Vector2(w / 2f, h / 2f), Interface.Text, 1.5f, centered: true);
                sb.End();
                return;
            }

            // Timing panel (top left)
            var timing = new Rectangle(20, 20, 380, 230);
            Interface.DrawPanel(sb, timing);
            string lapText = IsTimeTrial ? $"LAP {laps.LapsCompleted + 1}" : $"LAP {Math.Min(laps.LapsCompleted + 1, totalLaps)}/{totalLaps}";
            Interface.DrawText(sb, lapText, new Vector2(40, 32), Interface.SubText);
            Interface.DrawText(sb, Interface.FormatTime(laps.CurrentLapTime), new Vector2(40, 70), laps.CurrentLapValid ? Interface.Text : Interface.Bad, 1.8f);
            Interface.DrawText(sb, "LAST", new Vector2(40, 150), Interface.SubText, 0.8f);
            Interface.DrawText(sb, Interface.FormatTime(laps.LastLapTime), new Vector2(150, 150), laps.LastLapValid ? Interface.Text : Interface.Bad, 0.8f);
            Interface.DrawText(sb, "BEST", new Vector2(40, 190), Interface.SubText, 0.8f);
            Interface.DrawText(sb, Interface.FormatTime(laps.BestLapTime), new Vector2(150, 190), Interface.Best, 0.8f);

            // Track info (top right)
            string[] info = { track.Info.Name, track.Info.RegionName, $"{track.Difficulty} - Seed {track.Seed}" };
            for (int i = 0; i < info.Length; i++)
            {
                float scale = i == 0 ? 1f : 0.7f;
                Interface.DrawText(sb, info[i], new Vector2(w - 30 - Interface.Measure(info[i], scale).X, 30 + i * 40), i == 0 ? Interface.Text : Interface.SubText, scale);
            }
            if (!IsTimeTrial && p.PenaltySeconds > 0)
            {
                string pen = $"PENALTIES +{p.PenaltySeconds:0}s";
                Interface.DrawText(sb, pen, new Vector2(w - 30 - Interface.Measure(pen, 0.8f).X, 150), Interface.Bad, 0.8f);
            }

            // Speed / gear / RPM (bottom right)
            var dash = new Rectangle(w - 330, h - 170, 310, 150);
            Interface.DrawPanel(sb, dash);
            Interface.DrawText(sb, p.Car.Gear.ToString(), new Vector2(dash.X + 30, dash.Y + 10), Interface.Text, 3f);
            Interface.DrawText(sb, $"{p.Car.Speed * 2.5f:0}", new Vector2(dash.X + 150, dash.Y + 20), Interface.Text, 1.6f);
            Interface.DrawText(sb, "KM/H", new Vector2(dash.X + 150, dash.Y + 80), Interface.SubText, 0.7f);
            float rpm = (p.Car.RPM - p.Car.Physics.RPMIdle) / (p.Car.Physics.RPMLimit - p.Car.Physics.RPMIdle);
            Interface.DrawBar(sb, new Rectangle(dash.X + 20, dash.Bottom - 30, dash.Width - 40, 12), rpm, rpm > 0.85f ? Interface.Bad : Interface.Good);

            // Pop-up message (fades out over its last half second)
            if (p.MessageTimer > 0)
                Interface.DrawText(sb, p.Message, new Vector2(w / 2f, h * 0.28f), p.MessageColor * MathHelper.Clamp(p.MessageTimer * 2f, 0f, 1f), 1.4f, centered: true);

            // Countdown
            if (!racing || raceTime < 0.8)
            {
                string text = racing ? "GO!" : Math.Max(1, (int)Math.Ceiling(countdown)).ToString();
                Interface.DrawText(sb, text, new Vector2(w / 2f, h * 0.42f), racing ? Interface.Good : Interface.Text, 4f, centered: true);
            }

            if (IsTimeTrial)
                Interface.DrawText(sb, "ESC - MENU", new Vector2(w / 2f, h - 30), Interface.SubText * 0.7f, 0.6f, centered: true, shadow: false);

            sb.End();
        }

        private void DrawResults(SpriteBatch sb)
        {
            var panel = new Rectangle(game.ScreenWidth / 2 - 400, game.ScreenHeight / 2 - 200, 800, 400);
            Interface.DrawRect(sb, new Rectangle(0, 0, game.ScreenWidth, game.ScreenHeight), Color.Black * 0.5f);
            Interface.DrawPanel(sb, panel);
            Interface.DrawText(sb, "RESULTS", new Vector2(panel.Center.X, panel.Y + 50), Interface.Text, 1.6f, centered: true);

            int i = 0;
            foreach (var p in players.OrderBy(p => p.Place))
            {
                int playerIndex = Array.IndexOf(players, p);
                float y = panel.Y + 120 + i++ * 70;
                Interface.DrawText(sb, $"P{p.Place}", new Vector2(panel.X + 50, y), p.Place == 1 ? Interface.Best : Interface.Text, 1.2f);
                Interface.DrawText(sb, $"PLAYER {playerIndex + 1}", new Vector2(panel.X + 160, y), Interface.PlayerColors[playerIndex], 1.2f);
                Interface.DrawText(sb, Interface.FormatTime(p.FinishTime), new Vector2(panel.X + 450, y), Interface.Text, 1.2f);
                if (p.PenaltySeconds > 0)
                    Interface.DrawText(sb, $"(+{p.PenaltySeconds:0}s)", new Vector2(panel.X + 650, y + 8), Interface.Bad, 0.8f);
            }

            Interface.DrawText(sb, "ENTER - MAIN MENU", new Vector2(panel.Center.X, panel.Bottom - 50), Interface.SubText, 0.8f, centered: true);
        }
    }
}
