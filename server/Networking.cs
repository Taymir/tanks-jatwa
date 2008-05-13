using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Networking
{
    // Класс сетевого пакета - позволяет "упаковывать" переменные в пакет и "распаковывать" из него
    public class Packet
    {
        protected List<byte> data;
        protected byte[] static_data;
        protected int readCount;

        public Packet()
        {
            data = new List<byte>();
            readCount = 0;
        }

        public Packet(byte[] bytes)
        {
            data = new List<byte>(bytes);
            readCount = 0;
        }

        public byte[] ToArray()
        {
            return data.ToArray();
        }

        public int getLength()
        {
            return data.Count;
        }

        public void Clear()
        {
            data.Clear();
            readCount = 0;
        }

        // Запись в пакет
        protected void doWrite(byte[] bytes)
        {
            data.AddRange(bytes);
        }

        public void Write(byte c)
        {
            data.Add(c);
        }

        public void Write(short c)
        {
            doWrite(BitConverter.GetBytes(c));
        }

        public void Write(long c)
        {
            doWrite(BitConverter.GetBytes(c));
        }

        public void Write(uint c)
        {
            doWrite(BitConverter.GetBytes(c));
        }

        public void Write(int c)
        {
            doWrite(BitConverter.GetBytes(c));
        }

        public void Write(double c)
        {
            doWrite(BitConverter.GetBytes(c));
        }

        public void Write(string c)
        {
            byte[] tmp = Encoding.UTF8.GetBytes(c);
            Write((short)tmp.Length);
            doWrite(tmp);
        }

        // Чтение из пакета
        public void beginReading()
        {
            static_data = data.ToArray();
            readCount = 0;
        }

        public byte ReadByte()
        {
            try
            {
                return static_data[readCount++];
            }
            catch (NullReferenceException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }
        }

        public short ReadShort()
        {
            try
            {
                short c = BitConverter.ToInt16(static_data, readCount);
                readCount += sizeof(short);

                return c;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }
        }

        public long ReadLong()
        {
            try
            {
                long c = BitConverter.ToInt64(static_data, readCount);
                readCount += sizeof(long);

                return c;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }
        }

        public uint ReadUInt()
        {
            try
            {
                uint c = BitConverter.ToUInt32(static_data, readCount);
                readCount += sizeof(uint);

                return c;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }
        }

        public int ReadInt()
        {
            try
            {
                int c = BitConverter.ToInt32(static_data, readCount);
                readCount += sizeof(int);

                return c;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }
        }

        public double ReadDouble()
        {
            try
            {
                double c = BitConverter.ToDouble(static_data, readCount);
                readCount += sizeof(double);

                return c;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }
        }

        public string ReadString()
        {
            try
            {
                short length = ReadShort();
                string c = Encoding.UTF8.GetString(static_data, readCount, length);
                readCount += length;

                return c;
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Перед чтением необходимо вызвать beginReading");
            }

        }
    }

    public class ServerConnection : Connection
    {
        public ServerConnection(Dispatcher dispatcher, IPAddress ip, int port)
        {
            // Сохраняем информацию о диспечере
            this.dispatcher = dispatcher;
            dispatcher.setConnection(this);

            // Используем протокол UDP (без подтверждения)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Биндимся к любому айпи-адресу данной машины, по указанному порту
            IPEndPoint ourIP = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ourIP);

            // Готовы соединиться с любым айпи-адресом с любого порта
            IPEndPoint hisIP = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epHisIp = (EndPoint)hisIP;

            // Начинаем принимать данные, в rawPacket пишется содержимое пакета
            recieveFrom(ref epHisIp);
        }
    }

    public class ClientConnection : Connection
    {
        private EndPoint serverEP;

        public ClientConnection(Dispatcher dispatcher, IPAddress ip, int port, Packet helloPacket)
        {
            // Сохраняем информацию о диспечере
            this.dispatcher = dispatcher;
            dispatcher.setConnection(this);

            // Используем протокол UDP (без подтверждения)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Указываем айпи и порт сервера
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
            EndPoint epServer = (EndPoint)ipEndPoint;

            this.serverEP = epServer;


            byte[] byteData = helloPacket.ToArray();

            // Отправка серверу первого пакета
            sendTo(byteData, epServer);

            // Готовы слушать пакеты с сервера
            recieveFrom(ref epServer);
        }

        // Отправка пакета
        public void sendTo(Packet message)
        {
            sendTo(message, serverEP);
        }

        public EndPoint getServerAdress()
        {
            return serverEP;
        }
    }

    public abstract class Connection
    {
        // Сетевой сокет
        protected Socket socket;

        // Содержимое пакета в исходном виде (набор байт)
        protected byte[] rawPacket = new byte[1024];//@BUG здесь фиксирована длина, а в классе Packet - не фиксирована

        // Диспечер
        protected Dispatcher dispatcher;

        // Готов получать пакеты от заданного адресата
        public void recieveFrom(ref EndPoint remoteEp)
        {
            try
            {
                socket.BeginReceiveFrom(rawPacket, 0, rawPacket.Length, SocketFlags.None, ref remoteEp, new AsyncCallback(OnReceive), remoteEp);
            }
            catch (SocketException)
            {
                Console.WriteLine("Клиент отсоединился без уведомления => Сервер упал.");//@BUG
            }
        }


        // Отправка пакета
        public void sendTo(byte[] message, EndPoint remoteEp)
        {
            socket.BeginSendTo(message, 0, message.Length, SocketFlags.None, remoteEp, new AsyncCallback(OnSend), remoteEp);
        }

        // Отправка пакета
        public void sendTo(Packet message, EndPoint remoteEp)
        {
            sendTo(message.ToArray(), remoteEp);
        }

        // Событие при получении пакета
        protected void OnReceive(IAsyncResult ar)
        {
            // Переменные для получения айпи-адреса отправителя (Экий АОН)
            IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = (EndPoint)ipeSender;

            // Сообщаем сокету о том, что пакет получен, в epSender - адрес отправителя
            socket.EndReceiveFrom(ar, ref epSender);

            // Набор байтов rawPacket превращаем в удобное представление пакета - recieved
            Packet recieved = new Packet(rawPacket);

            // Передача управления диспечеру
            dispatcher.onRecieve(recieved, epSender);

            // Продолжаем принимать сообщения от данного клиента
            recieveFrom(ref epSender);
        }

        // Событие при отправке пакета
        protected void OnSend(IAsyncResult ar)
        {
            // Пытаемся отправить пакет
            socket.EndSend(ar);

            // Передача управления диспечеру
            dispatcher.onSend();
        }
    }

    // Интерфейс диспечера (прототип класса, обрабатывающего сетевые пакеты)
    public abstract class Dispatcher
    {
        protected Connection connection;

        public void setConnection(Connection connection)
        {
            this.connection = connection;
        }

        public abstract void onRecieve(Packet packet, EndPoint sender);
        public abstract void onSend();
    }
}
