using Microsoft.Xna.Framework;
using System;


namespace pcsk1.src.Entities
{
    public interface IThrowable
    {
        Vector2 Position { get; }
        BoundingSphere Bounds { get; }

        float MaxRange { get; }
        float Speed { get; }

        IEntity Thrower { get; }

        bool IsFlying { get; }

        void Throw(Vector2 direction);
        void Update(GameTime gameTime);

    }
}
