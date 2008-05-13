using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    public class GraphSprite : BaseSprite
    {
        public float rotation = 0.0f;
        public float x_origin = 0, y_origin = 0;
        public float scale = 1.0f;

        public override void Render()
        {
            if (!(this.x >= 0 && this.x < 800 && this.y >= 0 && this.y < 600)) return;
            
            SpritesRenderer.spriteBatch.Begin();
            SpritesRenderer.spriteBatch.Draw(this.texture, new Vector2(this.x, this.y), this.srcRect, this.color, rotation, new Vector2(this.x_origin, this.y_origin), scale, SpriteEffects.None, 0f); //@TODO: передавать глубину!
            SpritesRenderer.spriteBatch.End();
        }
    }
}
