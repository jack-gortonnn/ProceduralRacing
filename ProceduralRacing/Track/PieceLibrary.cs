using Microsoft.Xna.Framework;
using System.Collections.Generic;
public static class PieceLibrary
{
    public static List<TrackPiece> All = new List<TrackPiece>()
    {
        new TrackPiece("5x1_grid", new Point(5,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(4,0), new Point(1,0))
        }),
        new TrackPiece("1x1_straight", new Point(1,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,0), new Point(1,0))
        }),
        new TrackPiece("2x1_straight", new Point(2,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,0), new Point(1,0))
        }),
        new TrackPiece("3x1_straight", new Point(3,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("4x1_straight", new Point(4,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("5x1_straight", new Point(5,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(4,0), new Point(1,0))
        }),
        new TrackPiece("1x1_turn", new Point(1,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,0), new Point(0,1))
        }),
        new TrackPiece("2x1_turn", new Point(2,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_turn", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_bendturn", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_boxturn", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_tightturn", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_zturn", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_copse", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_aintree", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_openaintree", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_wideturn", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_openclub", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x3_turn", new Point(3,3), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,2), new Point(0,1))
        }),
        new TrackPiece("4x3_copse", new Point(4,3), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(3,2), new Point(0,1))
        }),
        new TrackPiece("4x3_copsestraight", new Point(4,3), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(3,2), new Point(0,1))
        }),
        new TrackPiece("2x1_hairpin", new Point(2,1), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,1)),
            new Connection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_monacohairpin", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_hairpin", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(2,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_doubleapex", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(2,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_hungaryhairpin", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,1), new Point(-1,0))
        }),
        new TrackPiece("3x3_parabolica", new Point(3,3), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,2), new Point(-1,0))
        }),
        new TrackPiece("2x2_smoothchicane", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_tightchicane", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_smoothchicane", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("3x2_tightchicane", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("3x2_monzachicane", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(1,0))
        }),
        new TrackPiece("3x2_nouveauchicane", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(1,0))
        }),
        new TrackPiece("4x2_smoothchicane", new Point(4,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("4x2_tightchicane", new Point(4,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("2x2_singaporesling", new Point(2,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_ascari", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_barcelonalast", new Point(3,2), new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,0), new Point(0,-1))
        }),
        new TrackPiece("3x3_saopaulo", new Point(3,3), new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,2), new Point(0,1))
        }),
        new TrackPiece("5x2_maggotsbecketts", new Point(5,2), new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(4,1), new Point(1,0))
        }),
    };
}