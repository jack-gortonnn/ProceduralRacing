using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

public class Track
{
    private TrackGenerator Generator;

    private Grid Grid;

    public TrackInfo Info;

    public List<PlacedPiece> Pieces => Generator.Track;

    public int Seed { get; private set; }

    public TrackDifficulty Difficulty { get; private set; }

    private Texture2D backgroundTexture;

    public Track(Grid grid, int seed, TrackDifficulty difficulty)
    {
        Grid = grid;
        Seed = seed;
        Difficulty = difficulty;
        InitGenerator();
    }

    public void LoadContent(ContentManager content)
    {
        if (!string.IsNullOrEmpty(Info.BackgroundPath))
        {
            backgroundTexture = content.Load<Texture2D>(Info.BackgroundPath);
        }
    }

    private void InitGenerator()
    {
        Info = TrackInfo.Generate(Seed);
        Generator = new TrackGenerator(Grid, Seed, Difficulty);
        Generator.BeginTrack();
    }

    public void Update(GameTime gameTime)
    {
        Generator.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport, Camera camera)
    {
        if (backgroundTexture != null)
        {
            int texWidth = backgroundTexture.Width;
            int texHeight = backgroundTexture.Height;

            Vector2 cameraPos = camera.Position;

            float left = cameraPos.X - (viewport.Width / 2f) / camera.Zoom;
            float top = cameraPos.Y - (viewport.Height / 2f) / camera.Zoom;

            int startX = (int)Math.Floor(left / texWidth) * texWidth;
            int startY = (int)Math.Floor(top / texHeight) * texHeight;

            int endX = (int)Math.Ceiling((left + viewport.Width / camera.Zoom) / texWidth) * texWidth + texWidth + texWidth + texWidth + texWidth + texWidth + texWidth;
            int endY = (int)Math.Ceiling((top + viewport.Height / camera.Zoom) / texHeight) * texHeight + texHeight + texWidth + texWidth + texWidth + texWidth + texWidth;

            for (int x = startX; x < endX; x += texWidth)
            {
                for (int y = startY; y < endY; y += texHeight)
                {
                    spriteBatch.Draw(
                        backgroundTexture,
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        1f
                    );
                }
            }
        }

        foreach (var piece in Pieces)
        {
            piece.Draw(spriteBatch);
        }
    }

    public void Reset(ContentManager content, int? newSeed = null, TrackDifficulty? newDifficulty = null)
    {
        if (newSeed.HasValue) Seed = newSeed.Value;
        if (newDifficulty.HasValue) Difficulty = newDifficulty.Value;

        InitGenerator();
        LoadContent(content);
    }

    public void SetDifficulty(ContentManager content, TrackDifficulty newDifficulty)
    {
        Difficulty = newDifficulty;
        InitGenerator();
        LoadContent(content);
    }

    public void SetSeed(ContentManager content, int newSeed)
    {
        Seed = newSeed;
        InitGenerator();
        LoadContent(content);
    }
}
