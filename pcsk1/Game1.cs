using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using pcsk1.src.Inputs;
using pcsk1.src.Saves;
using pcsk1.src.Screens;
using pcsk1.src.Settings;
using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;


namespace pcsk1
{


    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly ExtendedScreenManager _screenManager;
        private SpriteFont _font = null!;
        private SpriteBatch _spriteBatch = null!;
        private Texture2D _whiteTexture = null!;
        private SaveService _saveService = null!;
        private GameSettings _settings = null!;
        private SettingsService _settingsService = null!;
        private Texture2D _menuBg;

        private bool _settingsDirty;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _screenManager = new ExtendedScreenManager();

            Components.Add(_screenManager);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });

            _font = Content.Load<SpriteFont>("Fonts/mserrat");
            _menuBg = Content.Load<Texture2D>("Backgrounds/bg1");

            _saveService = new SaveService();

            _settingsService = new SettingsService();
            _settings = _settingsService.Load();

            RequestApplySettings(_settings);

            _screenManager.ShowScreen(CreateMenu());
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if (GamePad.GetState(PlayerIndex.One)
                .Buttons.Back == ButtonState.Pressed)
            {
                Exit();
                return;
            }

            // O ScreenManager é atualizado por Game.Components.
            base.Update(gameTime);

            if (_settingsDirty)
            {
                _settingsDirty = false;
                ApplySettings();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);

            base.Draw(gameTime);
        }

        private MainMenuScreen CreateMenu()
        {
            return new MainMenuScreen(
                this,
                _screenManager,
                _spriteBatch,
                _whiteTexture,
                () => CreateGameplay(),
                Exit,
                _font,
                CreateLoadScreen,
                CreateSettingsScreen,
                _menuBg
            );
        }

        private GameplayScreen CreateGameplay(
            SaveData? save = null,
            int slot = 1)
        {
            return new GameplayScreen(
                this,
                _screenManager,
                GraphicsDevice,
                Content,
                _spriteBatch,
                _whiteTexture,
                CreateMenu,
                _font,
                _saveService,
                CreateSettingsScreen,
                save,
                slot
            );
        }

        private SettingsScreen CreateSettingsScreen()
        {
            return new SettingsScreen(
                this,
                _screenManager,
                _spriteBatch,
                _settings,
                _settingsService,
                RequestApplySettings,
                GraphicsDevice,
                _whiteTexture,
                _font
            );
        }

        private void RequestApplySettings(GameSettings settings)
        {
            _settingsDirty = true;
        }

        private void ApplySettings()
        {
            _graphics.PreferredBackBufferWidth = _settings.Width;
            _graphics.PreferredBackBufferHeight = _settings.Height;
            _graphics.IsFullScreen = _settings.Fullscreen;
            _graphics.ApplyChanges();

            SoundEffect.MasterVolume = _settings.MasterVolume;
            MediaPlayer.Volume = _settings.MasterVolume;
        }

        protected override void UnloadContent()
        {
            _whiteTexture?.Dispose();
            _spriteBatch?.Dispose();

            base.UnloadContent();
        }


        private LoadGameScreen CreateLoadScreen()
        {
            return new LoadGameScreen(
                this,
                _screenManager,
                _spriteBatch,
                _saveService,
                GraphicsDevice,
                _font,
                _whiteTexture,
                StartLoadedGame
            );
        }

        private void StartLoadedGame(SaveData save, int slot)
        {
            _screenManager.ClearScreens();

            _screenManager.ShowScreen(
                CreateGameplay(save, slot), new FadeTransition(GraphicsDevice, Color.Black, 1.5f)
            );
        }
    }

}
