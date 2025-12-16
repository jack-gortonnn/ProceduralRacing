using Microsoft.Xna.Framework;
using System.Collections.Generic;
public static class PieceLibrary
{
    public static List<TrackPiece> All = new List<TrackPiece>()
    {
        new TrackPiece("1x1_straight", new Point(1,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,0), new Point(1,0)),
        }),
        new TrackPiece("1x1_turn", new Point(1,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,0), new Point(0,1)),
        }),
        new TrackPiece("2x1_hairpin", new Point(2,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,1)),
            new Connection(new Point(1,0), new Point(0,1)),
        }),
        new TrackPiece("2x2_chicane", new Point(2,2), new List<Connection>{
            new Connection(new Point(1,0), new Point(0,-1)),
            new Connection(new Point(0,1), new Point(0,1)),
        }),
        new TrackPiece("2x2_singaporesling", new Point(2,2), new List<Connection>{
            new Connection(new Point(1,0), new Point(1,0)),
            new Connection(new Point(0,1), new Point(0,1)),
        }),
        new TrackPiece("2x2_tightturn", new Point(2,2), new List<Connection>{
            new Connection(new Point(1,0), new Point(1,0)),
            new Connection(new Point(0,1), new Point(0,1)),
        }),
        new TrackPiece("2x2_turn", new Point(2,2), new List<Connection>{
            new Connection(new Point(1,0), new Point(1,0)),
            new Connection(new Point(0,1), new Point(0,1)),
        }),
        new TrackPiece("3x1_grid", new Point(3,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0)),
        }),
        new TrackPiece("3x2_aintree", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,1), new Point(1,0)),
        }),
        new TrackPiece("3x2_chicane", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0)),
        }),
        new TrackPiece("3x2_club", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,1), new Point(1,0)),
        }),
        new TrackPiece("3x2_copse", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,0), new Point(1,0)),
        }),
        new TrackPiece("3x2_hairpin2", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,1), new Point(0,1)),
        }),
        new TrackPiece("3x2_hairpin", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,1), new Point(1,0)),
        }),
        new TrackPiece("3x2_hungary", new Point(3,2), new List<Connection>{
            new Connection(new Point(2,0), new Point(1,0)),
            new Connection(new Point(2,1), new Point(1,0)),
        }),
        new TrackPiece("3x2_monza", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(1,0)),
        }),
        new TrackPiece("3x2_singapore", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,1), new Point(0,1)),
        }),
        new TrackPiece("3x2_snake", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(1,1), new Point(0,1)),
        }),
        new TrackPiece("3x2_zandvoort", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(0,1)),
            new Connection(new Point(2,1), new Point(0,1)),
        }),
        new TrackPiece("3x3_horseshoe", new Point(3,3), new List<Connection>{
            new Connection(new Point(1,2), new Point(0,1)),
            new Connection(new Point(2,1), new Point(1,0)),
        }),
    };
}