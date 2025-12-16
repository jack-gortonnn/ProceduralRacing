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

        // Add starting grid
        var startPiece = piecePool.First(p => p.Name == "3x1_grid");
        var startPlaced = new PlacedPiece(startPiece, currentEnd, rotation: 0, isFlipped: false);
        OccupyGrid(startPlaced);
        track.Add(startPlaced);

        var startExit = startPlaced.TransformedConnections[1]; // right exit as it is a directional piece
        currentEnd = startPlaced.GridPosition + startExit.Position + startExit.Direction;


        TryPlacePiece(piecePool.First(p => p.Name == "2x2_singaporesling"), rotation: 1, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "3x1_straight"), rotation: 1, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "3x2_copse"), rotation: 3, flipped: false);

        return track;
    }

    public bool TryPlacePiece(TrackPiece piece, int rotation = 0, bool flipped = false)
    {
        var placed = new PlacedPiece(piece, currentEnd, rotation, flipped);

        // Check for collisions with already-occupied grid cells
        for (int dx = 0; dx < placed.TransformedSize.X; dx++)
        {
            for (int dy = 0; dy < placed.TransformedSize.Y; dy++)
            {
                var cell = new Point(placed.GridPosition.X + dx, placed.GridPosition.Y + dy);
                if (grid.ContainsKey(cell)) return false;
            }
        }

        // Occupy grid & add to track
        OccupyGrid(placed);
        track.Add(placed);

        // Choose the exit connection from the transformed connections.
        // Prefer a connection whose adjacent cell (position + direction) is NOT already occupied.
        Connection exitConnection = null;
        if (placed.TransformedConnections != null && placed.TransformedConnections.Count > 0)
        {
            exitConnection = placed.TransformedConnections
                .FirstOrDefault(con =>
                {
                    var adjacent = new Point(
                        placed.GridPosition.X + con.Position.X + con.Direction.X,
                        placed.GridPosition.Y + con.Position.Y + con.Direction.Y
                    );
                    return !grid.ContainsKey(adjacent);
                });

            // fallback to preferred indices if none found
            if (exitConnection == null)
            {
                if (placed.TransformedConnections.Count > 1)
                    exitConnection = placed.TransformedConnections[1];
                else
                    exitConnection = placed.TransformedConnections[0];
            }
        }
        else
        {
            exitConnection = new Connection(new Point(0, 0), new Point(0, 0));
        }

        // Update current endpoint in grid coordinates
        currentEnd = placed.GridPosition + exitConnection.Position + exitConnection.Direction;  

        return true;
    }

    private void OccupyGrid(PlacedPiece p)
    {
        for (int dx = 0; dx < p.TransformedSize.X; dx++)
            for (int dy = 0; dy < p.TransformedSize.Y; dy++)
                grid[new Point(p.GridPosition.X + dx, p.GridPosition.Y + dy)] = true;
    }
}
