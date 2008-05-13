using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace WindowsGame1
{
    // Класс цели (указателя стрельбы)
    public class Aim
    {
        // Скорость поворота цели
        public const float rotationSpeed = (float)Math.PI / 75;

        protected float rel_x, rel_y;     // Координаты цели относительно танка
        protected GraphSprite sprite;   // Спрайт с изображением курсора
        protected Tank tank;            // Танк, который управляется данным курсором


        public int x
        {
            get
            {
                return (int)rel_x + (int)tank.x;
            }
        }
        public int y
        {
            get
            {
                return (int)rel_y + (int)tank.y;
            }
        }
        public int relative_x
        {
            get
            {
                return (int)this.rel_x;
            }
            set
            {
                checkDistance(value, (int)rel_y);

                // Проверка, не выходит ли курсор за экран
                if (this.x < 0)
                    this.rel_x = -(int)tank.x;
                else if (this.x > 800)
                    this.rel_x = 800 - (int)tank.x;

                

                sprite.x = this.x;
            }
        }
        public int relative_y
        {
            get
            {
                return (int)this.rel_y;
            }
            set
            {
                checkDistance((int)rel_x, value);

                // Проверка, не выходит ли курсор за экран
                if (this.y < 0)
                    this.rel_y = -(int)tank.y;
                else if (this.y > 600)
                    this.rel_y = 600 - (int)tank.y;

                

                sprite.y = this.y;
            }
        }

        public Aim(Tank tank)
        {
            this.tank = tank;

            this.sprite = new GraphSprite();
            this.sprite.loadTextureFromFile("kursor.png");
            this.sprite.x_origin = 15;
            this.sprite.y_origin = 15;
            this.sprite.visible = true;

            this.relative_x = 0;
            this.relative_y = 20;
        }

        public void rotate(float angle)
        {
            angle *= rotationSpeed;

            float fx = (float)Math.Cos(angle) * this.relative_x - (float)Math.Sin(angle) * this.relative_y;
            float fy = (float)Math.Sin(angle) * this.relative_x + (float)Math.Cos(angle) * this.relative_y;

            this.relative_x = (int)Math.Round(fx);
            this.relative_y = (int)Math.Round(fy);
        }

        public void updateCoords()
        {
            this.relative_x = this.relative_x;
            this.relative_y = this.relative_y;
        }

        protected void checkDistance(int x, int y)
        {
            double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            if (distance > 150)
            {
                double percent = 150 / distance;
                rel_x = (int)Math.Round((double)x * percent);
                rel_y = (int)Math.Round((double)y * percent);
            }
            else if (distance < 50)
            {
                double percent = 50 / distance;
                rel_x = (int)Math.Round((double)x * percent);
                rel_y = (int)Math.Round((double)y * percent);
            }
            else
            {
                rel_x = x;
                rel_y = y;
            }
        }

    }
}