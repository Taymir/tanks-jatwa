using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Networking;

namespace server
{
    class Server
    {
        private Protocol protocol;
        private ServerConnection connection;
        public const int port = 1200;
        public IPAddress ip = IPAddress.Any;
        
        private uint client_id = 1;
        private Dictionary<uint, ClientInfo> clients;

        public Server()
        {
            clients = new Dictionary<uint, ClientInfo>();
            protocol = new Protocol();

            protocol.OnPosition += new EventHandler<Protocol.PositionStruct>(protocol_OnPosition);
            protocol.OnConnect += new EventHandler<Protocol.ConnectStruct>(protocol_OnConnect);
            protocol.OnDisconnect += new EventHandler<Protocol.DisconnectStruct>(protocol_OnDisconnect);
            protocol.OnMessageToServer += new EventHandler<Protocol.MessageToServerStruct>(protocol_OnMessageToServer);
            protocol.OnShoot += new EventHandler<Protocol.ShootStruct>(protocol_OnShoot);
			protocol.OnHit += new EventHandler<Protocol.HitStruct>(protocol_OnHit);
            protocol.OnDead += new EventHandler<Protocol.DeadStruct>(protocol_OnDead);

            connection = new ServerConnection(protocol, ip, port);

            Console.WriteLine("������ �������. ������� 'quit' ��� ���������� ��� ������.");
            while (true)
            {
                Command cmd = new Command(Console.ReadLine());

                if (cmd.command == "quit")
                {
                    Console.WriteLine("������ �����������...");
                    break;
                }
                else if (cmd.command == "chat")
                {
                    // ��������� ��������� ���� ��������
                    SendMessageToAllClients("server", cmd.parameter, 0);
                }
                else if (cmd.command == "help")
                {
                    // ������ ���� �������
                    Console.WriteLine("chat [message]\t���������� ��������� � ���");
                    Console.WriteLine("help\t\t������� ������ ���� ��������� ������");
                    Console.WriteLine("list\t\t������� ������ ���� ������������ ��������");
                    Console.WriteLine("quit\t\t��������� ������ �������");
                }
                else if (cmd.command == "list")
                {
                    Console.WriteLine("������ ������������ ��������: ");
                    foreach (uint id in clients.Keys)
                    {
                        Console.WriteLine("{0} [{1}]",clients[id], id.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("������� '{0}' �� ��������. ������� help ��� ������ ���� ������.", cmd.command);
                }
            }
        }

        void protocol_OnDead(object sender, Protocol.DeadStruct e)
        {
            foreach (uint id in clients.Keys)//@TMP, OPTIMIZE
            {
                //if (id == e.client_id) continue;

                e.adress = clients[id].adress;
                ((Protocol)sender).SendDead(e);
            }
        }

        void protocol_OnShoot(object sender, Protocol.ShootStruct e)
        {
            // ��������� ��������� ��������� ���� �������� //@TODO ����� ���������� ������� �������� ��������, extract method
            foreach (uint id in clients.Keys)
            {
                e.adress = clients[id].adress;
                ((Protocol)sender).SendShoot(e);
            }
        }

        void protocol_OnDisconnect(object sender, Protocol.DisconnectStruct e)
        {
            if (clients.ContainsKey(e.id))
            {
                // �������� ����� ������� � �������
                Console.WriteLine("[{0}] ������������ {1} ������� ���.", e.id, clients[e.id].nick);

                // ������� ������� �� ������ ��������
                clients.Remove(e.id);

                // ��������� ���� ����������� ������ �������
                ListClients();
            }
        }

        void protocol_OnPosition(object sender, Protocol.PositionStruct e)
        {
            //Console.WriteLine("[{0}] ���� �� �����������: {1}, {2}", e.id, e.x, e.y); //@TMP
            // ��������� ���� �������� ���������� � ������� �����
            foreach (uint id in clients.Keys)//@TMP, OPTIMIZE
            {
                if (id == e.id) continue;

                e.adress = clients[id].adress;
                ((Protocol)sender).SendPosition(e);
            }
        }

        

        // ������������� ������� � �������
        private void protocol_OnConnect(object sender, Protocol.ConnectStruct e)
        {
            // ���������� ������������
            ClientInfo client = new ClientInfo();
            client.adress = e.adress;
            client.nick = e.nick;
            clients.Add(client_id, client);
            Console.WriteLine("[{0}] ������������ {1} ������������� � ���.", client_id, client.nick);

            // ���������� ��������� � ������������
            Protocol.WelcomeStruct s = new Protocol.WelcomeStruct();
            s.adress = e.adress;
            s.id = client_id;
            ((Protocol)sender).SendWelcome(s);

            // ��������� ��������� � ������������ ��� ��������� �����
            Birth(client_id);

            // ��������� ���� ����������� ������ �������
            ListClients();

            client_id++;
        }
		
		private void protocol_OnHit(object sender, Protocol.HitStruct e)
        {
            foreach (uint id in clients.Keys)//@TMP, OPTIMIZE
            {
                //if (id == e.client_id) continue;

                e.adress = clients[id].adress;
                ((Protocol)sender).SendHit(e);
            }
        }

        // ��������� ������ ���� ������������ �������� ������� � �������� id
        private void ListClients(uint client_id)
        {
            Protocol.ListStruct s = new Protocol.ListStruct();
            s.adress = this.clients[client_id].adress;

            s.clients = new Dictionary<uint, string>();
            foreach (uint id in this.clients.Keys)
                s.clients[id] = this.clients[id].nick;

            protocol.SendList(s);
        }

        // ��������� ������ ���� ������������ �������� ���� ��������
        private void ListClients()
        {
            Protocol.ListStruct s = new Protocol.ListStruct();

            s.clients = new Dictionary<uint, string>();
            foreach (uint id in this.clients.Keys)
                s.clients[id] = this.clients[id].nick;

            foreach (uint id in this.clients.Keys)
            {
                s.adress = this.clients[id].adress;
                protocol.SendList(s);
            }
            
        }

        // ��� ��������� ��������� � ��������, ��������� ��
        private void protocol_OnMessageToServer(object sender, Protocol.MessageToServerStruct e)
        {
            uint client_id = e.client_id;
            string message = e.message;

            // �������� ������� ����������� ���������
            string nick = clients[client_id].nick;

            SendMessageToAllClients(nick, message, client_id);
        }

        private void SendMessageToAllClients(string sender_nick, string message, uint sender_id)
        {
            // ������ � ��������� ��������� ���������
            Protocol.MessageToClientStruct s = new Protocol.MessageToClientStruct();

            s.nick = sender_nick;
            s.message = message;


            // ���������� ������ �������������, ������� ����� ��������� ���������
            foreach (uint id in clients.Keys)
            {
                s.adress = clients[id].adress;
                // �������� ��������� [�������: ���������]
                protocol.SendMessageToClient(s);
            } // end foreach
            Console.WriteLine("[{0}] {1} �������: {2}", sender_id, sender_nick, message);
        }

        // ������� ������� ��� ��������
        private void Birth(uint id)
        {
            Random random = new Random();
            Protocol.PositionStruct s = new Protocol.PositionStruct();

            s.adress = clients[id].adress;
            s.id = id;
            s.x = (uint)random.Next(601) + 100;
            s.y = (uint)random.Next(401) + 100;
            s.angle = 0.0;
            s.angle_turret = 0.0;

            Console.WriteLine("[{0}] ���� �������� �� �����������: {1}, {2}", id, s.x, s.y);
            protocol.SendPosition(s);
        }
    }

    public class ClientInfo
    {
        public string nick;
        public EndPoint adress;
        public TankInfo tankInfo;
    }

    public class TankInfo
    {
        public uint x, y;
        public float angleTank;
        public float angleTurrel;
    }

    class Command
    {
        public string command;
        public string parameter;

        public Command()
        {
            command = "";
            parameter = "";
        }

        public Command(string rawCommand)
        {
            int spacePos = rawCommand.IndexOf(' ');
            if (spacePos == -1)
            {
                command = rawCommand;
                parameter = "";
            }
            else
            {
                command = rawCommand.Substring(0, spacePos);
                parameter = rawCommand.Substring(spacePos, rawCommand.Length - spacePos);
            }
        }
    }
}
