using System;
using System.IO;
using FlaxEngine;

namespace CollaboratePlugin
{
    public class TestPacket : Packet
    {
        public string Message;

        public TestPacket()
        {
        }

        public TestPacket(string msg)
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