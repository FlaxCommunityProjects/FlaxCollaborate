using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiUsersEditingPlugin.MultiUsersEditingPlugin
{
    public class ServerSession : EditingSession
    {
        private struct Client
        {
            public TcpClient Socket;
            public BinaryReader Reader;
            public BinaryWriter Writer;
        }

        private bool Running;
        private TcpListener Server;
        private IPAddress Address = IPAddress.Parse("127.0.0.1");
        private Client[] Clients = new Client[10];
        private Thread Thread;

        public override bool Start(String host, int port)
        {
            IsHosting = true;
            try
            {
                Server = new TcpListener(Address, port);
                Server.Start();

                Thread = new Thread(() =>
                {
                    Running = true;
                    while (Running)
                    {
                        if (Server.Pending())
                        {
                            for (int i = 0; i < Clients.Length; i++)
                            {
                                if (Clients[i].Socket == null)
                                {
                                    Clients[i].Socket = Server.AcceptTcpClient();
                                    Clients[i].Reader = new BinaryReader(Clients[i].Socket.GetStream());
                                    Clients[i].Writer = new BinaryWriter(Clients[i].Socket.GetStream());
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < Clients.Length; i++)
                        {
                            if (Clients[i].Socket == null)
                            {
                            }
                            else if (!Clients[i].Socket.Connected)
                            {
                                Clients[i].Socket = null;
                            }
                            else if (Clients[i].Socket.Available != 0)
                            {
                                bool broadcasted = Clients[i].Reader.ReadBoolean();
                                String s = Clients[i].Reader.ReadString();
                                Packet p = (Packet) Activator.CreateInstance(
                                    PacketTypeManager.subclassTypes.First((t) => t.Name.Equals(s)));
                                p.Author = Clients[i].Socket.GetHashCode();
                                p.Read(Clients[i].Reader);

                                if (broadcasted)
                                {
                                    ThreadPool.QueueUserWorkItem((state) => { SendPacket(p); });
                                }
                            }
                        }
                    }
                });

                Thread.Start();
            }
            catch (Exception e)
            {
                //Debug.LogError(e.ToString());
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public override bool SendPacket(Packet packet)
        {
            String s = PacketTypeManager.subclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name;

            foreach (var client in Clients)
            {
                if (client.Socket != null &&
                    client.Socket.GetHashCode() != packet.Author)
                {
                    lock (client.Socket)
                    {
                        try
                        {
                            client.Writer.Write(s);
                            packet.Write(client.Writer);
                        }
                        catch (Exception e)
                        {
                            //Debug.LogError(e.ToString());
                            Console.WriteLine(e);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Close()
        {
            Running = false;
        }
    }
}