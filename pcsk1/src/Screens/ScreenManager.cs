using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace pcsk1.src.Screens
{
    public sealed class ScreenManager
    {
        public IScreen CurrentScreen { get; private set; }

        public void ChangeState(IScreen newScreen)
        {
            CurrentScreen = newScreen ?? throw new ArgumentException($"{nameof(newScreen)} is inexistent");
        }

        public void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            CurrentScreen?.Draw(spriteBatch);
        }
    }
}
