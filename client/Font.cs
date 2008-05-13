using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsGame1
{
    public class Font
    {
        private string texture;
        private ushort char_width, char_height;
        private ushort first_symbol, last_symbol;
        private ushort bad_symbol;

        public Font(string texture, ushort char_width, ushort char_height,
                         ushort first_symbol, ushort last_symbol, ushort bad_symbol)
        {
            this.texture = texture;
            this.char_width = char_width;
            this.char_height = char_height;
            this.first_symbol = first_symbol;
            this.last_symbol = last_symbol;
            this.bad_symbol = bad_symbol;
        }

        public string getTexture()
        {
            return this.texture;
        }

        public ushort getCharWidth()
        {
            return this.char_width;
        }

        public ushort getCharHeight()
        {
            return this.char_height;
        }

        public ushort getFirstSymbol()
        {
            return this.first_symbol;
        }

        public ushort getLastSymbol()
        {
            return this.last_symbol;
        }

        public ushort getBadSymbol()
        {
            return this.bad_symbol;
        }

        public static Font LucidaBlack12
        {
            get
            {
                return new Font("font_black.png", 7, 11, 0, 255, 95);
            }
        }

        public static Font LucidaRed12
        {
            get
            {
                return new Font("font_red.png", 7, 11, 0, 255, 95);
            }
        }

        public static Font LucidaBlue12
        {
            get
            {
                return new Font("font_blue.png", 7, 11, 0, 255, 95);
            }
        }

        public static Font LucidaGreen14
        {
            get
            {
                return new Font("font_green.png", 8, 14, 0, 255, 95);
            }
        }

        public static Font LucidaYellow14
        {
            get
            {
                return new Font("font_yellow.png", 8, 14, 0, 255, 95);
            }
        }

        public static Font LucidaWhite14
        {
            get
            {
                return new Font("font_white.png", 8, 14, 0, 255, 95);
            }
        }

        public static Font BigDigits
        {
            get
            {
                return new Font("font_big.png", 22, 30, 0x30, 0x39, 0x30);
            }
        }
    }
}
