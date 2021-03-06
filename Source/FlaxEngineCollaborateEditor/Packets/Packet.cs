using System;
using System.IO;
using System.Net;

namespace CollaboratePlugin
{
    public abstract class Packet
    {
        public int Author { set; get; }

        public bool IsBroadcasted { protected set; get; } = true;

        public abstract void Read(BinaryReader bs);

        public abstract void Write(BinaryWriter bw);

        public abstract void Execute();
    }
}