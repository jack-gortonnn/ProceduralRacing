using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

// The Track class owns the TrackGenerator and manages the overall track generation process.
// It uses a TrackGenerator to create a series of PlacedPieces based on a seed and difficulty level.
// The class handles loading background textures, updating the track generation process,
// and drawing the track pieces to the screen. It also provides functionality to reset the track with new parameters.
public class Track
{
    private TrackGenerator Generator;

    private Grid Grid;

    public TrackInfo Info;

    public List<PlacedPiece> Pieces => Generator.Track;

    public bool IsComplete => Generator.TrackIsClosed;

    public int Seed { get; private set; }

    public TrackDifficulty Difficulty { get; private set; }

    private Texture2D backgroundTexture;

    public Track(Grid grid, int seed, TrackDifficulty difficulty)
    { // Initializes the track with a grid, seed, and difficulty level, and starts the track generation process
        Grid = grid;
        Seed = seed;
        Difficulty = difficulty;
        InitGenerator();
    }

    public void LoadContent(ContentManager content)
    { // Loads only the background texture, track textures are handled in the PieceLibrary
        backgroundTexture = content.Load<Texture2D>(Info.BackgroundPath);
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

    public void Draw(SpriteBatch spriteBatch, Grid grid)
    {
        grid.DrawGridBackground(spriteBatch, backgroundTexture);

        foreach (var piece in Pieces)
        {
            piece.Draw(spriteBatch);
        }
    }
}
