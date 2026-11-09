#nullable enable

using pcsk1.src.Camera;
using pcsk1.src.GameObjects;
using pcsk1.src.GameObjects.Playeer;
using pcsk1.src.World;
using pcsk1.src.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using pcsk1.src.Entities;
using pcsk1.src.Shader;
using pcsk1.src.Saves;
using pcsk1.src.Screens.UI;

namespace pcsk1.src.Screens
{

    using MonoGame.Extended.Screens;
    using System.Diagnostics;
    using ExtendedScreenManager = MonoGame.Extended.Screens.ScreenManager;

    public sealed class GameplayScreen : GameScreen
    {
        // Dependências e estado da tela.
        private readonly ExtendedScreenManager _screenManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ContentManager _content;
        private readonly SpriteBatch _spriteBatch;

        private readonly Func<GameScreen> _createMenu;
        private readonly Func<GameScreen> _createSettingsScreen;

        private readonly List<IEntity> _entities = new();

        private readonly Player _player;
        private readonly Enemy _enemy;
        private readonly Camera2D _camera;
        private readonly TiledMap _map;
        private CustomTileMap _tileMap;
        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;
        private readonly Texture2D _shaderMask;

        private readonly Effect _glowShader;
        private readonly Effect _dropShadowShader;
        private readonly Effect _enlightenmentShader;

        private readonly ShaderBinder _shaderBinder;
        private readonly ShaderBinder _dropShaderBinder;
        private readonly ShaderBinder _enlightenmentBinder;

        private readonly SaveService _saveService;
        private readonly SaveData _saveData;
        private readonly MentalSanityBar _sanityBar;

        private float _elapsedTime;
        private float _timer = 0.0f;

        private const int GlowSize = 260;
        private const float GlowIntensity = 11f;
        private const float InitialRadius = 0.12f;
        private const float FinalRadius = 0.42f;

        private int _vLightDirection = 1;
        private int _hLightDirection = 1;
        private int _saveSlot;

        public GameplayScreen(
            Game game,
            ExtendedScreenManager screenManager,
            GraphicsDevice graphicsDevice,
            ContentManager content,
            SpriteBatch spriteBatch,
            Texture2D whiteTexture,
            Func<GameScreen> createMenu,
            SpriteFont font,
            SaveService saveService,
            Func<GameScreen> createSettingsScreen,
            SaveData? loadedSave = null,
            int saveSlot = 1
        ) : base(game)
        {
            _screenManager = screenManager;
            _graphicsDevice = graphicsDevice;
            _content = content;
            _spriteBatch = spriteBatch;

            _whiteTexture = whiteTexture;
            _createMenu = createMenu;
            _font = font;
            _createSettingsScreen = createSettingsScreen;

            _saveService = saveService;
            _saveData = loadedSave ?? new SaveData();
            _saveSlot = saveSlot;

            // A gameplay não deve continuar atualizando ou desenhando
            // automaticamente quando uma tela de pausa estiver acima dela.
            UpdateWhenInactive = false;
            DrawWhenInactive = false;

            Texture2D playerTexture = content.Load<Texture2D>("sprites/idle_front");

            _shaderMask = content.Load<Texture2D>(
                "Shaders/Masks/light_sampler"
            );

            _player = new Player(
                playerTexture,
                _saveData.PlayerPosition
            );

            _player.LoadContent(
                content,
                "sprites/farmer_spritesheet"
            );

            _sanityBar = new MentalSanityBar(
                _player.MentalSanity,
                _player,
                _whiteTexture
            );

            _enemy = new Enemy(
                new Vector2(220, 220),
                _player,
                playerTexture
            );
            
            _map = ResolveMap(
                _saveData.CurrentMap,   
                graphicsDevice,
                content
            );

            _entities.Add(_player);
            _entities.Add(_enemy);

            _camera = new Camera2D();

            _glowShader = content.Load<Effect>("Glow");
            _shaderBinder = new ShaderBinder(_glowShader);

            _dropShadowShader = content.Load<Effect>("Shaders/Shadow");
            _dropShaderBinder = new ShaderBinder(_dropShadowShader);

            _enlightenmentShader = content.Load<Effect>(
                "Shaders/BlurredEnlightenment"
            );

            _enlightenmentBinder = new ShaderBinder(
                _enlightenmentShader
            );
        }

        // Resolve o caminho do mapa utilizado pela fase.
        private TiledMap ResolveMap(
            string mapPath,
            GraphicsDevice graphicsDevice,
            ContentManager content)
        {
            string path = mapPath switch
            {
                "street" => "Content/maps/street.tmx",
                _ => throw new ArgumentException(
                    "Mapa não disponível"
                )
            };

            return new TiledMap(graphicsDevice, path, content);
        }

        

