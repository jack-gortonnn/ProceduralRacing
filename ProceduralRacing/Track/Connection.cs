using Microsoft.Xna.Framework;

public class Connection
{
    public Point Position { get; set; }
    public Point Direction { get; set; }

    public Connection(Point position, Point direction)
    {
        Position = position;
        Direction = direction;
    }
}