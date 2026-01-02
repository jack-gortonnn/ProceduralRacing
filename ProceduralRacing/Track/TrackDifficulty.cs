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

public class TrackConfig
{
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
                    MaxLength = 15,
                    MinLength = 6,
                    OptionPoolSize = 3,
                    MaxAttempts = 200,
                    BaseScore = 100f,
                    CloseLoopBonus = 2000f,
                    SamePiecePenalty = 20f,
                    SameTypePenalty = 10f,
                    MaxEarlyBonus = 15f,
                    MaxLateBonus = 1500f,
                    AlignmentBonus = 150f,
                    Randomness = 20f,
                    MaxPieceDifficulty = 1
                };

            case TrackDifficulty.Medium:
                return new TrackConfig
                {
                    MaxLength = 25,
                    MinLength = 8,
                    OptionPoolSize = 5,
                    MaxAttempts = 200,
                    BaseScore = 50f,
                    CloseLoopBonus = 1000f,
                    SamePiecePenalty = 50f,
                    SameTypePenalty = 25f,
                    MaxEarlyBonus = 10f,
                    MaxLateBonus = 1000f,
                    AlignmentBonus = 100f,
                    Randomness = 10f,
                    MaxPieceDifficulty = 2
                };

            case TrackDifficulty.Hard:
                return new TrackConfig
                {
                    MaxLength = 35,
                    MinLength = 10,
                    OptionPoolSize = 7,
                    MaxAttempts = 200,
                    BaseScore = 40f,
                    CloseLoopBonus = 600f,
                    SamePiecePenalty = 75f,
                    SameTypePenalty = 40f,
                    MaxEarlyBonus = 6f,
                    MaxLateBonus = 600f,
                    AlignmentBonus = 60f,
                    Randomness = 2f,
                    MaxPieceDifficulty = 3
                };

            case TrackDifficulty.Extreme:
                return new TrackConfig
                {
                    MaxLength = 50,
                    MinLength = 12,
                    OptionPoolSize = 9,
                    MaxAttempts = 200,
                    BaseScore = 30f,
                    CloseLoopBonus = 400f,
                    SamePiecePenalty = 100f,
                    SameTypePenalty = 60f,
                    MaxEarlyBonus = 5f,
                    MaxLateBonus = 400f,
                    AlignmentBonus = 50f,
                    Randomness = 1f,
                    MaxPieceDifficulty = 4
                };

            default:
                throw new ArgumentOutOfRangeException(nameof(difficulty));
        }
    }

    public List<TrackPiece> GetAllowedPieces()
    {
        return PieceLibrary.All
            .Where(p => p.Difficulty <= MaxPieceDifficulty)
            .ToList();
    }
}
