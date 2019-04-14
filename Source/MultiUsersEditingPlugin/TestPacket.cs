using System;
using System.IO;

namespace MultiUsersEditingPlugin.MultiUsersEditingPlugin
{
    public class TestPacket : Packet
    {
        public String Message;
        
        public TestPacket()
        {
        }
        
        public TestPacket(String msg)
        {
            Message = msg;
            IsBroadcasted = true;
        }

        public override void Read(BinaryReader bs)
        {
            Message = bs.ReadString();
            Console.WriteLine(Message);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Message);
        }
    }
}