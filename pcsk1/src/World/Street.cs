using Microsoft.Xna.Framework.Graphics;
using pcsk1.src.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsk1.src.World
{
    public class Street : IEventable
    {
        private Dictionary<ushort, Texture2D> _tileTextures = new(); 

        private readonly ushort[,] _tileMap = new ushort[,]
        {
            {1, 2, 3, 4, 5, 6},
            {3, 5, 5, 6, 7, 8 }
        };
        public void Dispose()
        {
            
        }

        public void Initialize()
        {
            
        }
    }
}
