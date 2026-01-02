using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

// The Track class manages the procedural generation of a racing track using a TrackGenerator.
// It holds the generated pieces, seed, and difficulty level, and provides methods to update, draw, and reset the track.

public class Track
{
    private TrackGenerator generator;
    private Grid grid;

    public List<PlacedPiece> Pieces => generator.Track;
    public int Seed { get; private set; }
    public TrackDifficulty Difficulty { get; private set; }

    public Track(Grid grid, int seed, TrackDifficulty difficulty)
    {
        this.grid = grid;
        Seed = seed;
        Difficulty = difficulty;

        InitGenerator();
    }

    private void InitGenerator()
    {
        generator = new TrackGenerator(grid, Seed, Difficulty);
        generator.BeginTrack();
    }

    public void Update(GameTime gameTime)
    {
        generator.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var piece in Pieces)
        {
            piece.Draw(spriteBatch);
        }
    }

    public void Reset(int? newSeed = null, TrackDifficulty? newDifficulty = null)
    {
        if (newSeed.HasValue) Seed = newSeed.Value;
        if (newDifficulty.HasValue) Difficulty = newDifficulty.Value;

        InitGenerator();
    }

    public void SetDifficulty(TrackDifficulty newDifficulty)
    {
        Difficulty = newDifficulty;
        InitGenerator();
    }

    public void SetSeed(int newSeed)
    {
        Seed = newSeed;
        InitGenerator();
    }
}
