using System;
using System.Collections.Generic;
using FlaxEditor.SceneGraph;
using MultiUsersEditingPlugin;

namespace MultiUsersEditingPlugin
{
    public abstract class EditingSession
    {
        public readonly List<EditingUser> Users = new List<EditingUser>();
        
        public bool IsHosting { get; }

        public abstract bool Start(SessionSettings settings);

        public abstract bool SendPacket( Packet packet);

        public abstract void Close();
    }
}