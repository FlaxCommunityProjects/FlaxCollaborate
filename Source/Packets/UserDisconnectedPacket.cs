using System.IO;

namespace CollaboratePlugin
{
    public class UserDisconnectedPacket : Packet
    {
        public int UserId;

        public UserDisconnectedPacket()
        {
            
        }

        public UserDisconnectedPacket(int id)
        {
            UserId = id;
        }
        
        public override void Read(BinaryReader bs)
        {
            UserId = bs.ReadInt32();

            var user = EditingSessionPlugin.Instance.Session.GetUserById(UserId);
            EditingSessionPlugin.Instance.Session.Users.Remove(user);
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UserId);
        }
    }
}