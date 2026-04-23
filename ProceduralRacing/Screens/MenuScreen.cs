using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using ProceduralRacing;

public class MenuScreen : Screen
{
    private Game1 _game;
    private string _seedInput = "";
    private KeyboardState _prevKey;
    private Button _playButton;

    public MenuScreen(Game1 game)
    {
        _game = game;
        // Position the button below where the text input will be
        _playButton = new Button(new Rectangle(300, 450, 200, 50), "PLAY", () => {
            if (int.TryParse(_seedInput, out int result))
                _game.StartGame(result);
            else
                _game.StartGame(new Random().Next(1, 999999)); // Default if empty
        });
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState kb = Keyboard.GetState();

        // --- Simple Text Input Logic ---
        foreach (var key in kb.GetPressedKeys())
        {
            if (_prevKey.IsKeyUp(key)) // Only trigger once per press
            {
                if (key >= Keys.D0 && key <= Keys.D9) // Numbers only
                    _seedInput += key.ToString().Replace("D", "");

                if (key == Keys.Back && _seedInput.Length > 0) // Backspace
                    _seedInput = _seedInput.Substring(0, _seedInput.Length - 1);
            }
        }

        _playButton.Update();
        _prevKey = kb;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Begin();

        Interface.DrawTextWithBorder(spriteBatch, "ENTER SEED:", new Vector2(300, 350), Color.White, Color.Black, 2);

        // Draw the current typing buffer
        Color inputColor = Color.Cyan;
        Interface.DrawTextWithBorder(spriteBatch, _seedInput + "_", new Vector2(300, 400), inputColor, Color.Black, 2);

        _playButton.Draw(spriteBatch);

        spriteBatch.End();
    }
}