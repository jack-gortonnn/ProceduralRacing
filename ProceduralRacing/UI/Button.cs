using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Button
{
    public Rectangle Bounds { get; set; }
    public string Text { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public Color HoverColor { get; set; } = Color.Yellow;

    // An event or action to trigger when clicked
    public Action OnClick { get; set; }

    private bool _isHovering;

    public Button(Rectangle bounds, string text, Action onClick)
    {
        Bounds = bounds;
        Text = text;
        OnClick = onClick;
    }

    public void Update()
    {
        MouseState mouse = Mouse.GetState();
        Point mousePos = mouse.Position;

        _isHovering = Bounds.Contains(mousePos);

        if (_isHovering && mouse.LeftButton == ButtonState.Pressed)
        {
            OnClick?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color currentColor = _isHovering ? HoverColor : TextColor;

        Interface.DrawTextWithBorder(
            spriteBatch,
            Text,
            new Vector2(Bounds.X, Bounds.Y),
            currentColor,
            Color.Black,
            2
        );
    }
}