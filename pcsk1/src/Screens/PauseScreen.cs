// Apaguei a porra da classe sem querer vlw gpt por reescrever
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pcsk1.src.Inputs;
using pcsk1.src.Saves;
using pcsk1.src.Screens;
using pcsk1.src.Screens.UI;
using pcsk1.src.Shader;
using pcsk1.src.UI;
using System;
using System.Collections.Generic;

public sealed class PauseScreen : IScreen
{
    private readonly ScreenManager _screenManager;
    private readonly GameplayScreen _gameplay;

    private readonly SaveService _saveService;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Texture2D _whiteTexture;
    private readonly SpriteFont _font;
    private readonly Func<IScreen> _createMenu;
    private readonly List<AbstractButton> _buttons;
    private readonly Effect _frostedGlass;
    private readonly ShaderBinder _shaderBinder;
    private readonly RenderTarget2D _gameplayRenderTarget;

    private readonly Rectangle _continueBounds;
    private readonly Rectangle _saveBounds;
    private readonly Rectangle _menuBounds;
    private readonly Rectangle _titleBounds;

    public PauseScreen(
        ScreenManager screenManager,
        GameplayScreen gameplay,
        GraphicsDevice graphicsDevice,
        Texture2D whiteTexture,
        Func<IScreen> createMenu,
        ContentManager content,
        SpriteFont font,
        SaveService saveService)
    {
        _screenManager = screenManager;
        _gameplay = gameplay;
        _graphicsDevice = graphicsDevice;
        _whiteTexture = whiteTexture;
        _createMenu = createMenu;
        _font = font;
        _saveService = saveService;

        int width = graphicsDevice.Viewport.Width;
        int height = graphicsDevice.Viewport.Height;
        int centerX = width / 2;

        // Captura do gameplay para aplicar o Frosted Glass.
        _gameplayRenderTarget = new RenderTarget2D(
            graphicsDevice,
            width,
            height
        );

        // Shader utilizado no fundo pausado.
        _frostedGlass = content.Load<Effect>(
            "Shaders/FrostedGlass"
        );

        _shaderBinder = new ShaderBinder(_frostedGlass);

        SetBlur();

        // Layout: três botões com 30px de espaçamento.
        _continueBounds = new Rectangle(
            centerX - 130,
            265,
            260,
            60
        );

        _saveBounds = new Rectangle(
            centerX - 130,
            355,
            260,
            60
        );

        _menuBounds = new Rectangle(
            centerX - 130,
            445,
            260,
            60
        );

        _titleBounds = new Rectangle(
            0,
            145,
            width,
            70
        );

        _buttons = new List<AbstractButton>
        {
            new ActionButton(
                _continueBounds,
                _whiteTexture,
                Resume,
                Color.DarkSlateGray,
                Color.SteelBlue
            ),

            new ActionButton(
                _saveBounds,
                _whiteTexture,
                OpenSaveScreen,
                Color.DarkSlateGray,
                Color.SteelBlue
            ),

            new ActionButton(
                _menuBounds,
                _whiteTexture,
                BackToMenu,
                Color.DarkSlateGray,
                Color.IndianRed
            )
        };
    }

    public void Resume()
    {
        // Retorna à mesma instância da gameplay.
        _screenManager.ChangeState(_gameplay);
    }

    public void BackToMenu()
    {
        _screenManager.ChangeState(_createMenu());
    }

    private void OpenSaveScreen()
    {
        SaveScreen saveScreen = new SaveScreen(
            _screenManager,
            _gameplay.SaveGame,
            this,
            _graphicsDevice,
            _saveService,
            _font,
            _whiteTexture
        );

        _screenManager.ChangeState(saveScreen);
    }

    public void Update(GameTime gameTime)
    {
        if (Input.IsKeyPressed(Keys.Escape) ||
            Input.IsKeyPressed(Keys.R))
        {
            Resume();
            return;
        }

        foreach (AbstractButton button in _buttons)
        {
            button.Update();

            // Evita atualizar outros botões após navegar.
            if (_screenManager.CurrentScreen != this)
                return;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.SetRenderTarget(
            _gameplayRenderTarget
        );

        _graphicsDevice.Clear(Color.DarkMagenta);

        _gameplay.Draw(spriteBatch);

        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.DarkMagenta);

       
        spriteBatch.Begin(
            blendState: BlendState.Opaque,
            samplerState: SamplerState.LinearClamp,
            effect: _frostedGlass
        );

        spriteBatch.Draw(
            _gameplayRenderTarget,
            _graphicsDevice.Viewport.Bounds,
            Color.White
        );

        spriteBatch.End();

        // 4. Desenha a camada escura e a interface.
        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp
        );

        spriteBatch.Draw(
            _whiteTexture,
            _graphicsDevice.Viewport.Bounds,
            Color.Black * 0.40f
        );

        foreach (AbstractButton button in _buttons)
        {
            button.Draw(spriteBatch);
        }

        DrawCenteredText(
            spriteBatch,
            "PAUSADO",
            _titleBounds,
            Color.White
        );

        DrawCenteredText(
            spriteBatch,
            "CONTINUAR",
            _continueBounds,
            Color.White
        );

        DrawCenteredText(
            spriteBatch,
            "SALVAR",
            _saveBounds,
            Color.White
        );

        DrawCenteredText(
            spriteBatch,
            "MENU",
            _menuBounds,
            Color.White
        );

        spriteBatch.End();
    }

    private void DrawCenteredText(
        SpriteBatch spriteBatch,
        string text,
        Rectangle bounds,
        Color color)
    {
        Vector2 textSize = _font.MeasureString(text);

        Vector2 position = new Vector2(
            bounds.Center.X - textSize.X / 2f,
            bounds.Center.Y - textSize.Y / 2f
        );

        spriteBatch.DrawString(
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
                "u_resolution",
                new Vector2(
                    _gameplayRenderTarget.Width,
                    _gameplayRenderTarget.Height
                )
            )
            .And("u_blurRadius", 3.5f)
            .And("u_noiseIntensity", 0.0015f)
            .And("u_tintOpacity", 0.18f)
            .And(
                "u_tintColor",
                new Vector3(0.15f, 0.17f, 0.2f)
            );
    }
}
