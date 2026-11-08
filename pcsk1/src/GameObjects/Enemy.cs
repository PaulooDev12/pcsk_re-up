
#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Entities;
using pcsk1.src.Events;
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
        private float _attackCooldown = 1f;
        public Enemy(Vector2 position, IEntity target, Texture2D enemyTexture)
        {
            Position = position;
            _target = target;
            _enemyTexture = enemyTexture;
            Initialize();
        }

        public void Initialize()
        {
            EventBus<PlayerDeathEvent>.Subscribe(HandlePlayerDeath);
        }

        public void Update(GameTime gameTime, TiledMap? map)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _attackCooldown += deltaTime;
            BoundingSphere enemyPoint = new(new Vector3(Position, 0), _ray);
            BoundingSphere proximityPoint = new(new Vector3(_target.Position, 0), _ray / 2f);
            BoundingSphere playerPoint = new(new Vector3(_target.Position, 0), 1f);

            AttackTarget();

            if (enemyPoint.Intersects(playerPoint))
            {
                FollowTarget(deltaTime);
            }

        }

        private void Reset()
        {
            Position = new Vector2(200, 400);
        }

        private void FollowTarget(float deltaTime)
        {
            Vector2 direction = _target.Position - Position;
            if(direction != Vector2.Zero) direction.Normalize();

            Position += direction * deltaTime * _speed;

        }

        public void HandlePlayerDeath(PlayerDeathEvent e)
        {
            if(e.EnemyNeedsToReset)
            {
                Reset();
            }
        }


        public void AttackTarget()
        {
           Rectangle proximity = new Rectangle(Bounds.X - 1, Bounds.Y - 1, Bounds.Width + 2, Bounds.Height + 2);
           if(proximity.Intersects(_target.Bounds) && _attackCooldown >= 0.5f)
            {
                if (_target is Player player)
                {
                    player.TakeMentalDamage(20);
                    _attackCooldown = 0f;
                }
            }

        }
        public void Draw(SpriteBatch spriteBatch)
        {
            int padding = 8;
            Rectangle renderBounds = new(Bounds.X - padding, Bounds.Y - padding, Bounds.Width + (padding * 2), Bounds.Height + (padding * 2));
            spriteBatch.Draw(_enemyTexture, renderBounds, Color.White);
        }
    }
}