using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



public class TrackGenerator
{
    // --- Properties ---

    public List<PlacedPiece> Track { get; private set; }
    private WorldConnection _currentConnection;
    private WorldConnection _startConnection;
    private Random _random;

    private TrackConfig _config;
    private Grid _grid;

    private int _totalAttempts = 0;
    private int _seed;

    // List of all precomputed unique pieces, given their rotation and flip state. The A/B connections
    // have no entry/exit labels here since that depends on the incoming direction at placement time
    private List<(TrackPiece piece, int rotation, bool flipped, (LocalConnection A, LocalConnection B) connections)> _uniquePieces;

    // List of potential options and their respective scores
    // for each piece in the track, used for backtracking
    private List<List<(PlacedPiece Piece, float Score)>> _optionsStack = new();

    // Track is closed if the last piece connects back to the start and the track is at least the minimum length
    public bool TrackIsClosed => IsClosingPiece(Track[^1]) &&
                                 Track.Count >= _config.MinLength;

    // Track generation is considered failed if it exceeds the maximum length or maximum attempts
    private bool GenerationLimitReached => Track.Count >= _config.MaxLength ||
                                           _totalAttempts >= _config.MaxAttempts;

    // --- Constructor ---
    public TrackGenerator(Grid grid, int startingSeed, TrackDifficulty difficulty)
    { // Initializes the generator with available pieces, grid, and random seed
        Track = new List<PlacedPiece>();
        _grid = grid;
        _seed = startingSeed;
        _random = new Random(_seed);
        _config = TrackConfig.FromDifficulty(difficulty);
        _uniquePieces = PieceLibrary.PrecomputeUniqueTransforms(_config.GetAllowedPieces());
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
        if (TrackIsClosed) { return; }

        // If we've gone too long or exceeded attempts, restart
        if (GenerationLimitReached) { RestartTrack(); return; }

        // We can now try to place the next piece from the current end's options
        if (TryAddNextCandidate()) { return; }

        // If we can't even place a new piece, we need to backtrack
        Backtrack();
    }

    private void Backtrack()
    { // Removes the last piece, attempts to place next candidate or backtracks further

        if (Track.Count <= 1) { RestartTrack(); return; }

        RemovePiece(Track[^1]);

        if (TryAddNextCandidate()) { return; }

        Backtrack();
    }

    private bool TryAddNextCandidate()
    { // Attempts to add the next piece from the current end's remaining options

        var options = _optionsStack[^1];
        if (options.Count == 0) return false;

        // Take the next candidate, remove it from options, and add it to the track
        var candidate = options[0];
        options.RemoveAt(0);

        AddPiece(candidate.Piece);

        // If the track isn't closed, refresh options for the new end
        if (!TrackIsClosed) GenerateOptionsForCurrentEnd();

        return true;
    }

