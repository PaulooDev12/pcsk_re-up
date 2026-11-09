using FlatRedBall.Glue.StateInterpolation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Entities;
using pcsk1.src.Events;
using pcsk1.src.Inputs;
using pcsk1.src.World;
using System;
using System.Collections.Generic;

namespace pcsk1.src.GameObjects.Playeer
{
    public class Player : IEntity

    {

        public Vector2 Position { get; set; }

        private Sparkle sparkle;

        public int MentalSanity { get; private set; } = 100;
        public Rectangle Bounds => new((int)Position.X, (int)Position.Y, 32, 32);

        private bool _isRunning;

        private Texture2D _texture;

        private Texture2D _spriteSheet;

        public int Life { get; set; } = 100;

        private float Speed => _isRunning ? 500f : 200f;
        public void LoadContent(ContentManager content, string assetName)

        {

            content.Load<Texture2D>(assetName);

        }


        public Player(Texture2D texture, Vector2 position)

        {

            _texture = texture;

            Position = position;

            Initialize();

        }





        public void Initialize()

        {
        }



        private void Reset()

        {

            Position = new Vector2(400, 300);

        }



        public void Draw(SpriteBatch spriteBatch)
        {
            int horizontalPadding = 8;
            int verticalPadding = 20;

            Rectangle renderBounds = new(
                (int)Position.X - horizontalPadding,
                (int)Position.Y - horizontalPadding,
                Bounds.Width + (horizontalPadding * 2),
                Bounds.Height + verticalPadding
            );
            spriteBatch.Draw(_texture, renderBounds, Color.White);

            sparkle?.Draw(spriteBatch);

        }



        public void Update(GameTime gameTime, TiledMap? map)

        {

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 direction = new(Input.GetAxis("horizontal"), Input.GetAxis("vertical"));



            if (direction != Vector2.Zero) direction.Normalize();

            sparkle?.Update(gameTime);

            if (Input.IsKeyPressed(Keys.Space))

            {

                sparkle = new Sparkle(this, _texture);

                sparkle.Throw(direction);


            }

            _isRunning = Input.IsKeyDown(Keys.LeftShift);

            Position = Collisions.MoveAndCollide(Position, 32, 32, direction, Speed, deltaTime, map!.CollisionsRect);

        }



        public void TakeMentalDamage(int amount)
        {

            MentalSanity = Math.Clamp(MentalSanity - amount, 0, 100);

            EventBus<AttributeChangedEvent<int>>.Publish(new AttributeChangedEvent<int>("player", "MentalSanity", MentalSanity));

        }

    }
}
