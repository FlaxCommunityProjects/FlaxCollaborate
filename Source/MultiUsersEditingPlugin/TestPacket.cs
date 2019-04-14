using System;
using System.IO;
using FlaxEngine;

namespace MultiUsersEditingPlugin
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
            Debug.Log(Message);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Message);
        }
    }
}