using System;
using System.IO;
using FlaxEditor;
using FlaxEngine;
using MultiUsersEditing.Source.MultiUsersEditingPlugin;

namespace CollaboratePlugin
{
    public class UserConnectedPacket : Packet
    {
        public int UserId;
        public String Username;
        public bool IsServer;
        public Color SelectionColor;
        public Vector3 Position;
        public Quaternion Orientation;

        public UserConnectedPacket()
        {
        }

        public UserConnectedPacket(int id, String username, Color selectionColor, bool isServer, Vector3 position,
            Quaternion orientation)
        {
            UserId = id;
            Username = username;
            IsServer = isServer;
            SelectionColor = selectionColor;
            Position = position;
            Orientation = orientation;
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

            Position = bs.ReadVector3();
            Orientation = bs.ReadQuaternion();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UserId);
            bw.Write(Username);
            bw.Write(SelectionColor.R);
            bw.Write(SelectionColor.G);
            bw.Write(SelectionColor.B);
            bw.Write(IsServer);
            bw.Write(ref Position);
            bw.Write(ref Orientation);
        }

        public override void Execute()
        {
            EditingUser user;
            EditingSessionPlugin.Instance.Session.AddUser(user = new EditingUser(UserId, Username, SelectionColor,
                IsServer, Position, Orientation));
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();

            user.Outliner = FlaxEngine.Object.New<CustomOutliner>();
            user.Outliner.UserId = UserId;
        }
    }
}