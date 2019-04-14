using System;

namespace MultiUsersEditingPlugin
{
    public abstract class EditingSession
    {
        public bool IsHosting { protected set; get; }
        public abstract bool Start(String host, int port);
        public abstract bool SendPacket(Packet packet);
    }
}