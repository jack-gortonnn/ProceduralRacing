using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



public class TrackGenerator
{
    // --- Properties ---

    public List<PlacedPiece> Track;
    public WorldConnection currentConnection;
    public WorldConnection startConnection;
    public Random random;

    private List<(TrackPiece piece, int rotation, bool flipped, List<Connection> connections)> uniquePieces;
    private TrackConfig config;
    private Grid Grid;

    private int totalAttempts = 0;
    private int seed;

    public bool TrackIsClosed => Track.Count > 1 &&
                                 Track[^1].UsedExitConnection != null &&
                                 IsClosingExit(Track[^1].UsedExitConnection, Track[^1]) &&
                                 Track.Count >= config.MinLength;

    // --- Constructor ---
    public TrackGenerator(Grid grid, int startingSeed, TrackDifficulty difficulty)
    { // Initializes the generator with available pieces, grid, and random seed
        Track = new List<PlacedPiece>();
        Grid = grid;
        seed = startingSeed;
        random = new Random(seed);
        config = TrackConfig.FromDifficulty(difficulty);
        uniquePieces = PieceLibrary.PrecomputeUniqueTransforms(config.GetAllowedPieces());
    }

    // --- Track Generation Logic ---

    // How do we generate a track?
    // 1. Start with a starting piece at a fixed location and orientation
    // 2. From the current end of the track, score all possible next pieces and their placements
    // 3. Select the best N options and store them as candidates for that end
    // 4. Each frame, try to add the next candidate from the current end's options
    // 5. If successful, refresh candidates for the new end and repeat
    // 6. If no candidates are left, backtrack by removing the last piece and trying its next candidate
    // 7. Continue until the track is closed or reaches max length

    public void Update(GameTime gameTime)
    { // Main update loop for track generation

        // If the track is already closed, do nothing
        if (TrackIsClosed) return;

        // If we've gone too long or exceeded attempts, restart
        if (Track.Count >= config.MaxLength || totalAttempts >= config.MaxAttempts)
        {
            RestartTrack();
            return;
        }

        if (TryNextPieceFromCurrentEnd()) return;

        // Otherwise, backtrack
        Backtrack();
    }

