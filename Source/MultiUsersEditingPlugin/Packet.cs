using System;
using System.IO;
using System.Net;

namespace MultiUsersEditingPlugin
{
    public abstract class Packet
    {
        public int Author { set; get; }
        public bool IsBroadcasted { protected set; get; } = true;

        public abstract void Read(BinaryReader bs);

        public abstract void Write(BinaryWriter bw);
    }
}