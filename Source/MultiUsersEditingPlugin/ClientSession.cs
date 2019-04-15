using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FlaxEngine;
using Microsoft.VisualBasic;

namespace MultiUsersEditingPlugin
{
    public class ClientSession : IEditingSession
    {
        private TcpClient _socket;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private Thread _thread;
        private bool _running;

        public bool IsHosting => false;

        public bool Start(SessionSettings settings)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    _socket.Close();
                    _stream.Close();
                }

                _socket = new TcpClient(settings.Host, settings.Port);
                _stream = _socket.GetStream();
                _writer = new BinaryWriter(_stream);
                _reader = new BinaryReader(_stream);

                _thread = new Thread(ReceiveLoop);
                _thread.IsBackground = true;
                _thread.Start();
                Debug.Log("Session client launched !");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
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
                    String s = _reader.ReadString();
                    Packet p = (Packet)Activator.CreateInstance(PacketTypeManager.SubclassTypes.First((t) => t.Name.Equals(s)));
                    p.Read(_reader);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        public bool SendPacket(Packet packet)
        {
            lock (this)
            {
                try
                {
                    _writer.Write(packet.IsBroadcasted);
                    _writer.Write(PacketTypeManager.SubclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name);
                    packet.Write(_writer);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    return false;
                }

                return true;
            }
        }

        public void Close()
        {
            _running = false;
            _writer.Close();
            _reader.Close();
            _socket.Close();
            _stream.Close();
        }
    }
}