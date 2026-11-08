
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Inputs;
using pcsk1.src.Saves;
using pcsk1.src.Screens;
using pcsk1.src.Screens.UI;
using pcsk1.src.Settings;
using pcsk1.src.Shader;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;
using MonoGame.Extended.Screens;
using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;

namespace pcsk1.src.Screens
{

    public sealed class PauseScreen : GameScreen
    {
        private readonly ExtendedScreenManager _screenManager;
        private readonly GameplayScreen _gameplay;

        private readonly SaveService _saveService;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _whiteTexture;
        private readonly SpriteFont _font;
        private readonly Func<GameScreen> _createMenu;
        private readonly Func<GameScreen> _createSettingsScreen;
        private readonly List<AbstractButton> _buttons = new();
        private readonly Effect _frostedGlass;
        private readonly ShaderBinder _shaderBinder;

        private RenderTarget2D _gameplayRenderTarget;

        private Rectangle _continueBounds;
        private Rectangle _saveBounds;
        private Rectangle _menuBounds;
        private Rectangle _settingsBounds;
        private Rectangle _titleBounds;
        private readonly Texture2D _noiseTex;
        private int _layoutWidth;
        private int _layoutHeight;

        public PauseScreen(
            Game game,
            ExtendedScreenManager screenManager,
            GameplayScreen gameplay,
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            Texture2D whiteTexture,
            Func<GameScreen> createMenu,
            ContentManager content,
            SpriteFont font,
            SaveService saveService,
            Func<GameScreen> createSettingsScreen
        ) : base(game)
        {
            _screenManager = screenManager;
            _gameplay = gameplay;

            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _whiteTexture = whiteTexture;
            _font = font;

            _createMenu = createMenu;
            _createSettingsScreen = createSettingsScreen;
            _saveService = saveService;

            UpdateWhenInactive = false;
            DrawWhenInactive = true;

            _frostedGlass = content.Load<Effect>(
                "Shaders/FrostedGlass"
            );

            _shaderBinder = new ShaderBinder(_frostedGlass);

            int width = graphicsDevice.Viewport.Width;
            int height = graphicsDevice.Viewport.Height;

            _gameplayRenderTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height
            );

            SetBlur();
            BuildLayout();
        }

        private void BuildLayout()
        {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;
            int centerX = width / 2;

            _layoutWidth = width;
            _layoutHeight = height;

            int buttonWidth = 260;
            int buttonHeight = 55;
            int spacing = 15;

            int firstButtonY = (int)(height * 0.34f);

            _continueBounds = new Rectangle(
                centerX - buttonWidth / 2,
                firstButtonY,
                buttonWidth,
                buttonHeight
            );

            _saveBounds = new Rectangle(
                centerX - buttonWidth / 2,
                firstButtonY + buttonHeight + spacing,
                buttonWidth,
                buttonHeight
            );

            _settingsBounds = new Rectangle(
                centerX - buttonWidth / 2,
                firstButtonY + (buttonHeight + spacing) * 2,
                buttonWidth,
                buttonHeight
            );

            _menuBounds = new Rectangle(
                centerX - buttonWidth / 2,
                firstButtonY + (buttonHeight + spacing) * 3,
                buttonWidth,
                buttonHeight
            );

            _titleBounds = new Rectangle(
                0,
                (int)(height * 0.18f),
                width,
                65
            );

            _buttons.Clear();

            _buttons.Add(new ActionButton(
                _continueBounds,
                _whiteTexture,
                Resume,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));

            _buttons.Add(new ActionButton(
                _saveBounds,
                _whiteTexture,
                OpenSaveScreen,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));

            _buttons.Add(new ActionButton(
                _settingsBounds,
                _whiteTexture,
                OpenSettings,
                Color.DarkSlateGray,
                Color.SteelBlue
            ));

            _buttons.Add(new ActionButton(
                _menuBounds,
                _whiteTexture,
                OpenConfirmation,
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

        private void EnsureRenderTargetSize()
        {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;

            if (_gameplayRenderTarget.Width == width &&
                _gameplayRenderTarget.Height == height)
            {
                return;
            }

            _gameplayRenderTarget.Dispose();

            _gameplayRenderTarget = new RenderTarget2D(
                _graphicsDevice,
                width,
                height
            );

            SetBlur();
        }

        public void Resume()
        {

            _screenManager.CloseScreen();
        }
        private void OpenConfirmation()
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
           ["Aviso: Ao deixar o jogo todo progresso nao salvo sera perdido", "Prosseguir com a descisao?"],
           onConfirm: BackToMenu,
           onClosed: () => DrawWhenInactive = previousDrawState
       )
   );

        }
        public void BackToMenu()
        {

            _screenManager.ClearScreens();

            _screenManager.ShowScreen(_createMenu());
        }

