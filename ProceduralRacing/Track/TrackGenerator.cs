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
        if (Track.Count >= Constants.MaxTrackLength)
            return;

        var candidates = GetValidCandidates();
        if (candidates.Count == 0)
            return;

        // Pick the candidate with the highest score
        var chosen = candidates.OrderByDescending(c => c.score).First();

        AddPiece(chosen.placed, chosen.exit);
    }

    private List<(PlacedPiece placed, Connection exit, float score)> GetValidCandidates()
    {
        var valid = new List<(PlacedPiece, Connection, float)>();

        foreach (var (piece, rotation, flipped, connections) in uniquePieces)
        {
            if (TryFindPlacement(piece, rotation, flipped, out var placed, out var exit))
            {
                float score = ScorePiece(placed, placed.TransformedConnections);
                valid.Add((placed, exit, score));
            }
        }

        return valid;
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    {
        placed = null;
        exit = null;

        var connections = piece.GetTransformedConnections(rotation, flipped);

        if (!HasValidEntry(connections, out var entry))
            return false;

        var candidate = new PlacedPiece(piece, rotation, flipped, currentConnection.GridPosition - entry.Position);

        if (!HasValidPlacement(candidate))
            return false;

        if (!HasValidExit(connections, entry, candidate, out exit) && !HasClosingExit(connections, candidate, out exit))
            return false;

        placed = candidate;
        return true;
    }

    private float ScorePiece(PlacedPiece piece, List<Connection> connections)
    {
        float score = 0f;

        if (HasClosingExit(connections, piece, out _))
            score += 1000f;

        score += (float)(random.NextDouble() * 0.5);

        return score;
    }

    // ----- Helpers -----
    private bool HasValidEntry(List<Connection> connections, out Connection entry)
    {
        entry = connections.FirstOrDefault(c => c.IsOpposite(currentConnection.Direction));
        return entry != null;
    }

    private bool HasValidPlacement(PlacedPiece p)
    {
        return Grid.IsRectangleInBounds(p.GridPosition, p.TransformedSize) &&
               !Grid.IsRectangleOccupied(p.GridPosition, p.TransformedSize);
    }

    private bool HasValidExit(List<Connection> connections, Connection entry, PlacedPiece candidate, out Connection exit)
    {
        exit = connections.FirstOrDefault(c =>
        {
            if (c == entry) return false;
            return c.LeadsToEmptySpace(candidate.GridPosition, Grid);
        });

        return exit != null;
    }

    private bool HasClosingExit(List<Connection> connections, PlacedPiece candidate, out Connection exit)
    {
        var startEntry = Track[0].TransformedConnections[0];
        Point startWorldPos = Track[0].GridPosition + startEntry.Position;

        exit = connections.FirstOrDefault(c =>
        {
            Point candidateWorldPos = candidate.GridPosition + c.Position;
            Point adjacentPos = candidateWorldPos + c.Direction;

            // Match if adjacent along direction and opposite directions
            return adjacentPos == startWorldPos && c.IsOpposite(startEntry.Direction);
        });

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
