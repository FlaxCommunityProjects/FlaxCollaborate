using FlaxEditor.Gizmo;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using FlaxEngine.Rendering;
using CollaboratePlugin;

namespace MultiUsersEditing.Source.MultiUsersEditingPlugin
{
    public class CustomOutliner : BaseSelectionOutline
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
            }
        }
        
        public override bool CanRender
        {
            get
            {
                var user = EditingSessionPlugin.Instance.Session.GetUserById(_userId);
                if (user != null)
                {
                    return user.Selection != null && user.Selection.Length > 0;
                }

                return false;
            }
        }

        protected override void DrawSelectionDepth(GPUContext context, SceneRenderTask task, RenderTarget customDepth)
        {
            var selection = EditingSessionPlugin.Instance.Session.GetUserById(_userId).Selection;
            
            _actors.Capacity = Mathf.NextPowerOfTwo(Mathf.Max(_actors.Capacity, selection.Length));
            for (int i = 0; i < selection.Length; i++)
            {
                if (selection[i] is ActorNode actorNode)
                    _actors.Add(actorNode.Actor);
            }
            context.DrawSceneDepth(task, customDepth, true, _actors, ActorsSources.CustomActors);
        }
    }
}