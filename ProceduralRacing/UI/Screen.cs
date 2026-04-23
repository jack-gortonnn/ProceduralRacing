using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface Screen
{
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}