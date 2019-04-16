using System.Linq;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEngine;
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
            _leaveButton.Clicked += () =>
            {
                // Debug.LogError(FlaxEngine.Json.JsonSerializer.Serialize(new BaseClass()));
                // Debug.LogError(FlaxEngine.Json.JsonSerializer.Serialize(new DerivedClass()));
                /* var n = new SelectionChangeAction(new FlaxEditor.SceneGraph.SceneGraphNode[] { Editor.Scene.GetActorNode(SceneManager.FindActor("Camera")), null }, null, null);

                 var fields = n.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                 for (int i = 0; i < fields.Length; i++)
                 {
                     var f = fields[i];
                     var attributes = f.GetCustomAttributes();
                     bool x = attributes.Any(a => a is SerializeAttribute);
                     Debug.LogError(f);
                     Debug.LogError(x);
                     if (x)
                     {
                     }
                 }
                 Debug.Log("========");
                 Debug.Log(FlaxEngine.Json.JsonSerializer.Serialize(n));
                 Debug.Log("========");*/

                EditingSession?.Close(); EditingSession = null;
            };
            //leaveButton.Enabled = false;

            Editor.Undo.ActionDone += (IUndoAction action) =>
            {
                if (EditingSession == null)
                    return;

                Packet p = new GenericUndoActionPacket(action);
                EditingSession.SendPacket(p);
            };
        }

        public override void Deinitialize()
        {
            EditingSession?.Close();
            EditingSession = null;
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