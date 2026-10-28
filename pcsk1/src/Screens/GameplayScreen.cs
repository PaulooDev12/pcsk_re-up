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
using System.Threading;

namespace pcsk1.src.Screens
    {
        public sealed class GameplayScreen : IScreen
        {
            private readonly ScreenManager _screenManager;
            private readonly GraphicsDevice _graphicsDevice;
            private readonly Func<IScreen> _createMenu;
            private List<IEntity> _entities = new();
            private readonly Player _player;
            private readonly Enemy _enemy;
            private SpriteFont _font;
            private readonly Camera2D _camera;
            private readonly TiledMap _map;
            private readonly Effect _glowShader;
            private readonly Texture2D _whiteTexture;
            private float _elapsedTime;
            private const int GlowSize = 260;
            private const float GlowIntensity = 11f;
            private const float InitialRadius = 0.12f;
            private const float FinalRadius = 0.42f;
            private ShaderBinder _shaderBinder;
            private readonly ContentManager _content;
            private float _timer = 0.0f;
            private readonly SaveService _saveService;
            private readonly SaveData _saveData;
            private int _saveSlot;


            private static readonly Vector3 GlowColor = new Vector3(0.2f, 0.2f, 1.0f);

            public GameplayScreen(
                ScreenManager screenManager,
                GraphicsDevice graphicsDevice,
                ContentManager content,
                Texture2D whiteTexture,
                Func<IScreen> createMenu,
                SpriteFont font,
                SaveService saveService,
                SaveData? loadedSave = null,
                int saveSlot = 1
                )
            {
                _screenManager = screenManager;
                _graphicsDevice = graphicsDevice;
                _whiteTexture = whiteTexture;
                _createMenu = createMenu;
                _content = content;
                _font = font;
                Texture2D playerTexture =
                    content.Load<Texture2D>("d");

                _saveService = saveService;
                _saveData = loadedSave ?? new SaveData();
                _saveSlot = saveSlot;
                
                _player = new Player(
                    playerTexture,
                    _saveData != null ? _saveData.PlayerPosition : new Vector2(200, 200)
                );
                
                
                _enemy = new Enemy(
                    new Vector2(220, 220),
                    _player,
                    playerTexture
                );

                _map = ResolveMap(_saveData?.CurrentMap ?? "mapaa", graphicsDevice, content);
                
                _entities.Add(_player);
                _entities.Add(_enemy);

                _camera = new Camera2D();

                _glowShader = content.Load<Effect>("Glow");
                _shaderBinder = new ShaderBinder(_glowShader);
                
        }

        private TiledMap ResolveMap(string mapPath, GraphicsDevice graphicsDevice, ContentManager content)
        {
            string path = mapPath switch
            {
                "mapaa" => "Content/maps/mapaa.tmx",
                _ => throw new ArgumentException("mapa não disponivel")
            };
            return new TiledMap(graphicsDevice, path, content);
        }
        public void Update(GameTime gameTime)
        {
            if (Input.IsKeyPressed(Keys.Escape))
            {
                _screenManager.ChangeState(
                    new PauseScreen(
                        _screenManager,
                        this,
                        _graphicsDevice,
                        _whiteTexture,
                        _createMenu,
                        _content,
                        _font,
                        _saveService
                        )
                    );
                return;
            }
            if(Input.IsKeyPressed(Keys.F5))
            {
                SaveGame(_saveSlot);
            }
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += deltaTime;
            if(_timer >= 1200.0f)
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

        }
            public void Draw(SpriteBatch spriteBatch)
            {
            DrawWorld(spriteBatch);
            DrawGlow(spriteBatch, _enemy);
        }

            private void UpdateGlow(GameTime gameTime)
            {
                _elapsedTime +=
                    (float)gameTime.ElapsedGameTime.TotalSeconds;

            _shaderBinder.SetUniform("u_time", _elapsedTime)
            .And("u_intensidade", GlowIntensity)
            .And("u_glowColor", new Vector3(0.2f, 0.7f, 1.0f))
            .And("u_initialRay", InitialRadius)
            .And("u_finalRay", FinalRadius);
                
            }

            private void DrawWorld(SpriteBatch spriteBatch)
            {
                spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.Transform
                );
                    _map.Draw(spriteBatch);
                    _player.Draw(spriteBatch);
                    _enemy.Draw(spriteBatch);


            spriteBatch.End();

            }
            private void DrawGlow(SpriteBatch spriteBatch, IEntity target)
            {
            Vector2 targetCenter = target.Position + new Vector2(target.Bounds.Width / 2, target.Bounds.Width / 2);
            Vector2 glowPos = targetCenter - new Vector2(GlowSize / 2f);

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
            }
            public void SaveGame(int slot)
            {
            _saveData.PlayerPosition = _player.Position;
    
            _saveService.Save(_saveData, slot);
            _saveSlot = slot;
            }
         }

            
    }