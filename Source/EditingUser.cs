using System;
using FlaxEditor;
using FlaxEditor.Gizmo;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

namespace CollaboratePlugin
{
    public class EditingUser
    {
        public int Id { get; private set; }
        public String Name { get; private set; }
        public bool IsServer { get; private set; }
        public Color SelectionColor { get; private set; }
        [NoSerialize] private CustomOutliner _customOutliner;
        public CustomOutliner Outliner {
            get { return _customOutliner; }
            set
            {
                Editor.Instance.Windows.EditWin.Viewport.Task.CustomPostFx.Remove(_customOutliner);
                _customOutliner = value;
                Editor.Instance.Windows.EditWin.Viewport.Task.CustomPostFx.Add(value);
            }
            
        }
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