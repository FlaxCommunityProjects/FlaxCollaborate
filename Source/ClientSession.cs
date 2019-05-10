using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using FlaxEngine.Utilities;
using Microsoft.VisualBasic;

namespace CollaboratePlugin
{
    public class ClientSession : EditingSession
    {
        private TcpClient _socket;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private Thread _thread;
        private bool _running;
        private readonly object _lockObject = new object();

        public override bool IsHosting => false;

        public override async Task<bool> Start(SessionSettings settings)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    _socket.Close();
                    _stream.Close();
                }

                _socket = new TcpClient();
                await _socket.ConnectAsync(settings.Host, settings.Port);
                _stream = _socket.GetStream();
                _writer = new BinaryWriter(_stream);
                _reader = new BinaryReader(_stream);
                _thread = new Thread(ReceiveLoop);

                _writer.Write(settings.Username);

                if (!_reader.ReadBoolean())
                {
                    Debug.Log("Username already taken, closing connection !");
                    Close();
                    return false;
                }

                var c = settings.SelectionColor;
                _writer.Write(ref c);

                var wp = Editor.Instance.Windows.EditWin.Viewport;

                var position = wp.ViewPosition;
                var orientation = wp.ViewOrientation;
                
                _writer.Write(ref position);
                _writer.Write(ref orientation);
                
                int id = _reader.ReadInt32();
                User = new EditingUser(id, settings.Username, settings.SelectionColor, false, position, orientation);

                _thread.IsBackground = true;
                _thread.Start();

                Debug.Log("Session client launched !");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        private void ReceiveLoop()
        {
            _running = true;
            while (_running)
            {
                if (!_socket.Connected)
                {
                    _running = false;
                }
                else if (_socket.Available != 0)
                {
                    int senderId = _reader.ReadInt32();
                    string s = _reader.ReadString();
                    Packet p = (Packet)Activator.CreateInstance(PacketTypeManager.SubclassTypes.First((t) => t.Name.Equals(s)));
                    p.Author = senderId;
                    p.Read(_reader);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }
        }

        public override bool SendPacket(Packet packet)
        {
            lock (_lockObject)
            {
                try
                {
                    _writer.Write(User.Id);
                    _writer.Write(packet.IsBroadcasted);
                    _writer.Write(PacketTypeManager.SubclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name);
                    packet.Write(_writer);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return false;
                }

                return true;
            }
        }

        public override void Close()
        {
            Users.ForEach(user => user.Close());
            if (User != null)
            {
                SendPacket(new UserDisconnectedPacket(User.Id));
                User.Close();
            }
            _running = false;
            _writer.Close();
            _reader.Close();
            _socket.Close();
            _stream.Close();
        }
    }
}