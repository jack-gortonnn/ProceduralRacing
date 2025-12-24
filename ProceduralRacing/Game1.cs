using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProceduralRacing
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Camera camera;
        private Grid grid;
        private TrackGenerator generator;
        private List<PlacedPiece> track;
        private List<TrackPiece> pieces;

        private Random random = new Random();
        private int seed;

        private Texture2D pixel;
        private SpriteFont font;

        private Vector2 worldOffset = new Vector2(0,0);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true; // for now
            IsMouseVisible = true;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            seed = random.Next(1, 9999999);
            grid = new Grid(minX: 0, maxX: 25, minY: 0, maxY: 25, tileSize: Constants.TileSize);
            pieces = PieceLibrary.All;
            generator = new TrackGenerator(pieces, grid, seed);
            track = generator.GenerateTrack();
            camera = new Camera(new Vector2(0,0), 1f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (var piece in track)
            {
                piece.BasePiece.Texture = Content.Load<Texture2D>($"textures/{piece.BasePiece.Name}");
            }

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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(),
                samplerState: Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp);

            grid.Draw(spriteBatch, pixel);

            foreach (var piece in track)
            {
                piece.Draw(spriteBatch);
            }

            spriteBatch.DrawString(font, $"Seed {seed}", new Vector2(10, 10), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
