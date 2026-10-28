using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Inputs;

namespace pcsk1.src.Screens.UI
{
    public abstract class AbstractButton
    {
        protected Rectangle _bounds;
        protected Texture2D _texture;
        protected Texture2D _hoverTexture;

        public bool IsHovered { get; protected set; }
        protected AbstractButton(Rectangle bounds, Texture2D texture, Texture2D hoverTexture = null)
        {
            _bounds = bounds;
            _texture = texture;
            _hoverTexture = hoverTexture;
        }

        public virtual void Update()
        {
            Point mousePoint = Input.MousePosition;
            IsHovered = _bounds.Contains(mousePoint);

            if (IsHovered && Input.IsLeftButtonPressed())
            {
                OnClick();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Texture2D textureToDraw = (IsHovered && _hoverTexture != null) ? _hoverTexture : _texture;
            if (textureToDraw != null) spriteBatch.Draw(textureToDraw, _bounds, Color.White);
        }
        public abstract void OnClick();


    }
}
