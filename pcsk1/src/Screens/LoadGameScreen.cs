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
    public sealed class LoadGameScreen : IScreen
    {
        private readonly ScreenManager _screenManager;
        private readonly SaveService _saveService;
        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;
        private readonly Action<SaveData, int> _loadGameplay;
        private readonly Func<IScreen> _createMenu;
        private readonly List<AbstractButton> _buttons = new();

        private readonly Rectangle[] _slotBounds = new Rectangle[3];

        private string _message = "";


        public LoadGameScreen(
            ScreenManager screenManager,
            SaveService saveService,
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            Texture2D whiteTexture,
            Action<SaveData, int> loadGameplay,
            Func<IScreen> createMenu)
        {
            _screenManager = screenManager;
            _saveService = saveService;
            _font = font;
            _whiteTexture = whiteTexture;
            _loadGameplay = loadGameplay;
            _createMenu = createMenu;

            int centerX = graphicsDevice.Viewport.Width / 2;

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;

                Rectangle bounds = new Rectangle(
                    centerX - 160,
                    200 + i * 90,
                    320,
                    70
                    );

                _slotBounds[i] = bounds;
                _buttons.Add(
                    new ActionButton(
                        bounds,
                        whiteTexture,
                        () => LoadSlot(slot),
                        Color.DarkSlateGray,
                        Color.IndianRed
                    ));
            }
            _buttons.Add(new ActionButton(
            new Rectangle(centerX - 110, 510, 220, 50),
            whiteTexture,
            BackToMenu,
            Color.DarkSlateGray,
            Color.IndianRed
            ));
        }

        private void LoadSlot(int slot)
        {
            try
            {
                SaveData? save = _saveService.Load(slot);
                if (save == null)
                {
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
                Console.WriteLine(ex);
            }
        }

        private void BackToMenu()
        {
            _screenManager.ChangeState(_createMenu());
        }

        public void Update(GameTime gameTime)
        {
            if (Input.IsKeyPressed(Keys.Escape))
            {
                BackToMenu();
                return;
            }
            foreach (AbstractButton button in _buttons)
            {
                button.Update();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
            samplerState: SamplerState.PointClamp
            );

            foreach (AbstractButton button in _buttons)
            {
                button.Draw(spriteBatch);
            }

            for (int i = 0; i < 3; i++)
            {
                int slot = i + 1;

                string label = _saveService.Exists(slot)
                    ? $"SLOT {slot} - Carregar"
                    : $"SLOT {slot} - Vazio";

                DrawCenteredText(
                    spriteBatch,
                    label,
                    _slotBounds[i],
                    Color.White
                );
            }

            DrawCenteredText(
                spriteBatch,
                "VOLTAR",
                new Rectangle(530, 510, 220, 50),
                Color.White
            );

            spriteBatch.DrawString(
                _font,
                "CARREGAR JOGO",
                new Vector2(430, 100),
                Color.White
            );

            if (!string.IsNullOrEmpty(_message))
            {
                spriteBatch.DrawString(
                    _font,
                    _message,
                    new Vector2(400, 600),
                    Color.IndianRed
                );
            }

            spriteBatch.End();
        }

        private void DrawCenteredText(
            SpriteBatch spriteBatch,
            string text,
            Rectangle bounds,
            Color color)
        {
            Vector2 size = _font.MeasureString(text);

            Vector2 position = new Vector2(
                bounds.Center.X - size.X / 2f,
                bounds.Center.Y - size.Y / 2f
            );

            spriteBatch.DrawString(
                _font,
                text,
                position,
                color
            );
        }
    }
}
