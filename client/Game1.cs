using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
using Networking;

namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        KeyboardState keyboard;
        MouseState mouse;
        Tank tank;

        Aim aim;
        TextSprite fpsText;

        int frames;
        int milliseconds;

        Chat chat;
        GameTime time = new GameTime();

        ClientConnection connection;
        System.Net.EndPoint server;
        Protocol protocol;
        uint client_id;

        Dictionary<uint, ClientInfo> clients;
        CollisionManager collisionManager;

        SpritesRenderer spritesRenderer;

        bool isClickLeft = false;
		bool isClickRight = false;
        double thisTime = 0.0;
		HealthGui health;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            //graphics.ToggleFullScreen(); // ????????????? ?????

            this.Window.Title = "JATWa! v0.5.3";

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>

        protected override void Initialize()
        {
            // ????????????? ????????? - ????? ????????? ??????? ???????
            SpritesRenderer.Init(GraphicsDevice);
            spritesRenderer = new SpritesRenderer();

            // ?????????
            GraphSprite background = new GraphSprite();
            background.loadTextureFromFile("jatwa_map_table.png");
            background.visible = true;

            // ?????? ???????
            clients = new Dictionary<uint, ClientInfo>();

            // ?????????? ????????????
            collisionManager = new CollisionManager(clients);

            // ???????? ???? ??????
            tank = new Tank(Tank.Type.Green, this.collisionManager);
            tank.x = 400;
            tank.y = 400;

            // ???????? ????-????????? ??? ????????? ????? ?????
            aim = new Aim(tank);

            // ?????????? ??? ???????? ??????
            frames = 0;
            milliseconds = 0;

            // ????? ?????? ? FPS
            fpsText = new TextSprite(Font.LucidaWhite14);
            fpsText.x = 730;
            fpsText.y = 10;
            fpsText.visible = true;

            EventInput.EventInput.Initialize(this.Window);
            EventInput.EventInput.CharEntered += new EventInput.CharEnteredHandler(EventInput_CharEntered);
            EventInput.EventInput.KeyDown += new EventInput.KeyEventHandler(EventInput_KeyDown);
            chat = new Chat(new Point(20, 550), new Point(20, 530));

            // ??????? ???
            protocol = new Protocol();

            protocol.OnWelcome += new EventHandler<Protocol.WelcomeStruct>(protocol_OnWelcome);
            protocol.OnPosition += new EventHandler<Protocol.PositionStruct>(protocol_OnPosition);
            protocol.OnMessageToClient += new EventHandler<Protocol.MessageToClientStruct>(protocol_OnMessageToClient);
            protocol.OnList += new EventHandler<Protocol.ListStruct>(protocol_OnList);
            protocol.OnShoot += new EventHandler<Protocol.ShootStruct>(protocol_OnShoot);
			protocol.OnHit += new EventHandler<Protocol.HitStruct>(protocol_OnHit);
            protocol.OnDead += new EventHandler<Protocol.DeadStruct>(protocol_OnDead);

            // ??????????? ? ???????
            connection = new ClientConnection(protocol, System.Net.IPAddress.Parse("81.200.123.132"), 1200, Protocol.FormFirstPacket("u7"));
            server = connection.getServerAdress();


            collisionManager.protocol = protocol;//@TMP
            collisionManager.server = server;//@TMP

			health = new HealthGui(800 - 190, 600 - 130);

            base.Initialize();
        }

        void protocol_OnDead(object sender, Protocol.DeadStruct e)
        {
            if (e.client_id == client_id)
                this.tank.visible = false;
            else
                this.clients[e.client_id].tank.visible = false;
        }

        void protocol_OnShoot(object sender, Protocol.ShootStruct e)
        {
            //@TMP
            if (e.client_id != this.client_id)//@BUGFIX ??-?? ????, ??? ?? ?????? ??????? ??????????? ??????? ???????
            {
                if (e.weapon == Protocol.ShootStruct.Weapon.RocketStart)
                    this.clients[e.client_id].tank.launchMissle();
                else if (e.weapon == Protocol.ShootStruct.Weapon.GunStart)
                    this.clients[e.client_id].tank.startShooting();
                else if (e.weapon == Protocol.ShootStruct.Weapon.GunStop)
                    this.clients[e.client_id].tank.stopShooting();
            }
        }
		
		void protocol_OnHit(object sender, Protocol.HitStruct e)
        {
            if (e.client_id == client_id)
                this.tank.hit(e.damage);
            else
                this.clients[e.client_id].tank.hit(e.damage);
        }

        void EventInput_CharEntered(object sender, EventInput.CharacterEventArgs e)
        {
            if (chat.inputMode)
            {
                // ??????????? ??????? ?? win1251 ? UTF8
                short outputChar = (short)e.Character;
                if (outputChar >= 128) outputChar += (short)848;

                if (outputChar >= 32) // ???????? ???????????
                    chat.inputMessage += (char)outputChar;
            }
        }

        void EventInput_KeyDown(object sender, EventInput.KeyEventArgs e)
        {
            // ?????? ??????? backspace
            if (e.KeyCode == Keys.Back && chat.inputMode)
                chat.inputMessage = chat.inputMessage.Substring(0, (chat.inputMessage.Length - 1 < 0 ? chat.inputMessage.Length : chat.inputMessage.Length - 1));
        }

        void protocol_OnList(object sender, Protocol.ListStruct e)
        {
            // ????????? ?? ????? ????????
            foreach (uint id in e.clients.Keys)
            {
                if (!this.clients.ContainsKey(id) && id != client_id)
                {
                    Tank tank = new Tank(Tank.Type.Green, new NoCollisionManager(this.clients, this.tank));
                    tank.ClientId = id;

                    this.clients[id] = new ClientInfo();
                    this.clients[id].nick = e.clients[id];
                    this.clients[id].tank = tank;
                    this.clients[id].tank.x = 400;
                    this.clients[id].tank.y = 400;
                }
            }

            // ??????? ?????? ????????
            Dictionary<uint, ClientInfo> c = new Dictionary<uint, ClientInfo>(this.clients);

            foreach (uint id in c.Keys)
            {
                if (!e.clients.ContainsKey(id))
                {
                    this.clients[id].tank.visible = false;
                    this.clients.Remove(id);
                }
            }
        }



        void protocol_OnMessageToClient(object sender, Protocol.MessageToClientStruct e)
        {
            Console.WriteLine("{0}: {1}", e.nick, e.message);
            chat.addMessage(e.nick, e.message);
        }

        void protocol_OnPosition(object sender, Protocol.PositionStruct e)
        {
            if (e.id == client_id)
            {
                tank.x = e.x;
                tank.y = e.y;
                tank.tankRotation = (float)e.angle;
                tank.turretRotation = (float)e.angle_turret;
            }
            else
            {
                this.clients[e.id].tank.x = e.x;
                this.clients[e.id].tank.y = e.y;
                this.clients[e.id].tank.tankRotation = (float)e.angle;
                this.clients[e.id].tank.turretRotation = (float)e.angle_turret;
            }
        }

        void protocol_OnWelcome(object sender, Protocol.WelcomeStruct e)
        {
            this.client_id = e.id;
            this.tank.ClientId = e.id; //@REFACTOR
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            bool rotated = false;

            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

            // ??????????? ????
            if (mouse.X != Window.ClientBounds.Width / 2 || mouse.Y != Window.ClientBounds.Height / 2)
            {
                aim.relative_x += mouse.X - Window.ClientBounds.Width / 2;
                aim.relative_y += mouse.Y - Window.ClientBounds.Height / 2;

                rotated = true;
            }

            if (!chat.inputMode)
            {
                if (keyboard.IsKeyDown(Keys.W))
                {
                    tank.Move(-1);
                    aim.updateCoords();
                }

                if (keyboard.IsKeyDown(Keys.S))
                {
                    tank.Move(1);
                    aim.updateCoords();
                }

                if (keyboard.IsKeyDown(Keys.A))
                {
                    tank.tankRotation--;
                    if (!rotated)
                        aim.rotate(-1);
                }

                if (keyboard.IsKeyDown(Keys.D))
                {
                    tank.tankRotation++;
                    if (!rotated)
                        aim.rotate(1);
                }

                if (keyboard.IsKeyDown(Keys.Y))
                {
                    chat.inputMode = true;
                }
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.Escape))
                {
                    chat.inputMode = false;
                    chat.getInputAndHide();
                }

                if (keyboard.IsKeyDown(Keys.Enter))
                {
                    chat.inputMode = false;
                    Protocol.MessageToServerStruct s = new Protocol.MessageToServerStruct();
                    s.adress = server;
                    s.client_id = client_id;
                    s.message = chat.getInputAndHide();
                    if (s.message.Length > 0)
                        protocol.SendMessageToServer(s);
                }
            }

            if (this.IsActive)
            {
                // ????????
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    if (!isClickLeft && gameTime.TotalRealTime.TotalMilliseconds - thisTime >= 300)
                    {
                        tank.launchMissle();

                        // ???????? ????????? ?? ?????? ? ??????? ??????
                        Protocol.ShootStruct s = new Protocol.ShootStruct();
                        s.adress = server;
                        s.client_id = client_id;
                        s.weapon = Protocol.ShootStruct.Weapon.RocketStart;
                        protocol.SendShoot(s);

                        thisTime = gameTime.TotalRealTime.TotalMilliseconds;
                        isClickLeft = true;
                    }
                }

                if (mouse.LeftButton == ButtonState.Released && isClickLeft)
                {
                    isClickLeft = false;
                }

                if (mouse.RightButton == ButtonState.Pressed)
                {
                    if (!isClickRight)
                    {
                        tank.startShooting();

                        // ???????? ????????? ?? ?????? ? ?????? ???????? ?? ???????
                        Protocol.ShootStruct s = new Protocol.ShootStruct();
                        s.adress = server;
                        s.client_id = client_id;
                        s.weapon = Protocol.ShootStruct.Weapon.GunStart;
                        protocol.SendShoot(s);
                    }

                    isClickRight = true;
                }


                if (mouse.RightButton == ButtonState.Released && isClickRight)
                {
                    // ???????? ????????
                    isClickRight = false;

                    tank.stopShooting();

                    // ???????? ????????? ?? ?????? ? ????? ???????? ?? ???????
                    Protocol.ShootStruct s = new Protocol.ShootStruct();
                    s.adress = server;
                    s.client_id = client_id;
                    s.weapon = Protocol.ShootStruct.Weapon.GunStop;
                    protocol.SendShoot(s);
                }
            }

            chat.tick(gameTime);


            tank.aimTurret(aim);
            
            if (this.IsActive)
                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);

            // ???????? ????????? ? ???????????
            if (client_id > 0) //@BUGFIX
            {
                Protocol.PositionStruct s = new Protocol.PositionStruct();
                s.id = client_id;
                s.adress = server;
                s.angle = tank.tankRotation;
                s.angle_turret = tank.turretRotation;
                s.x = (uint)tank.x;
                s.y = (uint)tank.y;
                protocol.SendPosition(s);
            }
            // END ???????? ????????? ? ???????????

            health.health = tank.health;

            // ???????? ????????? ? ?????? ?????
            if (health.health <= 0)
            {
                this.Exit();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // ??????? ??????
            graphics.GraphicsDevice.Clear(Color.White);

            // ????? ???? ????????
            this.spritesRenderer.Render();

            // ??????????? ??????????
            base.Draw(gameTime);

            // ??????? ? ????? FPS
            if (gameTime.TotalRealTime.TotalMilliseconds - milliseconds >= 1000)
            {
                fpsText.text = "FPS: " + ((int)(frames * 1000 / (gameTime.TotalRealTime.TotalMilliseconds - milliseconds))).ToString();                
                frames = 0;
                milliseconds = (int)gameTime.TotalRealTime.TotalMilliseconds;
            }
            frames++;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Protocol.DisconnectStruct s = new Protocol.DisconnectStruct();
            s.adress = server;
            s.id = client_id;
            protocol.SendDisconnect(s);
            
            base.OnExiting(sender, args);
        }
    }

    public class ClientInfo
    {
        public string nick;
        public Tank tank;
    }
}
