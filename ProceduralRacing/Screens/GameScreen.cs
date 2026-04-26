using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProceduralRacing
{
    public class GameScreen : Screen
    {
        private Game1 game;
        private Camera camera;
        private Grid grid;
        private Track track;
        private Car car;

        private Random random = new Random();
        private int seed;

        private float timer = 0f;

        private float fpsTimer;
        private int fpsDisplay;
        private int fpsFrames;

        public GameScreen(Game1 _game, int _seed)
        {
            game = _game;
            Vector2 startPos = new Vector2((Settings.Generation.TileSize * 14) + 8, (Settings.Generation.TileSize * 14) + 44);
            seed = _seed;
            grid = new Grid(0, 25, 0, 25, Settings.Generation.TileSize);
            track = new Track(grid, seed, TrackDifficulty.Easy);
            car = new Car(startPos, Chassis.Basic, Engine.Basic, Tyres.Basic);
            camera = new Camera(startPos);
            car.LoadContent(game.Content);
            PieceLibrary.LoadContent(game.Content);
            track.LoadContent(game.Content);

            Interface.Initialize(game.Content, game.GraphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // --- Difficulty selection ---
            if (kb.IsKeyDown(Keys.D1)) track.SetDifficulty(game.Content, TrackDifficulty.Easy);
            if (kb.IsKeyDown(Keys.D2)) track.SetDifficulty(game.Content, TrackDifficulty.Medium);
            if (kb.IsKeyDown(Keys.D3)) track.SetDifficulty(game.Content, TrackDifficulty.Hard);
            if (kb.IsKeyDown(Keys.D4)) track.SetDifficulty(game.Content, TrackDifficulty.Extreme);

            // --- FPS calculation ---
            fpsFrames++;
            fpsTimer += dt;
            if (fpsTimer >= 0.5f)
            {
                fpsDisplay = (int)(fpsFrames / fpsTimer);
                fpsFrames = 0;
                fpsTimer = 0f;
            }

            // --- Regenerate track ---
            if (kb.IsKeyDown(Keys.R))
            {
                int newSeed = random.Next(10000, 99999);
                track.Reset(game.Content, newSeed);
                car.ResetCar();
            }

            // --- Track generation tick ---
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= Settings.Generation.SecondsPerStep)
            {
                track.Update(gameTime);
                timer = 0f;
            }

            // --- Update car ---
            car.Update(gameTime, track.Pieces);

            // --- Update camera ---
            camera.Update(dt, car, game.GraphicsDevice.Viewport);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);

            // --- World ---
            spriteBatch.Begin(
                transformMatrix: camera.GetViewMatrix(game.GraphicsDevice.Viewport),
                samplerState: SamplerState.PointClamp
            );

            var sw = System.Diagnostics.Stopwatch.StartNew();
            track.Draw(spriteBatch, grid);
            sw.Stop();

            car.Draw(spriteBatch);

            spriteBatch.End();

            // --- UI ---
            spriteBatch.Begin();

            Interface.DrawTextWithBorder(spriteBatch, $"Seed - {track.Seed}", new Vector2(10, 10), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Name - {track.Info.Name}",new Vector2(10, 58), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Region - {track.Info.RegionName}",new Vector2(10, 106), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Difficulty - {track.Difficulty}",new Vector2(10, 154), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"On track - {car.isOnTrack}", new Vector2(10, 200), Color.White, Color.Black, 2);

            Interface.DrawTextWithBorder(spriteBatch, $"Gear - {car.Gear}", new Vector2(10, 500), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"RPM - {car.RPM}", new Vector2(10, 548), Color.White, Color.Black, 2);

            Interface.DrawTextWithBorder(spriteBatch, $"FPS - {fpsDisplay}", new Vector2(10, 596), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Track draw: {sw.ElapsedMilliseconds}ms", new Vector2(10, 692), Color.White, Color.Black, 2);

            spriteBatch.End();
        }
    }
}
