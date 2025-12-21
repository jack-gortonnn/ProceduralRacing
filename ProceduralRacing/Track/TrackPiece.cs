using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
public class TrackPiece
{
    public string Name { get; set; }
    public Point Size { get; set; }
    public TrackType Type { get; }
    public List<Connection> Connections { get; set; }

    public Texture2D Texture { get; set; }

    public TrackPiece(string name, Point size, TrackType type, List<Connection> connections)
    {
        Name = name;
        Size = size;
        Type = type;
        Connections = connections;
    }

	// I don't like that this logic is here (trackpiece SHOULD be static data) but it's convenient for now,
	// and this way we don't have to make a temporary PlacedPiece just to get transformed connections
	public List<Connection> GetTransformedConnections(int rotation = 0, bool flipped = false)
	{
		List<Connection> transformedConnections = new List<Connection>();
		int w = Size.X;
		int h = Size.Y;

		foreach (var con in Connections)
		{
			Point pos = con.Position;
			Point dir = con.Direction;

			if (flipped)
			{
				pos = new Point(w - 1 - pos.X, pos.Y);
				dir = new Point(-dir.X, dir.Y);
			}

			Point finalPos = rotation switch
			{
				1 => new Point(h - 1 - pos.Y, pos.X),
				2 => new Point(w - 1 - pos.X, h - 1 - pos.Y),
				3 => new Point(pos.Y, w - 1 - pos.X),
				_ => pos
			};
			Point finalDir = rotation switch
			{
				1 => new Point(-dir.Y, dir.X),
				2 => new Point(-dir.X, -dir.Y),
				3 => new Point(dir.Y, -dir.X),
				_ => dir
			};

			transformedConnections.Add(new Connection(finalPos, finalDir));
		}

		return transformedConnections;
	}
}

// Enum to categorize track pieces by type, used for generation logic
public enum TrackType { Grid, Straight, Turn, Hairpin, Chicane, Complex }
