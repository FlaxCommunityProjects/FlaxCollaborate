using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlaxEngine;

namespace MultiUsersEditingPlugin
{
    public class ServerSession : IEditingSession
    {
        private struct Client
        {
            public TcpClient Socket;
            public BinaryReader Reader;
            public BinaryWriter Writer;
        }

        private bool _running;
        private TcpListener _server;
        private IPAddress _address = IPAddress.Parse("127.0.0.1");
        private Client[] _clients = new Client[10];
        private Thread _thread;

        public bool IsHosting => true;

        public bool Start(SessionSettings settings)
        {
            try
            {
                _server = new TcpListener(_address, settings.Port);
                _server.Start();

                _thread = new Thread(() =>
                {
                    _running = true;
                    while (_running)
                    {
                        if (_server.Pending())
                        {
                            for (int i = 0; i < _clients.Length; i++)
                            {
                                if (_clients[i].Socket == null)
                                {
                                    _clients[i].Socket = _server.AcceptTcpClient();
                                    _clients[i].Reader = new BinaryReader(_clients[i].Socket.GetStream());
                                    _clients[i].Writer = new BinaryWriter(_clients[i].Socket.GetStream());
                                    Debug.Log("New user connected !");
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < _clients.Length; i++)
                        {
                            if (_clients[i].Socket == null)
                            {
                            }
                            else if (!_clients[i].Socket.Connected)
                            {
                                _clients[i].Socket = null;
                            }
                            else if (_clients[i].Socket.Available != 0)
                            {
                                bool broadcasted = _clients[i].Reader.ReadBoolean();
                                String classname = _clients[i].Reader.ReadString();
                                Debug.Log("Incoming packet type : " + classname);
                                Packet p = (Packet)Activator.CreateInstance(
                                    PacketTypeManager.SubclassTypes.First((t) => t.Name.Equals(classname)));
                                p.Author = _clients[i].Socket.GetHashCode();
                                p.Read(_clients[i].Reader);

                                if (broadcasted)
                                {
                                    ThreadPool.QueueUserWorkItem((state) => { SendPacket(p); });
                                }
                            }
                            else
                            {
                                Thread.Yield();
                            }
                        }
                    }
                });

                _thread.IsBackground = true;
                _thread.Start();

                Debug.Log("Session server launched !");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return false;
            }

            return true;
        }

        public bool SendPacket(Packet packet)
        {
            string s = PacketTypeManager.SubclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name;

            foreach (var client in _clients)
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
                            Debug.LogError(e.ToString());
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Close()
        {
            _running = false;
        }
    }
}