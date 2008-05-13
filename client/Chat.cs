using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    class Chat
    {
        public Font font = Font.LucidaGreen14;
        public GameTime time = new GameTime();

        protected List<ChatMessage> messages;
        protected string inputMessageString;
        protected TextSprite inputMessageSprite;
        protected bool inputModeFlag;
        protected Point outputField;

        public string inputMessage
        {
            get
            {
                return this.inputMessageString;
            }
            set
            {
                this.inputMessageString = value;
                this.inputMessageSprite.text = "Say: " + value + "|";
            }
        }

        public bool inputMode
        {
            get 
            {
                return this.inputModeFlag;
            }
            set 
            {
                if (value)
                    this.inputMessageSprite.visible = true;
                else
                    this.inputMessageSprite.visible = false;

                this.inputModeFlag = value;
            }
        }

        public Chat(Point inputField, Point outputField)
        {
            this.messages = new List<ChatMessage>();

            this.outputField = outputField;

            this.inputMessageSprite = new TextSprite(this.font);
            this.inputMessageSprite.x = inputField.X;
            this.inputMessageSprite.y = inputField.Y;

            this.inputMessage = "";
        }

        public void addMessage(string nickText, string messageText)
        {
            this.pushMessagesUp();

            ChatMessage message = new ChatMessage(this.outputField, nickText, messageText, (int)this.time.TotalRealTime.TotalSeconds, this.font);

            this.messages.Add(message);
        }

        protected void pushMessagesUp()
        {
            for (int i = 0; i < this.messages.Count; ++i)
                messages[i].pushUp();
        }

        public string getInputAndHide()
        {
            string res = this.inputMessage;
            this.inputMessage = "";

            this.inputModeFlag = false;

            return res;
        }

        public void tick(GameTime timeStamp)
        {
            this.time = timeStamp;

            if (this.messages.Count > 0)
                if (this.time.TotalRealTime.TotalSeconds - messages[0].getTimeStamp() >= 5)
                {
                    messages[0].Hide();
                    messages.RemoveAt(0);
                }
        }
    }

    // Структура одной записи в чате
    class ChatMessage
    {
        public Font font;

        protected TextSprite nick;
        protected TextSprite message;
        protected int timestamp;

        public ChatMessage(Point coordinates, string nick, string message, int timestamp, Font font)
        {
            this.font = font;

            this.nick = new TextSprite(nick + ":", this.font);
            this.message = new TextSprite(message, this.font);

            this.nick.x = coordinates.X;
            this.nick.y = coordinates.Y;
            this.message.x = this.nick.x + this.nick.text.Length * this.nick.char_width + 20;
            this.message.y = coordinates.Y;

            this.nick.visible = true;
            this.message.visible = true;

            this.timestamp = timestamp;
        }

        public void pushUp()
        {
            this.nick.y -= this.font.getCharHeight() + 5;
            this.message.y -= this.font.getCharHeight() + 5;
        }

        public int getTimeStamp()
        {
            return this.timestamp;
        }

        public void Hide()
        {
            this.nick.visible = false;
            this.message.visible = false;
        }
    }
}
