using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEditor.SceneGraph;
using MultiUsersEditingPlugin;

namespace MultiUsersEditingPlugin
{
    public abstract class EditingSession
    {
        public EditingUser User { get; protected set; }
        
        public List<EditingUser> Users { get; } = new List<EditingUser>();
        
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
        
        public bool CanSelect(Guid id)
        {
            bool match = true;
            Users.ForEach((user) =>
            {
                if(user.Id != User.Id && user.Selection != null)
                    foreach (var sceneGraphNode in user.Selection)
                    {
                        if (sceneGraphNode.ID.Equals(id))
                            match = false;
                    }
            });

            return match;
        }
    }
}