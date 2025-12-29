using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class TrackGenerator
{
    public List<PlacedPiece> Track;
    private Grid Grid;
    private WorldConnection currentConnection;
    public Random random;
    private Point startPos;
    private Point startDir;
    private List<(TrackPiece piece, int rotation, bool flipped, List<Connection> connections)> uniquePieces;

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid, int seed)
    {
        Track = new List<PlacedPiece>();
        Grid = grid;
        random = new Random(7714948);
        uniquePieces = PieceLibrary.PrecomputeUniqueTransforms(availablePieces);
    }

    public void BeginTrack()
    {
        ResetState();

        var startPiece = new PlacedPiece(PieceLibrary.StartingPiece, 0, false,
                         new Point(Constants.TrackOriginX, Constants.TrackOriginY));

        var startEntry = startPiece.TransformedConnections[0];
        var startExit = startPiece.TransformedConnections[1];

        startPos = startPiece.GridPosition + startEntry.Position + startEntry.Direction;
        startDir = startEntry.Direction;

        AddPiece(startPiece, startExit);
    }

    public void Update(GameTime gameTime)
    { // Runs every frame and tries to extend track until it can't
        if (Track.Count >= Constants.MaxTrackLength) return;

        var bestCandidate = ScoreAllCandidates().OrderByDescending(c => c.Score).FirstOrDefault();

        if (bestCandidate?.Piece != null)
        {
            AddPiece(bestCandidate.Piece, bestCandidate.Exit);
        }
        else
        {
            Debug.WriteLine($"[{Track.Count} pieces placed] No valid piece found to extend track. Current connection: {currentConnection.Direction} at {currentConnection.GridPosition}");
        }
    }

    private List<Candidate> ScoreAllCandidates()
    { // Scores all unique pieces and returns a list of valid placements with their non-zero scores
        return uniquePieces.Select(u =>
        {
            var score = ScorePiece(u.piece, u.rotation, u.flipped, out var placed, out var exit);
            return new Candidate(placed, exit, score);
        })
        .Where(c => c.Score > 0f)
        .ToList();
    }

    private float ScorePiece(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    {
        float score = 50f;

        // 1. Does it have a valid placement? If not, it scores 0.
        if (!TryFindPlacement(piece, rotation, flipped, out placed, out exit)) return 0f;

        // 2. Does it have an exit that connects with the start? If it does, add 1000.
        if (IsClosingExit(exit, placed)) score += 1000f;

        // 3. Is it the same as the last piece? If it is, subtract 50.
        if ((Track.Count > 0) && Track[^1].BasePiece == piece) score -= 50f;

        // 4. Is it the same type as the last piece? If it is, subtract 25.
        if ((Track.Count > 0) && Track[^1].BasePiece.Type == piece.Type) score -= 25f;

        // 6. Does it get us back to the start? If it does, add up to 50.
        score += Math.Max(0f, 50f - DistanceToStart(placed, exit));

        // 7. Add enough random noise to tiebreak but not to choose 'bad' pieces
        score += (float)(random.NextDouble() * 2);

        // 8. Return non-negative score
        return Math.Max(0f, score);
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    {
        placed = null;
        exit = null;

        var connections = piece.GetTransformedConnections(rotation, flipped);

        // 1. Does it have a valid entry point?
        if (!HasValidEntry(connections, out var entry)) return false;

        // 2. If it does, make a placement based on that entry
        var candidate = new PlacedPiece(piece, rotation, flipped, currentConnection.GridPosition - entry.Position);

        // 3. Does this new placement fit in the grid?
        if (!HasValidPlacement(candidate)) return false;

        // 4. If it fits, does it have a valid exit point?
        if (!HasValidExit(candidate, entry, out exit)) return false;

        placed = candidate;
        return true;
    }

    // ------------------------ HELPERS -----------------------
    private void ResetState()
    { // Clear the track and grid, reset current connection
        Track.Clear();
        Grid.Clear();
        currentConnection = default;
    }

    private bool HasValidEntry(List<Connection> connections, out Connection entry)
    { // Returns a connection if it can find one that can connect with the current world connection
        entry = connections.FirstOrDefault(c => c.IsOpposite(currentConnection.Direction));
        return entry != null;
    }

    private bool HasValidPlacement(PlacedPiece p)
    { // Checks if the piece fits within the grid and doesn't overlap already occupied spaces
        return Grid.IsRectangleInBounds(p.GridPosition, p.TransformedSize)
           && !Grid.IsRectangleOccupied(p.GridPosition, p.TransformedSize);
    }

    private bool IsClosingExit(Connection candidateExit, PlacedPiece candidate)
    { // Checks if the candidate exit connects back to the start position and direction
        bool isClosing = (candidate.GridPosition + candidateExit.Position == startPos) && candidateExit.IsOpposite(startDir);
        return isClosing;
    }

    private bool HasValidExit(PlacedPiece candidate, Connection entry, out Connection exit)
    { // Finds the exit connection (the one that isn't the entry) and checks if it's either closing or leads to empty space
        exit = candidate.TransformedConnections.First(c => c != entry);
        if (IsClosingExit(exit, candidate)) return true;
        return exit.LeadsToEmptySpace(candidate.GridPosition, Grid);
    }

    public void AddPiece(PlacedPiece piece, Connection exit)
    { // Adds the piece to the track and updates the grid and current connection
        Track.Add(piece);
        Grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);
        currentConnection = new WorldConnection(piece.GridPosition + exit.Position + exit.Direction, exit.Direction);
    }

    private float DistanceToStart(PlacedPiece placed, Connection exit)
    { // Calculates Manhattan distance from the exit of the placed piece to the start position
        var candidateWorldPos = placed.GridPosition + exit.Position;
        return Math.Abs(candidateWorldPos.X - startPos.X) + Math.Abs(candidateWorldPos.Y - startPos.Y);
    }
}