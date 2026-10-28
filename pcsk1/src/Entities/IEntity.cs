#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.World;

namespace pcsk1.src.Entities
{
    public interface IEntity
    {
        Vector2 Position { get; set; }
        Rectangle Bounds { get; }
        void Update(GameTime gameTime, TiledMap? map);
        void Draw(SpriteBatch spriteBatch);
    }
}
