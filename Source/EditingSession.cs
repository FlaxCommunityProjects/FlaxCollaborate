using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollaboratePlugin;
using FlaxEditor.SceneGraph;

namespace CollaboratePlugin
{
    public abstract class EditingSession
    {
        public EditingUser User { get; protected set; }

        public List<EditingUser> Users { get; } = new List<EditingUser>();

        public SessionSettings Settings { get; protected set; }

        public abstract bool IsHosting { get; }

        public abstract Task<bool> Start(SessionSettings settings);

        public abstract bool SendPacket(Packet packet);

        public abstract void Close();

        public EditingUser GetUserById(int id)
        {
            try
            {
                return Users.First((user) => user.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool CanSelect(Guid id)
        {
            bool match = true;
            Users.ForEach((user) =>
            {
                if (user.Id != User.Id && user.Selection != null)
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