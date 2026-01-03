using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

public static class Interface
{
    private static SpriteFont font;
    private static Texture2D pixel;

    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
    {
        font = content.Load<SpriteFont>("fonts/Formula1");

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public static void DrawTextWithBorder(SpriteBatch spriteBatch,string text,Vector2 position, Color textColor, Color borderColor, int thickness)
    {
        for (int x = -thickness; x <= thickness; x++)
        {
            for (int y = -thickness; y <= thickness; y++)
            {
                if (x == 0 && y == 0) continue;

                spriteBatch.DrawString(font,text,position + new Vector2(x, y), borderColor);
            }
        }

        spriteBatch.DrawString(font, text, position, textColor);
    }
}