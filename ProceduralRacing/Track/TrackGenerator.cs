using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class TrackGenerator
{
    // --------------------------------------------------------
    // -------------- PROPERTIES AND CONSTRUCTOR --------------
    // --------------------------------------------------------

    public List<PlacedPiece> Track;
    private Grid Grid;
    public WorldConnection currentConnection;
    public WorldConnection startConnection;
    public Random random;
    private List<(TrackPiece piece, int rotation, bool flipped, List<Connection> connections)> uniquePieces;

    public bool TrackIsClosed => Track.Count > 1 &&
                                 Track[^1].UsedExitConnection != null &&
                                 IsClosingExit(Track[^1].UsedExitConnection, Track[^1]);

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid, int seed)
    { // Initializes the generator with available pieces, grid, and random seed
        Track = new List<PlacedPiece>();
        Grid = grid;
        random = new Random(seed);
        uniquePieces = PieceLibrary.PrecomputeUniqueTransforms(availablePieces);

    }

    // --------------------------------------------------------
    // ---------------- TRACK GENERATION LOGIC ----------------
    // --------------------------------------------------------

    // Every frame, we try to extend the track until it either closes or hits max length
    // If we try all options from the current end and can't place anything, we backtrack
    // When we backtrack, we remove the last piece and try new options from the new end

    public void Update(GameTime gameTime)
    { // Runs every frame and tries to extend track until it can't
        if (TrackIsClosed || Track.Count >= Constants.MaxTrackLength) return;
        if (TryNextPieceFromCurrentEnd()) return;
        Backtrack();
    }

    private void Backtrack()
    { // Removes the last piece and tries to find new options from the new tip
        if (Track.Count <= 1) return;
        RemovePiece();
        if (!TryNextPieceFromCurrentEnd()) Backtrack();
    }

    private bool TryNextPieceFromCurrentEnd()
    { // Attempts to add the next piece from the last piece's remaining options
        var lastPiece = Track[^1];

        if (lastPiece.RemainingOptions.Count == 0) return false;

        // Take the next candidate, remove it from options, and add it to the track
        var candidate = lastPiece.RemainingOptions[0];
        lastPiece.RemainingOptions.RemoveAt(0);

        AddPiece(candidate.Piece, candidate.Exit);

        // If the track is not closed, refresh options for the new end
        if (!TrackIsClosed) RefreshNextCandidates();

        return true;
    }

    private float ScorePiece(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    { // Scores a piece placement based on various criteria

        float score = 50f;
        float progress = Track.Count / (float)Constants.MaxTrackLength;

        // 1. Does it have a valid placement? If not, it scores 0.
        if (!TryFindPlacement(piece, rotation, flipped, out placed, out exit)) return 0f;

        // 2. Does it have an exit that connects with the start? If it does, add 1000.
        if (IsClosingExit(exit, placed)) score += 1000f;

        // 3. Is it the same as the last piece? If it is, subtract 50.
        if ((Track.Count > 0) && Track[^1].BasePiece == piece) score -= 50f;

        // 4. Is it the same type as the last piece? If it is, subtract 25.
        if ((Track.Count > 0) && Track[^1].BasePiece.Type == piece.Type) score -= 25f;

        // 5. Does it initially lead away from the start? Stronger reward early (up to 5)
        score += Math.Min(5f, ManhattanToStart(placed, exit)) * (1f - progress);

        // 6. Does it bring us home towards the end? Stronger reward late (up to 1000)
        score += (1000f - Math.Min(1000f, ManhattanToStart(placed, exit))) * progress;

        // 7. Add enough random noise to tiebreak but not to choose 'bad' pieces
        score += (float)random.NextDouble() * 1f;

        // 8. Return non-negative score
        return Math.Max(1f, score);
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    { // Tries to find a valid placement for a piece when given the current world connection

        placed = null;
        exit = null;
        var connections = piece.GetTransformedConnections(rotation, flipped);

        // 1. Does it have a valid entry point?
        if (!HasValidEntry(connections, out var entry))
            return false;

        // 2. Find the grid position based on that entry point and the current world connection
        var gridPos = currentConnection.GridPosition - (entry.Position + entry.Direction);

        // 3. Make a placement based on that entry grid position
        var placement = new PlacedPiece(piece, rotation, flipped, gridPos);

        // 4. Does this new placement fit in the grid?
        if (!HasValidPlacement(placement))
            return false;

        // 5. If it fits, does it have a valid exit point?
        if (!HasValidExit(placement, entry, out exit))
            return false;

        placed = placement;
        return true;
    }


    // --------------------------------------------------------
    // ------------------------ HELPERS -----------------------
    // --------------------------------------------------------


    // --- State Management Helpers --- 

    private void ResetState()
    { // Clear the track and grid, reset current connection

        Track.Clear();
        Grid.Clear();
        currentConnection = default;
    }

    private void OnTrackClosed(PlacedPiece closingPiece)
    { // Clears remaining options and logs closure

        closingPiece.RemainingOptions = new List<Candidate>();
        Debug.WriteLine($"Track closed with {Track.Count} pieces placed");
    }


    // --- Candidate Management Helpers ---

    private void RefreshNextCandidates()
    { // Generates options for the current end of the track

        if (Track.Count == 0 || TrackIsClosed) return;

        var options = ScoreAllCandidates()
            .OrderByDescending(c => c.Score)
            .Take(Constants.OptionPoolSize)
            .ToList();

        Track[^1].RemainingOptions = options;
    }

    private List<Candidate> ScoreAllCandidates()
    { // Scores all unique pieces and returns a list of valid placements with their scores

        return uniquePieces.Select(u =>
        {
            var score = ScorePiece(u.piece, u.rotation, u.flipped, out var placed, out var exit);
            return new Candidate(placed, exit, score);
        })
        .Where(c => c.Score > 0f)
        .ToList();
    }


    // --- Placement Validation Helpers --- 

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
        return candidateExit.GetNextCell(candidate.GridPosition) == startConnection.GridPosition &&
               candidateExit.IsOpposite(startConnection.Direction);
    }

    private bool HasValidExit(PlacedPiece candidate, Connection entry, out Connection exit)
    { // Finds the exit connection (the one that isn't the entry) and checks if it's either closing or leads to empty space
        exit = candidate.TransformedConnections.First(c => c != entry);
        if (IsClosingExit(exit, candidate)) return true;
        return exit.LeadsToEmptySpace(candidate.GridPosition, Grid);
    }

    private float ManhattanToStart(PlacedPiece placed, Connection exit)
    { // Calculates Manhattan distance from the exit of the placed piece to the start position
        var candidateWorldPos = placed.GridPosition + exit.Position;
        return Math.Abs(candidateWorldPos.X - startConnection.GridPosition.X) + Math.Abs(candidateWorldPos.Y - startConnection.GridPosition.Y);
    }


    // --- Track Management Helpers --- 
    public void BeginTrack()
    { // Initializes the track with the starting piece

        ResetState();

        var startPiece = new PlacedPiece(PieceLibrary.StartingPiece, 0, false,
                         new Point(Constants.TrackOriginX, Constants.TrackOriginY));
        var startEntry = startPiece.TransformedConnections[0];
        var startExit = startPiece.TransformedConnections[1];
        startConnection = new WorldConnection(startPiece.GridPosition + startEntry.Position,
                                              startEntry.Direction);

        AddPiece(startPiece, startExit);
        RefreshNextCandidates();
    }

    public void AddPiece(PlacedPiece piece, Connection exit)
    { // Adds a piece to the track, occupies the grid and updates current connection

        Track.Add(piece);
        Grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);
        piece.UsedExitConnection = exit;
        currentConnection = new WorldConnection(piece.GridPosition + exit.Position,
                                                exit.Direction);

        // Check for closure
        if (IsClosingExit(exit, piece)) OnTrackClosed(piece);
    }

    public void RemovePiece()
    { // Removes the last piece from the track, unoccupies the grid and updates current connection

        if (Track.Count == 0) return;

        var lastPiece = Track[^1];
        Track.RemoveAt(Track.Count - 1);
        Grid.UnoccupyRectangle(lastPiece.GridPosition, lastPiece.TransformedSize);

        // Restore currentConnection from the new last piece
        currentConnection = Track.Count > 0
            ? new WorldConnection(Track[^1].GridPosition + Track[^1].UsedExitConnection.Position,
                                  Track[^1].UsedExitConnection.Direction)
            : default;

        lastPiece.UsedExitConnection = null;
        lastPiece.RemainingOptions?.Clear();
    }
}