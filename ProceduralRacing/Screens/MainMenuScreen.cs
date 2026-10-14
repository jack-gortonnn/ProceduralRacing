using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProceduralRacing;

// The first screen: choose Time Trial (1 player) or 1v1 Race (2 players, split screen).

public class MainMenuScreen : Screen
{
    private readonly Button[] buttons;
    private readonly Game1 game;

    public MainMenuScreen(Game1 game)
    {
        this.game = game;
        int x = game.ScreenWidth / 2 - 200, y = game.ScreenHeight / 2;

        buttons = new[]
        {
            new Button(new Rectangle(x, y,       400, 70), "TIME TRIAL", () => game.GoToSetup(1)),
            new Button(new Rectangle(x, y + 90,  400, 70), "1V1 RACE",   () => game.GoToSetup(2)),
            new Button(new Rectangle(x, y + 180, 400, 70), "QUIT",       game.Exit),
        };
    }

    public void Update(GameTime gameTime)
    {
        foreach (var b in buttons) b.Update();
    }

    public void Draw(SpriteBatch sb, GameTime gameTime)
    {
        sb.Begin();
        Vector2 centre = new Vector2(game.ScreenWidth / 2f, game.ScreenHeight / 2f);

        Interface.DrawText(sb, "PROCEDURAL RACING", centre - new Vector2(0, 260), Interface.Text, 3f, centered: true);
        Interface.DrawRect(sb, new Rectangle((int)centre.X - 300, (int)centre.Y - 195, 600, 6), Interface.Accent);
        Interface.DrawText(sb, "A new track, every time", centre - new Vector2(0, 150), Interface.SubText, 1f, centered: true);

        foreach (var b in buttons) b.Draw(sb);
        sb.End();
    }
}
