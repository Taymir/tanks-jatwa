using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Networking;

namespace WindowsGame1
{
    public abstract class BaseCollisionManager
    {
        public abstract bool isCollision(float x, float y, uint current_id); //@REFACTOR БРЕД БРЕД БРЕД!!!! ИДИОТИЗМ

        public abstract int collisionDistanceSqr(float x, float y, float rotation, uint current_id);

        public abstract bool isTankCollision(float x, float y);//@TMP

        public static int calcDistanceSqr(Point A, Point B)
        {
            return (int)(Math.Pow(B.X - A.X, 2) + Math.Pow(B.Y - A.Y, 2));

        }
    }

    /*public class NoCollisionManager : BaseCollisionManager
    {

        public override bool isCollision(float x, float y)
        {
            return false;
        }

        public override int collisionDistanceSqr(float x, float y, float rotation)//@TODO заменить на формулу
        {
            float iX = x;
            float iY = y;

            while (iX >= 0 && iY >= 0 && iX < 800 && iY < 600)
            {
                iX -= (float)Math.Sin(-rotation);
                iY -= (float)Math.Cos(-rotation);
            }

            return calcDistanceSqr(new Point((int)iX, (int)iY), new Point((int)x, (int)y));
        }
    }*/

    public class NoCollisionManager : BaseCollisionManager
    {
        protected Dictionary<uint, ClientInfo> clients;
        protected Tank tank;//@TMP

        public NoCollisionManager(Dictionary<uint, ClientInfo> clients, Tank tank)
        {
            this.clients = clients;
            this.tank = tank;
        }

        public override bool isCollision(float x, float y, uint current_id)
        {
            foreach (uint id in this.clients.Keys)
            {
                if (id == current_id) continue;

                //if (client.tank == null) continue; //@BUGFIX dirty hack на случай, если клиент создан, а танк еще нет 
                if (x >= clients[id].tank.x - 30 && x < clients[id].tank.x + 30 && y >= clients[id].tank.y - 30 && y < clients[id].tank.y + 30)
                {
                    // collision is!
                    return true;
                }
            }
            if (x >= this.tank.x - 30 && x < this.tank.x + 30 && y >= this.tank.y - 30 && y < this.tank.y + 30)//@TMP УЖС УЖС УЖС!
                // collision is!
                return true;

            return false;
        }

        public override int collisionDistanceSqr(float x, float y, float rotation, uint current_id)
        {
            float iX = x;
            float iY = y;

            while (iX >= 0 && iY >= 0 && iX < 800 && iY < 600)
            {
                foreach (uint id in this.clients.Keys)
                {
                    if (id == current_id) continue;

                    if (iX >= clients[id].tank.x - 30 && iX < clients[id].tank.x + 30 && iY >= clients[id].tank.y - 30 && iY < clients[id].tank.y + 30)
                    {
                        // collision is!
                        return calcDistanceSqr(new Point((int)iX, (int)iY), new Point((int)x, (int)y));
                    }
                }
                if (iX >= tank.x - 30 && iX < tank.x + 30 && iY >= tank.y - 30 && iY < tank.y + 30)//@TMP УЖС УЖС УЖС!
                    // collision is!
                    return calcDistanceSqr(new Point((int)iX, (int)iY), new Point((int)x, (int)y));

                iX -= (float)Math.Sin(-rotation);
                iY -= (float)Math.Cos(-rotation);
            }

            return calcDistanceSqr(new Point((int)iX, (int)iY), new Point((int)x, (int)y));
        }

        public override bool isTankCollision(float x, float y)
        {
            return false;
        }
    }

    public class CollisionManager : BaseCollisionManager
    {
        public Dictionary<uint, ClientInfo> clients;

        public CollisionManager(Dictionary<uint, ClientInfo> clients)
        {
            this.clients = clients;
        }

        public override bool isCollision(float x, float y, uint current_id)
        {
            foreach (uint id in this.clients.Keys)
            {
                //if (client.tank == null) continue; //@BUGFIX dirty hack на случай, если клиент создан, а танк еще нет 
                if (x >= clients[id].tank.x - 30 && x < clients[id].tank.x + 30 && y >= clients[id].tank.y - 30 && y < clients[id].tank.y + 30)
                {
                    // collision is!
                    //clients[id].tank.hit(20);

                    sendHit(25, id); //@TMP

                    return true;
                }
            }

            return false;
        }

        public Protocol protocol;//@TMP
        public System.Net.EndPoint server;

        // Передача по сети информации о повреждении //@TMP
        protected void sendHit(short damage, uint client_id)
        {
            Protocol.HitStruct s = new Protocol.HitStruct();
            s.adress = server;
            
            s.client_id = client_id;
            s.damage = damage;

            protocol.SendHit(s);
        }

        public override int collisionDistanceSqr(float x, float y, float rotation, uint current_id)
        {
            float iX = x;
            float iY = y;

            while (iX >= 0 && iY >= 0 && iX < 800 && iY < 600)
            {
                foreach (uint id in this.clients.Keys)
                {
                    if (iX >= clients[id].tank.x - 30 && iX < clients[id].tank.x + 30 && iY >= clients[id].tank.y - 30 && iY < clients[id].tank.y + 30)
                    {
                        // collision is!
                        //clients[id].tank.hit(1);

                        sendHit(1, id); //@TMP

                        return calcDistanceSqr(new Point((int)iX, (int)iY), new Point((int)x, (int)y));
                    }
                }

                iX -= (float)Math.Sin(-rotation);
                iY -= (float)Math.Cos(-rotation);
            }

            return calcDistanceSqr(new Point((int)iX, (int)iY), new Point((int)x, (int)y));
        }

        public override bool isTankCollision(float x, float y)
        {
            foreach (uint id in this.clients.Keys)
            {
                if (x >= clients[id].tank.x - 60 && x < clients[id].tank.x + 60 && y >= clients[id].tank.y - 60 && y < clients[id].tank.y + 60)
                {
                    // collision is!

                    return true;
                }
            }
            if (x >= 800 - 30 || x <= 0 + 20 || y >= 600 - 30 || y <= 0 + 30)
                return true;

            return false;
        }
    }
}
