using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> piecePool;
    private List<PlacedPiece> track;
    private Dictionary<Point, bool> grid;

    public IReadOnlyDictionary<Point, bool> Grid => grid;

    private Point currentEnd;
    private Point lastExitDirection;

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

        // Place starting piece
        var startPiece = piecePool.First(p => p.Name == "3x1_grid");
        var startPlaced = new PlacedPiece(startPiece, new Point(GridOriginX, GridOriginY), rotation: 0, isFlipped: false);

        OccupyGrid(startPlaced);
        track.Add(startPlaced);

        var startExit = startPlaced.TransformedConnections[1];
        currentEnd = startPlaced.GridPosition + startExit.Position + startExit.Direction;
        lastExitDirection = startExit.Direction;

        // Test pieces
        TryPlacePiece(piecePool.First(p => p.Name == "2x2_singaporesling"), rotation: 1, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "3x1_straight"), rotation: 1, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "3x2_copse"), rotation: 3, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "2x1_straight"), rotation: 0, flipped: false);
        TryPlacePiece(piecePool.First(p => p.Name == "3x2_copse"), rotation: 1, flipped: true);
        TryPlacePiece(piecePool.First(p => p.Name == "3x2_copse"), rotation: 0, flipped: true);
        TryPlacePiece(piecePool.First(p => p.Name == "3x2_copse"), rotation: 3, flipped: false);

        return track;
    }

    public bool TryPlacePiece(TrackPiece piece, int rotation = 0, bool flipped = false)
    {
        var tempPlaced = new PlacedPiece(piece, new Point(0, 0), rotation, flipped);

        // Find required entry direction (opposite of last exit)
        Point requiredEntryDirection = new Point(-lastExitDirection.X, -lastExitDirection.Y);

        // Find compatible entry connection
        var entryCon = tempPlaced.TransformedConnections
            .FirstOrDefault(c => c.Direction == requiredEntryDirection);

        // No compatible entry port facing the right way, discard piece
        if (entryCon == null)
            return false;

        // Calculate grid position based on current end and entry connection
        Point gridPos = currentEnd - entryCon.Position;
        var placed = new PlacedPiece(piece, gridPos, rotation, flipped);

        // Collision check, otherwise discard piece
        var size = placed.TransformedSize;
        for (int dx = 0; dx < size.X; dx++)
        {
            for (int dy = 0; dy < size.Y; dy++)
            {
                var cell = new Point(gridPos.X + dx, gridPos.Y + dy);
                if (grid.ContainsKey(cell))
                    return false;
            }
        }

        // Place piece
        OccupyGrid(placed);
        track.Add(placed);

        // Choose exit connection, ideally one that leads to empty space
        Connection exitConnection = placed.TransformedConnections
            .FirstOrDefault(con =>
            {
                var adjacent = placed.GridPosition + con.Position + con.Direction;
                return !grid.ContainsKey(adjacent);
            });

        // Couldn't find an exit leading to empty space, just pick any other exit
        if (exitConnection == null || exitConnection.Direction == entryCon.Direction)
        {
            exitConnection = placed.TransformedConnections
                .FirstOrDefault(c => c != entryCon)
                ?? placed.TransformedConnections[0];
        }

        // Update current endpoint and last exit direction
        currentEnd = placed.GridPosition + exitConnection.Position + exitConnection.Direction;
        lastExitDirection = exitConnection.Direction;

        return true;
    }

    private void OccupyGrid(PlacedPiece p)
    {
        var size = p.TransformedSize;
        for (int dx = 0; dx < size.X; dx++)
            for (int dy = 0; dy < size.Y; dy++)
                grid[new Point(p.GridPosition.X + dx, p.GridPosition.Y + dy)] = true;
    }
}