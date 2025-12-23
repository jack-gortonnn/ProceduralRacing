using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrackGenerator
{
    private List<TrackPiece> PiecePool;
    private List<PlacedPiece> Track;
    private Grid Grid;

    private WorldConnection currentConnection;
    
    public Random random;

    private int GridOriginX = 10;
    private int GridOriginY = 14;

    public TrackGenerator(List<TrackPiece> availablePieces, Grid grid, int seed)
    {
        PiecePool = availablePieces;
        Track = new List<PlacedPiece>();
        Grid = grid;
        random = new Random(seed);
    }

    private void ResetState()
    {
        Track.Clear();
        Grid.Clear();
        currentConnection = default;
    }

    public List<PlacedPiece> GenerateTrack()
    {
        ResetState();

        var startPiece = new PlacedPiece(GetPiece("5x1_grid"), 0, false, new Point(GridOriginX, GridOriginY));
        Grid.OccupyRectangle(startPiece.GridPosition, startPiece.TransformedSize);
        Track.Add(startPiece);

        Connection startExit = startPiece.TransformedConnections[1];
        currentConnection = new WorldConnection(
            startPiece.GridPosition + startExit.Position + startExit.Direction,
            startExit.Direction
        );

        for (int i = 0; i < Constants.MaxTrackLength; i++)
        {
            var randomPiece = PiecePool[random.Next(PiecePool.Count)];

            int rotation = random.Next(4);
            bool flipped = random.Next(2) == 0;

            if (TryFindPlacement(randomPiece, rotation, flipped, out var placed, out var exit))
            {
                AddPiece(placed, exit);
            }
            else
            {
                continue;
            }
        }

        return Track;
    }


    public bool TryFindPlacement(TrackPiece piece, int rotation, bool flipped, out PlacedPiece placed, out Connection exit)
    {
        placed = null;
        exit = null;

        var connections = piece.GetTransformedConnections(rotation, flipped);

        // 1. Find a valid entry connection
        if (!HasValidEntry(connections, out var entry))
            return false;

        // 2. Make a placement based on that entry and see if it fits
        var candidate = new PlacedPiece(piece, rotation, flipped, currentConnection.GridPosition - entry.Position);
        if (!HasValidPlacement(candidate))
            return false;

        // 3. Find a valid exit connection that leads to empty space
        if (!HasValidExit(connections, entry, candidate, out exit))
            return false;

        placed = candidate;
        return true;
    }

    // ----- Helpers -----

    private TrackPiece GetPiece(string name)
    {
        return PiecePool.FirstOrDefault(p => p.Name == name);
    }

    private bool HasValidEntry(List<Connection> connections, out Connection entry)
    {
        entry = connections.FirstOrDefault(c => c.IsOpposite(currentConnection.Direction));
        return entry != null;
    }

    private bool HasValidPlacement(PlacedPiece p)
    {
        bool placement = !Grid.IsRectangleInBounds(p.GridPosition, p.TransformedSize)
                       || Grid.IsRectangleOccupied(p.GridPosition, p.TransformedSize);
        return !placement;
    }

    private bool HasValidExit(List<Connection> connections, Connection entry, PlacedPiece p, out Connection exit)
    {
        exit = connections.FirstOrDefault(c => c != entry
                                            && c.LeadsToEmptySpace(p.GridPosition, Grid));
        return exit != null;
    }

    public void AddPiece(PlacedPiece piece, Connection exit)
    {
        Grid.OccupyRectangle(piece.GridPosition, piece.TransformedSize);
        Track.Add(piece);

        currentConnection = new WorldConnection(
            piece.GridPosition + exit.Position + exit.Direction,
            exit.Direction
        );
    }
}
