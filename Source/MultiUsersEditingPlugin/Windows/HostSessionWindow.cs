using System;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
	public class HostSessionWindow : CustomEditorWindow
	{
		private ServerSessionSettings serverSettings = new ServerSessionSettings();

		public override void Initialize(LayoutElementsContainer layout)
		{
			layout.Label("Hosting Session", TextAlignment.Center);
			layout.Space(20);

			var serverSettingsEditor = new CustomEditorPresenter(null);
			serverSettingsEditor.Panel.Parent = layout.ContainerControl;
			serverSettingsEditor.Select(serverSettings);

			var button = layout.Button("Host");
			button.Button.Clicked += OnButtonClicked;
		}

		private void OnButtonClicked()
		{
			EditingSessionPlugin.GetInstance().EditingSession = new ServerSession();
			EditingSessionPlugin.GetInstance().EditingSession.Start(serverSettings);
		}
	}
}