using Microsoft.Xna.Framework;

namespace pcsk1.src.Camera
{
    public class Camera2D
    {
        public Matrix Transform { get; private set; }

        private Vector2 _currentPosition = Vector2.Zero;
        private readonly float _lerpSpeed = 0.1f;

        public float Zoom { get; set; } = 2.5f;

        public void Follow(Vector2 targetPosition, int screenWidth, int screenHeight)
        {

            Vector2 playerCenter = new(
                targetPosition.X + (64f / 2f),
                targetPosition.Y + (64f / 2f)
                );
            Vector2 targetCenter = new(
                playerCenter.X - (screenWidth / 2f / Zoom),
                playerCenter.Y - (screenHeight / 2f / Zoom)
            );
            _currentPosition = Vector2.Lerp(_currentPosition, targetCenter, _lerpSpeed);

            Transform = Matrix.CreateTranslation(new Vector3(-_currentPosition, 0)) *
            Matrix.CreateScale(Zoom, Zoom, 1.0f);
        }

    }
}
