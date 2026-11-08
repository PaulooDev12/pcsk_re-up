using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsk1.src.GameObjects
{
    public class Sparkle : IThrowable
    {
        private const float Radius = 8f;
        public Texture2D _texture { get; set; }
        private Vector2 _startPosition;
        private Vector2 _direction;
        private float _distanceTraveled;
       

        public Vector2 Position { get; private set; }

        public float MaxRange { get; init; } = 250f;
        public float Speed { get; set; } = 400f;

        public IEntity Thrower { get; }

        public bool IsFlying { get; private set; }

        public BoundingSphere Bounds => new(
            new Vector3(Position, 0f),
            Radius
        );


        public Sparkle(IEntity thrower, Texture2D texture)
        {
            Thrower = thrower ?? throw new ArgumentNullException(nameof(thrower));
            _texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Position = thrower.Position;
        }

        public void Throw(Vector2 direction)
        {
            if (IsFlying || direction == Vector2.Zero)
                return;
            _startPosition = Thrower.Position;
            Position = _startPosition;

            _direction = Vector2.Normalize(direction);

            _distanceTraveled = 0f;
            IsFlying = true;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime =
            (float)gameTime.ElapsedGameTime.TotalSeconds;

            float remaingDistance = MaxRange - _distanceTraveled;

            float movement = MathF.Min(Speed * deltaTime, remaingDistance);

            Position += _direction * movement;

            _distanceTraveled += movement;

            if (_distanceTraveled >= MaxRange)
            {
                IsFlying = false;
                OnReachMaxRange();
            }
        }
        public void OnReachMaxRange()
        {
            return;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new(Position.X - Radius, Position.Y - Radius), null, Color.White, 0f, Vector2.Zero, new Vector2(Radius * 2 / _texture.Width, Radius * 2 / _texture.Height), SpriteEffects.None, 0f);
            
        }
    }

}

