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
		private ContextMenuButton leaveButton;

		public override void InitializeEditor()
		{
			base.InitializeEditor();
			Instance = this;

			mainButton = Editor.UI.MainMenu.GetButton("Tools").ContextMenu.GetOrAddChildMenu("Collaborate");
			hostButton = mainButton.ContextMenu.AddButton("Host session");
			hostButton.Clicked += OnHostClick;
			joinButton = mainButton.ContextMenu.AddButton("Join session");
			joinButton.Clicked += OnJoinClick;

			leaveButton = mainButton.ContextMenu.AddButton("Quit session");
			leaveButton.Clicked += () => { EditingSession?.Close(); EditingSession = null; };
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
			mainButton?.Dispose();
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