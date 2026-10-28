using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace pcsk1.src.World
{
    public static class Collisions
    {
        public static Vector2 MoveAndCollide(
            Vector2 position,
            int width,
            int height,
            Vector2 direction,
            float speed,
            float deltaTime,
            List<Rectangle> colliders
            )
        {

            position.X += direction.X * deltaTime * speed;
            Rectangle boundsX = new((int)position.X, (int)position.Y, width, height);
            foreach (var collider in colliders)
            {
                if (boundsX.Intersects(collider))
                {
                    if (direction.X > 0) position.X = collider.Left - width;
                    else if (direction.X < 0) position.X = collider.Right;

                    boundsX.X = (int)position.X;
                }
            }
            position.Y += direction.Y * deltaTime * speed;
            Rectangle boundsY = new((int)position.X, (int)position.Y, width, height);
            foreach (var collider in colliders)
            {
                if (boundsY.Intersects(collider))
                {
                    if (direction.Y > 0) position.Y = collider.Top - height;
                    else if (direction.Y < 0) position.Y = collider.Bottom;

                    boundsY.Y = (int)position.Y;
                }
            }
            return position;
        }
    }
}