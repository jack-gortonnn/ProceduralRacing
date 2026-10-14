using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProceduralRacing;

// Lets the player type a seed (blank = random) and pick a difficulty before going to the garage.

public class RaceSetupScreen : Screen
{
    private const int MaxSeedLength = 9;

    private readonly Game1 game;
    private readonly int playerCount;
    private readonly Button[] difficultyButtons;
    private readonly Button randomButton, backButton, continueButton;
    private readonly TrackDifficulty[] difficulties = (TrackDifficulty[])Enum.GetValues(typeof(TrackDifficulty));

    private TrackDifficulty selected = TrackDifficulty.Medium;
    private string seedInput = "";
    private KeyboardState previousKeys = Keyboard.GetState();

    public RaceSetupScreen(Game1 game, int playerCount)
    {
        this.game = game;
        this.playerCount = playerCount;
        int cx = game.ScreenWidth / 2, cy = game.ScreenHeight / 2;

        randomButton = new Button(new Rectangle(cx + 220, cy - 150, 160, 60), "RANDOM", () => seedInput = new Random().Next(1, 999999).ToString());

        difficultyButtons = new Button[difficulties.Length];
        for (int i = 0; i < difficulties.Length; i++)
        {
            var d = difficulties[i];
            difficultyButtons[i] = new Button(new Rectangle(cx - 380 + i * 190, cy + 20, 180, 60), d.ToString().ToUpper(), () => selected = d);
        }

        backButton = new Button(new Rectangle(cx - 380, cy + 220, 300, 70), "BACK", game.GoToMenu);
        continueButton = new Button(new Rectangle(cx + 80, cy + 220, 300, 70), "CONTINUE", Continue);
    }

    private void Continue()
    {
        int seed = int.TryParse(seedInput, out int typed) ? typed : new Random().Next(1, 999999);
        game.GoToGarage(playerCount, seed, selected);
    }

    public void Update(GameTime gameTime)
    {
        // Seed entry: digits only, backspace to delete, enter to continue
        KeyboardState keys = Keyboard.GetState();
        foreach (var key in keys.GetPressedKeys())
        {
            if (previousKeys.IsKeyDown(key)) continue; // only on the frame the key goes down

            if (key >= Keys.D0 && key <= Keys.D9 && seedInput.Length < MaxSeedLength) seedInput += (char)('0' + (key - Keys.D0));
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9 && seedInput.Length < MaxSeedLength) seedInput += (char)('0' + (key - Keys.NumPad0));
            if (key == Keys.Back && seedInput.Length > 0) seedInput = seedInput[..^1];
            if (key == Keys.Enter) { Continue(); return; }
            if (key == Keys.Escape) { game.GoToMenu(); return; }
        }
        previousKeys = keys;

        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            difficultyButtons[i].Update();
            difficultyButtons[i].Selected = difficulties[i] == selected;
        }
        randomButton.Update();
        backButton.Update();
        continueButton.Update();
    }

    public void Draw(SpriteBatch sb, GameTime gameTime)
    {
        sb.Begin();
        int cx = game.ScreenWidth / 2, cy = game.ScreenHeight / 2;

        Interface.DrawText(sb, playerCount == 1 ? "TIME TRIAL" : "1V1 RACE", new Vector2(cx, cy - 330), Interface.Text, 2f, centered: true);
        Interface.DrawText(sb, "Set up your track", new Vector2(cx, cy - 260), Interface.SubText, 1f, centered: true);

        // Seed box
        Interface.DrawText(sb, "SEED", new Vector2(cx - 380, cy - 200), Interface.SubText);
        var seedBox = new Rectangle(cx - 380, cy - 150, 580, 60);
        Interface.DrawPanel(sb, seedBox);
        bool caretOn = (int)(gameTime.TotalGameTime.TotalSeconds * 2) % 2 == 0;
        string shown = seedInput.Length == 0 ? "Random" : seedInput;
        Interface.DrawText(sb, shown + (caretOn ? "_" : ""), new Vector2(seedBox.X + 20, seedBox.Y + 12), seedInput.Length == 0 ? Interface.SubText : Interface.Text);
        randomButton.Draw(sb);

        // Difficulty
        Interface.DrawText(sb, "DIFFICULTY", new Vector2(cx - 380, cy - 30), Interface.SubText);
        foreach (var b in difficultyButtons) b.Draw(sb);

        var config = TrackConfig.FromDifficulty(selected);
        Interface.DrawText(sb, $"{config.Laps} laps", new Vector2(cx, cy + 130), Interface.SubText, 1f, centered: true);

        backButton.Draw(sb);
        continueButton.Draw(sb);
        sb.End();
    }
}
