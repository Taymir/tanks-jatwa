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

            Console.WriteLine("Сервер запущен. Введите 'quit' для завершения его работы.");
            while (true)
            {
                Command cmd = new Command(Console.ReadLine());

                if (cmd.command == "quit")
                {
                    Console.WriteLine("Сервер выключается...");
                    break;
                }
                else if (cmd.command == "chat")
                {
                    // Отправить сообщение всем клиентам
                    SendMessageToAllClients("server", cmd.parameter, 0);
                }
                else if (cmd.command == "help")
                {
                    // Список всех комманд
                    Console.WriteLine("chat [message]\tОтправляет сообщение в чат");
                    Console.WriteLine("help\t\tВыводит список всех доступных команд");
                    Console.WriteLine("list\t\tВыводит список всех подключенных клиентов");
                    Console.WriteLine("quit\t\tЗавершает работу сервера");
                }
                else if (cmd.command == "list")
                {
                    Console.WriteLine("Список подключенных клиентов: ");
                    foreach (uint id in clients.Keys)
                    {
                        Console.WriteLine("{0} [{1}]",clients[id], id.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Команда '{0}' не опознана. Введите help для списка всех команд.", cmd.command);
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
            // Очередная пересылка сообщения всем клиентам //@TODO много одинаковых методов массовой рассылки, extract method
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
                // Отражаем выход клиента в консоли
                Console.WriteLine("[{0}] Пользователь {1} покинул нас.", e.id, clients[e.id].nick);

                // Убираем клиента из списка клиентов
                clients.Remove(e.id);

                // Отправить всем обновленный список игроков
                ListClients();
            }
        }

        void protocol_OnPosition(object sender, Protocol.PositionStruct e)
        {
            //Console.WriteLine("[{0}] Танк по координатам: {1}, {2}", e.id, e.x, e.y); //@TMP
            // Разослать всем клиентам информацию о позиции танка
            foreach (uint id in clients.Keys)//@TMP, OPTIMIZE
            {
                if (id == e.id) continue;

                e.adress = clients[id].adress;
                ((Protocol)sender).SendPosition(e);
            }
        }

        

        // Присоединение клиента к серверу
        private void protocol_OnConnect(object sender, Protocol.ConnectStruct e)
        {
            // Добавление пользователя
            ClientInfo client = new ClientInfo();
            client.adress = e.adress;
            client.nick = e.nick;
            clients.Add(client_id, client);
            Console.WriteLine("[{0}] Пользователь {1} присоединился к нам.", client_id, client.nick);

            // Отправляем сообщение с приветствием
            Protocol.WelcomeStruct s = new Protocol.WelcomeStruct();
            s.adress = e.adress;
            s.id = client_id;
            ((Protocol)sender).SendWelcome(s);

            // Отправить сообщение с координатами для появления танка
            Birth(client_id);

            // Отправить всем обновленный список игроков
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

        // Отправить список всех подключенных клиентов клиенту с заданным id
        private void ListClients(uint client_id)
        {
            Protocol.ListStruct s = new Protocol.ListStruct();
            s.adress = this.clients[client_id].adress;

            s.clients = new Dictionary<uint, string>();
            foreach (uint id in this.clients.Keys)
                s.clients[id] = this.clients[id].nick;

            protocol.SendList(s);
        }

        // Отправить список всех подключенных клиентов всем клиентам
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

        // При получении сообщений с клиентов, разослать их
        private void protocol_OnMessageToServer(object sender, Protocol.MessageToServerStruct e)
        {
            uint client_id = e.client_id;
            string message = e.message;

            // выясняем никнейм отправителя сообщения
            string nick = clients[client_id].nick;

            SendMessageToAllClients(nick, message, client_id);
        }

        private void SendMessageToAllClients(string sender_nick, string message, uint sender_id)
        {
            // создаём и заполняем структуру сообщения
            Protocol.MessageToClientStruct s = new Protocol.MessageToClientStruct();

            s.nick = sender_nick;
            s.message = message;


            // перебираем массив пользователей, которым нужно переслать сообщение
            foreach (uint id in clients.Keys)
            {
                s.adress = clients[id].adress;
                // отослать сообщение [никнейм: сообщение]
                protocol.SendMessageToClient(s);
            } // end foreach
            Console.WriteLine("[{0}] {1} говорит: {2}", sender_id, sender_nick, message);
        }

        // Говорим клиенту где родиться
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

            Console.WriteLine("[{0}] Танк появился по координатам: {1}, {2}", id, s.x, s.y);
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
