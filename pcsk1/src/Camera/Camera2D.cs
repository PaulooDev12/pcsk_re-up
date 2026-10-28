using Microsoft.Xna.Framework;

namespace pcsk1.src.Camera
{
    public class Camera2D
    {
        public Matrix Transform { get; private set; }

        private Vector2 _currentPosition = Vector2.Zero;
        private readonly float _lerpSpeed = 0.1f;

        public void Follow(Vector2 targetPosition, int screenWidth, int screenHeight)
        {
            Vector2 targetCenter = new(
                targetPosition.X - (screenWidth / 2f),
                targetPosition.Y - (screenHeight / 2f)
            );
            _currentPosition = Vector2.Lerp(_currentPosition, targetCenter, _lerpSpeed);

            Transform = Matrix.CreateTranslation(-_currentPosition.X, -_currentPosition.Y, 0);
        }

    }
}
