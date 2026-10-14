using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProceduralRacing;

// Each player picks a chassis, engine and tyres. Stat bars show how the combination changes the car,
// and the car preview changes size with the chassis.

public class GarageScreen : Screen
{
    private class PlayerPanel
    {
        public Rectangle Bounds;
        public int Chassis, Engine, Tyres; // index into each part's All array
        public Button[] Arrows;
    }

    private readonly Game1 game;
    private readonly Action<CarLoadout[]> onConfirm;
    private readonly PlayerPanel[] panels;
    private readonly Button startButton, backButton;
    private readonly Texture2D carSprite;

    private static readonly string[] RowNames = { "CHASSIS", "ENGINE", "TYRES" };
    private const int RowStartY = 330, RowSpacing = 110;

    public GarageScreen(Game1 game, int playerCount, Action<CarLoadout[]> onConfirm)
    {
        this.game = game;
        this.onConfirm = onConfirm;
        carSprite = game.Content.Load<Texture2D>("textures/cars/car");

        int panelWidth = 700, gap = 60;
        int totalWidth = playerCount * panelWidth + (playerCount - 1) * gap;
        int startX = (game.ScreenWidth - totalWidth) / 2;

        panels = new PlayerPanel[playerCount];
        for (int p = 0; p < playerCount; p++)
        {
            var panel = new PlayerPanel { Bounds = new Rectangle(startX + p * (panelWidth + gap), 120, panelWidth, 820) };
            panel.Arrows = new Button[6];

            for (int row = 0; row < 3; row++)
            {
                int r = row, y = panel.Bounds.Y + RowStartY + row * RowSpacing;
                panel.Arrows[row * 2] = new Button(new Rectangle(panel.Bounds.X + 30, y, 60, 60), "<", () => Cycle(panel, r, -1));
                panel.Arrows[row * 2 + 1] = new Button(new Rectangle(panel.Bounds.Right - 90, y, 60, 60), ">", () => Cycle(panel, r, 1));
            }
            panels[p] = panel;
        }

        backButton = new Button(new Rectangle(game.ScreenWidth / 2 - 420, game.ScreenHeight - 110, 300, 70), "BACK", () => game.GoToSetup(playerCount));
        startButton = new Button(new Rectangle(game.ScreenWidth / 2 + 120, game.ScreenHeight - 110, 300, 70), "START RACE", Confirm);
    }

    private static void Cycle(PlayerPanel panel, int row, int direction)
    {
        static int Wrap(int i, int count) => (i + count) % count;
        if (row == 0) panel.Chassis = Wrap(panel.Chassis + direction, Chassis.All.Length);
        if (row == 1) panel.Engine = Wrap(panel.Engine + direction, Engine.All.Length);
        if (row == 2) panel.Tyres = Wrap(panel.Tyres + direction, Tyres.All.Length);
    }

    private static CarLoadout LoadoutOf(PlayerPanel p) => new CarLoadout
    {
        Chassis = Chassis.All[p.Chassis],
        Engine = Engine.All[p.Engine],
        Tyres = Tyres.All[p.Tyres],
    };

    private void Confirm()
    {
        var loadouts = new CarLoadout[panels.Length];
        for (int i = 0; i < panels.Length; i++) loadouts[i] = LoadoutOf(panels[i]);
        onConfirm(loadouts);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var panel in panels)
            foreach (var arrow in panel.Arrows) arrow.Update();
        backButton.Update();
        startButton.Update();
    }

    public void Draw(SpriteBatch sb, GameTime gameTime)
    {
        sb.Begin(samplerState: SamplerState.PointClamp);
        Interface.DrawText(sb, "GARAGE", new Vector2(game.ScreenWidth / 2f, 60), Interface.Text, 2f, centered: true);

        for (int p = 0; p < panels.Length; p++)
            DrawPanel(sb, panels[p], p, gameTime);

        backButton.Draw(sb);
        startButton.Draw(sb);
        sb.End();
    }

    private void DrawPanel(SpriteBatch sb, PlayerPanel panel, int playerIndex, GameTime gameTime)
    {
        var b = panel.Bounds;
        var loadout = LoadoutOf(panel);
        Color playerColor = Interface.PlayerColors[playerIndex];

        Interface.DrawPanel(sb, b);
        Interface.DrawText(sb, panels.Length == 1 ? "YOUR CAR" : $"PLAYER {playerIndex + 1}", new Vector2(b.X + 30, b.Y + 20), playerColor, 1.2f);
        Interface.DrawText(sb, playerIndex == 0 ? "W A S D" : "NUMPAD 8 4 5 6", new Vector2(b.Right - 30 - Interface.Measure(playerIndex == 0 ? "W A S D" : "NUMPAD 8 4 5 6").X, b.Y + 26), Interface.SubText);

        // Car preview - slowly rotating, scaled by chassis size
        float spin = (float)gameTime.TotalGameTime.TotalSeconds * 0.8f;
        float previewScale = 1.6f * loadout.Chassis.Size;
        sb.Draw(carSprite, new Vector2(b.Center.X, b.Y + 190), null, playerColor, spin,
            new Vector2(carSprite.Width / 2f, carSprite.Height / 2f), previewScale, SpriteEffects.None, 0f);

        // Part selectors
        string[] names = { loadout.Chassis.DisplayName, loadout.Engine.DisplayName, loadout.Tyres.DisplayName };
        string[] descriptions = { loadout.Chassis.Description, loadout.Engine.Description, loadout.Tyres.Description };
        for (int row = 0; row < 3; row++)
        {
            int y = b.Y + RowStartY + row * RowSpacing;
            Interface.DrawText(sb, RowNames[row], new Vector2(b.Center.X, y - 18), Interface.SubText, 0.7f, centered: true);
            Interface.DrawText(sb, names[row], new Vector2(b.Center.X, y + 22), Interface.Text, 1f, centered: true);
            Interface.DrawText(sb, descriptions[row], new Vector2(b.Center.X, y + 58), Interface.SubText, 0.7f, centered: true);
        }
        foreach (var arrow in panel.Arrows) arrow.Draw(sb);

        // Stat bars (1.0 multiplier = half-full bar, so upgrades and downgrades are both visible)
        float topSpeed = loadout.Engine.TopSpeed * loadout.Chassis.Speed;
        float grip = loadout.Tyres.Grip * loadout.Chassis.Grip;
        (string label, float value)[] stats =
        {
            ("TOP SPEED", topSpeed),
            ("ACCELERATION", loadout.Engine.Acceleration),
            ("GRIP", grip),
            ("BRAKING", loadout.Tyres.Braking),
            ("TURNING", loadout.Tyres.Turning),
        };

        int statY = b.Y + 650;
        for (int i = 0; i < stats.Length; i++)
        {
            int y = statY + i * 32;
            Interface.DrawText(sb, stats[i].label, new Vector2(b.X + 30, y), Interface.SubText, 0.6f, shadow: false);
            Color fill = stats[i].value > 1.01f ? Interface.Good : stats[i].value < 0.99f ? Interface.Bad : Interface.Text;
            Interface.DrawBar(sb, new Rectangle(b.X + 260, y + 4, b.Width - 300, 14), stats[i].value / 2f, fill);
        }
    }
}
