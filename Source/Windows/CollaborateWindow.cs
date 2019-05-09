using System;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.GUI;
using FlaxEditor.Windows;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Rendering;
using FlaxEngine.Utilities;

namespace CollaboratePlugin
{
    public class CollaborateWindow : CustomEditorWindow
    {
        private SessionSettings _settings;
        private LayoutElementsContainer _layout;
 
        public override void Initialize(LayoutElementsContainer layout)
        {
            _layout = layout;
            Rebuild();
        }

        private void showJoin()
        {
            _layout.ContainerControl.DisposeChildren();
            _layout.ContainerControl.AnchorStyle = AnchorStyle.Upper;
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.Join;
            _layout.ClearLayout();
            _layout.Space(5);
            var label = _layout.Label("Joining Session", TextAlignment.Center);
            label.Label.Font.Size = 11;
            _layout.Space(5);

            _settings = new SessionSettings();
            
            var clientSettingsEditor = new CustomEditorPresenter(null);
            clientSettingsEditor.Panel.Parent = _layout.ContainerControl;
            clientSettingsEditor.Select(_settings);

            var button = _layout.Button("Join");
            var returnButton = _layout.Button("Return");
            
            button.Button.Clicked += () =>
            {

                EditingSessionPlugin.Instance.Session = new ClientSession();
                if (EditingSessionPlugin.Instance.Session.Start(_settings))
                {
                    showSession();
                }
            };

            returnButton.Button.Clicked += () =>
            {
                showNoSession();
            };
        }

        private void showHost()
        {
            _layout.ContainerControl.DisposeChildren();
            _layout.ContainerControl.AnchorStyle = AnchorStyle.Upper;
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.Host;
            _layout.ClearLayout();
            _layout.Space(5);
            var label = _layout.Label("Hosting Session", TextAlignment.Center);
            label.Label.Font.Size = 11;
            _layout.Space(5);

            _settings = new ServerSessionSettings();
            
            var serverSettingsEditor = new CustomEditorPresenter(null);
            serverSettingsEditor.Panel.Parent = _layout.ContainerControl;
            serverSettingsEditor.Select(_settings);

            var button = _layout.Button("Host");
            var returnButton = _layout.Button("Return");
            
            button.Button.Clicked += () =>
            {
                EditingSessionPlugin.Instance.Session = new ServerSession();
                if (EditingSessionPlugin.Instance.Session.Start(_settings))
                {
                    showSession();
                }
            };
            
            returnButton.Button.Clicked += () =>
            {
                showNoSession();
            };
        }

        private void showSession()
        {
            var tableHeaderColor = new Color(60, 60, 60);
            
            if (_settings == null)
                _settings = EditingSessionPlugin.Instance.Session.Settings;
            
            _layout.ContainerControl.DisposeChildren();
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.Session;

            var vpanel = _layout.ContainerControl.AddChild<VerticalPanel>();
            vpanel.Width = _layout.ContainerControl.Width;
            
            var userDropPanel = vpanel.AddChild<DropPanel>();
            userDropPanel.HeaderText = "Users List";
            userDropPanel.EnableDropDownIcon = true;
            userDropPanel.Width = vpanel.Width;
            
            var userTable = userDropPanel.AddChild<Table>();
            userTable.Width = userDropPanel.Width;
            
            var nameDef = new ColumnDefinition();
            nameDef.Title = "Display Name";
            nameDef.CellAlignment = TextAlignment.Near;
            nameDef.TitleBackgroundColor = tableHeaderColor;
            
            var actionDef = new ColumnDefinition();
            actionDef.Title = "Actions";
            actionDef.CellAlignment = TextAlignment.Near;
            actionDef.TitleBackgroundColor = tableHeaderColor;
            
            userTable.Columns = new[] {nameDef, actionDef};
            
            EditingSessionPlugin.Instance.Session.Users.ForEach((user) =>
            {
                var name = user.Name;
                
                if (user.IsServer)
                    name += " (Server)";
                if (user.Id == EditingSessionPlugin.Instance.Session.User.Id)
                    name += " (You)";
                
                var row = new Row()
                {
                    Values = new object[]
                    {
                        user.Name,
                        "No actions"
                    },
                    Parent = userTable,
                };
            });

            var history = vpanel.AddChild<DropPanel>();
            history.HeaderText = "History";
            history.EnableDropDownIcon = true;
            
            var settings = vpanel.AddChild<DropPanel>();
            settings.HeaderText = "Settings";
            settings.EnableDropDownIcon = true;
            
            var p = vpanel.AddChild<Panel>();
            p.BackgroundColor = Color.Transparent;
            p.Height = 10;
            
            var disconnectButton = vpanel.AddChild<Button>();
            disconnectButton.Text = "Disconnect";
            disconnectButton.Clicked += () =>
            {
                EditingSessionPlugin.Instance.Session.Close();
                showNoSession();
            };
            
            userDropPanel.PerformLayout(true);
            _layout.ContainerControl.PerformLayout(true);
        }

        private void showNoSession()
        {
            _layout.ContainerControl.DisposeChildren();
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.NoSession;
            
            int bSize = 100;
            int emptySpace = 25;

            _layout.ContainerControl.DockStyle = DockStyle.Fill;
            _layout.ContainerControl.AnchorStyle = AnchorStyle.Center;
            
            var panel = _layout.ContainerControl.AddChild<Panel>();
            panel.AnchorStyle = AnchorStyle.Center;
            
            var joinButton = panel.AddChild<Button>();
            joinButton.Text = "Join session";
            joinButton.Width = bSize;
            joinButton.Height = 70;
            joinButton.Font.Size = 10;
            joinButton.X = panel.Width / 2 - bSize - emptySpace;
            
            var hostButton = panel.AddChild<Button>();
            hostButton.Text = "Host session";
            hostButton.Width = bSize;
            hostButton.Height = 70;
            hostButton.Font.Size = 10;
            hostButton.X = panel.Width / 2 + emptySpace;
            
            _layout.ContainerControl.SizeChanged += (size) =>
            {
                joinButton.X = panel.Width / 2 - bSize - emptySpace;
                    
                hostButton.X = panel.Width / 2 + emptySpace;
            };

            joinButton.Clicked += () =>
            {
                showJoin();
            };
            
            hostButton.Clicked += () =>
            {
                showHost();
            };
        }

        public void Rebuild()
        {
            switch (EditingSessionPlugin.Instance.SessionState)
            {
                case EditingSessionPlugin.State.Join:
                    showJoin();
                    break;
                case EditingSessionPlugin.State.Host:
                    showHost();
                    break;
                case EditingSessionPlugin.State.Session:
                    showSession();
                    break;
                case EditingSessionPlugin.State.NoSession:
                    showNoSession();
                    break;
            }
        }
    }
}