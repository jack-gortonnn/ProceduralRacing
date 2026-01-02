using System;
using System.Collections.Generic;
using System.Linq;

public enum TrackDifficulty
{
    Easy,
    Medium,
    Hard,
    Extreme
}

// Track Config holds all configuration parameters for track generation based on difficulty level.
// These parameters influence the length, complexity, and scoring of the generated track.

public class TrackConfig
{
    // --- Difficulty ---
    public TrackDifficulty Difficulty;

    // --- Track limits ---
    public int MaxLength;           // maximum number of pieces in the track
    public int MinLength;           // minimum number of pieces before closure is allowed
    public int OptionPoolSize;      // how many candidate pieces are scored at each step
    public int MaxAttempts;         // maximum number of moves to try before giving up
    public int MaxPieceDifficulty;  // maximum allowed difficulty of pieces

    // --- Scoring ---
    public float BaseScore;         // starting score for a candidate
    public float CloseLoopBonus;    // reward for closing the track
    public float SamePiecePenalty;  // penalty if repeating the same piece
    public float SameTypePenalty;   // penalty if repeating the same type of piece
    public float MaxEarlyBonus;     // reward for moving away from start early
    public float MaxLateBonus;      // reward for moving closer to start late
    public float AlignmentBonus;    // reward for proper alignment toward start
    public float Randomness;        // random factor in scoring

    public static TrackConfig FromDifficulty(TrackDifficulty difficulty)
    {
        switch (difficulty)
        {
            case TrackDifficulty.Easy:
                return new TrackConfig
                {
                    Difficulty = difficulty,
                    MaxLength = 15,
                    MinLength = 6,
                    OptionPoolSize = 2,
                    MaxAttempts = 200,
                    BaseScore = 100f,
                    CloseLoopBonus = 2000f,
                    SamePiecePenalty = 20f,
                    SameTypePenalty = 10f,
                    MaxEarlyBonus = 10f,
                    MaxLateBonus = 700f,
                    AlignmentBonus = 150f,
                    Randomness = 13f,
                    MaxPieceDifficulty = 1
                };

            case TrackDifficulty.Medium:
                return new TrackConfig
                {
                    Difficulty = difficulty,
                    MaxLength = 20,
                    MinLength = 10,
                    OptionPoolSize = 3,
                    MaxAttempts = 200,
                    BaseScore = 90f,
                    CloseLoopBonus = 1000f,
                    SamePiecePenalty = 50f,
                    SameTypePenalty = 25f,
                    MaxEarlyBonus = 20f,
                    MaxLateBonus = 600f,
                    AlignmentBonus = 100f,
                    Randomness = 25f,
                    MaxPieceDifficulty = 2
                };

            case TrackDifficulty.Hard:
                return new TrackConfig
                {
                    Difficulty = difficulty,
                    MaxLength = 30,
                    MinLength = 15,
                    OptionPoolSize = 4,
                    MaxAttempts = 200,
                    BaseScore = 80f,
                    CloseLoopBonus = 600f,
                    SamePiecePenalty = 75f,
                    SameTypePenalty = 40f,
                    MaxEarlyBonus = 30f,
                    MaxLateBonus = 500f,
                    AlignmentBonus = 60f,
                    Randomness = 50f,
                    MaxPieceDifficulty = 3
                };

            case TrackDifficulty.Extreme:
                return new TrackConfig
                {
                    Difficulty = difficulty,
                    MaxLength = 50,
                    MinLength = 20,
                    OptionPoolSize = 5,
                    MaxAttempts = 200,
                    BaseScore = 70f,
                    CloseLoopBonus = 400f,
                    SamePiecePenalty = 100f,
                    SameTypePenalty = 60f,
                    MaxEarlyBonus = 40f,
                    MaxLateBonus = 400f,
                    AlignmentBonus = 50f,
                    Randomness = 100f,
                    MaxPieceDifficulty = 4
                };

            default:
                throw new ArgumentOutOfRangeException(nameof(difficulty));
        }
    }

    public List<TrackPiece> GetAllowedPieces()
    { // Return list of pieces allowed based on max piece difficulty
        return PieceLibrary.All
            .Where(p => p.Difficulty <= MaxPieceDifficulty)
            .ToList();
    }

    private static readonly Dictionary<TrackDifficulty, Dictionary<int, float>> RatingBonuses = new()
    { // Dictionary of rating bonuses based on track difficulty and piece rating
        [TrackDifficulty.Easy] = new() { [1] = 50f }, 
        [TrackDifficulty.Medium] = new() { [1] = 25f, [2] = 50f },
        [TrackDifficulty.Hard] = new() { [1] = 13f, [2] = 25f, [3] = 50f },
        [TrackDifficulty.Extreme] = new() { [1] = 6f, [2] = 13f, [3] = 25f, [4] = 50f }
    };

    public float GetRatingBonus(int pieceRating)
    { // Get rating bonus for a piece based on its rating and track difficulty
        return RatingBonuses.TryGetValue(Difficulty, out var bonuses) &&
               bonuses.TryGetValue(pieceRating, out var bonus) ? bonus : 0f;
    }

}
