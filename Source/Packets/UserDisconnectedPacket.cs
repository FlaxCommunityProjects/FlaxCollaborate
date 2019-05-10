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
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(UserId);
        }

        public override void Execute()
        {
            var user = EditingSessionPlugin.Instance.Session.GetUserById(UserId);
            user.Close();
            EditingSessionPlugin.Instance.Session.RemoveUser(user);
            EditingSessionPlugin.Instance.CollaborateWindow.Rebuild();
        }
    }
}