using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace pcsk1.src.Screens
{
    public interface IScreen
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
