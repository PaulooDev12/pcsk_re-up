using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Entities;
using pcsk1.src.Events;
using pcsk1.src.GameObjects.Playeer;

namespace pcsk1.src.Screens.UI
{
    public class MentalSanityBar : IEventable, IUiComponent
    {
        private readonly Player _playerInstance;
        public int CurrentSanity { get; private set; }
        private readonly Vector2 _position = new Vector2(-10, 25);
        public int MaxSanity { get; set; } = 100;
        private readonly int _maxBarWidth = 20;
        private Color _barColor = Color.White;

        private Rectangle Bounds { get; set; }
        private readonly Texture2D _whiteTexture;
        public MentalSanityBar(int currentSanity, Player player, Texture2D whiteTexutre)
        {
            CurrentSanity = currentSanity;
            _playerInstance = player;
            CurrentSanity = _playerInstance.MentalSanity;
            _whiteTexture = whiteTexutre;
            Initialize();
            UpdateBounds();
        }

        public void Initialize()
        {
            EventBus<AttributeChangedEvent<int>>.Subscribe(HandleChange);
        }


        public void Dispose()
        {
            EventBus<AttributeChangedEvent<int>>.Unsubscribe(HandleChange);
        }


        private void HandleChange(AttributeChangedEvent<int> e)
        {
            if(e.TargetId == "player" && e.AttributeName == "MentalSanity")
            {
                CurrentSanity = e.Value;
                UpdateBounds();
            }
        }

        private void UpdateBounds()
        {
            float sanityPercentage = MaxSanity > 0 ? (float)CurrentSanity / MaxSanity : 0f;
            sanityPercentage = MathHelper.Clamp(sanityPercentage, 0f, 1f);
            int currentWidth = (int)(_maxBarWidth * sanityPercentage) * 20;
            Bounds = new Rectangle((int)_position.X, (int)_position.Y, currentWidth, 13);
            GetBarColor(sanityPercentage);
        }

        private void GetBarColor(float percentage)
        {
            switch (percentage)
            {
                case > 0.8f:
                    _barColor = Color.WhiteSmoke;
                    break;
                case > 0.6f:
                    _barColor = Color.LightGray;
                    break;
                case > 0.3f:
                    _barColor = Color.Gray;
                    break;
                default:
                    _barColor = Color.Black;
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_whiteTexture, Bounds, _barColor);
        }
    }
}
