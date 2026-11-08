using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Inputs;
using pcsk1.src.Screens.UI;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;
using MonoGame.Extended.Screens;
using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;
using MonoGame.Extended.Screens.Transitions;


namespace pcsk1.src.Screens
{
    public sealed class MainMenuScreen : GameScreen
    {
        private readonly ExtendedScreenManager _screenManager;
        private readonly Func<GameScreen> _createGameplay;
        private readonly Func<GameScreen> _createLoadScreen;
        private readonly Func<GameScreen> _createSettingsScreen;
        private readonly Action _exitGame;

        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;

        private readonly Texture2D _backgroundTexture;
        private Color _textColor = Color.White;
        private readonly List<AbstractButton> _buttons = new();

        private int _layoutWidth;
        private int _layoutHeight;
        private int _buttonStartY;

        public MainMenuScreen(
            Game game,
            ExtendedScreenManager screenManager,
            SpriteBatch spriteBatch,
            Texture2D whiteTexture,
            Func<GameScreen> createGameplay,
            Action exitGame,
            SpriteFont font,
            Func<GameScreen> createLoadScreen,
            Func<GameScreen> createSettingsScreen,
            Texture2D background
        ) : base(game)
        {
            _screenManager = screenManager;
            _spriteBatch = spriteBatch;
            _whiteTexture = whiteTexture;
            _createGameplay = createGameplay;
            _exitGame = exitGame;
            _font = font;
            _createLoadScreen = createLoadScreen;
            _createSettingsScreen = createSettingsScreen;
            _backgroundTexture = background;

            BuildLayout();
        }

        private void BuildLayout()
        {
            var viewport = GraphicsDevice.Viewport;

            _layoutWidth = viewport.Width;
            _layoutHeight = viewport.Height;

            int centerX = viewport.Width / 2;
            _buttonStartY = (int)(viewport.Height * 0.36f);

            const int buttonWidth = 260;
            const int buttonHeight = 55;
            const int spacing = 15;

            _buttons.Clear();

            _buttons.Add(new ActionButton(
                new Rectangle(
                    centerX - buttonWidth / 2,
                    _buttonStartY,
                    buttonWidth,
                    buttonHeight),
                _whiteTexture,
                StartGame,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));

            _buttons.Add(new ActionButton(
                new Rectangle(
                    centerX - buttonWidth / 2,
                    _buttonStartY + buttonHeight + spacing,
                    buttonWidth,
                    buttonHeight),
                _whiteTexture,
                OpenLoadScreen,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));

            _buttons.Add(new ActionButton(
                new Rectangle(
                    centerX - buttonWidth / 2,
                    _buttonStartY + (buttonHeight + spacing) * 2,
                    buttonWidth,
                    buttonHeight),
                _whiteTexture,
                OpenSettings,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));

            _buttons.Add(new ActionButton(
                new Rectangle(
                    centerX - buttonWidth / 2,
                    _buttonStartY + (buttonHeight + spacing) * 3,
                    buttonWidth,
                    buttonHeight),
                _whiteTexture,
                _exitGame,
                Color.DarkSlateGray,
                Color.IndianRed
            ));
        }

        private void EnsureLayout()
        {
            var viewport = GraphicsDevice.Viewport;

            if (_layoutWidth != viewport.Width ||
                _layoutHeight != viewport.Height)
            {
                BuildLayout();
            }
        }

        public override void Update(GameTime gameTime)
        {
            EnsureLayout();

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

        public override void Draw(GameTime gameTime)
        {
            EnsureLayout();

            int centerX = GraphicsDevice.Viewport.Width / 2;

            _spriteBatch.Begin();
            Rectangle bgRect = new(0, 0, _layoutWidth, _layoutHeight);
            _spriteBatch.Draw(_backgroundTexture, bgRect, Color.White);

            foreach (AbstractButton button in _buttons)
            {
                button.Draw(_spriteBatch);
            }

            DrawCenteredText(
                "MAIN MENU",
                new Vector2(centerX, GraphicsDevice.Viewport.Height * 0.22f)
            );

            const int buttonHeight = 55;
            const int spacing = 15;

            string[] labels =
            {
            "JOGAR",
            "CARREGAR",
            "CONFIGURACOES",
            "SAIR"
        };

            for (int i = 0; i < labels.Length; i++)
            {
                float centerY =
                    _buttonStartY + (buttonHeight + spacing) * i
                    + buttonHeight / 2f;

                DrawCenteredText(
                    labels[i],
                    new Vector2(centerX, centerY)
                );
            }

            _spriteBatch.End();
        }

        private void DrawCenteredText(string text, Vector2 center)
        {
            Vector2 textSize = _font.MeasureString(text);
            Vector2 targetPos = center - textSize / 2f;
            _spriteBatch.DrawString(_font, text, new Vector2(targetPos.X - 1.6f, targetPos.Y), Color.Black);
            _spriteBatch.DrawString(_font, text, new Vector2(targetPos.X + 1.6f, targetPos.Y), Color.Black);
            _spriteBatch.DrawString(_font, text, new Vector2(targetPos.X, targetPos.Y - 1.6f), Color.Black);
            _spriteBatch.DrawString(_font, text, new Vector2(targetPos.X, targetPos.Y + 1.6f), Color.Black);


            _spriteBatch.DrawString(
                _font,
                text,
                center - textSize / 2f,
                Color.White
            );
        }

        private void OpenSettings()
        {
            _screenManager.ShowScreen(_createSettingsScreen());
        }

        private void OpenLoadScreen()
        {
            _screenManager.ShowScreen(_createLoadScreen());
        }

        public void StartGame()
        {
            _screenManager.ReplaceScreen(_createGameplay(), new FadeTransition(GraphicsDevice, Color.Black, 2.5f));
        }
    }

}