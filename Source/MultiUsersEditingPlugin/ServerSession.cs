using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlaxEditor.SceneGraph;
using FlaxEngine;

namespace MultiUsersEditingPlugin
{
    public class ServerSession : IEditingSession
    {
        private struct User
        {
            public int Id;
            public TcpClient Socket;
            public BinaryReader Reader;
            public BinaryWriter Writer;
        }

        private bool _running;
        private TcpListener _server;
        private IPAddress _address = IPAddress.Parse("127.0.0.1");
        private List<User> _users = new List<User>();
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
                    List<User> _usersToDelete = new List<User>();
                    bool _activity = false;
                    _running = true;
                    while (_running)
                    {
                        if (_server.Pending())
                        {
                            User newUser = new User();
                            newUser.Id = _users.Count + 1;
                            newUser.Socket = _server.AcceptTcpClient();
                            newUser.Reader = new BinaryReader(newUser.Socket.GetStream());
                            newUser.Writer = new BinaryWriter(newUser.Socket.GetStream());
                            _users.Add(newUser);
                            newUser.Writer.Write(newUser.Id);
                            SendPacket(new UserConnectedPacket(newUser.Id));
                            Debug.Log("New user connected !");
                            
                        }

                        foreach (var user in _users)
                        {
                            if (!user.Socket.Connected)
                            {
                                _usersToDelete.Add(user);
                                SendPacket(new UserDisconnectedPacket(user.Id));
                            }
                            else if (user.Socket.Available != 0)
                            {
                                _activity = true;
                                int senderId = user.Reader.ReadInt32();
                                bool broadcasted = user.Reader.ReadBoolean();
                                string classname = user.Reader.ReadString();
                                Debug.Log("Incoming packet type : " + classname);
                                Packet p = (Packet)Activator.CreateInstance(
                                    PacketTypeManager.SubclassTypes.First((t) => t.Name.Equals(classname)));
                                p.Author = user.Socket.GetHashCode();
                                p.Read(user.Reader);

                                if (broadcasted)
                                {
                                    ThreadPool.QueueUserWorkItem((state) => { SendPacket(senderId, p); });
                                }
                            }
                        }

                        if (_usersToDelete.Count != 0)
                        {
                            _usersToDelete.ForEach((user) => _users.Remove(user));
                            _usersToDelete.Clear();
                        }
                        
                        if (!_activity)
                            Thread.Yield();
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
            return SendPacket(-1, packet);
        }

        public bool SendPacket(int senderId, Packet packet)
        {
            string s = PacketTypeManager.SubclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name;

            foreach (var user in _users)
            {
                if (user.Socket != null &&
                    user.Socket.GetHashCode() != packet.Author)
                {
                    lock (user.Socket)
                    {
                        try
                        {
                            user.Writer.Write(senderId);
                            user.Writer.Write(s);
                            packet.Write(user.Writer);
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
            _server.Stop();
            foreach (var user in _users)
            {
                user.Socket?.Close();
            }
        }
    }
}