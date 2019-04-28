using System;
using FlaxEditor.Gizmo;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

namespace MultiUsersEditingPlugin
{
    public class EditingUser
    {
        public int Id { get; private set; }
        public String Name { get; private set; }
        public bool IsServer { get; private set; }
        public Color SelectionColor { get; private set; }
        [NoSerialize]
        public CustomOutliner Outline { get; set; }
        public SceneGraphNode[] Selection { set; get; }

        public EditingUser(int id, String username, Color selectionColor, bool server)
        {
            Id = id;
            Name = username;
            IsServer = server;
            SelectionColor = selectionColor;
        }
    }
}