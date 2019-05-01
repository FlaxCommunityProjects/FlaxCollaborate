using System;
using System.Linq;
using System.Net;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEngine;
using FlaxEngine.GUI;

namespace CollaboratePlugin
{
    public class EditingSessionPlugin : EditorPlugin
    {
        public enum State
        {
            Join,
            Host,
            Session,
            NoSession
        }

        public State SessionState { get; set; } = State.NoSession;
        
        public EditingSession Session;

        private ContextMenuButton _collaborateButton;
        private Label _labelConnected;
        
        public CollaborateWindow CollaborateWindow { get; private set; }

        // Player position caching
        private Vector3 Position;
        private Quaternion Orientation;

        public override void InitializeEditor()
        {
            base.InitializeEditor();
            Instance = this;
            _collaborateButton = Editor.UI.MainMenu.GetButton("Window").ContextMenu.AddButton("Collaborate");
            _collaborateButton.Clicked += () =>
            {
                CollaborateWindow = new CollaborateWindow();
                CollaborateWindow.Show();
            };
            
            _labelConnected = Editor.UI.StatusBar.AddChild<Label>();
            _labelConnected.X = Editor.UI.StatusBar.Width - Editor.UI.StatusBar.Width * 0.3f;
            _labelConnected.DockStyle = DockStyle.None;
            _labelConnected.OnChildControlResized += (control) => { _labelConnected.X = Editor.UI.StatusBar.Width - Editor.UI.StatusBar.Width * 0.3f;};
            _labelConnected.Text = "Disconnected";

            Editor.Undo.ActionDone += (IUndoAction action) =>
            {            
                if (Session == null)
                    return;

                if (action is SelectionChangeAction selectionAction)
                {
                    if (selectionAction.Data.After.Length != 0 && !Session.CanSelect(selectionAction.Data.After[0].ID))
                    {
                        action.Undo();
                        return;
                    }
                }
                
                Packet p = new GenericUndoActionPacket(action);
                Session.SendPacket(p);
                
            };

            UserDrawer.Initialize();
            Scripting.Update += SendPlayerPosition;
        }

        private void SendPlayerPosition()
        {
            if (Session == null)
                return;

            var wp = Editor.Instance.Windows.EditWin.Viewport;

            var vpos = wp.ViewPosition;
            var vrot = wp.ViewOrientation;
            if(vpos != Position || vrot != Orientation)
            {
                Position = vpos;
                Orientation = vrot;

                Packet p = new UserPositionPacket() { Position = vpos, Orientation = vrot };
                Session.SendPacket(p);
            }
            
        }

        public override void Deinitialize()
        {
            Scripting.Update -= SendPlayerPosition;
            UserDrawer.Deinitialize();
            Session?.Close();
            Session = null;
            _collaborateButton.Dispose();
            _labelConnected.Dispose();
            base.Deinitialize();
        }
        
        private static EditingSessionPlugin _instance;

        public static EditingSessionPlugin Instance
        {
            get
            {
                return _instance;
            }

            protected set { _instance = value; }
        }
    }
}