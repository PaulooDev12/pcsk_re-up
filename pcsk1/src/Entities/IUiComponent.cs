using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsk1.src.Entities
{
    internal interface IUiComponent
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);

    }
}
