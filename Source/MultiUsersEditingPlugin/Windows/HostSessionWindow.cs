using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
    public class HostSessionWindow : CustomEditorWindow
    {
        private TextBoxElement PortTextBox;
        
        public override void Initialize(LayoutElementsContainer layout)
        {
            layout.Label("Hosting Session", TextAlignment.Center);
            layout.Space(20);
            layout.CustomContainer<GroupElement>();
            PortTextBox = layout.TextBox();
            PortTextBox.TextBox.DockStyle = DockStyle.None;
            PortTextBox.TextBox.AnchorStyle = AnchorStyle.Center;
            PortTextBox.TextBox.Width = 10;
            var button = layout.Button("Host");
            button.Button.Clicked += OnButtonClicked;
            
        }
        
        private void OnButtonClicked()
        {
            
        }
    }
}