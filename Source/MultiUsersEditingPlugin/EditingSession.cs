using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEditor.SceneGraph;
using MultiUsersEditingPlugin;

namespace MultiUsersEditingPlugin
{
    public abstract class EditingSession
    {
        public readonly List<EditingUser> Users = new List<EditingUser>();
        
        public abstract bool IsHosting { get; }

        public abstract bool Start(SessionSettings settings);

        public abstract bool SendPacket(Packet packet);

        public abstract void Close();
        
        public EditingUser GetUserById(int id)
        {
            try
            {
                return Users.First((user) => user.Id == id);
            }
            catch (Exception e)
            {
                return null;
            }
            
        }
    }
}