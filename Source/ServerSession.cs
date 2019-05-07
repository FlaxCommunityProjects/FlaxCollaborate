using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

namespace CollaboratePlugin
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
        private List<SocketUser> _socketUsers = new List<SocketUser>();
        private Thread _thread;

        public override bool IsHosting => true;

        public override Task<bool> Start(SessionSettings settings)
        {
            try
            {
                try
                {
                    _server = new TcpListener(IPAddress.Any, settings.Port);
                    _server.Start();
                }
                catch (SocketException e)
                {
                    Debug.LogError("Failed to create the server");
                    Debug.LogException(e);
                    return Task.FromResult(false);
                }

                Users.Add(User = new EditingUser(CreateId(settings.Username), settings.Username,
                    settings.SelectionColor, true));
                _thread = new Thread(() =>
                {
                    List<SocketUser> _usersToDelete = new List<SocketUser>();
                    bool _activity = false;
                    _running = true;
                    while (_running)
                    {
                        // TODO: Heartbeat - to make sure that client disconnections get detected.

                        // If a new user is attempting to connect to the server
                        if (_server.Pending())
                        {
                            _activity = true;
                            SocketUser newSocketUser = new SocketUser();
                            newSocketUser.Socket = _server.AcceptTcpClient();
                            newSocketUser.Reader = new BinaryReader(newSocketUser.Socket.GetStream());
                            newSocketUser.Writer = new BinaryWriter(newSocketUser.Socket.GetStream());

                            string username = newSocketUser.Reader.ReadString();

                            var id = CreateId(username);

                            if (id == -1) // Username is already taken -> send the info and drop the user
                            {
                                newSocketUser.Writer.Write(false);
                            }
                            else // User accepted
                            {
                                newSocketUser.Writer.Write(true);

                                newSocketUser.Id = id;
                                _socketUsers.Add(newSocketUser);

                                var color = new Color();
                                color.R = newSocketUser.Reader.ReadSingle();
                                color.G = newSocketUser.Reader.ReadSingle();
                                color.B = newSocketUser.Reader.ReadSingle();
                                color.A = 1;

                                newSocketUser.Writer.Write(newSocketUser.Id);

                                // Send hosting user info
                                var p = new UsersListPacket(Users);
                                SendPacket(p);

                                EditingUser newEditUser = new EditingUser(id, username, color, false);
                                Users.Add(newEditUser);
                                SendPacket(id, new UserConnectedPacket(id, username, color, false));
                                Scripting.InvokeOnUpdate(() =>
                                {
                                    EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();

                                    newEditUser.Outliner = FlaxEngine.Object.New<CustomOutliner>();
                                    newEditUser.Outliner.UserId = newEditUser.Id;
                                });
                            }
                        }

                        foreach (var user in _socketUsers)
                        {
                            if (!user.Socket.Client.Connected)
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

                                Packet p = (Packet)Activator.CreateInstance(
                                    PacketTypeManager.SubclassTypes.First((t) => t.Name.Equals(classname)));
                                p.Author = senderId;
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
                            Thread.Sleep(0);

                        _activity = false;
                    }
                });

                _thread.IsBackground = true;
                _thread.Start();

                Debug.Log("Session server launched !");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public override bool SendPacket(Packet packet)
        {
            return SendPacket(User.Id, packet);
        }

        private static object _sendLock = new object();

        public bool SendPacket(int senderId, Packet packet)
        {
            lock (_sendLock)
            {
                string s = PacketTypeManager.SubclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name;

                foreach (var user in _socketUsers)
                {
                    if (user.Socket != null && user.Socket.Connected &&
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
                                // TODO: Catch the case where the user is disconnected
                                Debug.LogException(e);
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
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

        private int CreateId(string username)
        {
            int id = username.GetHashCode();

            if (GetUserById(id) != null)
                return -1;

            return id;
        }
    }
}