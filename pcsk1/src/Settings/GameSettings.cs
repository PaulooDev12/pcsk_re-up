using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsk1.src.Settings
{
    public sealed class GameSettings
    {
        public int Width { get; private set; } = 1280;
        public int Height { get; private set; } = 720;
        public bool Fullscreen { get; private set; } = false;
        public float MasterVolume { get; private set; } = 1f;

        public void SetResolution(int width, int height)
        {
            if(width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(width)} Resoulução inválida");
            }
            Width = width;
            Height = height;
        }
        public void SetFullscreen(bool enabled)
        {
            Fullscreen = enabled;
        }

        public void SetVolume(float volume)
        {
            MasterVolume = Math.Clamp(volume, 0f, 1f);
        }
    }
}
