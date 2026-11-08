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
using MonoGame.Extended.Screens;
using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;
namespace pcsk1.src.Screens
{

    public sealed class SaveScreen : GameScreen
    {
        private readonly ExtendedScreenManager _screenManager;
        private readonly Action<int> _saveGame;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SaveService _saveService;
        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;
        private readonly SpriteBatch _spriteBatch;

        private readonly Rectangle[] _slotBounds = new Rectangle[3];
        private readonly List<AbstractButton> _buttons = new();

        private int _selectedSlot;

        private Rectangle _backBounds;
        private Rectangle _titleBounds;

        private int _layoutWidth;
        private int _layoutHeight;

        private string _message = "";

        public SaveScreen(
            Game game,
            ExtendedScreenManager screenManager,
            SpriteBatch spriteBatch,
            Action<int> saveGame,
            GraphicsDevice graphicsDevice,
            SaveService saveService,
            SpriteFont font,
            Texture2D whiteTexture
        ) : base(game)
        {
            _screenManager = screenManager;
            _spriteBatch = spriteBatch;
            _saveGame = saveGame;

            _graphicsDevice = graphicsDevice;
            _saveService = saveService;
            _font = font;
            _whiteTexture = whiteTexture;

            BuildLayout();
        }

        private void BuildLayout()
        {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;
            int centerX = width / 2;

            _layoutWidth = width;
            _layoutHeight = height;

            const int slotWidth = 320;
            const int slotHeight = 70;
            const int spacing = 20;

            int firstSlotY = (int)(height * 0.27f);

            _buttons.Clear();

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;

                Rectangle bounds = new(
                    centerX - slotWidth / 2,
                    firstSlotY + i * (slotHeight + spacing),
                    slotWidth,
                    slotHeight
                );

                _slotBounds[i] = bounds;

                _buttons.Add(new ActionButton(
                    bounds,
                    _whiteTexture,
                    () => SaveSlot(slot),
                    Color.DarkSlateGray,
                    Color.SteelBlue
                ));
            }

            _backBounds = new Rectangle(
                centerX - 110,
                (int)(height * 0.82f),
                220,
                55
            );

            _titleBounds = new Rectangle(
                0,
                (int)(height * 0.10f),
                width,
                70
            );

            _buttons.Add(new ActionButton(
                _backBounds,
                _whiteTexture,
                BackToPause,
                Color.DarkSlateGray,
                Color.IndianRed
            ));
        }

        private void EnsureLayout()
        {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;

            if (_layoutWidth != width || _layoutHeight != height)
            {
                BuildLayout();
            }
        }

        private void SelectAndSaveSlot()
        {
            _saveGame(_selectedSlot);
        }

        private void SaveSlot(int slot)
        {
            try
            {
                _selectedSlot = slot;
                if (_saveService.Exists(slot))
                {

                    bool previousDrawState = DrawWhenInactive;

                    DrawWhenInactive = true;

                    _screenManager.ShowScreen(
                        new ConfirmScreen(
                            Game,
                            _graphicsDevice,
                            _screenManager,
                            _spriteBatch,
                            _whiteTexture,
                            _font,
                            [$"Deseja Mesmo Sobrescrever o slot {slot}?"],
                            onConfirm: SelectAndSaveSlot,
                            onClosed: () => DrawWhenInactive = previousDrawState
                         )
                        );
                    return;
                }
                _saveGame(slot);

                // Fecha a tela e retorna à pausa.
                _screenManager.CloseScreen();
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is ArgumentException ||
                ex is NotSupportedException)
            {
                _message = $"Falha ao salvar: {ex.Message}";
            }
        }

        private void BackToPause()
        {
            _screenManager.CloseScreen();
        }

        public override void Update(GameTime gameTime)
        {
            EnsureLayout();

            if (Input.IsKeyPressed(Keys.Escape))
            {
                BackToPause();
                return;
            }

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
                samplerState: SamplerState.PointClamp
            );

            _spriteBatch.Draw(
                _whiteTexture,
                new Rectangle(
                    0,
                    0,
                    _graphicsDevice.Viewport.Width,
                    _graphicsDevice.Viewport.Height
                    ),
                Color.DarkBlue
                );

            foreach (AbstractButton button in _buttons)
            {
                button.Draw(_spriteBatch);
            }

            DrawCenteredText(
                "SALVAR JOGO",
                _titleBounds,
                Color.White
            );

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;

                string label = _saveService.Exists(slot)
                    ? $"SLOT {slot} - SOBRESCREVER"
                    : $"SLOT {slot} - SALVAR";

                DrawCenteredText(
                    label,
                    _slotBounds[i],
                    Color.White
                );
            }

            DrawCenteredText(
                "VOLTAR",
                _backBounds,
                Color.White
            );

            if (!string.IsNullOrEmpty(_message))
            {
                Vector2 messageSize = _font.MeasureString(_message);

                _spriteBatch.DrawString(
                    _font,
                    _message,
                    new Vector2(
                        (_graphicsDevice.Viewport.Width - messageSize.X) / 2f,
                        _graphicsDevice.Viewport.Height * 0.92f
                    ),
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
