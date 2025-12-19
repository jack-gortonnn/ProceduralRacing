using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> PiecePool;
    private List<PlacedPiece> Track;
    private Grid Grid;

    private Point currentEnd;
    private Connection lastExitConnection;

    public int GridOriginX = 3;
    public int GridOriginY = 3;

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid)
    {
        PiecePool = availablePieces;
        Track = new List<PlacedPiece>();
        Grid = grid;
        currentEnd = new Point(GridOriginX, GridOriginY);
    }

    private void ResetState()
    {
        Track.Clear();
        Grid.Clear();
        currentEnd = new Point(GridOriginX, GridOriginY);
        lastExitConnection = null;
    }

    public List<PlacedPiece> GenerateTrack()
    {
        ResetState();

        // Place starting piece
        var startPiece = new PlacedPiece(PiecePool.First(p => p.Name == "5x1_grid"), new Point(GridOriginX, GridOriginY), rotation: 0, isFlipped: false);

        Grid.OccupyRectangle(startPiece.GridPosition, startPiece.TransformedSize); 
        Track.Add(startPiece);

        lastExitConnection = startPiece.TransformedConnections[1];
        currentEnd = startPiece.GridPosition + lastExitConnection.Position + lastExitConnection.Direction;

        // Test pieces
        TryPlacePiece(PiecePool.First(p => p.Name == "2x2_singaporesling"), rotation: 1, flipped: false);

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

        // Check for valid exit connection
        Connection exitCon = placed.TransformedConnections.FirstOrDefault(c => c.LeadsToEmptySpace(placed.GridPosition, Grid));
        if (exitCon == null || exitCon == entryCon) return false;

        // Place piece
        Track.Add(placed);
        Grid.OccupyRectangle(placed.GridPosition, placed.TransformedSize);

        // Update state for next piece
        lastExitConnection = exitCon;
        currentEnd = placed.GridPosition + exitCon.Position + exitCon.Direction;

        return true;
    }
}