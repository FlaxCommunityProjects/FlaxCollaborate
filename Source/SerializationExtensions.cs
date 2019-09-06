using System.IO;
using FlaxEngine;

namespace CollaboratePlugin
{
    /// <summary>
    /// Set of extensions for serialization
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
        /// Reads the <see cref="Plane "/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read plane.</returns>
        public static Plane ReadPlane(this BinaryReader reader)
        {
            return new Plane(reader.ReadVector3(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads the <see cref="Matrix"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read matrix.</returns>
        public static Matrix ReadMatrix(this BinaryReader reader)
        {
            return new Matrix(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
            );
        }

        /// <summary>
        /// Reads the <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read bounding box.</returns>
        public static BoundingBox ReadBoundingBox(this BinaryReader reader)
        {
            return new BoundingBox(reader.ReadVector3(), reader.ReadVector3());
        }

        /// <summary>
        /// Reads the <see cref="OrientedBoundingBox"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read bounding box.</returns>
        public static OrientedBoundingBox ReadOrientedBoundingBox(this BinaryReader reader)
        {
            return new OrientedBoundingBox(reader.ReadVector3(), reader.ReadMatrix());
        }

        /// <summary>
        /// Reads the <see cref="BoundingFrustum "/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read bounding box.</returns>
        public static BoundingFrustum ReadBoundingFrustum(this BinaryReader reader)
        {
            return new BoundingFrustum(reader.ReadMatrix());
        }

        /// <summary>
        /// Reads the <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The read bounding sphere.</returns>
        public static BoundingSphere ReadBoundingSphere(this BinaryReader reader)
        {
            return new BoundingSphere(reader.ReadVector3(), reader.ReadSingle());
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

        /// <summary>
        /// Writes the specified <see cref="Plane"/>
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="p">The plane.</param>
        public static void Write(this BinaryWriter writer, ref Plane p)
        {
            writer.Write(ref p.Normal);
            writer.Write(p.D);
        }

        /// <summary>
        /// Writes the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="matrix">The matrix.</param>
        public static void Write(this BinaryWriter writer, ref Matrix matrix)
        {
            writer.Write(matrix.M11);
            writer.Write(matrix.M12);
            writer.Write(matrix.M13);
            writer.Write(matrix.M14);

            writer.Write(matrix.M21);
            writer.Write(matrix.M22);
            writer.Write(matrix.M23);
            writer.Write(matrix.M24);

            writer.Write(matrix.M31);
            writer.Write(matrix.M32);
            writer.Write(matrix.M33);
            writer.Write(matrix.M34);

            writer.Write(matrix.M41);
            writer.Write(matrix.M42);
            writer.Write(matrix.M43);
            writer.Write(matrix.M44);
        }

        /// <summary>
        /// Writes the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="boundingBox">The bounding box.</param>
        public static void Write(this BinaryWriter writer, ref BoundingBox boundingBox)
        {
            writer.Write(ref boundingBox.Minimum);
            writer.Write(ref boundingBox.Maximum);
        }

        /// <summary>
        /// Writes the specified <see cref="OrientedBoundingBox"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="boundingBox">The oriented bounding box.</param>
        public static void Write(this BinaryWriter writer, ref OrientedBoundingBox boundingBox)
        {
            writer.Write(ref boundingBox.Extents);
            writer.Write(ref boundingBox.Transformation);
        }

        /// <summary>
        /// Writes the specified <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="boundingFrustum">The bounding frustum.</param>
        public static void Write(this BinaryWriter writer, ref BoundingFrustum boundingFrustum)
        {
            Matrix matrix = boundingFrustum.Matrix;
            writer.Write(ref matrix);
        }

        /// <summary>
        /// Writes the specified <see cref="BoundingSphere "/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="boundingSphere">The bounding sphere.</param>
        public static void Write(this BinaryWriter writer, ref BoundingSphere boundingSphere)
        {
            writer.Write(ref boundingSphere.Center);
            writer.Write(boundingSphere.Radius);
        }
    }
}
