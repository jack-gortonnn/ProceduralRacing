using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> PiecePool;
    private List<PlacedPiece> Track;
    private Grid Grid;

    private WorldConnection currentConnection;

    public int GridOriginX = 5;
    public int GridOriginY = 10;

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid)
    {
        PiecePool = availablePieces;
        Track = new List<PlacedPiece>();
        Grid = grid;
    }

    private void ResetState()
    {
        Track.Clear();
        Grid.Clear();
        currentConnection = default;
    }

    public List<PlacedPiece> GenerateTrack()
    {
        ResetState();

        var startPiece = new PlacedPiece(
            PiecePool.First(p => p.Name == "5x1_grid"),
            new Point(GridOriginX, GridOriginY),
            rotation: 0,
            isFlipped: false
        );

        Grid.OccupyRectangle(startPiece.GridPosition, startPiece.TransformedSize);
        Track.Add(startPiece);

        Connection startExit = startPiece.TransformedConnections[1];
        currentConnection = new WorldConnection(
            startPiece.GridPosition + startExit.Position + startExit.Direction,
            startExit.Direction
        );

        TryPlacePiece(PiecePool.First(p => p.Name == "2x2_singaporesling"), rotation: 1, flipped: false);
        TryPlacePiece(PiecePool.First(p => p.Name == "3x1_straight"), rotation: 1, flipped: true);
        TryPlacePiece(PiecePool.First(p => p.Name == "2x2_singaporesling"), rotation: 0, flipped: true);

        return Track;
    }

    public bool TryPlacePiece(TrackPiece piece, int rotation = 0, bool flipped = false)
    {
        var transformedConnections = piece.GetTransformedConnections(rotation, flipped);  
        
        Connection entryCon = transformedConnections.FirstOrDefault(c => c.IsOpposite(currentConnection.Direction));
        if (entryCon == null) return false;
   
        var placed = new PlacedPiece(piece, currentConnection.GridPosition - entryCon.Position, rotation, flipped);                             

        if (Grid.IsRectangleOccupied(placed.GridPosition, placed.TransformedSize)) return false;

        Connection exitCon = transformedConnections.FirstOrDefault(c => c != entryCon && c.LeadsToEmptySpace(placed.GridPosition, Grid));  
        if (exitCon == null) return false;

        AddPiece(placed, exitCon);

        return true;
    }

    public void AddPiece(PlacedPiece piece, Connection exit)
    {
        Track.Add(piece);
        Grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);

        currentConnection = new WorldConnection(
            piece.GridPosition + exit.Position + exit.Direction,
            exit.Direction
        );
    }
}
