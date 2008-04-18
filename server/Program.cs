using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Сервер запущен...");

            //Connection connection = new Connection(2200);

            waitToExit();
            return;
        }

        static void waitToExit()
        {
            Console.WriteLine("Нажмите любую клавишу для остановки сервера...");

            Console.ReadKey(false);
        }
    }
}
/*
namespace Networking
{



    // Пакет, передоваемый по сети (комманда / сообщение)
    class Packet
    {
        // Варианты команд
        public enum Command
        {
            // Сообщения от клиента к серверу
            Connect,        // Подключение к игре
            Disconnect,     // Отключение от игры

            Chat,           // Сообщение в чат

            Move,           // Движение танка

            // Сообщения от сервера к клиенту
            Welcome,        // Разрешение подключения
            Birth,          // Появление танка
            Position,       // Расположение танка на поле

            Null            // Команда не определена
        }


        // Содержимое пакета
        public Command command; // Комманда

        // Конец содержимого пакета

        // Создание пустого пакета
        public Packet()
        {
            // Здесь все поля просто иницилизируются пустыми значениями
            this.command = Command.Null;
        }

        // Преобразование пакета из набора байт во внутренее представление, с которым удобно работать
        public Packet(byte[] rawBytes)
        {
            // Первые 16 байт (8 символов в УТФ8) - имя клиента
            this.user = Encoding.UTF8.GetString(rawBytes, 0, 16);       // Получить строку из rawBytes начиная с 0го байта, длиной 16 байт

            // Следующие 4 байта - номер команды (из списка Command)
            this.command = (Command)BitConverter.ToInt32(rawBytes, 16); // Получить четырехбайтовый идентификатор номера команды из rawBytes начиная с 16го байта
        }

        // Преобразование пакета из внутреннего представления в набор байт (для отправки по сети)
        public byte[] ToByte()
        {
            // Создаем динамический массив (список) байтов
            List<byte> result = new List<byte>();

            // Добавляем 8 байт - имя клиента
            result.AddRange(Encoding.UTF8.GetBytes(this.user.Substring(0, Math.Min(8, this.user.Length))));

            // Добавляем 4 байта - идентификатор команды
            result.AddRange(BitConverter.GetBytes((int)this.command));

            // Возвращаем результат в виде [обычного] массива байт
            return result.ToArray();
        }


    }

    // Класс сетевого соединения
    class Connection
    {
        // Сетевой сокет
        protected Socket socket;

        // Содержимое пакета в исходном виде (набор байт)
        protected byte[] rawPacket = new byte[1024];

        // Подключение
        public Connection(int port)
        {
            try
            {
                // Используем протокол UDP (без подтверждения)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Биндимся к любому айпи-адресу данной машины, по указанному порту
                IPEndPoint ourIP = new IPEndPoint(IPAddress.Any, port);
                socket.Bind(ourIP);

                // Готовы соединиться с любым айпи-адресом с любого порта
                IPEndPoint hisIP = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epHisIp = (EndPoint)hisIP;

                // Начинаем принимать данные, в rawPacket пишется содержимое пакета
                socket.BeginReceiveFrom(rawPacket, 0, rawPacket.Length, SocketFlags.None, ref epHisIp, new AsyncCallback(OnReceive), null);
            }
            catch (Exception e)
            {
                throw e; //@TODO Свой эксепшн должен быть
            }
        }

        // Событие при получении пакета
        protected void OnReceive(IAsyncResult ar)
        {
            try
            {
                // Переменные для получения айпи-адреса отправителя (Экий АОН)
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                // Сообщаем сокету о том, что пакет получен, в epSender - адрес отправителя
                socket.EndReceiveFrom(ar, ref epSender);

                // Набор байтов rawPacket превращаем в удобное представление пакета - recieved
                Packet recieved = new Packet(rawPacket);

                switch (recieved.command)
                {
                    case Packet.Command.hello:
                        Console.WriteLine(recieved.user + "Said HELLO to us =)");
                        break;

                    //@TODO
                }


            }
            catch (Exception e)
            {
                throw e; //@TODO Свой эксепшн должен быть
            }
        }

        // Событие при отправке пакета
        protected void OnSend(IAsyncResult ar)
        {
            try
            {
                // Пытаемся отправить пакет
                socket.EndSend(ar);
            }
            catch (Exception e)
            {
                throw e; //@TODO Свой эксепшн должен быть
            }
        }

    }
}*/

