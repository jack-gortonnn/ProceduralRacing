using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// Shared drawing helpers and colour palette, so every screen uses the same visual style.

public static class Interface
{
    // --- Palette ---
    public static readonly Color Background = new Color(18, 20, 26);
    public static readonly Color Panel = new Color(30, 34, 44) * 0.9f;
    public static readonly Color PanelBorder = new Color(70, 78, 96);
    public static readonly Color Accent = new Color(225, 6, 0);      // racing red
    public static readonly Color Text = Color.White;
    public static readonly Color SubText = new Color(160, 168, 184);
    public static readonly Color Good = new Color(60, 220, 110);
    public static readonly Color Bad = new Color(255, 80, 60);
    public static readonly Color Best = new Color(190, 90, 255);     // purple, like F1 timing screens
    public static readonly Color[] PlayerColors = { new Color(0, 200, 255), new Color(255, 150, 0) };

    private static SpriteFont font;
    private static Texture2D pixel;

    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
    {
        if (font != null) return;
        font = content.Load<SpriteFont>("fonts/Formula1");
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
    }

    public static Vector2 Measure(string text, float scale = 1f) => font.MeasureString(text) * scale;

    public static void DrawText(SpriteBatch sb, string text, Vector2 position, Color color, float scale = 1f, bool centered = false, bool shadow = true)
    {
        if (centered) position -= Measure(text, scale) / 2f;
        position = new Vector2(MathF.Round(position.X), MathF.Round(position.Y));
        if (shadow) sb.DrawString(font, text, position + new Vector2(2, 2), Color.Black * 0.6f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        sb.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public static void DrawRect(SpriteBatch sb, Rectangle rect, Color color) => sb.Draw(pixel, rect, color);

    public static void DrawPanel(SpriteBatch sb, Rectangle rect, Color? border = null)
    { // A filled box with a thin border and a red accent strip along the left edge
        DrawRect(sb, rect, Panel);
        DrawOutline(sb, rect, border ?? PanelBorder, 2);
        DrawRect(sb, new Rectangle(rect.X, rect.Y, 4, rect.Height), Accent);
    }

    public static void DrawOutline(SpriteBatch sb, Rectangle r, Color color, int t)
    {
        DrawRect(sb, new Rectangle(r.X, r.Y, r.Width, t), color);
        DrawRect(sb, new Rectangle(r.X, r.Bottom - t, r.Width, t), color);
        DrawRect(sb, new Rectangle(r.X, r.Y, t, r.Height), color);
        DrawRect(sb, new Rectangle(r.Right - t, r.Y, t, r.Height), color);
    }

    public static void DrawBar(SpriteBatch sb, Rectangle rect, float fraction, Color fill)
    { // A horizontal progress/stat bar
        DrawRect(sb, rect, Color.Black * 0.5f);
        DrawRect(sb, new Rectangle(rect.X, rect.Y, (int)(rect.Width * MathHelper.Clamp(fraction, 0f, 1f)), rect.Height), fill);
    }

    public static void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        Vector2 delta = end - start;
        sb.Draw(pixel, start, null, color, MathF.Atan2(delta.Y, delta.X), Vector2.Zero, new Vector2(delta.Length(), thickness), SpriteEffects.None, 0f);
    }

    public static string FormatTime(double seconds)
    { // 83.456 -> "1:23.456"
        if (seconds <= 0) return "-:--.---";
        int minutes = (int)(seconds / 60);
        return $"{minutes}:{seconds - minutes * 60:00.000}";
    }
}
