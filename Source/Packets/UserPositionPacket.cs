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
        public Quaternion Orientation;
        //public Vector2 ScreenResolution;
        //public Vector2 MousePosition;

        public UserPositionPacket()
        {

        }

        public override void Read(BinaryReader bs)
        {
            Position = bs.ReadVector3();
            Orientation = bs.ReadQuaternion();
            //ScreenResolution = bs.ReadVector2();
            //MousePosition = bs.ReadVector2();

            UserDrawer.ProcessPacket(this);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(ref Position);
            bw.Write(ref Orientation);
        }
    }
}
