using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Entities;
using pcsk1.src.Inputs;
using Microsoft.Xna.Framework.Input;
using System;
using pcsk1.src.World;

namespace pcsk1.src.GameObjects.Playeer
{
    public class Player : IEntity
    {
        public Vector2 Position { get; set; }
        public Rectangle Bounds => new((int)Position.X, (int)Position.Y, 64, 64);
        private bool _isRunning;
        private Texture2D _texture;
        private float Speed => _isRunning ? 500f : 200f; 
 
        public Player(Texture2D texture, Vector2 position)
        {
            _texture = texture;
            Position = position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Bounds, Color.White);
        }

        public void Update(GameTime gameTime, TiledMap? map)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = new(Input.GetAxis("horizontal"), Input.GetAxis("vertical"));

            if(direction != Vector2.Zero) direction.Normalize();

            _isRunning = Input.IsKeyDown(Keys.LeftShift);
            Position = Collisions.MoveAndCollide(Position, 64, 64, direction, Speed, deltaTime, map!.CollisionsRect);
        }
    }
}
