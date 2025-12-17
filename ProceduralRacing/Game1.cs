using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProceduralRacing
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private TrackGenerator generator;
        private List<PlacedPiece> track;
        private List<TrackPiece> pieces;

        private Texture2D pixel;
        private Grid grid;

        private Vector2 worldOffset = new Vector2(0,0);

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
            grid = new Grid(minX: 0, maxX: 25, minY: 0, maxY: 25, tileSize: Constants.TileSize);
            pieces = PieceLibrary.All;
            generator = new TrackGenerator(pieces, grid);
            track = generator.GenerateTrack();

            // Mark occupied cells in grid
            foreach (var piece in track)
            {
                grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);

                Debug.WriteLine($"Placed {piece.BasePiece.Name} at {piece.GridPosition} | Rotation: {piece.Rotation * 90}° | Flipped: {piece.IsFlipped}");
                foreach (var conn in piece.TransformedConnections)
                {
                    Debug.WriteLine($"   Connection: Pos={conn.Position} Dir={conn.Direction}");
                }
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

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            grid.Draw(spriteBatch, pixel, worldOffset);

            foreach (var piece in track)
            {
                piece.Draw(spriteBatch, worldOffset, pixel);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
