using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> PiecePool;
    private List<PlacedPiece> Track;
    private Grid Grid;

    private Point currentEnd;
    private Connection lastExitConnection;

    public int GridOriginX = 9;
    public int GridOriginY = 7;

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid)
    {
        PiecePool = availablePieces;
        Track = new List<PlacedPiece>();
        Grid = grid;
        currentEnd = new Point(GridOriginX, GridOriginY);
    }

    public List<PlacedPiece> GenerateTrack()
    {
        Track.Clear();
        Grid.Clear();

        // Place starting piece
        var startPiece = PiecePool.First(p => p.Name == "3x1_grid");
        var startPlaced = new PlacedPiece(startPiece, new Point(GridOriginX, GridOriginY), rotation: 0, isFlipped: false);

        Grid.OccupyRectangle(startPlaced.GridPosition, startPlaced.TransformedSize); 
        Track.Add(startPlaced);

        lastExitConnection = startPlaced.TransformedConnections[1];
        currentEnd = startPlaced.GridPosition + lastExitConnection.Position + lastExitConnection.Direction;

        // Test pieces
        TryPlacePiece(PiecePool.First(p => p.Name == "2x2_singaporesling"), rotation: 1, flipped: false);
        TryPlacePiece(PiecePool.First(p => p.Name == "3x1_straight"), rotation: 1, flipped: false);
        TryPlacePiece(PiecePool.First(p => p.Name == "3x2_copse"), rotation: 3, flipped: false);
        TryPlacePiece(PiecePool.First(p => p.Name == "2x1_straight"), rotation: 0, flipped: false);
        TryPlacePiece(PiecePool.First(p => p.Name == "3x2_copse"), rotation: 1, flipped: true);
        TryPlacePiece(PiecePool.First(p => p.Name == "3x2_copse"), rotation: 0, flipped: true);
        TryPlacePiece(PiecePool.First(p => p.Name == "3x2_copse"), rotation: 3, flipped: false);

        return Track;
    }

    // Attempts to place a piece at the current end of the track, then updates state if successful
    public bool TryPlacePiece(TrackPiece piece, int rotation = 0, bool flipped = false)
    {
        var tempPlaced = new PlacedPiece(piece, new Point(0, 0), rotation, flipped);

        // Check for matching entry connection
        Connection entryCon = tempPlaced.TransformedConnections.FirstOrDefault(c => c.IsOpposite(lastExitConnection));
        if (entryCon == null) return false;

        // Check for no overlap with occupied cells
        var placed = new PlacedPiece(piece, currentEnd - entryCon.Position, rotation, flipped);
        if (Grid.IsRectangleOccupied(placed.GridPosition, placed.TransformedSize)) return false;

        // Place piece
        Track.Add(placed);
        Grid.OccupyRectangle(placed.GridPosition, placed.TransformedSize);

        // Check for valid exit connection
        Connection exitCon = placed.TransformedConnections.FirstOrDefault(c => c.LeadsToEmptySpace(placed.GridPosition, Grid));
        if (exitCon == null ||  exitCon == entryCon) return false;

        // Update state for next piece
        lastExitConnection = exitCon;
        currentEnd = placed.GridPosition + exitCon.Position + exitCon.Direction;

        return true;
    }
}