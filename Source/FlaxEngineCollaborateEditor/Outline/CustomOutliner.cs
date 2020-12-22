using FlaxEditor.Gizmo;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using CollaboratePlugin;

namespace MultiUsersEditing.Source.MultiUsersEditingPlugin
{
    public class CustomOutliner : SelectionOutline
    {
        private int _userId;

        public int UserId
        {
            set
            {
                var user = EditingSessionPlugin.Instance.Session.GetUserById(value);
                _userId = user.Id;
                SelectionOutlineColor0 = user.SelectionColor;
                SelectionOutlineColor1 = user.SelectionColor;

                UseEditorOptions = false;

                SelectionGetter = () => user.Selection;
            }
        }
    }
}