        // Atualiza entradas, entidades, câmera e efeitos.
        public override void Update(GameTime gameTime)
        {
            if (Input.IsKeyPressed(Keys.Escape))
            {
                _screenManager.ShowScreen(
                    new PauseScreen(
                         Game,
                         _screenManager,
                         this,
                         _graphicsDevice,
                         _spriteBatch,
                         _whiteTexture,
                         _createMenu,
                         _content,
                         _font,
                         _saveService,
                         _createSettingsScreen
                        )
                    );

                return;
            }

            if (Input.IsKeyPressed(Keys.F5))
            {
                SaveGame(_saveSlot);
            }

            if (Input.IsKeyPressed(Keys.D1))
            {
                _hLightDirection = (int)HLightDirection.Left;
                Debug.WriteLine("Left");
            }
                

            if (Input.IsKeyPressed(Keys.D2))
            {

                _hLightDirection = (int)HLightDirection.Right;
            }

            if (Input.IsKeyPressed(Keys.D3))
            {

                _vLightDirection = (int)VLightDirection.Top;
            }
                

            if (Input.IsKeyPressed(Keys.D4))
            {
                _vLightDirection = (int)VLightDirection.Down;
                Debug.WriteLine("Down");
            }
                

            float deltaTime =
                (float)gameTime.ElapsedGameTime.TotalSeconds;

            _timer += deltaTime;

            if (_timer >= 1200.0f)
            {
                SaveGame(_saveSlot);
                _timer = 0.0f;
            }

            _player.Update(gameTime, _map);
            _enemy.Update(gameTime, null);

            _camera.Follow(
                _player.Position,
                _graphicsDevice.Viewport.Width,
                _graphicsDevice.Viewport.Height
            );

            UpdateGlow(gameTime);
            UpdateEnlightenment();
        }

        // Ciclo de desenho normal da tela.
        public override void Draw(GameTime gameTime)
        {
            DrawScene(_spriteBatch);
        }

        // Permite que PauseScreen renderize a gameplay em seu RenderTarget.
        public void DrawScene(SpriteBatch spriteBatch)
        {
            DrawWorld(spriteBatch);
            DrawGlow(spriteBatch, _enemy);
        }

        private void UpdateEnlightenment()
        {
            _enlightenmentBinder
                .SetUniform("Time", _elapsedTime)
                .And("Opacity", 0.43f)
                .And("BlurRadius", 2f)
                .And("RandomBlurBounds", 1800f)
                .And("NoiseIntensity", 0.0015f)
                .And(
                    "Resolution",
                    new Vector2(
                        _graphicsDevice.Viewport.Width / 2,
                        _graphicsDevice.Viewport.Height / 2
                    )
                )
                .And("WaveSpeed", 1.35f)
                .And("WaveFrequency", 15.0f)
                .And("WaveAmplitude", 0.0095f)
                .And(
                    "TintColor",
                    new Vector3(1.0f, 0.97f, 0.75f)
                );
        }

        private void UpdateGlow(GameTime gameTime)
        {
            _elapsedTime +=
                (float)gameTime.ElapsedGameTime.TotalSeconds;

            _shaderBinder
                .SetUniform("u_time", _elapsedTime)
                .And("u_intensidade", GlowIntensity)
                .And(
                    "u_glowColor",
                    new Vector3(0.2f, 0.7f, 1.0f)
                )
                .And("u_initialRay", InitialRadius)
                .And("u_finalRay", FinalRadius);

            _dropShaderBinder
                .SetUniform(
                    "ShadowOffset",
                    new Vector2(
                        0.20f * _hLightDirection,
                        0.30f * _vLightDirection
                    )
                )
                .And(
                    "DropShadowColor",
                    new Vector4(0.0f, 0.0f, 0.0f, 0.5f)
                )
                .And("Time", _elapsedTime)
                .And("WindSpeed", 0.025f);
        }

        private void DrawWorld(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.Transform
            );

            _map.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                effect: _dropShadowShader,
                transformMatrix: _camera.Transform
            );

            _player.Draw(spriteBatch);
            _enemy.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.Transform
            );

            spriteBatch.End();
        }

        private void DrawGlow(SpriteBatch spriteBatch, IEntity target)
        {
            Vector2 targetCenter = target.Position +
                new Vector2(
                    target.Bounds.Width / 2,
                    target.Bounds.Width / 2
                );

            Vector2 glowPos = targetCenter -
                new Vector2(GlowSize / 2f);

            spriteBatch.Begin(
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                effect: _glowShader,
                transformMatrix: _camera.Transform
            );

            spriteBatch.Draw(
                _whiteTexture,
                new Rectangle(
                    (int)glowPos.X,
                    (int)glowPos.Y,
                    GlowSize,
                    GlowSize
                ),
                Color.White
            );

            spriteBatch.End();

            spriteBatch.Begin(
                blendState: BlendState.Additive,
                samplerState: SamplerState.LinearWrap,
                effect: _enlightenmentShader
            );

            Rectangle destRect = new(
                0,
                0,
                _graphicsDevice.Viewport.Width,
                _graphicsDevice.Viewport.Width - 200
            );

            spriteBatch.Draw(
                _shaderMask,
                destRect,
                Color.MonoGameOrange
            );

            spriteBatch.End();

            spriteBatch.Begin();

            _sanityBar.Draw(spriteBatch);

            spriteBatch.DrawString(
                _font,
                $"Mental Sanity {_player.MentalSanity}",
                new Vector2(10, 10),
                Color.White
            );

            spriteBatch.End();
        }

        // Persiste o estado atual do jogo no slot informado.
        public void SaveGame(int slot)
        {
            _saveData.PlayerPosition = _player.Position;

            _saveService.Save(_saveData, slot);

            _saveSlot = slot;
        }
    }

}
