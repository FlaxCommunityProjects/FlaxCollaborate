using System.IO;

namespace CollaboratePlugin
{
    public class SelectionChangePacket : Packet
    {
        // player session ID
        // scene nodes
        
        public SelectionChangePacket()
        {
            
        }
        
        public override void Read(BinaryReader bs)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(BinaryWriter bw)
        {
            throw new System.NotImplementedException();
        }
    }
}