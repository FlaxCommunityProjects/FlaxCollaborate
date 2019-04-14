using System;
using MultiUsersEditingPlugin;

namespace MultiUsersEditingPlugin
{
	public interface EditingSession
	{
		bool IsHosting { get; }

		bool Start(SessionSettings settings);

		bool SendPacket(Packet packet);

		void Close();
	}
}