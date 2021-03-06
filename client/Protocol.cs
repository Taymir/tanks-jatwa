using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Networking
{
    public class Protocol : Dispatcher
    {
        // ?????? ???? ????? ??????? (???????? ??????? ????????? ??????? ???? ?????? - ?????)
        #region packet_types
        public enum Type
        {
            // ??????????
            Connect,
            // ?????? ??????????
            Disconnect,
            // ????????? ????
            MessageToServer,
            // ?????????? ???????????
            Welcome,
            // ????????? ??????? ?? ?????
            Position,
            // ???????? ????????? ???? ????????
            MessageToClient,
            // ?????? ???? ????????
            List,
            // ????????
            Shoot,
            // ?????????
            Hit, 
            // ??????
            Dead
        }
        #endregion packet_types

        // ??????? ??????????? ???????
        #region packet_structs
        public class BaseStruct : EventArgs
        {
            // ????? ??????????? ??? ?????????? ??????
            public EndPoint adress;
        }

        public class ConnectStruct : BaseStruct
        {
            // | ??? [string]  |
            public string nick;
        }

        public class DisconnectStruct : BaseStruct
        {
            // | ????????????? ??????? [uint32] |
            public uint id;
        }

        public class WelcomeStruct : BaseStruct
        {
            // | ????????????? ??????? [uint32] |
            public uint id;
        }

        public class PositionStruct : BaseStruct
        {
            // | ????????????? ??????? [uint32] |
            public uint id;
            // | ?????????? X ????? [uint32] |
            public uint x;
            // | ?????????? Y ????? [uint32] |
            public uint y;
            // | ???? ???????? ????? [double] |
            public double angle;
            // | ???? ???????? ??????? [double] |
            public double angle_turret;
        }

        public class MessageToServerStruct : BaseStruct
        {
            // | ????????????? ??????? [uint32] |
            public uint client_id;
            // | ????????? ???? [string] |
            public string message;
        }

        public class MessageToClientStruct : BaseStruct
        {
            // | ??????? ?????? ????????? [string] |
            public string nick;
            // | ????????? [string] |
            public string message;
        }

        public class ListStruct : BaseStruct
        {
            // | ?????? ???????? [byte Count | uint Id | string Nick | ... | uint Id | string Nick] |
            public Dictionary<uint, string> clients;
        }

        public class ShootStruct : BaseStruct
        {
            public enum Weapon
            {
                // ?????? ??????
                RocketStart,
                // ??? ?????????????, ?? ????????????
                RocketStop,
                // ?????? ???????? ????????
                GunStart,
                // ????? ???????? ????????
                GunStop
            }

            // | ????????????? ??????? [uint32] |
            public uint client_id;
            // | ??? ?????? [byte] |
            public Weapon weapon;

        }

        public class HitStruct : BaseStruct
        {
            // | ????????????? ??????? [uint32] |
            public uint client_id;
            // | ??????????? [short] |
            public short damage;
        }

        public class DeadStruct : BaseStruct
        {
            // | ????????????? ??????? [uint32] |
            public uint client_id;
        }
        #endregion packet_structs

        // ???????, ????????????? ??? ??????? ??????????????? ???????
        #region events
        public event EventHandler<ConnectStruct> OnConnect;
        public event EventHandler<DisconnectStruct> OnDisconnect;
        public event EventHandler<WelcomeStruct> OnWelcome;
        public event EventHandler<PositionStruct> OnPosition;
        public event EventHandler<MessageToServerStruct> OnMessageToServer;
        public event EventHandler<MessageToClientStruct> OnMessageToClient;
        public event EventHandler<ListStruct> OnList;
        public event EventHandler<ShootStruct> OnShoot;
        public event EventHandler<HitStruct> OnHit;
        public event EventHandler<DeadStruct> OnDead;
        #endregion events

        public override void onRecieve(Packet packet, EndPoint sender)
        {
            packet.beginReading();

            // | ??? ?????? [int32] | ?????? [...] |
            Type type = (Type)packet.ReadInt();

            switch (type)
            {
                // ???????? ???????? ??? ????????? ?????????
                #region actions

                // | ??? [string]  |
                case Type.Connect:
                    {
                        ConnectStruct s = new ConnectStruct();
                        s.adress = sender;

                        s.nick = packet.ReadString();

                        if (OnConnect != null)
                            OnConnect(this, s);
                    }
                    break;

                // | ????????????? ??????? [uint32] |
                case Type.Disconnect:
                    {
                        DisconnectStruct s = new DisconnectStruct();
                        s.adress = sender;

                        s.id = packet.ReadUInt();

                        if (OnDisconnect != null)
                            OnDisconnect(this, s);
                    }
                    break;

                // | ????????????? ??????? [uint32] |
                case Type.Welcome:
                    {
                        WelcomeStruct s = new WelcomeStruct();
                        s.adress = sender;

                        s.id = packet.ReadUInt();

                        if (OnWelcome != null)
                            OnWelcome(this, s);
                    }
                    break;


                // | ??????? ??????? [uint32] |
                case Type.Position:
                    {
                        PositionStruct s = new PositionStruct();
                        s.adress = sender;

                        s.id = packet.ReadUInt();
                        s.x = packet.ReadUInt();
                        s.y = packet.ReadUInt();
                        s.angle = packet.ReadDouble();
                        s.angle_turret = packet.ReadDouble();

                        if (OnPosition != null)
                            OnPosition(this, s);
                    }
                    break;


                case Type.MessageToServer:
                    {
                        MessageToServerStruct s = new MessageToServerStruct();
                        s.adress = sender;

                        s.client_id = packet.ReadUInt();
                        s.message = packet.ReadString();

                        if (OnMessageToServer != null)
                            OnMessageToServer(this, s);
                    }
                    break;


                case Type.MessageToClient:
                    {
                        MessageToClientStruct s = new MessageToClientStruct();
                        s.adress = sender;

                        s.nick = packet.ReadString();
                        s.message = packet.ReadString();

                        if (OnMessageToClient != null)
                            OnMessageToClient(this, s);
                    }
                    break;

                case Type.List:
                    {
                        ListStruct s = new ListStruct();
                        s.adress = sender;

                        byte count = packet.ReadByte();
                        s.clients = new Dictionary<uint, string>();
                        for (int i = 0; i < count; ++i)
                        {
                            uint id = packet.ReadUInt();
                            string nick = packet.ReadString();

                            s.clients[id] = nick;
                        }

                        if (OnList != null)
                            OnList(this, s);
                    }
                    break;
                case Type.Shoot:
                    {
                        ShootStruct s = new ShootStruct();
                        s.adress = sender;

                        s.client_id = packet.ReadUInt();
                        s.weapon = (ShootStruct.Weapon)packet.ReadByte();

                        if (OnShoot != null)
                            OnShoot(this, s);
                    }
                    break;
                case Type.Hit:
                    {
                        HitStruct s = new HitStruct();
                        s.adress = sender;

                        s.client_id = packet.ReadUInt();
                        s.damage = packet.ReadShort();

                        if (OnHit != null)
                            OnHit(this, s);
                    }
                    break;
                case Type.Dead:
                    {
                        DeadStruct s = new DeadStruct();
                        s.adress = sender;

                        s.client_id = packet.ReadUInt();

                        if (OnDead != null)
                            OnDead(this, s);
                    }
                    break;

                #endregion actions
            }
        }

        public override void onSend()
        {
        }

        public static Packet FormFirstPacket(string nick)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Connect);

            packet.Write(nick);
            return packet;
        }

        #region send_packets
        public void SendConnect(ConnectStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Connect);

            packet.Write(s.nick);
            connection.sendTo(packet, s.adress);
        }

        public void SendWelcome(WelcomeStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Welcome);

            packet.Write(s.id);
            connection.sendTo(packet, s.adress);
        }

        public void SendDisconnect(DisconnectStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Disconnect);

            packet.Write(s.id);
            connection.sendTo(packet, s.adress);
        }

        public void SendPosition(PositionStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Position);

            packet.Write(s.id);
            packet.Write(s.x);
            packet.Write(s.y);
            packet.Write(s.angle);
            packet.Write(s.angle_turret);
            connection.sendTo(packet, s.adress);
        }

        public void SendMessageToServer(MessageToServerStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.MessageToServer);

            packet.Write(s.client_id);
            packet.Write(s.message);
            connection.sendTo(packet, s.adress);
        }

        public void SendMessageToClient(MessageToClientStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.MessageToClient);

            packet.Write(s.nick);
            packet.Write(s.message);
            connection.sendTo(packet, s.adress);
        }

        public void SendList(ListStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.List);

            byte count = (byte)s.clients.Count;
            packet.Write(count);

            foreach (uint id in s.clients.Keys)
            {
                string nick = s.clients[id];

                packet.Write(id);
                packet.Write(nick);
            }
            connection.sendTo(packet, s.adress);

        }

        public void SendShoot(ShootStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Shoot);

            packet.Write(s.client_id);
            packet.Write((byte)s.weapon);
            connection.sendTo(packet, s.adress);
        }

        public void SendHit(HitStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Hit);

            packet.Write(s.client_id);
            packet.Write(s.damage);
            connection.sendTo(packet, s.adress);
        }

        public void SendDead(DeadStruct s)
        {
            Packet packet = new Packet();
            packet.Write((int)Type.Dead);

            packet.Write(s.client_id);
            connection.sendTo(packet, s.adress);
        }
        #endregion send_packets
    };
}
