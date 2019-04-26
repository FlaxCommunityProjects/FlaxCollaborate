using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using FlaxEditor.SceneGraph;
using FlaxEngine;

namespace MultiUsersEditingPlugin
{
    public class ServerSession : EditingSession
    {
        private struct SocketUser
        {
            public int Id;
            public TcpClient Socket;
            public BinaryReader Reader;
            public BinaryWriter Writer;
        }

        private bool _running;
        private TcpListener _server;
        private IPAddress _address = IPAddress.Parse("127.0.0.1");
        private List<SocketUser> _socketUsers = new List<SocketUser>();
        
        private Thread _thread;

        public override bool IsHosting => true;

        
        public override bool Start(SessionSettings settings)
        {
            try
            {
                _server = new TcpListener(_address, settings.Port);
                _server.Start();

                _thread = new Thread(() =>
                {
                    List<SocketUser> _usersToDelete = new List<SocketUser>();
                    bool _activity = false;
                    _running = true;
                    while (_running)
                    {
                        if (_server.Pending())
                        {
                            SocketUser newSocketUser = new SocketUser();
                            newSocketUser.Socket = _server.AcceptTcpClient();
                            newSocketUser.Reader = new BinaryReader(newSocketUser.Socket.GetStream());
                            newSocketUser.Writer = new BinaryWriter(newSocketUser.Socket.GetStream());

                            String username = newSocketUser.Reader.ReadString();
                            
                            var id = CreateId(username);
                            
                            if (id == -1) // Username is already taken -> send the info and drop the user 
                            {
                                newSocketUser.Writer.Write(false);
                            }
                            else // User accepted
                            {
                                newSocketUser.Id = id;
                                _socketUsers.Add(newSocketUser);
                                newSocketUser.Writer.Write(newSocketUser.Id);
                                
                                EditingUser newEditUser = new EditingUser(id, username, false);
                                Users.Add(newEditUser);
                                SendPacket(id, new UserConnectedPacket(id, username, false));
                                Debug.Log("New user connected !");
                            }
                        }

                        foreach (var user in _socketUsers)
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
                                //Debug.Log("Incoming packet type : " + classname);
                                
                                Packet p = (Packet)Activator.CreateInstance(
                                    PacketTypeManager.SubclassTypes.First((t) => t.Name.Equals(classname)));
                                p.Author = user.Id;
                                p.Read(user.Reader);

                                if (broadcasted)
                                {
                                    ThreadPool.QueueUserWorkItem((state) => { SendPacket(senderId, p); });
                                }
                            }
                        }

                        if (_usersToDelete.Count != 0)
                        {
                            _usersToDelete.ForEach((user) => _socketUsers.Remove(user));
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

        public override bool SendPacket(Packet packet)
        {
            return SendPacket(-1, packet);
        }

        public bool SendPacket(int senderId, Packet packet)
        {
            string s = PacketTypeManager.SubclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name;

            foreach (var user in _socketUsers)
            {
                if (user.Socket != null &&
                    user.Id != packet.Author)
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
        
        public override void Close()
        {
            _running = false;
            _server.Stop();
            foreach (var user in _socketUsers)
            {
                user.Socket?.Close();
            }
        }

        private int CreateId(String username)
        {
            int id = username.GetHashCode();

            if (GetUserById(id) != null)
                return -1;

            return id;
        }
    }
}