using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
public class PieceLibrary
{
    public static TrackPiece StartingPiece = 
        new TrackPiece("5x1_grid", new Point(5, 1), TrackType.Grid, 0, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(4,0), new Point(1,0))
    });
    public static List<TrackPiece> All = new List<TrackPiece>()
    {
        new TrackPiece("1x1_straight", new Point(1,1), TrackType.Straight, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,0), new Point(1,0))
        }),
        new TrackPiece("2x1_straight", new Point(2,1), TrackType.Straight, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,0), new Point(1,0))
        }),
        new TrackPiece("3x1_straight", new Point(3,1), TrackType.Straight, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("4x1_straight", new Point(4,1), TrackType.Straight, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("5x1_straight", new Point(5,1), TrackType.Straight, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(4,0), new Point(1,0))
        }),
        new TrackPiece("1x1_turn", new Point(1,1), TrackType.Turn, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,0), new Point(0,1))
        }),
        new TrackPiece("2x1_turn", new Point(2,1), TrackType.Turn, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_turn", new Point(2,2), TrackType.Turn, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_bendturn", new Point(2,2), TrackType.Turn, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_boxturn", new Point(2,2), TrackType.Turn, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_tightturn", new Point(2,2), TrackType.Turn, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_zturn", new Point(2,2), TrackType.Turn, 4, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_copse", new Point(3,2), TrackType.Turn, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_aintree", new Point(3,2), TrackType.Turn, 3, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_openaintree", new Point(3,2), TrackType.Turn, 2, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_wideturn", new Point(3,2), TrackType.Turn, 1, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_openclub", new Point(3,2), TrackType.Turn, 2, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x3_turn", new Point(3,3), TrackType.Turn, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,2), new Point(0,1))
        }),
        new TrackPiece("4x3_copse", new Point(4,3), TrackType.Turn, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(3,2), new Point(0,1))
        }),
        new TrackPiece("4x3_copsestraight", new Point(4,3), TrackType.Turn, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(3,2), new Point(0,1))
        }),
        new TrackPiece("2x1_hairpin", new Point(2,1), TrackType.Hairpin, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,1)),
            new Connection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_monacohairpin", new Point(2,2), TrackType.Hairpin, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_hairpin", new Point(3,2), TrackType.Hairpin, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(2,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_doubleapex", new Point(3,2), TrackType.Hairpin, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(2,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_hungaryhairpin", new Point(3,2), TrackType.Hairpin, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,1), new Point(-1,0))
        }),
        new TrackPiece("3x3_parabolica", new Point(3,3), TrackType.Hairpin, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(0,2), new Point(-1,0))
        }),
        new TrackPiece("2x1_chicane", new Point(2,1), TrackType.Chicane, 1, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_smoothchicane", new Point(2,2), TrackType.Chicane, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_tightchicane", new Point(2,2), TrackType.Chicane, 2, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_smoothchicane", new Point(3,2), TrackType.Chicane, 1, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("3x2_tightchicane", new Point(3,2), TrackType.Chicane, 1, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("3x2_monzachicane", new Point(3,2), TrackType.Chicane, 2, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(1,0))
        }),
        new TrackPiece("3x2_nouveauchicane", new Point(3,2), TrackType.Chicane, 3, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(1,0))
        }),
        new TrackPiece("4x2_smoothchicane", new Point(4,2), TrackType.Chicane, 2, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("4x2_tightchicane", new Point(4,2), TrackType.Chicane, 3, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("2x2_singaporesling", new Point(2,2), TrackType.Complex, 4, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_ascari", new Point(3,2), TrackType.Complex, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_barcelonalast", new Point(3,2), TrackType.Complex, 3, new List<Connection>{
            new Connection(new Point(0,0), new Point(0,-1)),
            new Connection(new Point(1,0), new Point(0,-1))
        }),
        new TrackPiece("3x3_saopaulo", new Point(3,3), TrackType.Complex, 4, new List<Connection>{
            new Connection(new Point(0,0), new Point(-1,0)),
            new Connection(new Point(2,2), new Point(0,1))
        }),
        new TrackPiece("5x2_maggotsbecketts", new Point(5,2), TrackType.Complex, 4, new List<Connection>{
            new Connection(new Point(0,1), new Point(-1,0)),
            new Connection(new Point(4,1), new Point(1,0))
        }),
    };
    public static List<(TrackPiece, int, bool, List<Connection>)> PrecomputeUniqueTransforms(List<TrackPiece> pieces)
    {
        var precomputedUniqueTransforms = new List<(TrackPiece, int, bool, List<Connection>)>();
        var seenSignatures = new HashSet<string>();

        foreach (var piece in pieces)
        {
            for (int rotation = 0; rotation < 4; rotation++)
            {
                for (int flip = 0; flip < 2; flip++)
                {
                    bool flipped = flip == 1;
                    var connections = piece.GetTransformedConnections(rotation, flipped);

                    // Create a unique signature for this layout of connections
                    string signature = string.Join("|",
                        connections
                            .OrderBy(c => c.Position.X)
                            .ThenBy(c => c.Position.Y)
                            .ThenBy(c => c.Direction.X)
                            .ThenBy(c => c.Direction.Y)
                            .Select(c => $"{c.Position.X},{c.Position.Y}:{c.Direction.X},{c.Direction.Y}")
                    );

                    // Only add if we haven't seen this exact connection layout before for this piece
                    if (seenSignatures.Add(piece.Name + "_" + signature))
                    {
                        precomputedUniqueTransforms.Add((piece, rotation, flipped, connections));
                    }
                }
            }
        }

        return precomputedUniqueTransforms;
    }
}