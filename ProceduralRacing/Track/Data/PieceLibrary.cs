using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

// --- Track Type ---
public enum TrackType { Grid, Straight, Turn, Hairpin, Chicane, Complex }



// The Piece Library contains all predefined track pieces used in procedural generation.
// It provides access to the starting piece and a list of all available pieces,
// along with utility methods for transforming piece connections based on rotation and flipping.

public class PieceLibrary
{
    // --- Starting Piece ---
    public static TrackPiece StartingPiece = 
        new TrackPiece("5x1_grid", new Point(5, 1), TrackType.Grid, 0, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(4,0), new Point(1,0))
    });

    // --- All Pieces ---
    public static List<TrackPiece> All = new List<TrackPiece>()
    {
        new TrackPiece("1x1_straight", new Point(1,1), TrackType.Straight, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(0,0), new Point(1,0))
        }),
        new TrackPiece("2x1_straight", new Point(2,1), TrackType.Straight, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,0), new Point(1,0))
        }),
        new TrackPiece("3x1_straight", new Point(3,1), TrackType.Straight, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("4x1_straight", new Point(4,1), TrackType.Straight, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("5x1_straight", new Point(5,1), TrackType.Straight, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(4,0), new Point(1,0))
        }),
        new TrackPiece("6x1_straight", new Point(6,1), TrackType.Straight, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(5,0), new Point(1,0))
        }),
        new TrackPiece("7x1_straight", new Point(7,1), TrackType.Straight, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(6,0), new Point(1,0))
        }),
        new TrackPiece("8x1_straight", new Point(8,1), TrackType.Straight, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(7,0), new Point(1,0))
        }),
        new TrackPiece("1x1_turn", new Point(1,1), TrackType.Turn, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(0,0), new Point(0,1))
        }),
        new TrackPiece("2x2_turn", new Point(2,2), TrackType.Turn, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_bendturn", new Point(2,2), TrackType.Turn, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_boxturn", new Point(2,2), TrackType.Turn, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_tightturn", new Point(2,2), TrackType.Turn, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_zturn", new Point(2,2), TrackType.Turn, 4, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_copse", new Point(3,2), TrackType.Turn, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_aintree", new Point(3,2), TrackType.Turn, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_openaintree", new Point(3,2), TrackType.Turn, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_wideturn", new Point(3,2), TrackType.Turn, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_openclub", new Point(3,2), TrackType.Turn, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x3_turn", new Point(3,3), TrackType.Turn, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(2,2), new Point(0,1))
        }),
        new TrackPiece("4x3_copse", new Point(4,3), TrackType.Turn, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(3,2), new Point(0,1))
        }),
        new TrackPiece("4x3_copsestraight", new Point(4,3), TrackType.Turn, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(3,2), new Point(0,1))
        }),
        new TrackPiece("2x1_hairpin", new Point(2,1), TrackType.Hairpin, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,1)),
            new LocalConnection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_monacohairpin", new Point(2,2), TrackType.Hairpin, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(1,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_hairpin", new Point(3,2), TrackType.Hairpin, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(2,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_doubleapex", new Point(3,2), TrackType.Hairpin, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(2,0), new Point(0,-1))
        }),
        new TrackPiece("3x2_hungaryhairpin", new Point(3,2), TrackType.Hairpin, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(0,1), new Point(-1,0))
        }),
        new TrackPiece("3x3_parabolica", new Point(3,3), TrackType.Hairpin, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(0,2), new Point(-1,0))
        }),
        new TrackPiece("2x1_chicane", new Point(2,1), TrackType.Chicane, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(1,0), new Point(0,1))
        }),
        new TrackPiece("2x2_smoothchicane", new Point(2,2), TrackType.Chicane, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("2x2_tightchicane", new Point(2,2), TrackType.Chicane, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_smoothchicane", new Point(3,2), TrackType.Chicane, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("3x2_tightchicane", new Point(3,2), TrackType.Chicane, 1, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,0), new Point(1,0))
        }),
        new TrackPiece("3x2_monzachicane", new Point(3,2), TrackType.Chicane, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(1,0))
        }),
        new TrackPiece("3x2_nouveauchicane", new Point(3,2), TrackType.Chicane, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(1,0))
        }),
        new TrackPiece("4x2_smoothchicane", new Point(4,2), TrackType.Chicane, 2, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("4x2_tightchicane", new Point(4,2), TrackType.Chicane, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(3,0), new Point(1,0))
        }),
        new TrackPiece("2x2_singaporesling", new Point(2,2), TrackType.Complex, 4, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(1,1), new Point(0,1))
        }),
        new TrackPiece("3x2_ascari", new Point(3,2), TrackType.Complex, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(2,1), new Point(0,1))
        }),
        new TrackPiece("3x2_barcelonalast", new Point(3,2), TrackType.Complex, 3, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(0,-1)),
            new LocalConnection(new Point(1,0), new Point(0,-1))
        }),
        new TrackPiece("3x3_saopaulo", new Point(3,3), TrackType.Complex, 4, new List<LocalConnection>{
            new LocalConnection(new Point(0,0), new Point(-1,0)),
            new LocalConnection(new Point(2,2), new Point(0,1))
        }),
        new TrackPiece("5x2_maggotsbecketts", new Point(5,2), TrackType.Complex, 4, new List<LocalConnection>{
            new LocalConnection(new Point(0,1), new Point(-1,0)),
            new LocalConnection(new Point(4,1), new Point(1,0))
        }),
    };

    // --- Content Loading ---
    public static void LoadContent(ContentManager Content)
    { // Loads textures for all pieces in the library
        foreach (var piece in All)
        {
            piece.Texture = Content.Load<Texture2D>($"textures/pieces/{piece.Name}");
            piece.CachePixelData();
        }

        if (StartingPiece != null)
        {
            StartingPiece.Texture = Content.Load<Texture2D>($"textures/pieces/{StartingPiece.Name}");
            StartingPiece.CachePixelData();
        }
    }

    // --- Utility Helpers ---
    public static List<(TrackPiece piece, int rotation, bool flipped, (LocalConnection A, LocalConnection B) LocalConnections)> PrecomputeUniqueTransforms(List<TrackPiece> pieces)
    { // Returns a list of tuples containing the piece, rotation, flip status, and transformed LocalConnections for each unique configuration
        var precomputed = new List<(TrackPiece, int, bool, (LocalConnection, LocalConnection))>();
        var seenSignatures = new HashSet<(string Name, int LocalConnectionsHash)>();

        foreach (var piece in pieces)
        {
            for (int rotation = 0; rotation < 4; rotation++)
            {
                for (int flip = 0; flip < 2; flip++)
                {
                    bool flipped = flip == 1;

                    var LocalConnections = GetTransformedConnections(piece, rotation, flipped);

                    // XOR is order-independent, so swapping which LocalConnection is A vs B still hashes the same
                    int LocalConnectionsHash = LocalConnections.A.GetHashCode() ^ LocalConnections.B.GetHashCode();

                    if (seenSignatures.Add((piece.Name, LocalConnectionsHash)))
                    {
                        precomputed.Add((piece, rotation, flipped, LocalConnections));
                    }
                }
            }
        }

        return precomputed;
    }

    public static (LocalConnection A, LocalConnection B) GetTransformedConnections(TrackPiece piece, int rotation = 0, bool flipped = false)
    { // Transforms both of a piece's LocalConnections based on rotation and flip status
        var transformed = piece.Connections
            .Select(con => TransformLocalConnection(con, piece.Size, rotation, flipped))
            .ToArray();

        return (transformed[0], transformed[1]);
    }

    private static LocalConnection TransformLocalConnection(LocalConnection con, Point size, int rotation, bool flipped)
    { // Applies flip then rotation to a single LocalConnection's position and direction
        int w = size.X;
        int h = size.Y;

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

        return new LocalConnection(finalPos, finalDir);
    }

    public static bool TryGetEntryAndExit((LocalConnection A, LocalConnection B) localCons, Point incomingDirection, out LocalConnection entry, out LocalConnection exit)
    { // Whichever LocalConnection faces the incoming direction is the entry; the other is the exit
        bool bIsEntry = localCons.B.IsOpposite(incomingDirection);

        entry = bIsEntry ? localCons.B : localCons.A;
        exit = bIsEntry ? localCons.A : localCons.B;

        return entry.IsOpposite(incomingDirection);
    }

}