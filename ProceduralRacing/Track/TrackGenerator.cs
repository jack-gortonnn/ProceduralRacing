using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class TrackGenerator
{
    private List<TrackPiece> PiecePool;
    public List<PlacedPiece> Track;
    private Grid Grid;

    private WorldConnection currentConnection;
    
    public Random random;

    private int GridOriginX = 10;
    private int GridOriginY = 14;

    private List<(TrackPiece piece, int rotation, bool flipped, List<Connection> connections)> uniquePieces;

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid, int seed)
    {
        PiecePool = availablePieces;
        Track = new List<PlacedPiece>();
        Grid = grid;
        random = new Random(seed);

        uniquePieces = PieceLibrary.PrecomputeUniqueTransforms(availablePieces);

        Debug.WriteLine($"Precomputed {uniquePieces.Count} unique transformations (from {PieceLibrary.All.Count * 8} total possible)");
    }


    private void ResetState()
    {
        Track.Clear();
        Grid.Clear();
        currentConnection = default;
    }

    public void GenerateTrack()
    {
        ResetState();

        var startPiece = new PlacedPiece(PieceLibrary.StartingPiece, 0, false, new Point(GridOriginX, GridOriginY));
        Grid.OccupyRectangle(startPiece.GridPosition, startPiece.TransformedSize);
        Track.Add(startPiece);

        Connection startExit = startPiece.TransformedConnections[1];
        currentConnection = new WorldConnection(
            startPiece.GridPosition + startExit.Position + startExit.Direction,
            startExit.Direction
        );
    }

    public void Update(GameTime gameTime)
    {
        if (Track.Count >= Constants.MaxTrackLength) return;

        var validCandidates = GetValidCandidates();

        if (validCandidates.Count == 0) return;

        var (placed, exit) = validCandidates[random.Next(validCandidates.Count)];

        AddPiece(placed, exit);
    }

    private List<(PlacedPiece placed, Connection exit)> GetValidCandidates()
    {
        var valid = new List<(PlacedPiece placed, Connection exit)>();

        foreach (var (piece, rotation, flipped, connections) in uniquePieces)
        {
            if (TryFindPlacement(piece, rotation, flipped, out var placed, out var exit))
            {
                valid.Add((placed, exit));
            }
        }

        return valid;
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    {
        placed = null;
        exit = null;

        var connections = piece.GetTransformedConnections(rotation, flipped);

        // 1. Find a valid entry connection
        if (!HasValidEntry(connections, out var entry))
            return false;

        // 2. Make a placement based on that entry and see if it fits
        var candidate = new PlacedPiece(piece, rotation, flipped, currentConnection.GridPosition - entry.Position);
        if (!HasValidPlacement(candidate))
            return false;

        // 3. Find a valid exit connection that leads to empty space
        if (!HasValidExitOrClosesTrack(connections, entry, candidate, out exit))
            return false;

        placed = candidate;
        return true;
    }

    // ----- Helpers -----

    private TrackPiece GetPiece(string name)
    {
        return PiecePool.FirstOrDefault(p => p.Name == name);
    }

    private bool HasValidEntry(List<Connection> connections, out Connection entry)
    {
        entry = connections.FirstOrDefault(c => c.IsOpposite(currentConnection.Direction));
        return entry != null;
    }

    private bool HasValidPlacement(PlacedPiece p)
    {
        bool valid = Grid.IsRectangleInBounds(p.GridPosition, p.TransformedSize)
                 && !Grid.IsRectangleOccupied(p.GridPosition, p.TransformedSize);
        return valid;
    }

    private bool HasValidExitOrClosesTrack(List<Connection> connections, Connection entry, PlacedPiece candidate, out Connection exit)
    {
        var startEntry = Track[0].TransformedConnections[0];

        Point startWorldPos = Track[0].GridPosition + startEntry.Position + startEntry.Direction;

        exit = connections.FirstOrDefault(c => c != entry && (c.LeadsToEmptySpace(candidate.GridPosition, Grid)                   // Exit is not entry and leads to empty space
                || (candidate.GridPosition + c.Position + c.Direction == startWorldPos && c.IsOpposite(startEntry.Direction))));  // Or exit connects back to start piece

        return exit != null;
    }



    public void AddPiece(PlacedPiece piece, Connection exit)
    {
        Grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);
        Track.Add(piece);

        currentConnection = new WorldConnection(
            piece.GridPosition + exit.Position + exit.Direction,
            exit.Direction
        );
    }
}
