using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Networking
{
    class Protocol : Dispatcher
    {
        // Список всех типов пакетов (описание формата заданного каждого вида пакета - далее)
        #region packet_types
        public enum Type
        {
            // Сообщения от клиента к серверу:

            // Соединение
            Connect,
            // Разрыв соединения
            Disconnect,

            // Сообщения от сервера к клиенту:

            // Соединение произведено
            Welcome
        }
        #endregion packet_types

        // Структы содержимого пакетов
        #region packet_structs
        public class BaseStruct : EventArgs
        {
            // Адрес отправителя или получателя пакета
            public EndPoint adress;
        }

        public class ConnectStruct : BaseStruct
        {
            // | Ник [string]  |
            public string nick;
        }

        public class DisconnectStruct : BaseStruct 
        {
            // | |
        }

        public class WelcomeStruct : BaseStruct 
        {
            // | Идентификатор клиента [uint32] |
            public uint id;
        }
        #endregion packet_structs

        // События, срабатывающие при приходе соответствующих пакетов
        #region events
        public event EventHandler<ConnectStruct> OnConnect;
        public event EventHandler<DisconnectStruct> OnDisconnect;
        public event EventHandler<WelcomeStruct> OnWelcome;
        #endregion events

        public override void onRecieve(Packet packet, EndPoint sender)
        {
            packet.beginReading();

            // | Тип пакета [int32] | Данные [...] |
            Type type = (Type)packet.ReadInt();

            switch (type)
            {
                // Описание действий при получении сообщений
                #region actions

                // | Ник [string]  |
                case Type.Connect:
                    {
                        ConnectStruct s = new ConnectStruct();
                        s.adress = sender;

                        s.nick = packet.ReadString();

                        if (OnConnect != null)
                            OnConnect(this, s);
                    }
                    break;

                // | |
                case Type.Disconnect:
                    {
                        DisconnectStruct s = new DisconnectStruct();
                        s.adress = sender;

                        if (OnDisconnect != null)
                            OnDisconnect(this, s);
                    }
                    break;

                // | Идентификатор клиента [uint32] |
                case Type.Welcome:
                    {
                        WelcomeStruct s = new WelcomeStruct();
                        s.adress = sender;

                        s.id = packet.ReadUInt();

                        if (OnWelcome != null)
                            OnWelcome(this, s);
                    }
                    break;

                #endregion actions
            }
        }

        public override void onSend()
        {
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

            connection.sendTo(packet, s.adress);
        }
        #endregion send_packets
    };
}
