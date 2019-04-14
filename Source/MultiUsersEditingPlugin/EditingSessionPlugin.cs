using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
    public class EditingSessionPlugin : EditorPlugin
    {
        public EditingSession EditingSession;

        private ContextMenuChildMenu mainButton;
        private ContextMenuButton hostButton;
        private ContextMenuButton joinButton;
        
        public override void InitializeEditor()
        {
            base.InitializeEditor();
            Instance = this;
            
            mainButton = Editor.UI.MainMenu.GetButton("Tools").ContextMenu.GetOrAddChildMenu("Collaborate");
            hostButton = mainButton.ContextMenu.AddButton("Host session");
            hostButton.Clicked += OnHostClick;
            joinButton = mainButton.ContextMenu.AddButton("Join session");
            joinButton.Clicked += OnJoinClick;
            
            Editor.Undo.ActionDone += delegate(IUndoAction action)
            {
                if (EditingSession == null)
                    return;
                
                if ((action as TransformObjectsAction) != null)
                {
                    var transAction = (TransformObjectsAction) action;
                    Packet p = new TransformObjectPacket(transAction.Data.Selection[0].ID, transAction.Data.After[0].Translation);
                    EditingSession.SendPacket(p);
                }
            };
        }

        public override void Deinitialize()
        {
            Editor.UI.ToolStrip.Children.Remove(mainButton);
            base.Deinitialize();
        }

        public void OnHostClick()
        {
            new HostSessionWindow().Show();
        }
        
        public void OnJoinClick()
        {
            new JoinSessionWindow().Show();
        }


        private static EditingSessionPlugin Instance;
        
        public static EditingSessionPlugin GetInstance()
        {
            return Instance;
        }
    }
}