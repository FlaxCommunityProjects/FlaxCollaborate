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

        public UserPositionPacket()
        {

        }

        public override void Read(BinaryReader bs)
        {
            Position = bs.ReadVector3();
            Orientation = bs.ReadQuaternion();

            Scripting.InvokeOnUpdate(() =>
            {
                var user = EditingSessionPlugin.Instance.Session.GetUserById(Author);
                user.Position = Position;
                user.Orientation = Orientation;
            });
            
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(ref Position);
            bw.Write(ref Orientation);
        }
    }
}
