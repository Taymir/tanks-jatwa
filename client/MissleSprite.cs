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
    public class MissleSprite : GraphSprite
    {
        protected BaseCollisionManager collisionManager;
        protected uint client_id;//@REFACTOR

        public MissleSprite(BaseCollisionManager collisionManager, uint client_id)
        {
            this.collisionManager = collisionManager;
            this.client_id = client_id;
        }

        public override void Render()
        {
            if (!(this.x >= 0 && this.x < 800 && this.y >= 0 && this.y < 600))
            {
                this.visible = false;
                return;
            }

            SpritesRenderer.spriteBatch.Begin();
            SpritesRenderer.spriteBatch.Draw(this.texture, new Vector2(this.x, this.y), this.srcRect, this.color, rotation, new Vector2(this.x_origin, this.y_origin), scale, SpriteEffects.None, 0f);     
            this.Move();
            if (this.collisionManager.isCollision(this.x, this.y, this.client_id))
                this.visible = false; // Снаряд взрывается

            SpritesRenderer.spriteBatch.End();
        }

        protected void Move()
        {
            float speed = 10f;
            this.x -= (float)Math.Sin(-this.rotation) * speed;
            this.y -= (float)Math.Cos(-this.rotation) * speed;
        }
    }
}
