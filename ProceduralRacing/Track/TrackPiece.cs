using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

// Track Pieces are the data definitions for individual segments of the track.
// They define the shape, size, type, and connection points of each piece,
// but do not have a specific position or orientation on the track grid.

public class TrackPiece
{
    // --- Properties ---
    public string Name { get; set; }
    public Point Size { get; set; }
    public TrackType Type { get; set; }
	public int Difficulty { get; set; }
    public List<Connection> Connections { get; set; }

    // --- Rendering ---
    public Texture2D Texture { get; set; }

    // --- Constructor ---
    public TrackPiece(string name, Point size, TrackType type, int difficulty, List<Connection> connections)
    {
        Name = name;
        Size = size;
        Type = type;
		Difficulty = difficulty;
        Connections = connections;
    }
}