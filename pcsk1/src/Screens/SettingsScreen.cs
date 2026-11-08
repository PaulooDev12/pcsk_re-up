
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Inputs;
using pcsk1.src.Screens;
using pcsk1.src.Screens.UI;
using pcsk1.src.Settings;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using MonoGame.Extended.Screens;
using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;
using System.Threading.Tasks;
namespace pcsk1.src.Screens{

    public sealed class SettingsScreen : GameScreen
    {
        private readonly ExtendedScreenManager _screenManager;
        private readonly SpriteBatch _spriteBatch;

        private readonly GameSettings _settings;
        private readonly SettingsService _service;
        private readonly Action<GameSettings> _applySettings;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _whiteTexture;
        private readonly SpriteFont _font;
        private int _limit = 0;

        private readonly List<AbstractButton> _buttons = new();

        private Rectangle _titleBounds;
        private Rectangle _resolutionBounds;
        private Rectangle _fullscreenBounds;
        private Rectangle _volumeMinusBounds;
        private Rectangle _volumePlusBounds;
        private Rectangle _volumeTextBounds;
        private Rectangle _saveBounds;
        private Rectangle _backBounds;
        private Rectangle _messageBounds;
        private float _cooldown = 0.5f;
        private float _timer = 0.0f;
        private bool _isLocked => _timer >= 0.5f;
        private int _viewportWidth;
        private int _viewportHeight;

        private string _message = "";

        private static readonly (int Width, int Height)[] Resolutions =
        {
        (1280, 720),
        (1600, 900),
        (1920, 1080)
    };

        public SettingsScreen(
            Game game,
            ExtendedScreenManager screenManager,
            SpriteBatch spriteBatch,
            GameSettings settings,
            SettingsService service,
            Action<GameSettings> applySettings,
            GraphicsDevice graphicsDevice,
            Texture2D whiteTexture,
            SpriteFont font
        ) : base(game)
        {
            _screenManager = screenManager;
            _spriteBatch = spriteBatch;

            _settings = settings;
            _service = service;
            _applySettings = applySettings;

            _graphicsDevice = graphicsDevice;
            _whiteTexture = whiteTexture;
            _font = font;

            UpdateWhenInactive = false;
            DrawWhenInactive = false;

            BuildButtons();
        }

        private void BuildButtons()
        {
            _buttons.Clear();

            _viewportWidth = _graphicsDevice.Viewport.Width;
            _viewportHeight = _graphicsDevice.Viewport.Height;

            int centerX = _viewportWidth / 2;

            int top = Math.Max(
                20,
                (_viewportHeight - 550) / 2
            );

            _titleBounds = new Rectangle(
                0, top + 5, _viewportWidth, 65
            );

            _resolutionBounds = new Rectangle(
                centerX - 210, top + 100, 420, 55
            );

            _fullscreenBounds = new Rectangle(
                centerX - 210, top + 175, 420, 55
            );

            _volumeMinusBounds = new Rectangle(
                centerX - 210, top + 250, 80, 55
            );

            _volumeTextBounds = new Rectangle(
                centerX - 120, top + 250, 240, 55
            );

            _volumePlusBounds = new Rectangle(
                centerX + 130, top + 250, 80, 55
            );

            _backBounds = new Rectangle(
                centerX - 210, top + 370, 195, 55
            );

            _saveBounds = new Rectangle(
                centerX + 15, top + 370, 195, 55
            );

            _messageBounds = new Rectangle(
                0, top + 455, _viewportWidth, 50
            );

            AddButton(_resolutionBounds, NextResolution);
            AddButton(_fullscreenBounds, ToggleFullscreen);
            AddButton(_volumeMinusBounds, DecreaseVolume);
            AddButton(_volumePlusBounds, IncreaseVolume);
            AddButton(_backBounds, Back);
            AddButton(_saveBounds, SaveSettings);
        }

        private void AddButton(Rectangle bounds, Action action)
        {
            _buttons.Add(new ActionButton(
                bounds,
                _whiteTexture,
                action,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));
        }

        private void EnsureLayout()
        {
            if (_viewportWidth != _graphicsDevice.Viewport.Width ||
                _viewportHeight != _graphicsDevice.Viewport.Height)
            {
                BuildButtons();
            }
        }

        private void NextResolution()
        {
            int index = Array.FindIndex(
                Resolutions,
                resolution =>
                    resolution.Width == _settings.Width &&
                    resolution.Height == _settings.Height
            );

            int nextIndex = (index + 1) % Resolutions.Length;

            var next = Resolutions[nextIndex];

            
            if (_settings.Fullscreen && next != (1920, 1080))
            {

                ToggleFullscreen();
                _settings.SetResolution(next.Width, next.Height);
                RequestApply();
                return;

            }
            

            _settings.SetResolution(next.Width, next.Height);
            RequestApply();
        }



        private void ToggleFullscreen()
        {
            _settings.SetFullscreen(!_settings.Fullscreen);
            RequestApply();
        }

        private void DecreaseVolume()
        {
            _settings.SetVolume(
                _settings.MasterVolume - 0.1f
            );

            RequestApply();
        }

        private void IncreaseVolume()
        {
            _settings.SetVolume(
                _settings.MasterVolume + 0.1f
            );

            RequestApply();
        }

        private void RequestApply()
        {
            _applySettings(_settings);

            _message = "Alteracoes nao salvas.";
        }

        private void SaveSettings()
        {
            try
            {
                _service.Save(_settings);

                _message = "Configuracoes salvas!";
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is ArgumentException ||
                ex is NotSupportedException)
            {
                _message = $"Erro ao salvar: {ex.Message}";

                Debug.WriteLine(ex);
            }
        }

        private void Back()
        {
            _screenManager.CloseScreen();
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += deltaTime;
            if(_timer >= _cooldown)
            {
                _timer -= _cooldown;
            }
            if (!IsActive)
                return;

            if (Input.IsKeyPressed(Keys.Escape))
            {
                Back();
                return;
            }

            EnsureLayout();

            foreach (AbstractButton button in _buttons)
            {
                button.Update();

                // A tela pode ter sido fechada pelo botão.
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
                "CONFIGURACOES",
                _titleBounds
            );

            DrawCenteredText(
                $"RESOLUCAO: {_settings.Width}x{_settings.Height}",
                _resolutionBounds
            );

            DrawCenteredText(
                $"TELA CHEIA: {(_settings.Fullscreen ? "SIM" : "NAO")}",
                _fullscreenBounds
            );

            DrawCenteredText("-", _volumeMinusBounds);
            DrawCenteredText("+", _volumePlusBounds);

            DrawCenteredText(
                $"VOLUME: {_settings.MasterVolume:P0}",
                _volumeTextBounds
            );

            DrawCenteredText("VOLTAR", _backBounds);
            DrawCenteredText("SALVAR", _saveBounds);

            if (!string.IsNullOrEmpty(_message))
            {
                DrawCenteredText(_message, _messageBounds);
            }

            _spriteBatch.End();
        }

        private void DrawCenteredText(
            string text,
            Rectangle bounds)
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
                Color.White
            );
        }
    }



}

