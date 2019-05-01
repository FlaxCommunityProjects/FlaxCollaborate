using FlaxEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaboratePlugin
{
    public class UserPositionPacket : Packet
    {
        public Vector3 Position;
        public Quaternion Rotation;
        //public Vector2 ScreenResolution;
        //public Vector2 MousePosition;

        public override void Read(BinaryReader bs)
        {
            Position = bs.ReadVector3();
            Rotation = bs.ReadQuaternion();
            //ScreenResolution = bs.ReadVector2();
            //MousePosition = bs.ReadVector2();
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(ref Position);
            bw.Write(ref Rotation);
        }
    }
}
