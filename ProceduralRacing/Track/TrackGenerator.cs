using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> piecePool;
    private List<PlacedPiece> track;
    private Dictionary<Point, bool> grid;

    public IReadOnlyDictionary<Point, bool> Grid => grid;
    public Point currentEnd { get; private set; }

    public int GridOriginX = 9;
    public int GridOriginY = 7;

    public TrackGenerator(List<TrackPiece> availablePieces)
    {
        piecePool = availablePieces;
        track = new List<PlacedPiece>();
        grid = new Dictionary<Point, bool>();
        currentEnd = new Point(GridOriginX, GridOriginY);
    }

    public List<PlacedPiece> GenerateTrack()
    {
        track.Clear();
        grid.Clear();

        TryPlacePiece(piecePool.First(p => p.Name == "3x1_grid"), rotation: 0, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "3x2_aintree"), rotation: 2, flipped: false);

        return track;
    }

    public bool TryPlacePiece(TrackPiece piece, int rotation = 0, bool flipped = false)
    {
        var placed = new PlacedPiece(piece, currentEnd, rotation, flipped);

        // Check for collisions
        for (int dx = 0; dx < placed.TransformedSize.X; dx++)
        {
            for (int dy = 0; dy < placed.TransformedSize.Y; dy++)
            {
                var cell = new Point(placed.GridPosition.X + dx, placed.GridPosition.Y + dy);
                if (grid.ContainsKey(cell)) return false;
            }
        }

        // Occupy grid & add to track
        track.Add(placed);
        OccupyGrid(placed);

        // Update current endpoint
        if (placed.TransformedConnections.Count > 0)
        {
            var conn = placed.TransformedConnections[1];
            currentEnd = placed.GridPosition + conn.Position + conn.Direction;
        }

        return true;
    }

    private void OccupyGrid(PlacedPiece p)
    {
        for (int dx = 0; dx < p.TransformedSize.X; dx++)
            for (int dy = 0; dy < p.TransformedSize.Y; dy++)
                grid[new Point(p.GridPosition.X + dx, p.GridPosition.Y + dy)] = true;
    }
}
