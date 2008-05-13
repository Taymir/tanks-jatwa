using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Networking
{
    // ����� �������� ������ - ��������� "�����������" ���������� � ����� � "�������������" �� ����
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

        // ������ � �����
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

        // ������ �� ������
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
                throw new Exception("����� ������� ���������� ������� beginReading");
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
                throw new Exception("����� ������� ���������� ������� beginReading");
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
                throw new Exception("����� ������� ���������� ������� beginReading");
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
                throw new Exception("����� ������� ���������� ������� beginReading");
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
                throw new Exception("����� ������� ���������� ������� beginReading");
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
                throw new Exception("����� ������� ���������� ������� beginReading");
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
                throw new Exception("����� ������� ���������� ������� beginReading");
            }

        }
    }

    public class ServerConnection : Connection
    {
        public ServerConnection(Dispatcher dispatcher, IPAddress ip, int port)
        {
            // ��������� ���������� � ���������
            this.dispatcher = dispatcher;
            dispatcher.setConnection(this);

            // ���������� �������� UDP (��� �������������)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // �������� � ������ ����-������ ������ ������, �� ���������� �����
            IPEndPoint ourIP = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ourIP);

            // ������ ����������� � ����� ����-������� � ������ �����
            IPEndPoint hisIP = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epHisIp = (EndPoint)hisIP;

            // �������� ��������� ������, � rawPacket ������� ���������� ������
            recieveFrom(ref epHisIp);
        }
    }

    public class ClientConnection : Connection
    {
        private EndPoint serverEP;

        public ClientConnection(Dispatcher dispatcher, IPAddress ip, int port, Packet helloPacket)
        {
            // ��������� ���������� � ���������
            this.dispatcher = dispatcher;
            dispatcher.setConnection(this);

            // ���������� �������� UDP (��� �������������)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // ��������� ���� � ���� �������
            IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
            EndPoint epServer = (EndPoint)ipEndPoint;

            this.serverEP = epServer;


            byte[] byteData = helloPacket.ToArray();

            // �������� ������� ������� ������
            sendTo(byteData, epServer);

            // ������ ������� ������ � �������
            recieveFrom(ref epServer);
        }

        // �������� ������
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
        // ������� �����
        protected Socket socket;

        // ���������� ������ � �������� ���� (����� ����)
        protected byte[] rawPacket = new byte[1024];//@BUG ����� ����������� �����, � � ������ Packet - �� �����������

        // ��������
        protected Dispatcher dispatcher;

        // ����� �������� ������ �� ��������� ��������
        public void recieveFrom(ref EndPoint remoteEp)
        {
            try
            {
                socket.BeginReceiveFrom(rawPacket, 0, rawPacket.Length, SocketFlags.None, ref remoteEp, new AsyncCallback(OnReceive), remoteEp);
            }
            catch (SocketException)
            {
                Console.WriteLine("������ ������������ ��� ����������� => ������ ����.");//@BUG
            }
        }


        // �������� ������
        public void sendTo(byte[] message, EndPoint remoteEp)
        {
            socket.BeginSendTo(message, 0, message.Length, SocketFlags.None, remoteEp, new AsyncCallback(OnSend), remoteEp);
        }

        // �������� ������
        public void sendTo(Packet message, EndPoint remoteEp)
        {
            sendTo(message.ToArray(), remoteEp);
        }

        // ������� ��� ��������� ������
        protected void OnReceive(IAsyncResult ar)
        {
            // ���������� ��� ��������� ����-������ ����������� (���� ���)
            IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = (EndPoint)ipeSender;

            // �������� ������ � ���, ��� ����� �������, � epSender - ����� �����������
            socket.EndReceiveFrom(ar, ref epSender);

            // ����� ������ rawPacket ���������� � ������� ������������� ������ - recieved
            Packet recieved = new Packet(rawPacket);

            // �������� ���������� ���������
            dispatcher.onRecieve(recieved, epSender);

            // ���������� ��������� ��������� �� ������� �������
            recieveFrom(ref epSender);
        }

        // ������� ��� �������� ������
        protected void OnSend(IAsyncResult ar)
        {
            // �������� ��������� �����
            socket.EndSend(ar);

            // �������� ���������� ���������
            dispatcher.onSend();
        }
    }

    // ��������� ��������� (�������� ������, ��������������� ������� ������)
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
