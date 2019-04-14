using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
    public class JoinSessionWindow : CustomEditorWindow
    {
        private TextBoxElement IPTextbox;
        private TextBoxElement PortTextBox;
        
        public override void Initialize(LayoutElementsContainer layout)
        {
            layout.Label("Joining Session", TextAlignment.Center);
            layout.Space(20);
            IPTextbox = layout.TextBox();
            PortTextBox = layout.TextBox();
            var button = layout.Button("Join");
            button.Button.Clicked += OnButtonClicked;
        }
        
        private void OnButtonClicked()
        {
            
        }
    }
}