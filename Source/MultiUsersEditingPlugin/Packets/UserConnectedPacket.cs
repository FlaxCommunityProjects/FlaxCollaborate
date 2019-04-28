using System;
using System.IO;
using FlaxEditor;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

namespace MultiUsersEditingPlugin
{
    public class UserConnectedPacket : Packet
    {
        public int UserId;
        public String Username;
        public bool IsServer;
        public Color SelectionColor;
        
        public UserConnectedPacket()
        {
            
        }

        public UserConnectedPacket(int id, String username, Color selectionColor, bool isServer)
        {
            UserId = id;
            Username = username;
            IsServer = isServer;
            SelectionColor = selectionColor;
        }
        
        public override void Read(BinaryReader bs)
        {
            UserId = bs.ReadInt32();
            Username = bs.ReadString();
            SelectionColor = new Color();
            SelectionColor.R = bs.ReadSingle();
            SelectionColor.G = bs.ReadSingle();
            SelectionColor.B = bs.ReadSingle();
            SelectionColor.A = 1;
            
            IsServer = bs.ReadBoolean();

            EditingUser user;
            EditingSessionPlugin.Instance.Session.Users.Add(user = new EditingUser(UserId, Username, SelectionColor, IsServer));
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();
            
            Scripting.InvokeOnUpdate(() =>
            {
                user.Outline = FlaxEngine.Object.New<CustomOutliner>();
                user.Outline.UserId = UserId;
                Editor.Instance.Windows.EditWin.Viewport.Task.CustomPostFx.Add(user.Outline);
            });
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UserId);
            bw.Write(Username);
            bw.Write(SelectionColor.R);
            bw.Write(SelectionColor.G);
            bw.Write(SelectionColor.B);
            bw.Write(IsServer);
        }
    }
}