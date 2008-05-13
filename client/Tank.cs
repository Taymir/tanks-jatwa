using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    public class Tank
    {
        // Типы танков: Зеленый и Желтый
        public enum Type
        {
            Green,
            Yellow
        }

        // Скорость поворота танка
        public const float rotationSpeed = (float)Math.PI / 75;

        // При получении повреждения более чем на данное число, возле танка происходит небольшой взрыв
        protected const int explosionHit = 15;

        // Спрайты с изображениями танка и башни
        protected GraphSprite tankSprite;
        protected GraphSprite turretSprite;

        // Здоровье
        protected int hp = 1000;
        //protected int gunAmmo = 999;
        //protected int missles = 20;

        // Менеджер по проверке столкновений
        BaseCollisionManager collisionManager;

        // Спрайт с изображением стрельбы из пулемета
        protected BulletsRow bulletsRow;

        // Номер клиента
        public uint client_id = 0;//@TMP

        public uint ClientId//@TMP
        {
            set
            {
                this.client_id = value;
                this.bulletsRow.client_id = value;
            }
        }

        public uint health
        {
            get
            {
                if (this.hp <= 0)
                    return 0;
                return (uint)this.hp / 10;
            }
        }

        // Поворот танка
        public float tankRotation
        {
            get
            {
                return this.tankSprite.rotation / rotationSpeed;
            }
            set
            {
                this.tankSprite.rotation = value * rotationSpeed;
            }
        }

        // Поворот башни
        public float turretRotation
        {
            get
            {
                return this.turretSprite.rotation / rotationSpeed;
            }
            set
            {
                this.turretSprite.rotation = value * rotationSpeed;

                this.bulletsRow.rotation = value * rotationSpeed;
            }
        }

        // Исчезновение - появление танка
        public bool visible
        {
            get
            {
                return this.tankSprite.visible;
            }
            set
            {
                this.tankSprite.visible = value;
                this.turretSprite.visible = value;
            }
        }

        // Доступ извне к координатам танка
        public float x
        {
            get
            {
                return tankSprite.x;
            }

            set
            {
                this.tankSprite.x = value;
                this.turretSprite.x = value;
                
                this.bulletsRow.x = value;
            }
        }
        public float y
        {
            get
            {
                return tankSprite.y;
            }

            set
            {
                this.tankSprite.y = value;
                this.turretSprite.y = value;
                
                this.bulletsRow.y = value;
            }
        }

        // В конструкторе загружаются текстуры в спрайт танка (в зависимости от типа танка, разные текстуры)
        public Tank(Type type, BaseCollisionManager collisionManager)
        {
            this.collisionManager = collisionManager;

            tankSprite = new GraphSprite();
            turretSprite = new GraphSprite();

            if (type == Type.Green)
            {
                tankSprite.loadTextureFromFile("jatwa_model_body.png");
                turretSprite.loadTextureFromFile("jatwa_model_turret.png");
            }
            else
            {
                tankSprite.loadTextureFromFile("jatwa_model_body_t2.png");
                turretSprite.loadTextureFromFile("jatwa_model_turret_t2.png");
            }

            tankSprite.x_origin = 18;
            tankSprite.y_origin = 28;

            turretSprite.x_origin = 15;
            turretSprite.y_origin = 52;

            bulletsRow = new BulletsRow(collisionManager, this.client_id);
            bulletsRow.loadTextureFromFile("bullet.png");
            bulletsRow.x_origin = 9;
            bulletsRow.y_origin = 3 + 24;

            this.x = 0;
            this.y = 0;
            
            bulletsRow.visible = false;
            tankSprite.visible = true;
            turretSprite.visible = true;
        }

        // Движение танка вперёд и назад
        public void Move (float speedAndDirection)
        {
            float tx = this.x;
            float ty = this.y;
            tx += (float)Math.Sin(-tankSprite.rotation) * speedAndDirection;
            ty += (float)Math.Cos(-tankSprite.rotation) * speedAndDirection;

            if (!this.collisionManager.isTankCollision(tx, ty))
            {
                this.x = tx;
                this.y = ty;
            }
        }

        // Вращение туррели в направлении указателя
        public void aimTurret(Aim aim)
        {
            int dx = aim.x - (int)this.x;
            int dy = aim.y - (int)this.y;

            this.turretRotation = (float)Math.Atan2(dx, -dy) / rotationSpeed;
        }

        // Получение танком повреждения
        public void hit(int damage)
        {
            if (damage > Tank.explosionHit)
            {
                //@TODO: анимация взрыва
                AnimSprite explosion = new AnimSprite();
                explosion.loadTextureFromFile("explosion.png");
                explosion.srcRect = new Rectangle(0, 0, 83, 86);
                explosion.x_origin = 41;
                explosion.y_origin = 43;
                explosion.x = this.x;
                explosion.y = this.y;
                explosion.framesAmount = 6;
                explosion.delay = 3;
                explosion.visible = true;
            }

            this.hp -= damage;
            //Console.WriteLine("Здоровье: " + this.hp.ToString());//@TMP
        }

        // Стрельба из башни
        public void launchMissle()
        {
            // Запуск ракеты
            MissleSprite Missle = new MissleSprite(this.collisionManager, this.client_id);

            Missle.loadTextureFromFile("missle.png");

            Missle.x = this.x;
            Missle.y = this.y;

            Missle.rotation = this.turretRotation * Tank.rotationSpeed;

            Missle.x_origin = 2;
            Missle.y_origin = 24 + 52;

            Missle.visible = true;
        }

        // Начало стрельбы из пулемета
        public void startShooting()
        {
            this.bulletsRow.visible = true;
        }

        // Конец стрельбы из пулемета
        public void stopShooting()
        {
            this.bulletsRow.visible = false;
        }
    }
}
