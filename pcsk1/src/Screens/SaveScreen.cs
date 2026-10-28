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

namespace pcsk1.src.Screens
{
    public class SaveScreen : IScreen
    {
        private readonly ScreenManager _screenManager;
        private readonly Action<int> _saveGame;
        private readonly IScreen _pauseScreen;
        private readonly Rectangle[] _slotBounds = new Rectangle[3];
        private readonly SaveService _saveService;
        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;
        private readonly List<AbstractButton> _buttons = new();

        private readonly Rectangle _backBounds;

        private string _message = "";

        public SaveScreen(
        ScreenManager screenManager,
        Action<int> saveGame,
        IScreen pauseScreen,
        GraphicsDevice graphicsDevice,
        SaveService saveService,
        SpriteFont font,
        Texture2D whiteTexture)
            {
            _screenManager = screenManager;
            _saveService = saveService;
            _saveGame = saveGame;
            _pauseScreen = pauseScreen;
            _font = font;
            _whiteTexture = whiteTexture;

            int centerX = graphicsDevice.Viewport.Width / 2;

            for(int i = 0; i < 3; i++)
            {
                int slot = i + 1;
                Rectangle bounds = new Rectangle(
                    centerX - 160,
                    200 + i * 90,
                    320,
                    70
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
            }

        private void SaveSlot(int slot)
        {
            try
            {
                _saveGame(slot);
                _message = $"Jogo salvo no slot {slot}.";
                BackToPause();
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is ArgumentException ||
                ex is NotSupportedException)
            {
                Console.WriteLine(ex);
            }

        }

        private void BackToPause()
        {
            _screenManager.ChangeState(_pauseScreen);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp
                );
            foreach(AbstractButton button in _buttons)
            {
                button.Draw(spriteBatch);
            }
            for(int i = 0; i < 3; i++)
            {
                int slot = i + 1;
                string label = _saveService.Exists(slot)
                ? $"SLOT {slot} - SOBRESCREVER"
                : $"SLOT {slot} - SALVAR";

                DrawCenteredText(
                spriteBatch,
                label,
                _slotBounds[i],
                Color.White
            );

                DrawCenteredText(
                spriteBatch,
                "VOLTAR",
                _backBounds,
                Color.White
                );

                DrawCenteredText(
                    spriteBatch,
                    "SALVAR JOGO",
                    new Rectangle(0, 90, 1280, 70),
                    Color.White
                );

                if(!string.IsNullOrEmpty(_message))
                {
                    spriteBatch.DrawString(_font, _message, new Vector2(400, 610), Color.White);
                }

            }
                spriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {
            if (Input.IsKeyPressed(Keys.Escape))
            {
                BackToPause();
                return;
            }
            foreach(AbstractButton button in _buttons)
            {
                button.Update();
            }


        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color)
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
