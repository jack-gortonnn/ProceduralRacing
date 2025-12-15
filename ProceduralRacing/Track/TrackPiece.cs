using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public class TrackPiece
{
    public string Name { get; set; }
    public Point Size { get; set; }
    public List<Connection> Connections { get; set; }
    public Texture2D Texture { get; set; }

    public TrackPiece(string name, Point size, List<Connection> connections)
    {
        Name = name;
        Size = size;
        Connections = connections;
    }
}