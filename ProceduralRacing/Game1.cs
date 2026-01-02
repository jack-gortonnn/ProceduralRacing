using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ProceduralRacing
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Camera camera;
        private Grid grid;
        private Track track;
        private List<TrackPiece> pieces;

        private Random random = new Random();
        private int seed;

        private float timer = 0f;

        private Texture2D pixel;
        private SpriteFont font;

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
            pieces = PieceLibrary.All;

            track = new Track(grid, seed, TrackDifficulty.Easy);

            camera = new Camera(new Vector2(0, 0), 1f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (var piece in pieces)
            {
                piece.Texture = Content.Load<Texture2D>($"textures/{piece.Name}");
            }

            PieceLibrary.StartingPiece.Texture =
                Content.Load<Texture2D>($"textures/{PieceLibrary.StartingPiece.Name}");

            font = Content.Load<SpriteFont>("fonts/Arial");

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            Vector2 move = Vector2.Zero;

            if (kb.IsKeyDown(Keys.W)) move.Y -= 10f;
            if (kb.IsKeyDown(Keys.S)) move.Y += 10f;
            if (kb.IsKeyDown(Keys.A)) move.X -= 10f;
            if (kb.IsKeyDown(Keys.D)) move.X += 10f;

            camera.Move(move);

            if (kb.IsKeyDown(Keys.Q)) camera.AddZoom(-0.01f);
            if (kb.IsKeyDown(Keys.E)) camera.AddZoom(0.01f);

            // --- Difficulty selection via number keys ---
            if (kb.IsKeyDown(Keys.D1)) track.SetDifficulty(TrackDifficulty.Easy);
            if (kb.IsKeyDown(Keys.D2)) track.SetDifficulty(TrackDifficulty.Medium);
            if (kb.IsKeyDown(Keys.D3)) track.SetDifficulty(TrackDifficulty.Hard);
            if (kb.IsKeyDown(Keys.D4)) track.SetDifficulty(TrackDifficulty.Extreme);

            if (kb.IsKeyDown(Keys.R)) track.SetSeed(random.Next(10000,99999));

            if (timer > Constants.SecondsPerStep)
            {
                track.Update(gameTime);
                timer = 0f;
            }
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(),
                samplerState: Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp);


            track.Draw(spriteBatch);

            spriteBatch.DrawString(font, $"Seed {track.Seed}", new Vector2(10, 10), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
