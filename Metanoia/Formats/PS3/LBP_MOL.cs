using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using OpenTK;
using Metanoia.Tools;

namespace Metanoia.Formats.PS3
{
    [Format(Extension = ".mol", Description = "Little Big Planet Model")]
    public class LBP_MOL : IModelFormat
    {
        private Vector4[] PositionBuffer;
        private Vector2[] UV0Buffer;
        private Vector2[] UV1Buffer;
        private Vector2[] UV2Buffer;

        private short[] IndexBuffer;

        private GenericSkeleton skeleton = new GenericSkeleton();

        public void Open(FileItem File)
        {
            var Data = File.GetFileBinary();
            byte[] DecompressedData = null;
            /*using (DataReader reader = new DataReader(new MemoryStream(Data)))
            {
                reader.BigEndian = true;
                reader.Seek(0x14); // skip header stuff
                ushort CompressedChunkCount = reader.ReadUInt16();

                ushort[] CompressedSize = new ushort[CompressedChunkCount];
                for(int i = 0; i < CompressedChunkCount; i++)
                {
                    CompressedSize[i] = reader.ReadUInt16();
                    reader.ReadUInt16();
                }

                MemoryStream data = new MemoryStream();
                using (BinaryWriter o = new BinaryWriter(data))
                {
                    foreach (ushort compsize in CompressedSize)
                    {
                        o.Write(Metanoia.Tools.Decompress.ZLIB(reader.ReadBytes(compsize)));
                    }
                    DecompressedData = data.GetBuffer();
                    data.Close();
                    System.IO.File.WriteAllBytes("LBP\\Decomp.bin", DecompressedData);
                }

                reader.PrintPosition();
            }*/

            using (DataReader reader = new DataReader(new MemoryStream(File.GetFileBinary())))
            {
                reader.BigEndian = true;

                // bit pack hell
                reader.Seek(0x10);
                int bufferSizeCount = reader.ReadInt32();
                int uvchannelCount = reader.ReadInt32();
                int morphCount = reader.ReadInt32();

                Console.WriteLine(bufferSizeCount);
                
                reader.Seek(0x220);
                Console.WriteLine(reader.ReadSingle() + " " + reader.ReadSingle() + " " + reader.ReadInt32());
                Console.WriteLine(reader.ReadSingle() + " " + reader.ReadSingle() + " " + reader.ReadSingle() + " " + reader.ReadInt32());

                int[] bufferOffsets = new int[bufferSizeCount];
                for(int i = 0; i < bufferSizeCount; i++)
                {
                    bufferOffsets[i] = reader.ReadInt32();
                }

                var vertexBufferSize = reader.ReadUInt32();
                var start = reader.Position;
                PositionBuffer = new Vector4[bufferOffsets[0] / 0x10];
                for(int i = 0; i < PositionBuffer.Length; i++)
                {
                    PositionBuffer[i] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }
                reader.Seek(start + vertexBufferSize);

                var uvbufferSize = reader.ReadUInt32();
                UV0Buffer = new Vector2[uvbufferSize / 0x18];
                UV1Buffer = new Vector2[uvbufferSize / 0x18];
                UV2Buffer = new Vector2[uvbufferSize / 0x18];
                for (int i = 0; i < uvbufferSize / 0x18; i++)
                {
                    if (uvchannelCount > 0)
                        UV0Buffer[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    if (uvchannelCount > 1)
                        UV1Buffer[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    if (uvchannelCount > 2)
                        UV2Buffer[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                }

                Console.WriteLine(PositionBuffer.Length + " " + UV0Buffer.Length);

                var trianglebufferSize = reader.ReadInt32();
                IndexBuffer = new short[trianglebufferSize / 2];
                for(int i = 0; i < IndexBuffer.Length; i++)
                {
                    IndexBuffer[i] = reader.ReadInt16();
                }

                uint boneCount = reader.ReadUInt32();
                reader.Skip(0x22 * boneCount);

                uint boneCount2 = reader.ReadUInt32();
                var boneStart = reader.Position;
                Matrix4[] wtransforms = new Matrix4[boneCount2];
                Matrix4[] itransforms = new Matrix4[boneCount2];
                for (int i = 0; i < boneCount2; i++)
                {
                    reader.Seek((uint)(boneStart + (i * 0x114))); // 0xD4 for older versions
                    var bone = new GenericBone();
                    bone.Name = reader.ReadString((uint)(boneStart + (i * 0x114)), -1);
                    reader.Skip(0x28);
                    bone.ParentIndex = reader.ReadInt32();
                    reader.Skip(0x8);
                    wtransforms[i] = new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    itransforms[i] = new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    if (bone.ParentIndex != -1 && bone.ParentIndex != 0)
                    {
                        var par = skeleton.GetWorldTransform(bone.ParentIndex).Inverted();
                        var reltransform = wtransforms[i] * par;
                        bone.Transform = reltransform;
                        bone.QuaternionRotation = wtransforms[i].ExtractRotation() * par.ExtractRotation();
                    }
                    else
                    if (i != 0)
                        bone.Transform = wtransforms[i];
                    else
                        bone.Transform = Matrix4.Identity;

                    skeleton.Bones.Add(bone);
                }

                reader.Seek(boneStart + (boneCount2 * 0x114));

                var boneIndexTableCount = reader.ReadUInt32();
                reader.Skip(boneIndexTableCount * 2);

                var bonecountTableCount = reader.ReadUInt32();
                reader.Skip(bonecountTableCount);

                var boneIndexTableCount2 = reader.ReadUInt32();
                reader.Skip(boneIndexTableCount2 * 2);

                reader.Skip(5);
                reader.Skip(reader.ReadUInt32() * 0x10); // vector 4s?
                
                reader.Skip(reader.ReadUInt32() * 0x40); // some matrices?

                reader.Skip(reader.ReadUInt32() * 0x4); // float table? maybe weights?

                int clusterStringCount = reader.ReadInt32();
                var clusterstringstart = reader.Position;
                for(int i =0; i < clusterStringCount; i++)
                {

                }
                reader.Seek(clusterstringstart + 0x20 * (uint)clusterStringCount);

                reader.Skip(reader.ReadUInt32() * 0x8); // ?? may have something to do with weighting

                reader.Skip(reader.ReadUInt32() * 0x4); // ?? may have something to do with weighting

                reader.Skip(reader.ReadUInt32() * 0x4); // ?? another float table??

                reader.PrintPosition();
            }
        }

        public GenericModel ToGenericModel()
        {
            var model = new GenericModel();

            model.Skeleton = skeleton;

            var mesh = new GenericMesh();
            for(int i = 0; i < PositionBuffer.Length; i++)
            {
                var vertex = new GenericVertex() { Pos = PositionBuffer[i].Xyz };
                if (UV0Buffer.Length == PositionBuffer.Length)
                    vertex.UV0 = UV0Buffer[i];
                if (UV1Buffer.Length == PositionBuffer.Length)
                    vertex.UV1 = UV1Buffer[i];
                if (UV2Buffer.Length == PositionBuffer.Length)
                    vertex.UV2 = UV2Buffer[i];
                mesh.Vertices.Add(vertex);
            }

            List<short> tris = new List<short>();
            tris.AddRange(IndexBuffer);
            TriangleConverter.StripToList(tris, out tris);

            short max = 0;
            foreach (var t in tris)
            {
                if (t > mesh.VertexCount)
                    break;
                max = Math.Max(max, t);
                mesh.Triangles.Add((uint)t);
            }

            Console.WriteLine(max.ToString("X") + " " + mesh.VertexCount.ToString("X"));

            model.Meshes.Add(mesh);

            return model;
        }
    }
    
}
