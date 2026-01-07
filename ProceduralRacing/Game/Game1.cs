using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ProceduralRacing
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Camera camera;
        private Grid grid;
        private Track track;
        private Car car; // ADD THIS
        private Random random = new Random();
        private int seed;
        private float timer = 0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            seed = random.Next(1, 9999999);
            grid = new Grid(0, 25, 0, 25, Constants.TileSize);
            track = new Track(grid, seed, TrackDifficulty.Easy);

            Vector2 startPos = new Vector2(
                Constants.TrackOriginX * Constants.TileSize + (Constants.TileSize * 2.5f),
                Constants.TrackOriginY * Constants.TileSize + (Constants.TileSize * 0.5f)
            );

            camera = new Camera(startPos, 1f);
            car = new Car(startPos, 0f);

            Interface.Initialize(Content, GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            PieceLibrary.LoadContent(Content);
            track.LoadContent(Content);
            car.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            // ADD THIS - Update car
            car.Update(gameTime, kb);

            // UPDATE THIS - Make camera follow the car
            camera.Position = car.Position;
            camera.Update(gameTime, kb);

            // --- Difficulty selection ---
            if (kb.IsKeyDown(Keys.D1)) track.SetDifficulty(Content, TrackDifficulty.Easy);
            if (kb.IsKeyDown(Keys.D2)) track.SetDifficulty(Content, TrackDifficulty.Medium);
            if (kb.IsKeyDown(Keys.D3)) track.SetDifficulty(Content, TrackDifficulty.Hard);
            if (kb.IsKeyDown(Keys.D4)) track.SetDifficulty(Content, TrackDifficulty.Extreme);

            // --- Regenerate track ---
            if (kb.IsKeyDown(Keys.R))
            {
                int newSeed = random.Next(10000, 99999);
                track.Reset(Content, newSeed);

                // ADD THIS - Reset car position when track regenerates
                Vector2 startPos = new Vector2(
                    Constants.TrackOriginX * Constants.TileSize + (Constants.TileSize * 2.5f),
                    Constants.TrackOriginY * Constants.TileSize + (Constants.TileSize * 0.5f)
                );
                car.Reset(startPos, 0f);
            }

            // --- Track generation tick ---
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= Constants.SecondsPerStep)
            {
                track.Update(gameTime);
                timer = 0f;
            }

            camera.Rotation = car.Rotation + MathHelper.PiOver2;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Calculate screen center
            Vector2 screenCenter = new Vector2(
                GraphicsDevice.Viewport.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f
            );

            // --- World ---
            spriteBatch.Begin(
                transformMatrix: camera.GetViewMatrix(
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height
                ),
                samplerState: SamplerState.PointClamp
            );
            track.Draw(spriteBatch, GraphicsDevice.Viewport, camera);
            spriteBatch.End();

            // --- Car (drawn separately without camera transform, fixed at screen center) ---
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            car.Draw(spriteBatch, screenCenter, camera.Zoom);
            spriteBatch.End();

            // --- UI ---
            spriteBatch.Begin();
            Interface.DrawTextWithBorder(spriteBatch, $"Seed - {track.Seed}", new Vector2(10, 10), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Name - {track.Info.Name}", new Vector2(10, 58), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Region - {track.Info.RegionName}", new Vector2(10, 106), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Difficulty - {track.Difficulty}", new Vector2(10, 154), Color.White, Color.Black, 2);

            // ADD THIS - Show car info
            Interface.DrawTextWithBorder(spriteBatch, $"Speed - {car.Velocity.Length():F0}", new Vector2(10, 202), Color.White, Color.Black, 2);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}