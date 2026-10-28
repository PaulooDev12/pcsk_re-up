
#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Entities;
using pcsk1.src.GameObjects.Playeer;
using pcsk1.src.World;
using System;



namespace pcsk1.src.GameObjects
{
    public class Enemy : IEntity
    {
        public Vector2 Position { get; set; }
        private readonly IEntity _target;
        private readonly float _speed = 100f;
        private readonly float _ray = 350f;
        public Rectangle Bounds => new((int)Position.X, (int)Position.Y, 64, 64);
        private Texture2D _enemyTexture;
        public Enemy(Vector2 position, IEntity target, Texture2D enemyTexture)
        {
            Position = position;
            _target = target;
            _enemyTexture = enemyTexture;
        }

        public void Update(GameTime gameTime, TiledMap? map)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            BoundingSphere enemyPoint = new(new Vector3(Position, 0), _ray);
            BoundingSphere proximityPoint = new(new Vector3(_target.Position, 0), _ray / 2f);
            BoundingSphere playerPoint = new(new Vector3(_target.Position, 0), 1f);

            if(enemyPoint.Intersects(playerPoint))
            {
                FollowTarget(deltaTime);
            }

        }

        private void FollowTarget(float deltaTime)
        {
            Vector2 direction = _target.Position - Position;
            if(direction != Vector2.Zero) direction.Normalize();

            Position += direction * deltaTime * _speed;

        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_enemyTexture, Bounds, Color.White);
        }
    }
}