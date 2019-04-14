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
    public class ClientSession : EditingSession
    {
        private TcpClient Socket;
        private NetworkStream Stream;
        private BinaryReader Reader;
        private BinaryWriter Writer;
        private Thread Thread;
        private bool Running;
        
        public override bool Start(String host, int port)
        {
            IsHosting = false;
            try
            {
                if (Socket != null && Socket.Connected)
                {
                    Socket.Close();
                    Stream.Close();
                }
                
                Socket = new TcpClient(host, port);
                Stream = Socket.GetStream();
                Writer = new BinaryWriter(Stream);
                Reader = new BinaryReader(Stream);
                
                Thread = new Thread(ReceiveLoop);
                Thread.IsBackground = true;
                Thread.Start();
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
            Running = true;
            while (Running)
            {
                if (!Socket.Connected)
                {
                    Running = false;
                }
                else if (Socket.Available != 0)
                {
                    String s = Reader.ReadString();
                    Packet p = (Packet) Activator.CreateInstance(PacketTypeManager.subclassTypes.First((t) => t.Name.Equals(s)));
                    p.Read(Reader);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }
        
        public override bool SendPacket(Packet packet)
        {
            lock (this)
            {
                try
                {
                    Writer.Write(packet.IsBroadcasted);
                    Writer.Write(PacketTypeManager.subclassTypes.First(t => packet.GetType().IsEquivalentTo(t)).Name);
                    packet.Write(Writer);
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
            Running = false;
            Writer.Close();
            Reader.Close();
            Socket.Close();
            Stream.Close();
        }
    }
}