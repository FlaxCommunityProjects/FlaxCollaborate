using System;
using FlaxEditor.Gizmo;
using FlaxEditor.SceneGraph;

namespace MultiUsersEditingPlugin
{
    public class EditingUser
    {
        public int Id { get; private set; }
        public String Name { get; private set; }
        public bool IsServer { get; private set; }
        
        public SceneGraphNode[] Selection
        {
            set
            {
                // Update selection outline
            }

            get { return Selection; }
        }

        public EditingUser(int id, String username, bool server)
        {
            Id = id;
            Name = username;
            IsServer = server;
        }
    }
}