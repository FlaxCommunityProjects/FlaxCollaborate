using System.IO;

namespace MultiUsersEditingPlugin
{
    public class UserConnectedPacket : Packet
    {
        public int UserId;

        public UserConnectedPacket()
        {
            
        }

        public UserConnectedPacket(int id)
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