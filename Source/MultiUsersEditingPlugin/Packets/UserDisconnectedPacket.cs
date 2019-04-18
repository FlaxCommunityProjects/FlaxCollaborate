using System.IO;

namespace MultiUsersEditingPlugin
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
    }
}