    private float ScoreAndFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placement)
    { // Scores a piece placement based on various heuristics

        // 1. First check for invalid placement (overlap, out of bounds, no valid entry/exit, or blocks start)
        if (!TryFindPlacement(piece, rotation, flipped, out placement)) return 0f;

        // 2. Since we know the piece is technically valid, we know we can score it. Each piece will have:
        float score = _config.BaseScore;                                    // A base score for being valid
        float progress = Track.Count / (float)_config.MaxLength;           // A progress factor from 0 - 1
        float manhattan = GetManhattanToStart(placement);                  // A cheap distance to start 

        // 3. Reward moving away from start early on
        score += GetEarlyBonus(manhattan, progress, _config.MaxEarlyBonus);

        // 4. Reward moving closer to start later on
        score += GetLateBonus(manhattan, progress, _config.MaxLateBonus);

        // 5. Reward heavily if it closes the track
        score += GetCloseLoopBonus(placement, _config.CloseLoopBonus);

        // 6. Penalize if it's the same piece as the last one placed
        score += GetSamePiecePenalty(placement, _config.SamePiecePenalty);

        // 7. Penalize if it's the same type as the last one placed
        score += GetSameTypePenalty(placement, _config.SameTypePenalty);

        // 8. Reward/penalize based on direction alignment towards the start
        score += GetAlignmentBonus(placement, progress, _config.AlignmentBonus);

        // 9. Add some randomness to the score
        score += (float)_random.NextDouble() * _config.Randomness;

        return Math.Max(1f, score);
    }

    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placement)
    { // Tries to find a valid placement for a piece when given the current world connection

        placement = null;

        // 1. Get the transformed connections for the piece given its rotation and flip state. If no valid
        //    entry/exit pair is found, then this piece can't connect to the current end, so return false
        var connections = PieceLibrary.GetTransformedConnections(piece, rotation, flipped);
        if (!PieceLibrary.TryGetEntryAndExit(connections, _currentConnection.Direction,
                                             out var entry, out var exit)) return false;

        // 2. Find grid location and temporarily create a PlacedPiece to continue validity checks
        //    We can also check if it closes the track here, since that will affect some of the validity checks
        var gridPos = _currentConnection.GridPosition - (entry.Position + entry.Direction);
        var tempPiece = new PlacedPiece(piece, rotation, flipped, gridPos, entry, exit);
        bool closes = IsClosingPiece(tempPiece);

        // 3. Is the piece out of bounds or overlapping? If so, return false
        if (IsOutOfBoundsOrOverlapping(tempPiece)) return false;

        // 4. Does the piece not lead to an empty space? If so, return false
        if (DoesNotLeadToEmptySpace(tempPiece) && !closes) return false;

        // 5. Does the piece block us from ever closing the track? If so, return false
        if (BlocksStart(tempPiece) && !closes) return false;

        // 6. Does the piece go to a dead end where we can't reach the start? If so, return false
        if (CannotReachStart(tempPiece) && !closes) return false;

        placement = tempPiece;
        return true;
    }


    // --- State Management Helpers ---

    private void RestartTrack()
    { // Restarts track generation with a new (deterministic) seed
        _seed++;
        _random = new Random(_seed);
        BeginTrack();
    }

    private void ResetState()
    { // Clear the track and grid, reset current connection and attempt count

        Track.Clear();
        _optionsStack.Clear();
        _grid.Clear();
        _currentConnection = default;
        _totalAttempts = 0;
    }

    // --- Candidate Management Helpers ---

    private void GenerateOptionsForCurrentEnd()
    { // Generates a list of valid placements for the current end of the track and scores them

        if (Track.Count == 0 || TrackIsClosed) return;

        var options = ScoreAllCandidates()
            .OrderByDescending(c => c.Score)
            .Take(_config.OptionPoolSize)
            .ToList();

        _optionsStack.Add(options);
    }

    private List<(PlacedPiece Piece, float Score)> ScoreAllCandidates()
    { // Scores all unique pieces and returns a list of valid placements with their scores

        return _uniquePieces.Select(u =>
        {
            var score = ScoreAndFindPlacement(u.piece, u.rotation, u.flipped, out var placed);
            return (Piece: placed, Score: score);
        })
        .Where(c => c.Score > 0f)
        .ToList();
    }


    // --- Placement Validation Helpers --- 

    private bool IsOutOfBoundsOrOverlapping(PlacedPiece p) =>
    !_grid.IsRectangleInBounds(p.GridPosition, p.TransformedSize)
    || _grid.IsRectangleOccupied(p.GridPosition, p.TransformedSize);

    private bool BlocksStart(PlacedPiece candidate)
    { // Checks if the candidate occupies the start cell
        Point start = _startConnection.GridPosition + _startConnection.Direction;
        return _grid.DoesRectangleContain(candidate.GridPosition, candidate.TransformedSize, start);
    }

    private bool DoesNotLeadToEmptySpace(PlacedPiece candidate)
    { // Checks if the candidate's exit does NOT lead to an unoccupied cell
        return !candidate.ExitConnection.LeadsToEmptySpace(candidate.GridPosition, _grid);
    }

    private bool CannotReachStart(PlacedPiece candidate)
    { // Checks if the candidate's exit has no valid path to start
        LocalConnection exit = candidate.ExitConnection;
        Point startPos = candidate.GridPosition + exit.Position + exit.Direction;
        Point start = _startConnection.GridPosition + _startConnection.Direction;
        return !_grid.HasPath(startPos, start);
    }

    private bool IsClosingPiece(PlacedPiece candidate)
    { // unchanged
        LocalConnection exit = candidate.ExitConnection;
        return exit.GetNextCell(candidate.GridPosition) == _startConnection.GridPosition &&
               exit.IsOpposite(_startConnection.Direction);
    }

    private float GetManhattanToStart(PlacedPiece candidate)
    { // Calculates the Manhattan distance from the exit to the start connection
        Point nextCell = candidate.GridPosition + candidate.ExitConnection.Position + candidate.ExitConnection.Direction;
        Point start = _startConnection.GridPosition + _startConnection.Direction;
        return Math.Abs(start.X - nextCell.X) + Math.Abs(start.Y - nextCell.Y);
    }


    // --- Scoring Helpers ---

    private float GetCloseLoopBonus(PlacedPiece placement, float bonus)
    { // Rewards a placement that closes the track
        return IsClosingPiece(placement) ? bonus : 0f;
    }

    private float GetSamePiecePenalty(PlacedPiece placement, float penalty)
    { // Penalizes repeating the same piece as the last one placed
        return (Track.Count > 0 && Track[^1].BasePiece == placement.BasePiece) ? -penalty : 0f;
    }

    private float GetSameTypePenalty(PlacedPiece placement, float penalty)
    { // Penalizes repeating the same piece type as the last one placed
        return (Track.Count > 0 && Track[^1].BasePiece.Type == placement.BasePiece.Type) ? -penalty : 0f;
    }

    private float GetEarlyBonus(float manhattan, float progress, float maxBonus)
    { // Rewards moving away from start early on
        return Math.Min(maxBonus, manhattan) * (1f - progress);
    }

    private float GetLateBonus(float manhattan, float progress, float maxBonus)
    { // Rewards moving closer to start later on
        return (maxBonus - Math.Min(maxBonus, manhattan)) * progress;
    }
    private float GetAlignmentBonus(PlacedPiece placement, float progress, float maxBonus)
    { // Rewards alignment of the exit direction towards the start connection, scaled by distance and progress
        LocalConnection exit = placement.ExitConnection;

        // Calculate the vector from the exit to the critical start cell
        Vector2 nextCell = (placement.GridPosition + exit.Position + exit.Direction).ToVector2();
        Vector2 criticalCell = (_startConnection.GridPosition + _startConnection.Direction).ToVector2();

        // Normalize the vector so the result isn't affected by distance
        Vector2 toHome = Vector2.Normalize(criticalCell - nextCell);
        return Vector2.Dot(exit.Direction.ToVector2(), toHome) * maxBonus * progress;
    }


    // --- Track Management Helpers --- 

    public void BeginTrack()
    { // Initializes the track with the starting piece

        ResetState();

        // Place the starting piece at the origin with no rotation or flip
        var (startEntry, startExit) = PieceLibrary.GetTransformedConnections(PieceLibrary.StartingPiece, 0, false);
        var startPiece = new PlacedPiece(PieceLibrary.StartingPiece, 0, false,
                         new Point(Settings.Generation.TrackOriginX, Settings.Generation.TrackOriginY),
                         startEntry, startExit);

        // Initialize the startConnection based on the starting piece's entry connection
        _startConnection = new WorldConnection(startPiece.GridPosition + startEntry.Position,
                                              startEntry.Direction);

        AddPiece(startPiece);
        GenerateOptionsForCurrentEnd();
    }

    public void AddPiece(PlacedPiece piece)
    { // Adds a piece to the track, occupies the grid and updates current connection

        if (piece == null) return;

        _totalAttempts++;
        Track.Add(piece);
        _grid.Occupy(piece);

        // Update currentConnection to the exit of the newly added piece
        _currentConnection = new WorldConnection(piece.GridPosition + piece.ExitConnection.Position,
                                                piece.ExitConnection.Direction);
    }

    public void RemovePiece(PlacedPiece piece)
    { // Removes the given piece from the track, unoccupies the grid and updates current connection

        if (Track.Count <= 1 || piece == null) return;

        _totalAttempts++;
        Track.Remove(piece);
        _grid.Unoccupy(piece);

        // Update currentConnection to the exit of the new last piece in the track
        _currentConnection = new WorldConnection(Track[^1].GridPosition + Track[^1].ExitConnection.Position,
                                                Track[^1].ExitConnection.Direction);

        // Pop the last options list since we're backtracking to the previous piece
        if (_optionsStack.Count > 0) _optionsStack.RemoveAt(_optionsStack.Count - 1);
    }
}