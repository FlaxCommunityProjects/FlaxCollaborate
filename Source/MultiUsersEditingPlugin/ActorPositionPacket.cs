using System.IO;

namespace MultiUsersEditingPlugin
{
    public class ActorPositionPacket : Packet
    {
        public int Position;

        public ActorPositionPacket(int position)
        {
            Position = position;
        }
        
        public override void Read(BinaryReader bs)
        {
            Position = bs.ReadInt32();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Position);
        }
    }
}