    private void Backtrack()
    { // Removes the last piece and tries to find new options from the new tip

        if (Track.Count <= 1) return;

        RemovePiece();

        if (!TryNextPieceFromCurrentEnd()) 
            Backtrack();
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
    { // Scores a piece placement based on various heuristics

        float score = config.BaseScore;
        float progress = Track.Count / (float)config.MaxLength;

        // 1. Invalid placement (overlap, out of bounds, no valid entry/exit, or blocks start) returns 0
        if (!TryFindPlacement(piece, rotation, flipped, out placed, out exit)) return 0f;

        // 2. Reward heavily if it closes the track
        if (IsClosingExit(exit, placed)) score += config.CloseLoopBonus;

        // 3. Penalize if it's the same piece as the last one placed
        if ((Track.Count > 0) && Track[^1].BasePiece == piece) score -= config.SamePiecePenalty;

        // 4. Penalize if it's the same type as the last one placed
        if ((Track.Count > 0) && Track[^1].BasePiece.Type == piece.Type) score -= config.SameTypePenalty;

        // 5. Calculate Manhattan distance to start from the exit
        float manhattan = GetManhattanToStart(placed, exit);

        // 6. Reward moving away from start early on
        score += GetEarlyBonus(manhattan, progress, config.MaxEarlyBonus);

        // 7. Reward moving closer to start later on
        score += GetLateBonus(manhattan, progress, config.MaxLateBonus);

        // 8. Reward/penalize based on direction alignment towards the start
        score += GetAlignmentBonus(exit, placed, manhattan, progress, config.AlignmentBonus);

        // 9. Reward based on piece difficulty
        score += config.GetRatingBonus(piece.Difficulty);

        // 10. Add some randomness to the score
        score += (float)random.NextDouble() * config.Randomness;

        return Math.Max(1f, score);
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    { // Tries to find a valid placement for a piece when given the current world connection

        placed = null;
        exit = null;

        var connections = PieceLibrary.GetTransformedConnections(piece, rotation, flipped);

        // 1. Does it have a valid entry point?
        if (!HasValidEntry(connections, out var entry)) return false;

        // 2. Find the grid position based on that entry point and the current world connection
        var gridPos = currentConnection.GridPosition - (entry.Position + entry.Direction);

        // 3. Make a placement based on that entry grid position
        var placement = new PlacedPiece(piece, rotation, flipped, gridPos);

        // 4. Does this new placement fit in the grid?
        if (!HasValidPlacement(placement)) return false;

        // 5. Does it have a valid exit that either closes the track or leads to empty space?
        if (!HasValidExit(placement, entry, out exit)) return false;

        // 6. Does it improperly occupy the critical return cell without closing?
        if (OccupiesStartWithoutClosing(placement, exit)) return false;

        // 7. Is the critical cell reachable from the exit (unless closing)?
        if (!(IsAbleToReachStart((placement.GridPosition + exit.Position)
                     + exit.Direction)|| IsClosingExit(exit, placement))) return false;

        placed = placement;
        return true;
    }


    // --- State Management Helpers ---
 
    private void RestartTrack()
    { // Resets the track generation with a +1 seed tweak

        seed++;
        random = new Random(seed);
        BeginTrack();
    }

    private void ResetState()
    { // Clear the track and grid, reset current connection and attempt count

        Track.Clear();
        Grid.Clear();
        currentConnection = default;
        totalAttempts = 0;
    }

    // --- Candidate Management Helpers ---

    private void RefreshNextCandidates()
    { // Generates options for the current end of the track

        if (Track.Count == 0 || TrackIsClosed) return;

        var options = ScoreAllCandidates()
            .OrderByDescending(c => c.Score)
            .Take(config.OptionPoolSize)
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
    {// Checks if the piece has a valid exit that either closes the track or leads to empty space
        exit = candidate.TransformedConnections.First(c => c != entry);
        if (IsClosingExit(exit, candidate)) return true;
        return exit.LeadsToEmptySpace(candidate.GridPosition, Grid);
    }

    private bool IsAbleToReachStart(Point startPos)
    { // Checks if the current exit still has a valid path to end via floodfill
        Point start = startConnection.GridPosition + startConnection.Direction;

        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();
        queue.Enqueue(startPos);
        visited.Add(startPos);

        int steps = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            steps++;

            if (current == start) return true;

            foreach (var dir in new[] { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) })
            {
                var next = current + dir;
                if (!visited.Contains(next) &&
                    Grid.IsInBounds(next) &&
                    !Grid.IsOccupied(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return false;
    }

    private bool OccupiesStartWithoutClosing(PlacedPiece candidate, Connection exit)
    { // Checks if the candidate piece occupies the critical return cell without closing the track
        Point start = startConnection.GridPosition + startConnection.Direction;
        bool occupiesAdjacent = Grid.DoesRectangleContain(candidate.GridPosition,
                                                        candidate.TransformedSize, start);

        if (!occupiesAdjacent) return false;
        return !IsClosingExit(exit, candidate);
    }

    private float GetManhattanToStart(PlacedPiece placed, Connection exit)
    { // Calculates the Manhattan distance from the exit to the start connection
        Point nextCell = placed.GridPosition + exit.Position + exit.Direction;
        Point start = startConnection.GridPosition + startConnection.Direction;
        return Math.Abs(start.X - nextCell.X) + Math.Abs(start.Y - nextCell.Y);
    }


    // --- Scoring Helpers ---

    private float GetEarlyBonus(float manhattan, float progress, float maxBonus)
    { // Rewards moving away from start early on
        return Math.Min(maxBonus, manhattan) * (1f - progress);
    }

    private float GetLateBonus(float manhattan, float progress, float maxBonus)
    { // Rewards moving closer to start later on
        return (maxBonus - Math.Min(maxBonus, manhattan)) * progress;
    }

    private float GetAlignmentBonus(Connection exit, PlacedPiece placed, float manhattan, float progress, float maxBonus)
    { // Rewards/penalises exit connection being in/out of alignment with start connection

        if (manhattan <= 1f) return 0f;

        Vector2 currentDir = new(exit.Direction.X, exit.Direction.Y);

        Point nextCell = placed.GridPosition + exit.Position + exit.Direction;
        Point criticalCell = startConnection.GridPosition + startConnection.Direction;

        Vector2 toHome = Vector2.Normalize(new Vector2(criticalCell.X - nextCell.X, criticalCell.Y - nextCell.Y));

        float alignment = Vector2.Dot(currentDir, toHome);

        return alignment * maxBonus * progress;
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

        if (piece == null) return;

        totalAttempts++;
        Track.Add(piece);
        Grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);
        piece.UsedExitConnection = exit;
        currentConnection = new WorldConnection(piece.GridPosition + exit.Position,
                                                exit.Direction);
    }

    public void RemovePiece()
    { // Removes the last piece from the track, unoccupies the grid and updates current connection

        if (Track.Count == 0) return;

        totalAttempts++;
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