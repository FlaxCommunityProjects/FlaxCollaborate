using System;
using System.IO;

namespace MultiUsersEditingPlugin
{
    public class UserConnectedPacket : Packet
    {
        public int UserId;
        public String Username;
        public bool IsServer;
        
        public UserConnectedPacket()
        {
            
        }

        public UserConnectedPacket(int id, String username, bool isServer)
        {
            UserId = id;
            Username = username;
            IsServer = isServer;
        }
        
        public override void Read(BinaryReader bs)
        {
            UserId = bs.ReadInt32();
            Username = bs.ReadString();
            IsServer = bs.ReadBoolean();
            
            EditingSessionPlugin.Instance.Session.Users.Add(new EditingUser(UserId, Username, IsServer));
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UserId);
            bw.Write(Username);
            bw.Write(IsServer);
        }
    }
}