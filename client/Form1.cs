using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Networking;

namespace game
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Packet packet1 = new Packet();

            packet1.Write((int)Protocol.Type.Connect);
            packet1.Write("ОНОТОЛЕЙ");

            Protocol bla = new Protocol();
            bla.OnConnect += bla_OnConnect;
            bla.OnWelcome += bla_OnWelcome;

            ServerConnection server = new ServerConnection(bla, IPAddress.Any, 23334);

            ClientConnection client = new ClientConnection(bla, IPAddress.Parse("127.0.0.1"), 23334, packet1);
        }

        void bla_OnWelcome(object sender, Protocol.WelcomeStruct e)
        {
            MessageBox.Show("Меня попреведствовали!");
        }

        void bla_OnConnect(object sender, Protocol.ConnectStruct e)
        {
            Protocol.WelcomeStruct s = new Protocol.WelcomeStruct();
            s.adress = e.adress;
            s.id = 1234;

            ((Protocol)sender).SendWelcome(s);
        }
    }
}