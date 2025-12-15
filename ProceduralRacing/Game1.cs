using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProceduralRacing
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        TrackGenerator generator;
        List<PlacedPiece> track;

        List<TrackPiece> pieces;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 800;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            pieces = PieceLibrary.All;
            generator = new TrackGenerator(pieces);
            track = generator.GenerateTrack();

            Debug.WriteLine($"{track.Count} pieces generated.");

            foreach (var p in track)
            {
                Debug.WriteLine($"{p.BasePiece.Name} at {p.GridPosition}");
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (var piece in track)
            {
                piece.BasePiece.Texture = Content.Load<Texture2D>($"textures/{piece.BasePiece.Name}");
            }

        }

        protected override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            foreach (var piece in track)
            {
                piece.Draw(spriteBatch, new Vector2(100, 100));
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
