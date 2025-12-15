using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> piecePool;
    private List<PlacedPiece> track;
    private Dictionary<Point, bool> grid;

    public int GridOriginX = 8;
    public int GridOriginY = 8;

    public TrackGenerator(List<TrackPiece> availablePieces)
    {
        piecePool = availablePieces;
        track = new List<PlacedPiece>();
        grid = new Dictionary<Point, bool>();
    }

    public List<PlacedPiece> GenerateTrack()
    {
        track.Clear();
        grid.Clear();

        TrackPiece startPiece = piecePool.First(p => p.Name == "3x1_grid");

        var rotation = 0;
        var flipped = false;

        PlacedPiece placed = new PlacedPiece(
            startPiece,
            new Point(GridOriginX, GridOriginY),
            rotation,
            flipped
        );

        track.Add(placed);
        OccupyGrid(placed);

        return track;
    }

    private void OccupyGrid(PlacedPiece p)
    {
        int width = (p.Rotation % 2 == 0) ? p.BasePiece.Size.X : p.BasePiece.Size.Y;
        int height = (p.Rotation % 2 == 0) ? p.BasePiece.Size.Y : p.BasePiece.Size.X;

        for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
                grid[new Point(p.GridPosition.X + dx, p.GridPosition.Y + dy)] = true;
    }

}
