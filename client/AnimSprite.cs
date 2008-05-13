// AnimSprite.cs created with MonoDevelop
// User: akkarin at 13:06 13.05.2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
	
	
	public class AnimSprite : GraphSprite
	{
		private int delayNum = 0;
		private int frameNum = 0;

		public int framesAmount = 0;
		public int delay = 0;
		
		public override void Render()
        {			
			if (!(this.x >= 0 && this.x < 800 && this.y >= 0 && this.y < 600)) return;
			
			if (this.delayNum < this.delay)
			{
				this.delayNum++;
			}
			else
			{
				this.frameNum++;
				this.delayNum = 0;
			}		
			
			if (this.frameNum >= this.framesAmount)
			{
				this.visible = false;
			}
		
			SpritesRenderer.spriteBatch.Begin();
            SpritesRenderer.spriteBatch.Draw(this.texture, new Vector2(this.x, this.y), new Rectangle(this.frameNum * this.srcRect.Value.Width, 0, this.srcRect.Value.Width, this.srcRect.Value.Height), this.color, this.rotation, new Vector2(this.x_origin, this.y_origin), scale, SpriteEffects.None, 0f);
			SpritesRenderer.spriteBatch.End();
		}
	}
}
