using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowsGame1
{
    public class TextSprite : BaseSprite
    {
        public ushort char_width = 7, char_height = 11;
        public string text = "";
        public ushort first_symbol = 0, last_symbol = 255;
        public ushort bad_symbol = 95;

        public TextSprite()
        {
        }

        public TextSprite(Font font)
        {
            this.setFont(font);
        }

        public TextSprite(string text)
        {
            this.text = text;
        }

        public TextSprite(string text, Font font)
        {
            this.text = text;
            this.setFont(font);
        }

        public void setFont(Font font)
        {
            this.loadTextureFromFile(font.getTexture());

            this.char_width = font.getCharWidth();
            this.char_height = font.getCharHeight();

            this.first_symbol = font.getFirstSymbol();
            this.last_symbol = font.getLastSymbol();

            this.bad_symbol = font.getBadSymbol();
        }

        public override void Render()
        {
            if (!(this.x >= 0 && this.x < 800 && this.y >= 0 && this.y < 600)) return;

            int chr_index = 0;
            int x_offset = 0;
            int y_offset = 0;

            if (this.srcRect != null)
            {
                x_offset = this.srcRect.Value.X;
                y_offset = this.srcRect.Value.Y;
            }


            for (int i = 0; i < this.text.Length; ++i)
            {
                chr_index = (int)this.text[i];
                if (chr_index >= 976) chr_index -= 848;
                if (chr_index == 177) chr_index = 168; //@BUGFIX "Ё" почему-то не на своём месте...
                if (chr_index == 257) chr_index = 184; //@BUGFIX "ё" почему-то не на своём месте...
                if (chr_index < first_symbol || chr_index > last_symbol) chr_index = bad_symbol;

                SpritesRenderer.spriteBatch.Begin();
                SpritesRenderer.spriteBatch.Draw(this.texture, new Vector2(this.x + i * char_width, this.y), new Rectangle((chr_index - first_symbol) * char_width + x_offset, 0 + y_offset, this.char_width, this.char_height), this.color);
                SpritesRenderer.spriteBatch.End();
            }

        }
    }
}
