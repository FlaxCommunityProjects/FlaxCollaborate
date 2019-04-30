using System;
using System.IO;
using FlaxEditor;
using FlaxEditor.Options;
using FlaxEditor.SceneGraph;
using FlaxEngine;

namespace CollaboratePlugin
{
    public class TransformObjectPacket : Packet
    {
        public Guid Guid;
        public Vector3 Position;

        public TransformObjectPacket()
        {
        }

        public TransformObjectPacket(Guid guid, Vector3 position)
        {
            Guid = guid;
            Position = position;
        }

        public override void Read(BinaryReader bs)
        {
            Guid = Guid.Parse(bs.ReadString());
            Position.X = bs.ReadSingle();
            Position.Y = bs.ReadSingle();
            Position.Z = bs.ReadSingle();

            SceneGraphNode node = SceneGraphFactory.FindNode(Guid);

            if (node != null)
            {
                var trans = node.Transform;
                trans.Translation = Position;
                node.Transform = trans;
            }
        }

        public override void Write(BinaryWriter bw)
        {
            //Debug.Log(Guid);
            bw.Write(Guid.ToString());
            bw.Write(Position.X);
            bw.Write(Position.Y);
            bw.Write(Position.Z);
        }
    }
}