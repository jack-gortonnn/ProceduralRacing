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
        private Car car;

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
            Vector2 startPos = new Vector2((Constants.TileSize * 14) + 8, (Constants.TileSize * 14) + 44);
            seed = random.Next(1, 9999999);
            grid = new Grid(0, 25, 0, 25, Constants.TileSize);
            track = new Track(grid, seed, TrackDifficulty.Easy);
            car = new Car(startPos, CarPreset.Drifty);
            camera = new Camera(Vector2.Zero,1f);

            Interface.Initialize(Content, GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            car.LoadContent(Content);
            PieceLibrary.LoadContent(Content);
            track.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                car.ResetCar();
            }

            // --- Track generation tick ---
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= Constants.SecondsPerStep)
            {
                track.Update(gameTime);
                timer = 0f;
            }

            // --- Update car ---
            car.Update(gameTime);

            // --- Update camera ---
            camera.FollowCar(car, GraphicsDevice.Viewport, dt, 15f);
            camera.FollowRotation(car.Rotation, dt, 15f);
            camera.UpdateZoom(gameTime, kb);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // --- World ---
            spriteBatch.Begin(
                transformMatrix: camera.GetViewMatrix(GraphicsDevice.Viewport),
                samplerState: SamplerState.PointClamp
            );

            track.Draw(spriteBatch, grid);

            car.Draw(spriteBatch);

            spriteBatch.End();

            // --- UI ---
            spriteBatch.Begin();

            Interface.DrawTextWithBorder(spriteBatch, $"Seed - {track.Seed}", new Vector2(10, 10), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Name - {track.Info.Name}",new Vector2(10, 58), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Region - {track.Info.RegionName}",new Vector2(10, 106), Color.White, Color.Black, 2);
            Interface.DrawTextWithBorder(spriteBatch, $"Difficulty - {track.Difficulty}",new Vector2(10, 154), Color.White, Color.Black ,2);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
