using System;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.Windows;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Rendering;

namespace MultiUsersEditingPlugin
{
    public class CollaborateWindow : CustomEditorWindow
    {
        private enum State
        {
            Join,
            Host,
            Session,
            NoSession
        }

        private State _state = State.NoSession;
        
        private SessionSettings _settings = new SessionSettings();
        private LayoutElementsContainer _layout;
 
        public override void Initialize(LayoutElementsContainer layout)
        {
            _layout = layout;
            switch (_state)
            {
                case State.Join:
                    showJoin();
                    break;
                case State.Host:
                    showHost();
                    break;
                case State.Session:
                    showSession();
                    break;
                case State.NoSession:
                    showNoSession();
                    break;
            }
        }

        private void showJoin()
        {
            _layout.ContainerControl.DisposeChildren();
            _layout.ContainerControl.AnchorStyle = AnchorStyle.Upper;
            _state = State.Join;
            _layout.ClearLayout();
            var label = _layout.Label("Joining Session", TextAlignment.Center);
            label.Label.Font.Size = 11;
            _layout.Space(10);

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
            _state = State.Host;
            _layout.ClearLayout();
            var label = _layout.Label("Hosting Session", TextAlignment.Center);
            label.Label.Font.Size = 11;
            _layout.Space(10);

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
            _layout.ContainerControl.DisposeChildren();
            _layout.ContainerControl.AnchorStyle = AnchorStyle.Upper;
            _state = State.Session;

        }

        private void showNoSession()
        {
            _layout.ContainerControl.DisposeChildren();
            _state = State.NoSession;
            
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
    }
}