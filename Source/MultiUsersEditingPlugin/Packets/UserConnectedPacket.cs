using System;
using System.IO;
using FlaxEditor;
using FlaxEngine;

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
            
            EditingSessionPlugin.Instance.Session.Users.Add(new EditingUser(UserId, Username, SelectionColor, IsServer));
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();
            
            Scripting.InvokeOnUpdate(() =>
            {
                var sessionSelectionOutline = new SessionSelectionOutline(UserId);
                Editor.Instance.Windows.EditWin.Viewport.Task.CustomPostFx.Add(sessionSelectionOutline);
                Debug.Log("Outline added");
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