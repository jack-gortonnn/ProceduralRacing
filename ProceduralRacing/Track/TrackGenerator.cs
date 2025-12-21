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

        var piece = PiecePool.First(p => p.Name == "2x2_singaporesling");

        if (TryFindPlacement(piece, rotation: 1, flipped: false, out var placed, out var exit))
        {
            AddPiece(placed, exit);
        }

        var piece2 = PiecePool.First(p => p.Name == "5x2_maggotsbecketts");

        if (TryFindPlacement(piece2, rotation: 3, flipped: false, out var placed2, out var exit2))
        {
            AddPiece(placed2, exit2);
        }


        return Track;
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    {
        placed = null;
        exit = null;

        var transformedConnections = piece.GetTransformedConnections(rotation, flipped);

        var entry = transformedConnections.FirstOrDefault(c => c.IsOpposite(currentConnection.Direction));
        if (entry == null) return false;

        var candidate = new PlacedPiece(piece, currentConnection.GridPosition - entry.Position, rotation, flipped);

        // Collision check
        if (Grid.IsRectangleOccupied(candidate.GridPosition, candidate.TransformedSize)) return false;

        // Find valid exit
        var exitCon = transformedConnections.FirstOrDefault(c => c != entry && c.LeadsToEmptySpace(candidate.GridPosition, Grid));

        if (exitCon == null) return false;

        placed = candidate;
        exit = exitCon;
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
