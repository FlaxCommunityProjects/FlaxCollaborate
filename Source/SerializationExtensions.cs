using System.IO;
using FlaxEngine;

namespace CollaboratePlugin
{
    /// <summary>
    /// Set of exetions for serialization
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Reads the <see cref="Vector2"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read vector.</returns>
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads the <see cref="Vector3"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read vector.</returns>
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads the <see cref="Vector4"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read vector.</returns>
        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads the <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read quaternion.</returns>
        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads the <see cref="Color"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read color.</returns>
        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Writes the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vec2">The vector.</param>
        public static void Write(this BinaryWriter writer, ref Vector2 vec2)
        {
            writer.Write(vec2.X);
            writer.Write(vec2.Y);
        }

        /// <summary>
        /// Writes the specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vec3">The vector.</param>
        public static void Write(this BinaryWriter writer, ref Vector3 vec3)
        {
            writer.Write(vec3.X);
            writer.Write(vec3.Y);
            writer.Write(vec3.Z);
        }

        /// <summary>
        /// Writes the specified <see cref="Vector4"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="vec4">The vector.</param>
        public static void Write(this BinaryWriter writer, ref Vector4 vec4)
        {
            writer.Write(vec4.X);
            writer.Write(vec4.Y);
            writer.Write(vec4.Z);
            writer.Write(vec4.W);
        }

        /// <summary>
        /// Writes the specified <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="q">The quaternion.</param>
        public static void Write(this BinaryWriter writer, ref Quaternion q)
        {
            writer.Write(q.X);
            writer.Write(q.Y);
            writer.Write(q.Z);
            writer.Write(q.W);
        }

        /// <summary>
        /// Writes the specified <see cref="Color"/>
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="c">The color.</param>
        public static void Write(this BinaryWriter writer, ref Color c)
        {
            writer.Write(c.R);
            writer.Write(c.G);
            writer.Write(c.B);
            writer.Write(c.A);
        }
    }
}