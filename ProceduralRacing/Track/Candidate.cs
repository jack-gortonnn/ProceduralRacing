class Candidate
{
    public PlacedPiece Piece;
    public Connection Exit;
    public float Score;

    public Candidate(PlacedPiece piece, Connection exit, float score)
    {
        Piece = piece;
        Exit = exit;
        Score = score;
    }
}