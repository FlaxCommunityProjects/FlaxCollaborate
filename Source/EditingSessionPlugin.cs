using System;
using System.Linq;
using System.Net;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEditor.States;
using FlaxEngine;
using FlaxEngine.GUI;
using Object = FlaxEngine.Object;

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

        private ContextMenuButton _collaborateButton;
        private Label _labelConnected;

        public State SessionState { get; set; } = State.NoSession;

        public EditingSession Session;

        public CollaborateWindow CollaborateWindow { get; private set; }
        public bool IsPlayMode { get; set; } = false;

        public event Action EditorModeEnter;

        public Model CameraModel;

        public Material CameraMaterial;
        public static EditingSessionPlugin Instance { get; protected set; }

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
            Editor.UI.StatusBar.OnChildControlResized += ResizeLabel;
            _labelConnected.Text = "Disconnected";

            Editor.Undo.ActionDone += OnActionDone;

            CameraMaterial = Content.LoadAsync<Material>(StringUtils.CombinePaths(Globals.ContentFolder, "M_RemoteCamera.flax"));
            CameraModel = Content.LoadAsyncInternal<Model>("Editor/Camera/O_Camera");
            Editor.Instance.Windows.EditWin.Viewport.RenderTask.Draw += DrawUsers;
            Scripting.Update += SendPlayerPosition;

            FlaxEditorStateChanged();
            Editor.StateMachine.StateChanged += FlaxEditorStateChanged;
        }

        private void ResizeLabel(Control control)
        {
            _labelConnected.X = Editor.UI.StatusBar.Width - Editor.UI.StatusBar.Width * 0.3f;
        }
        
        private void FlaxEditorStateChanged()
        {
            // Switched to play mode
            if (Editor.StateMachine.CurrentState == Editor.StateMachine.PlayingState)
            {
                IsPlayMode = true;
            }

            // Switched to edit mode
            if (Editor.StateMachine.CurrentState == Editor.StateMachine.EditingSceneState)
            {
                IsPlayMode = false;
                EditorModeEnter?.Invoke();
            }
        }

        private readonly object _drawLocker = new object();

        private void DrawUsers(FlaxEngine.Rendering.DrawCallsCollector collector)
        {
            if (CameraModel == null || CameraModel.LoadedLODs == 0 || Session == null)
                return;

            lock (_drawLocker)
            {
                foreach (var user in Session.Users)
                {
                    if (user.Material != null)
                        collector.AddDrawCall(CameraModel.LODs[0].Meshes[0], user.Material, ref user.Transform, StaticFlags.None, false);
                }
            }
        }

        private void SendPlayerPosition()
        {
            if (Session == null)
                return;

            var wp = Editor.Instance.Windows.EditWin.Viewport;

            var vpos = wp.ViewPosition;
            var vrot = wp.ViewOrientation;

            if (vpos != Session.User.Position || vrot != Session.User.Orientation)
            {
                Session.User.Position = vpos;
                Session.User.Orientation = vrot;

                Packet p = new UserPositionPacket() { Position = vpos, Orientation = vrot };
                Session.SendPacket(p);
            }
        }

        public override void Deinitialize()
        {
            Editor.UI.StatusBar.OnChildControlResized -= ResizeLabel;
            
            var editorRenderTask = Editor.Instance?.Windows?.EditWin?.Viewport?.RenderTask;
            if (editorRenderTask != null) editorRenderTask.Draw -= DrawUsers;

            Object.Destroy(CameraModel);
            Object.Destroy(CameraMaterial);

            Session.Close();

            Editor.Undo.ActionDone -= OnActionDone;
            Scripting.Update -= SendPlayerPosition;
            Editor.StateMachine.StateChanged -= FlaxEditorStateChanged;
            Session?.Close();
            Session = null;
            _collaborateButton.Dispose();
            _labelConnected.Dispose();
            base.Deinitialize();
        }

        private void OnActionDone(IUndoAction action)
        {
            if (Session == null)
                return;

            // Don't send actions during play mode
            if (IsPlayMode) return;

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
        }
    }
}