        private void OpenSaveScreen()
        {
            _screenManager.ShowScreen(
                new SaveScreen(
                    Game,
                    _screenManager,
                    _spriteBatch,
                    _gameplay.SaveGame,
                    _graphicsDevice,
                    _saveService,
                    _font,
                    _whiteTexture
                )
            );
        }

        private void OpenSettings()
        {
            _screenManager.ShowScreen(
                _createSettingsScreen()
            );
        }

        public override void Update(GameTime gameTime)
        {
            EnsureLayout();

            if (Input.IsKeyPressed(Keys.Escape) ||
                Input.IsKeyPressed(Keys.R))
            {
                Resume();
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
            EnsureRenderTargetSize();


            _graphicsDevice.SetRenderTarget(
                _gameplayRenderTarget
            );

            _graphicsDevice.Clear(Color.DarkMagenta);

            _gameplay.DrawScene(_spriteBatch);


            _graphicsDevice.SetRenderTarget(null);

            _graphicsDevice.Clear(Color.DarkMagenta);

 
            _spriteBatch.Begin(
                blendState: BlendState.Opaque,
                samplerState: SamplerState.LinearClamp,
                effect: _frostedGlass
            );

            _spriteBatch.Draw(
                _gameplayRenderTarget,
                _graphicsDevice.Viewport.Bounds,
                Color.White
            );

           

            _spriteBatch.End();

            // Overlay e interface.
            _spriteBatch.Begin(
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );

            _spriteBatch.Draw(
                _whiteTexture,
                _graphicsDevice.Viewport.Bounds,
                Color.Black * 0.40f
            );

            foreach (AbstractButton button in _buttons)
            {
                button.Draw(_spriteBatch);
            }

            DrawCenteredText(
                "PAUSADO",
                _titleBounds,
                Color.White
            );

            DrawCenteredText(
                "CONTINUAR",
                _continueBounds,
                Color.White
            );

            DrawCenteredText(
                "SALVAR",
                _saveBounds,
                Color.White
            );

            DrawCenteredText(
                "CONFIGURACOES",
                _settingsBounds,
                Color.White
            );

            DrawCenteredText(
                "MENU",
                _menuBounds,
                Color.White
            );

            _spriteBatch.End();
        }



        private void DrawCenteredText(
            string text,
            Rectangle bounds,
            Color color)
        {
            Vector2 textSize = _font.MeasureString(text);

            Vector2 position = new(
                bounds.Center.X - textSize.X / 2f,
                bounds.Center.Y - textSize.Y / 2f
            );

            _spriteBatch.DrawString(
                _font,
                text,
                position,
                color
            );
        }


        private void SetBlur()
        {
            _shaderBinder
                .SetUniform(
                    "Resolution",
                    new Vector2(
                        _gameplayRenderTarget.Width,
                        _gameplayRenderTarget.Height
                    )
                )
                .And("BlurRadius", 3.5f)
                .And("NoiseIntensity", 0.0015f)
                .And("TintOpacity", 0.18f)
                .And(
                    "TintColor",
                    new Vector3(0.15f, 0.17f, 0.2f)
                );

        }

        public override void UnloadContent()
        {
            _gameplayRenderTarget?.Dispose();

            base.UnloadContent();
        }

        

    }
}
