#nullable enable

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;

namespace pcsk1.src.Screens.UI
{
    public class ConfirmScreen : GameScreen
    {
        private readonly string[] _lines;
        private readonly ExtendedScreenManager _screenManager;
        // private readonly Effect _grayScaleShader;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _whiteTexture;
        private readonly SpriteFont _font;
        private readonly Action? _onClosed;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Action _onConfirm;
        private readonly Action<int> _onConfirmSlot;
        private bool _isTextDraw = false;

        private Rectangle _messageBounds;
        private Rectangle _yesBounds;
        private Rectangle _noBounds;
        private ActionButton _yesButton = null!;
        private ActionButton _noButton = null!;

        private int _viewportWidth;
        private int _viewportHeight;

        private bool _isClosed;
        public ConfirmScreen(Game game, 
            GraphicsDevice graphicsDevice,
            ExtendedScreenManager screenManager, 
            SpriteBatch spriteBatch,
            Texture2D whiteTexture,
            SpriteFont font,
            string[] lines,
            Action onConfirm,
            Action? onClosed = null
            ) : base(game)
        {
            _graphicsDevice = graphicsDevice;
            _screenManager = screenManager;
            _spriteBatch = spriteBatch;
            _whiteTexture = whiteTexture;
            _font = font;
            _lines = lines;
            _onConfirm = onConfirm
                ?? throw new ArgumentNullException(nameof(onConfirm));

            _onClosed = onClosed;

            BuildLayout();

        }

        private void BuildLayout()
        {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;

            _viewportWidth = width;
            _viewportHeight = height;

            // Área da mensagem.
            _messageBounds = new Rectangle(
                (int)(width * 0.10f),
                (int)(height * 0.34f),
                (int)(width * 0.80f),
                (int)(height * 0.16f)
            );

            // Dimensões proporcionais dos botões.
            int buttonWidth = Math.Clamp(
                (int)(width * 0.14f), 120, 200
            );

            int buttonHeight = Math.Clamp(
                (int)(height * 0.08f), 45, 64
            );

            int gap = Math.Clamp(
                (int)(width * 0.04f), 24, 64
            );

            // Centraliza os dois botões como um conjunto.
            int totalWidth = buttonWidth * 2 + gap;
            int startX = (width - totalWidth) / 2;

            int buttonY = (int)(height * 0.62f);

            _yesBounds = new Rectangle(
                startX,
                buttonY,
                buttonWidth,
                buttonHeight
            );

            _noBounds = new Rectangle(
                startX + buttonWidth + gap,
                buttonY,
                buttonWidth ,
                buttonHeight
            );

            _yesButton = new ActionButton(
                _yesBounds,
                null,
                Confirm,
                Color.White,
                Color.LightSeaGreen,
                "SIM"
            );

            _noButton = new ActionButton(
                _noBounds,
                null,
                Cancel,
                Color.White,
                Color.LightSeaGreen,
                "NAO"
            );

        }

        public override void Draw(GameTime gameTime)
        {
            if (_viewportWidth != _graphicsDevice.Viewport.Width ||
       _viewportHeight != _graphicsDevice.Viewport.Height)
            {
                BuildLayout();
            }

            _spriteBatch.Begin(
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );
            Rectangle destRect = new(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_whiteTexture, destRect, Color.Black * 0.85f);


            
            for (int i = 0; i < _lines.Length; i++)
            {
                Rectangle destMessageRect = new(_messageBounds.X, _messageBounds.Y + (i * 40), _messageBounds.Width, _messageBounds.Height);

                DrawCenteredText(_lines[i], destMessageRect, Color.AliceBlue);

            }

          
            DrawCenteredText(
                _yesButton.Text,
                _yesBounds,
                _yesButton.CurrentColor
            );

            DrawCenteredText(
                _noButton.Text,
                _noBounds,
                _noButton.CurrentColor
            );

            _spriteBatch.End();
        }
        

        public override void Update(GameTime gameTime)
        {
            if (_isClosed || !IsActive)
                return;

            if (_viewportWidth != _graphicsDevice.Viewport.Width ||
                _viewportHeight != _graphicsDevice.Viewport.Height)
            {
                BuildLayout();
            }

            _yesButton.Update();

            if (_isClosed)
                return;

            _noButton.Update();
        }

        private void Confirm()
        {
            Action action = _onConfirm;

            CloseDialog();

            _onConfirm.Invoke();
        }

        private void DrawCenteredText(
    string text,
    Rectangle bounds,
    Color color)
        {
            Vector2 size = _font.MeasureString(text);

            Vector2 position = new(
                bounds.Center.X - size.X / 2f,
                bounds.Center.Y - size.Y / 2f
            );
            /*
            for(int x = -2; x <= 2; x+= 2)
            {
                for(int y = -2; y <= 2; y+= 2)
                {
                    if (x == 0 && y == 0) continue;

                    Vector2 offset = new Vector2(position.X + x, position.Y + y);
                    _spriteBatch.DrawString(_font, text, offset, Color.Blue);
                }
            }
            */
            _spriteBatch.DrawString(_font, text, new Vector2(position.X - 1.25f, position.Y), Color.Black);
            _spriteBatch.DrawString(_font, text, new Vector2(position.X + 1f, position.Y), Color.Black);
            _spriteBatch.DrawString(_font, text, new Vector2(position.X, position.Y - 1.25f), Color.Black);
            _spriteBatch.DrawString(_font, text, new Vector2(position.X, position.Y + 1f), Color.Black);

            _spriteBatch.DrawString(
                _font,
                text,
                position,
                color
            );
        }

        private void Cancel()
        {
            CloseDialog();
        }
        private void CloseDialog()
        {
            if (_isClosed)
                return;

            _isClosed = true;
            _isTextDraw = false;

            _screenManager.CloseScreen();
            _onClosed?.Invoke();
        }
    }
}
