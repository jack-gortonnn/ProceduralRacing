using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// A clickable box. Fires OnClick once per click (on the frame the mouse button goes down).

public class Button
{
    public Rectangle Bounds;
    public string Text;
    public Action OnClick;
    public bool Selected;     // drawn highlighted, e.g. the chosen difficulty
    public float TextScale = 1f;

    private bool isHovering;
    private bool wasPressed;

    public Button(Rectangle bounds, string text, Action onClick)
    {
        Bounds = bounds;
        Text = text;
        OnClick = onClick;
        wasPressed = Mouse.GetState().LeftButton == ButtonState.Pressed; // ignore a click that's still held from the previous screen
    }

    public void Update()
    {
        MouseState mouse = Mouse.GetState();
        isHovering = Bounds.Contains(mouse.Position);
        bool isPressed = mouse.LeftButton == ButtonState.Pressed;

        if (isHovering && isPressed && !wasPressed)
            OnClick?.Invoke();

        wasPressed = isPressed;
    }

    public void Draw(SpriteBatch sb)
    {
        Color fill = Selected ? Interface.Accent : isHovering ? new Color(55, 62, 80) : new Color(36, 40, 52);
        Interface.DrawRect(sb, Bounds, fill);
        Interface.DrawOutline(sb, Bounds, isHovering ? Color.White : Interface.PanelBorder, 2);
        Interface.DrawText(sb, Text, Bounds.Center.ToVector2(), Interface.Text, TextScale, centered: true);
    }
}
