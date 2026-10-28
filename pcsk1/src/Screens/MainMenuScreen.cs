using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Inputs;
using pcsk1.src.Screens.UI;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;

namespace pcsk1.src.Screens
{
    public sealed class MainMenuScreen : IScreen
    {
        private readonly ScreenManager _screenManager;
        private readonly Func<IScreen> _createGameplay;
        private readonly Action _exitGame;
        private SpriteFont _font;
        private readonly List<AbstractButton> _buttons;
        private readonly Func<IScreen> _createLoadScreen;
        public MainMenuScreen(
        ScreenManager screenManager,
        GraphicsDevice graphicsDevice,
        Texture2D whiteTexture,
        Func<IScreen> createGameplay,
        Action exitGame,
        SpriteFont font,
        Func<IScreen> createLoadScreen
        )
        {
            _screenManager = screenManager;
            _createGameplay = createGameplay;
            _exitGame = exitGame;
            _font = font;
            _createLoadScreen = createLoadScreen;

            int centerX = graphicsDevice.Viewport.Width / 2;
            _buttons = new List<AbstractButton>
            {
                new ActionButton(
                    new Rectangle(centerX - 110, 280, 220, 55),
                    whiteTexture,
                    StartGame,
                    Color.DarkSlateGray,
                    Color.Green
                    ),

                 new ActionButton(
                     new Rectangle(centerX - 110, 440, 220, 55),
                     whiteTexture,
                     OpenLoadScreen,
                     Color.DarkSlateGray,
                     Color.BlueViolet
                     ),

                 new ActionButton(
                    new Rectangle(centerX - 110, 380, 220, 55),
                    whiteTexture,
                    _exitGame,
                    Color.DarkSlateGray,
                    Color.IndianRed
            )
            };
        }
        public void Update(GameTime gameTime)
        {
            
            if (Input.IsKeyPressed(Keys.Enter))
            {
                StartGame();
                return;
            }

            foreach (AbstractButton button in _buttons)
            {
                button.Update();
            }
        }

        private void OpenLoadScreen()
        {
            _screenManager.ChangeState(_createLoadScreen());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            foreach (AbstractButton button in _buttons)
            {
                button.Draw(spriteBatch);
            }

            spriteBatch.DrawString(
                _font,
                "MAIN MENU",
                new Vector2(500, 160),
            Color.White
            );

            spriteBatch.DrawString(
                _font,
                "JOGAR",
                new Vector2(570, 295),
                Color.White
            );

            spriteBatch.DrawString(
                _font,
                "SAIR",
                new Vector2(580, 365),
                Color.White
            );

            spriteBatch.End();
        }



        public void StartGame()
        {
            _screenManager.ChangeState(_createGameplay());
        }
    }
}