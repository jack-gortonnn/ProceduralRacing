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

// The TrackConfig class holds all configuration parameters for track generation based on difficulty level.
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
    public int Laps;                // number of race laps for this difficulty

    public static TrackConfig FromDifficulty(TrackDifficulty difficulty)
    {
        switch (difficulty)
        {
            case TrackDifficulty.Easy:
                return new TrackConfig
                {
                    MaxLength = 10,
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
                    Randomness = 50f,
                    MaxPieceDifficulty = 1,
                    Laps = 5
                };

            case TrackDifficulty.Medium:
                return new TrackConfig
                {
                    MaxLength = 16,
                    MinLength = 12,
                    OptionPoolSize = 3,
                    MaxAttempts = 200,
                    BaseScore = 90f,
                    CloseLoopBonus = 1000f,
                    SamePiecePenalty = 50f,
                    SameTypePenalty = 25f,
                    MaxEarlyBonus = 20f,
                    MaxLateBonus = 600f,
                    AlignmentBonus = 100f,
                    Randomness = 75f,
                    MaxPieceDifficulty = 2,
                    Laps = 3
                };

            case TrackDifficulty.Hard:
                return new TrackConfig
                {
                    MaxLength = 22,
                    MinLength = 18,
                    OptionPoolSize = 4,
                    MaxAttempts = 200,
                    BaseScore = 80f,
                    CloseLoopBonus = 600f,
                    SamePiecePenalty = 75f,
                    SameTypePenalty = 40f,
                    MaxEarlyBonus = 30f,
                    MaxLateBonus = 500f,
                    AlignmentBonus = 60f,
                    Randomness = 100f,
                    MaxPieceDifficulty = 3,
                    Laps = 2
                };

            case TrackDifficulty.Extreme:
                return new TrackConfig
                {
                    MaxLength = 30,
                    MinLength = 26,
                    OptionPoolSize = 5,
                    MaxAttempts = 200,
                    BaseScore = 70f,
                    CloseLoopBonus = 400f,
                    SamePiecePenalty = 100f,
                    SameTypePenalty = 60f,
                    MaxEarlyBonus = 40f,
                    MaxLateBonus = 400f,
                    AlignmentBonus = 50f,
                    Randomness = 125f,
                    MaxPieceDifficulty = 4,
                    Laps = 1
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

}
