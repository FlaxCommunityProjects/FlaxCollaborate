using System;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
	public class JoinSessionWindow : CustomEditorWindow
	{
		private SessionSettings clientSettings = new SessionSettings();

		public override void Initialize(LayoutElementsContainer layout)
		{
			layout.Label("Joining Session", TextAlignment.Center);
			layout.Space(20);

			var clientSettingsEditor = new CustomEditorPresenter(null);
			clientSettingsEditor.Panel.Parent = layout.ContainerControl;
			clientSettingsEditor.Select(clientSettings);

			var button = layout.Button("Join");
			button.Button.Clicked += OnButtonClicked;
		}

		private void OnButtonClicked()
		{
			EditingSessionPlugin.GetInstance().EditingSession = new ClientSession();
			EditingSessionPlugin.GetInstance().EditingSession.Start(clientSettings);
		}
	}
}