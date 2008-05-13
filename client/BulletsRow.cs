using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Summary description for Class1
/// </summary>

namespace WindowsGame1
{
    public class BulletsRow : GraphSprite
    {
        protected BaseCollisionManager collisionManager;
        public uint client_id;//@REFACTOR

        public BulletsRow(BaseCollisionManager collisionManager, uint client_id)
        {
            this.collisionManager = collisionManager;
            this.client_id = client_id;
        }

        public override void Render()
        {
            if (!(this.x >= 0 && this.x < 800 && this.y >= 0 && this.y < 600))
            {
                return;
            }

            Random r = new Random();

            SpritesRenderer.spriteBatch.Begin();
            int i = 0;


            int maxDistance = collisionManager.collisionDistanceSqr(this.x, this.y, this.rotation, this.client_id);
            Point origPoint = new Point((int)this.x, (int)this.y);
            Point currentPoint = new Point((int)this.x, (int)this.y);

            while (true)
            {
                int shift = r.Next(50);

                currentPoint.X = (int)(this.x - Math.Round(Math.Sin(-this.rotation) * i * shift) + r.Next(2));
                currentPoint.Y = (int)(this.y - Math.Round(Math.Cos(-this.rotation) * i * shift) + r.Next(2));

                if (BaseCollisionManager.calcDistanceSqr(currentPoint, origPoint) >= maxDistance)
                    break;

                SpritesRenderer.spriteBatch.Draw(this.texture, new Vector2(currentPoint.X, currentPoint.Y), this.srcRect, this.color, rotation, new Vector2(this.x_origin, this.y_origin), scale, SpriteEffects.None, 0f);
                i++;
            }
            SpritesRenderer.spriteBatch.End();
        }
    }
}
