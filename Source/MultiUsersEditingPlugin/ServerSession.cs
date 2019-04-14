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

		public bool IsHosting => true;

		public bool Start(SessionSettings settings)
		{
			try
			{
				Server = new TcpListener(Address, settings.Port);
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
									Debug.Log("New user connected !");
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
								String classname = Clients[i].Reader.ReadString();
								Debug.Log("Incoming packet type : " + classname);
								Packet p = (Packet)Activator.CreateInstance(
									PacketTypeManager.subclassTypes.First((t) => t.Name.Equals(classname)));
								p.Author = Clients[i].Socket.GetHashCode();
								p.Read(Clients[i].Reader);

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

				Thread.IsBackground = true;
				Thread.Start();

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
			Running = false;
		}
	}
}