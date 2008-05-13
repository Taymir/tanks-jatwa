using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    public class SpritesRenderer
    {
        private static List<BaseSprite> sprites = new List<BaseSprite>();
        public static SpriteBatch spriteBatch;
        public static GraphicsDevice device;

        public static SpriteBatch Init(GraphicsDevice device)
        {
            SpritesRenderer.device = device;
            return SpritesRenderer.spriteBatch = new SpriteBatch(device);
        }

        public SpritesRenderer()
        {
            if (SpritesRenderer.spriteBatch == null)
                throw new Exception("Вы должны вызывать SpritesRenderer.Init перед создание объектов этого класса!");
        }

        public bool Contains(BaseSprite sprite)
        {
            return sprites.Contains(sprite);
        }

        public void Add(BaseSprite sprite)
        {
            sprites.Add(sprite);
        }

        public void Remove(BaseSprite sprite)
        {
            sprites.Remove(sprite);
        }

        public void Render()
        {
            for (int i = 0; i < sprites.Count; ++i)
            {
                sprites[i].Render();
            }
        }
    }
}
