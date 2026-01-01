using System.Collections.Generic;

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