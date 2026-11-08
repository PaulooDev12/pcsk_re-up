using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Screens.UI;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace pcsk1.src.UI
{
    public sealed class ActionButton : AbstractButton
    {
        public string Text { get; init; }
        private readonly Action _onClick;
        private readonly Color _normalColor;
        private readonly Color _hoverColor;
        public Color CurrentColor => IsHovered ? _hoverColor : _normalColor; 

        public ActionButton(
            Rectangle bounds,
            Texture2D texture,
            Action onClick,
            Color normalColor,
            Color hoverColor
        ) : base(bounds, texture)
        {
            _onClick = onClick
                ?? throw new ArgumentNullException(nameof(onClick));

            _normalColor = normalColor;
            _hoverColor = hoverColor;
        }

        public ActionButton(
            Rectangle bounds,
            Texture2D texture,
            Action onClick,
            Color normalColor,
            Color hoverColor,
            string text
        ) : base(bounds, texture)
        {
            _onClick = onClick
                ?? throw new ArgumentNullException(nameof(onClick));

            Text = text;
            _normalColor = normalColor;
            _hoverColor = hoverColor;
        }


        public override void OnClick()
        {
            _onClick();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color = IsHovered
                ? _hoverColor
                : _normalColor;

            spriteBatch.Draw(_texture, _bounds, color);
        }
        /*
        private void DrawButtonText(
            SpriteBatch spriteBatch,
            string text,
            Rectangle bounds,
            Color normalColor,
            Color hoverColor)
            {
            Color color = IsHovered
                ? hoverColor
                : normalColor;

            DrawCenteredText(
                spriteBatch,
                text,
                bounds,
                color
            );
        }
        */
        

        }

    }




