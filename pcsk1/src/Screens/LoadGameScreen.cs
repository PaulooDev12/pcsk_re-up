#nullable enable

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Inputs;
using pcsk1.src.Saves;
using pcsk1.src.Screens.UI;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace pcsk1.src.Screens
{
    using System.Diagnostics;
    using System.Text.Json;
    using MonoGame.Extended.Screens;

    using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;

    public sealed class LoadGameScreen : GameScreen
    {
        private readonly ExtendedScreenManager _screenManager;
        private readonly SpriteBatch _spriteBatch;

        private readonly SaveService _saveService;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;

        private readonly Action<SaveData, int> _loadGameplay;

        private readonly List<AbstractButton> _buttons = new();
        private readonly Rectangle[] _slotBounds = new Rectangle[3];

        private Rectangle _backBounds;
        private Rectangle _titleBounds;
        private Rectangle _messageBounds;

        private int _viewportWidth;
        private int _viewportHeight;

        private string _message = "";

        public LoadGameScreen(
            Game game,
            ExtendedScreenManager screenManager,
            SpriteBatch spriteBatch,
            SaveService saveService,
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            Texture2D whiteTexture,
            Action<SaveData, int> loadGameplay
        ) : base(game)
        {
            _screenManager = screenManager;
            _spriteBatch = spriteBatch;

            _saveService = saveService;
            _graphicsDevice = graphicsDevice;

            _font = font;
            _whiteTexture = whiteTexture;

            _loadGameplay = loadGameplay;

            UpdateWhenInactive = false;
            DrawWhenInactive = false;

            BuildLayout();
        }

        private void BuildLayout()
        {
            _buttons.Clear();

            _viewportWidth = _graphicsDevice.Viewport.Width;
            _viewportHeight = _graphicsDevice.Viewport.Height;

            int centerX = _viewportWidth / 2;

            int top = Math.Max(
                30,
                (_viewportHeight - 500) / 2
            );

            _titleBounds = new Rectangle(
                0,
                top,
                _viewportWidth,
                60
            );

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;

                Rectangle bounds = new(
                    centerX - 160,
                    top + 100 + i * 90,
                    320,
                    70
                );

                _slotBounds[i] = bounds;

                _buttons.Add(new ActionButton(
                    bounds,
                    _whiteTexture,
                    () => LoadSlot(slot),
                    Color.DarkSlateGray,
                    Color.IndianRed
                ));
            }

            _backBounds = new Rectangle(
                centerX - 110,
                top + 390,
                220,
                50
            );

            _messageBounds = new Rectangle(
                0,
                top + 445,
                _viewportWidth,
                50
            );

            _buttons.Add(new ActionButton(
                _backBounds,
                _whiteTexture,
                BackToMenu,
                Color.DarkSlateGray,
                Color.IndianRed
            ));
        }

        private void EnsureLayout()
        {
            if (_viewportWidth != _graphicsDevice.Viewport.Width ||
                _viewportHeight != _graphicsDevice.Viewport.Height)
            {
                BuildLayout();
            }
        }

        private void LoadSlot(int slot)
        {
            try
            {
                SaveData? save = _saveService.Load(slot);

                if (save == null)
                {
                    _message = $"Slot {slot} vazio.";
                    return;
                }

                _loadGameplay(save, slot);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException ||
                ex is NotSupportedException ||
                ex is ArgumentException)
            {
                _message = $"Erro ao carregar slot {slot}.";

                Debug.WriteLine(ex);
            }
        }

        private void BackToMenu()
        {
            _screenManager.CloseScreen();
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;

            if (Input.IsKeyPressed(Keys.Escape))
            {
                BackToMenu();
                return;
            }

            EnsureLayout();

            foreach (AbstractButton button in _buttons)
            {
                button.Update();

                if (!IsActive)
                    return;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            EnsureLayout();

            _spriteBatch.Begin(
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );

            foreach (AbstractButton button in _buttons)
            {
                button.Draw(_spriteBatch);
            }

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;

                string label = _saveService.Exists(slot)
                    ? $"SLOT {slot} - Carregar"
                    : $"SLOT {slot} - Vazio";

                DrawCenteredText(
                    label,
                    _slotBounds[i],
                    Color.White
                );
            }

            DrawCenteredText(
                "CARREGAR JOGO",
                _titleBounds,
                Color.White
            );

            DrawCenteredText(
                "VOLTAR",
                _backBounds,
                Color.White
            );

            if (!string.IsNullOrEmpty(_message))
            {
                DrawCenteredText(
                    _message,
                    _messageBounds,
                    Color.IndianRed
                );
            }

            _spriteBatch.End();
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

            _spriteBatch.DrawString(
                _font,
                text,
                position,
                color
            );
        }
    }
}
