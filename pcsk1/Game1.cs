using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using pcsk1.src.Inputs;
using pcsk1.src.Saves;
using pcsk1.src.Screens;


namespace pcsk1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private ScreenManager _screenManager;
        private SpriteFont _font;
        private SpriteBatch _spriteBatch;
        private Texture2D _whiteTexture;
        private SaveService _saveService;
  

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _screenManager = new ScreenManager();

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
            _font = Content.Load<SpriteFont>("Fonts/quaakykopBold");
            _saveService = new SaveService();
            _screenManager.ChangeState(CreateMenu());

        }

        private IScreen CreateMenu()
        {
            return new MainMenuScreen(
                _screenManager,
                GraphicsDevice,
                _whiteTexture,
                () => CreateGameplay(),
                Exit,
                _font,
                CreateLoadScreen
            );
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

            _screenManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkMagenta);

            _screenManager.Draw(_spriteBatch);

            base.Draw(gameTime);
        }

        private IScreen CreateGameplay(
            SaveData? save = null,
            int slot = 1)
        {
            return new GameplayScreen(
            _screenManager,
            GraphicsDevice,
            Content,
            _whiteTexture,
            CreateMenu,
            _font,
            _saveService,
            save,
            slot
    );
        }

        private IScreen CreateLoadScreen()
        {
            return new LoadGameScreen(
                _screenManager,
                _saveService,
                GraphicsDevice,
                _font,
                _whiteTexture,
                (save, slot) =>
                {
                    _screenManager.ChangeState(CreateGameplay(save, slot));
                },
                CreateMenu
                );
        }

        protected override void UnloadContent()
        {
            _whiteTexture?.Dispose();
            _spriteBatch?.Dispose();

            base.UnloadContent();
        }
    }
}
