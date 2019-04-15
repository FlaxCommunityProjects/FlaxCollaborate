using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
    public class EditingSessionPlugin : EditorPlugin
    {
        public IEditingSession EditingSession;

        private ContextMenuChildMenu _mainButton;
        private ContextMenuButton _hostButton;
        private ContextMenuButton _joinButton;
        private ContextMenuButton _leaveButton;

        public override void InitializeEditor()
        {
            base.InitializeEditor();
            Instance = this;

            _mainButton = Editor.UI.MainMenu.GetButton("Tools").ContextMenu.GetOrAddChildMenu("Collaborate");
            _hostButton = _mainButton.ContextMenu.AddButton("Host session");
            _hostButton.Clicked += OnHostClick;
            _joinButton = _mainButton.ContextMenu.AddButton("Join session");
            _joinButton.Clicked += OnJoinClick;
            _leaveButton = _mainButton.ContextMenu.AddButton("Quit session");
            _leaveButton.Clicked += () => { EditingSession?.Close(); EditingSession = null; };
            //leaveButton.Enabled = false;

            Editor.Undo.ActionDone += (IUndoAction action) =>
            {
                if (EditingSession == null)
                    return;

                if ((action as TransformObjectsAction) != null)
                {
                    var transAction = (TransformObjectsAction)action;
                    Packet p = new TransformObjectPacket(transAction.Data.Selection[0].ID, transAction.Data.After[0].Translation);
                    EditingSession.SendPacket(p);
                }
            };
        }

        public override void Deinitialize()
        {
            EditingSession?.Close();
            EditingSession = null;
            //Editor.UI.ToolStrip.Children.Remove(mainButton);
            _mainButton?.Dispose();
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