// Gui.cs created with MonoDevelop
// User: akkarin at 14:00 20.05.2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace WindowsGame1
{
	public class HealthGui
	{
		private GraphSprite background;
		private TextSprite printHP;
		private uint hp;
		
		public HealthGui(uint x, uint y)
		{
			hp = 100;
			
			background = new GraphSprite();
			background.loadTextureFromFile("hp_g_int.png");
			background.x = x;
			background.y = y;
			background.visible = true;
			
			printHP = new TextSprite(hp.ToString(), Font.BigDigits);
			printHP.x = x + 108;
			printHP.y = y + 50;
			printHP.visible = true;
		}
		
		public uint health
		{
			get
			{
				return hp;
			}
			set
			{
				hp = value;
				printHP.text = hp.ToString();
                printHP.x = background.x + 130 - (hp.ToString().Length * printHP.char_width) / 2;
			}
		}
	}
}
