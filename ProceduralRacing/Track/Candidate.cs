using System.Collections.Generic;


// Candidates represent a potential placement of a track piece during the generation process.
// They include the piece to be placed, the exit connection used for further connections,
// and a score for evaluating suitability.

public class Candidate
{
    public PlacedPiece Piece { get; }
    public Connection Exit { get; }
    public float Score { get; }

    public Candidate(PlacedPiece piece, Connection exit, float score)
    {
        Piece = piece;
        Exit = exit;
        Score = score;
    }
}