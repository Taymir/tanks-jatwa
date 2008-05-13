using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    public abstract class BaseSprite
    {
        public float x = 0, y = 0;
        public Color color = Color.White;
        public Texture2D texture;
        public Rectangle? srcRect = null;
        public bool visible
        {
            get
            {
                return renderer.Contains(this);       
            }
            set
            {
                if (renderer.Contains(this))
                {
                    if (!value)
                        renderer.Remove(this);
                }
                else
                {
                    if(value)
                        renderer.Add(this);
                }
            }
        }
        protected SpritesRenderer renderer = new SpritesRenderer();

        public void loadTextureFromFile(string filename)
        {
            this.texture = Texture2D.FromFile(SpritesRenderer.device, filename);
        }

        public virtual void Render()
        {
            if (!(this.x >= 0 && this.x < 800 && this.y >= 0 && this.y < 600)) return;

            SpritesRenderer.spriteBatch.Begin();
            SpritesRenderer.spriteBatch.Draw(this.texture, new Vector2(this.x, this.y), this.srcRect, this.color);
            SpritesRenderer.spriteBatch.End();
        }
    }
}
