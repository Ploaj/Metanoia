using Metanoia.Modeling;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Metanoia.Formats.Misc
{
    [FormatAttribute(Extension = ".obe", Description = "BlitzOBE")]
    public class OBE : IModelFormat
    {
        private GenericSkeleton Skeleton { get; set; } = new GenericSkeleton();

        public List<GenericVertex> vertices = new List<GenericVertex>();

        private List<List<MeshObject>> meshGroups = new List<List<MeshObject>>();

        private class MeshObject
        {
            public int VertexCount;
            public int PrimitiveType;
            public int UnkCount;
            public int[] BoneIndices;
        }
        private Dictionary<uint, int> offsetToIndex = new Dictionary<uint, int>();

        private Dictionary<string, Dictionary<int, GenericVertex>> _morphs = new Dictionary<string, Dictionary<int, GenericVertex>>();

        public void ParseOBE(byte[] data)
        {
            DataReader reader = new DataReader(new MemoryStream(data));
            reader.BigEndian = true;
            reader.Position = 0x20;
            var vertexCount = reader.ReadUInt32();
            var vertexOffset = reader.ReadUInt32();

            var dataChunkCount = reader.ReadInt32();
            var dataChunkOffset = reader.ReadUInt32();
            var dataOffset = reader.ReadUInt32();

            int[] chunkCounts = new int[dataChunkCount];
            reader.Position = dataChunkOffset;
            for (int i = 0; i < dataChunkCount; i++)
            {
                chunkCounts[i] = reader.ReadInt32();
                reader.Position += 12; // there's a hash here too
            }

            int totalChunks = 0;
            reader.Position = dataOffset;
            foreach (var count in chunkCounts)
            {
                var vertices = new List<MeshObject>();
                meshGroups.Add(vertices);
                totalChunks += count;
                for (int i = 0; i < count; i++)
                {
                    // GAMECUBE
                    var ob = new MeshObject();
                    ob.PrimitiveType = reader.ReadInt16() >> 8;
                    ob.VertexCount = reader.ReadInt16();
                    ob.BoneIndices = new int[reader.ReadByte()];
                    ob.UnkCount = reader.ReadByte();
                    reader.Position += 2;

                    for (int bi = 0; bi < ob.BoneIndices.Length; bi++)
                        ob.BoneIndices[bi] = reader.ReadByte();

                    vertices.Add(ob);
                    reader.Position += (uint)(0xA - ob.BoneIndices.Length);

                    // XBOX
                    /*var ob = new MeshObject()
                    {
                        PrimitiveType = reader.ReadInt16(),
                        VertexCount = reader.ReadInt16(),
                    };
                    ob.BoneIndices = new int[reader.ReadInt16()];
                    ob.UnkCount = reader.ReadInt16();

                    Console.WriteLine(ob.VertexCount.ToString("X") + " " + ob.PrimitiveType);

                    for (int bi = 0; bi < ob.BoneIndices.Length; bi++)
                        ob.BoneIndices[bi] = reader.ReadByte();

                    meshObjects.Add(ob);

                    reader.Position += (uint)(0x30 - ob.BoneIndices.Length);*/
                }
            }


            reader.Position = 0xA0; // 0x80
            var ModelInfoOffset = reader.ReadUInt32();
            var ModelInfoCount = reader.ReadUInt32();

            ParseBone(reader, ModelInfoOffset);

            //Skeleton.TransformWorldToRelative();

            reader.Position = 0x94;
            var SomeOffset = reader.ReadUInt32();
            var SomeCount = reader.ReadUInt32();

            for (var ob = 0; ob < SomeCount; ob++)
            {
                var hash = reader.ReadUInt32();
                var c = reader.ReadUInt16();
            }

            reader.Position = 0x50;
            int maxBoneWeight = reader.ReadByte();
            reader.ReadByte();
            int flag = reader.ReadUInt16();
            var VertexTableReaderOffset = reader.ReadUInt32();
            var VertexTableOffset = reader.ReadUInt32();
            var VertexTableSize = reader.ReadUInt32();
            var positionTableOffset = reader.ReadUInt32();
            var normalTableOffset = reader.ReadUInt32();
            var uvTableOffset = reader.ReadUInt32();
            var colorTableOffset = reader.ReadUInt32();
            reader.ReadUInt32();//vertex count
            reader.ReadUInt32(); //normal count
            var morphTargetOffset = reader.ReadUInt32();

            if (flag == 0x12 || flag == 0x1A)
            {
                reader.Position = VertexTableReaderOffset;
                for (int i = 0; i < totalChunks; i++)
                {
                    var offset = reader.ReadUInt32();
                    var size = reader.ReadUInt32();

                    var temp = reader.Position;
                    reader.Position = VertexTableOffset + offset;
                    int primType = reader.ReadInt16();
                    int vertCount = reader.ReadInt16();

                    for (int v = 0; v < vertCount; v++)
                    {
                        if (maxBoneWeight == 1)
                            reader.ReadByte(); // bone probably divide by 3
                        var vertexIndex = reader.ReadInt16();
                        var normalIndex = reader.ReadInt16();
                        var colorIndex = reader.ReadInt16();
                        var uvIndex = reader.ReadInt16();

                        var temp2 = reader.Position;

                        var vertex = new GenericVertex();

                        int stride = 0x20;
                        if (maxBoneWeight == 1)
                            stride = 0x10;

                        reader.Position = (uint)(vertexOffset + stride * vertexIndex);
                        vertex.Pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        if (maxBoneWeight == 1)
                        {
                            vertex.Bones = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            vertex.Weights = new Vector4(1, 0, 0, 0);
                        }
                        else
                        if (maxBoneWeight == 2 || maxBoneWeight == 3 || maxBoneWeight == 4)
                        {
                            vertex.Bones = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            vertex.Weights = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);
                            reader.ReadSingle();
                        }
                        else
                            throw new NotImplementedException($"Bone Weight {maxBoneWeight} not supported");

                        //reader.Position = (uint)(positionTableOffset + 12 * vertexIndex);
                        //vertex.Pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                        reader.Position = (uint)(normalTableOffset + 3 * normalIndex);
                        vertex.Nrm = new Vector3(reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte());
                        vertex.Nrm.Normalize();

                        reader.Position = (uint)(uvTableOffset + 8 * uvIndex);
                        vertex.UV0 = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                        reader.Position = (uint)(colorTableOffset + 4 * colorIndex);
                        vertex.Clr = new Vector4(reader.ReadByte() / 128f, reader.ReadByte() / 128f, reader.ReadByte() / 128f, reader.ReadByte() / 128f);

                        vertices.Add(vertex);

                        reader.Position = temp2;
                    }

                    reader.Position = temp;
                }

            }
            else
            {
                reader.Position = vertexOffset;
                for (var v = 0; v < vertexCount; v++)
                {
                    // GAMECUBE
                    var vertex = new GenericVertex();

                    vertex.Pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    vertex.Weights = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);
                    vertex.Bones = new Vector4(reader.ReadByte() / 3, reader.ReadByte() / 3, reader.ReadByte() / 3, reader.ReadByte() / 3);
                    vertex.Nrm = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    vertex.Clr = new Vector4(reader.ReadByte() / 128f, reader.ReadByte() / 128f, reader.ReadByte() / 128f, reader.ReadByte() / 128f);
                    vertex.UV0 = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                    vertices.Add(vertex);
                }

                FixBoneIndices();
            }


            //Morphs ------------------------

            reader.Position = morphTargetOffset;

            var numOfMorphs = reader.ReadInt32();
            var morphVertexCount = reader.ReadInt32();
            var morphFlag = reader.ReadInt32();

            reader.Position = morphTargetOffset + 0x40;
            for(int m = 0; m < numOfMorphs; m++)
            {
                var MorphVertex = new Dictionary<int, GenericVertex>();
                for (int i = 0; i < morphVertexCount; i++)
                {
                    var morphPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    int vertexPosition = reader.ReadInt32();
                    var morphNormal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    reader.ReadInt32();
                    // ??
                    if(morphFlag == 1)
                    {
                        reader.Position += 16;
                    }

                    MorphVertex.Add(vertexPosition, new GenericVertex()
                    {
                        Pos = vertices[vertexPosition].Pos + morphPos,
                        Nrm = morphNormal
                    });

                    /*if(m == 7)
                    {
                        var vertex = vertices[vertexPosition];
                        vertex.Pos = vertices[vertexPosition].Pos + morphPos;
                        vertices[vertexPosition] = vertex;
                    }*/
                }
                _morphs.Add($"Morph_{m}", MorphVertex);
            }
            
        }

        private void ParseBone(DataReader reader, uint offset, int parentIndex = -1)
        {
            var myIndex = Skeleton.Bones.Count;
            var myOffset = offset;
            if (offsetToIndex.ContainsKey(myOffset))
                return;
            Console.WriteLine("Bone " + myIndex + " " + offset.ToString("x"));

            var bone = new GenericBone();
            bone.Name = $"Bone_{myIndex}";

            reader.Position = offset + 0x130; // 0x120
            uint NameOffset = reader.ReadUInt32();
            if(NameOffset != 0)
                bone.Name = reader.ReadString(NameOffset, -1);

            reader.Position = offset + 0;
            bone.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            reader.Position = offset + 0x20;
            bone.QuaternionRotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            reader.Position = offset + 0x50;
            bone.Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            offsetToIndex.Add(myOffset, myIndex);

            Skeleton.Bones.Add(bone);

            //bone.ParentIndex = parentIndex;
            /*reader.Position = offset + 0x80; // 0x70
            bone.Transform = new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            try
            {
                bone.Transform.Invert();
            }
            catch (Exception)
            {
                Console.WriteLine("Invert Exception " + bone.Name);
            }*/

            reader.Position = offset + 0x110; // 0x100

            var siblingLeft = reader.ReadUInt32();
            var siblingRight = reader.ReadUInt32();
            var parentOffset = reader.ReadUInt32();
            var childOffset = reader.ReadUInt32();
            var flags = reader.ReadUInt32();

            //console.log(flags.toString(16));

            if (childOffset != myOffset && childOffset != 0)
            {
                ParseBone(reader, childOffset, myIndex);
            }

            /*if (siblingRight != myOffset)
            {
                ParseBone(reader, siblingRight, parentIndex);
            }*/

            if (siblingLeft != myOffset && siblingLeft != 0)
            {
                ParseBone(reader, siblingLeft, parentIndex);
            }

            /*if (parentOffset != myOffset && parentOffset != 0)
                bone.ParentIndex = offsetToIndex[parentOffset];*/
            bone.ParentIndex = parentIndex;
        }

        private void FixBoneIndices()
        {
            int offset = 0;
            foreach (var gr in meshGroups)
            {
                foreach (var mo in gr)
                {
                    if (mo.BoneIndices.Length == 0)
                        continue;

                    for (int i = offset; i < offset + mo.VertexCount; i++)
                    {
                        var X = vertices[i].Bones.X < mo.BoneIndices.Length ? mo.BoneIndices[(int)vertices[i].Bones.X] : 0;
                        var Y = vertices[i].Bones.Y < mo.BoneIndices.Length ? mo.BoneIndices[(int)vertices[i].Bones.Y] : 0;
                        var Z = vertices[i].Bones.Z < mo.BoneIndices.Length ? mo.BoneIndices[(int)vertices[i].Bones.Z] : 0;
                        var W = vertices[i].Bones.W < mo.BoneIndices.Length ? mo.BoneIndices[(int)vertices[i].Bones.W] : 0;
                        var vert = vertices[i];
                        vert.Bones = new Vector4(X, Y, Z, W);
                        vertices[i] = vert;
                    }

                    offset += mo.VertexCount;
                }
            }
        }

        public GenericModel GetGenericModel()
        {
            var ml = new List<GenericMesh>();
            int offset = 0;
            int MaxBoneIndex = 0;
            foreach (var gr in meshGroups)
            {
                foreach (var mo in gr)
                {
                    var mesh = new GenericMesh();
                    mesh.Name = "mesh" + gr.Count.ToString("X");
                    mesh.MaterialName = "material";

                    mesh.Vertices = new List<GenericVertex>();
                    mesh.Triangles = new List<uint>();

                    var stringToMorph = new Dictionary<string, GenericMorph>();

                    for (int i = offset; i < offset + mo.VertexCount; i++)
                    {
                        mesh.Vertices.Add(vertices[i]);
                        mesh.Triangles.Add((uint)(i - offset));
                        
                        // Morphs
                        foreach (var m in _morphs)
                        {
                            //contains vertex
                            if (m.Value.ContainsKey(i))
                            {
                                if (!stringToMorph.ContainsKey(m.Key))
                                {
                                    GenericMorph morph = new GenericMorph()
                                    {
                                        Name = m.Key
                                    };
                                    mesh.Morphs.Add(morph);
                                    stringToMorph.Add(m.Key, morph);
                                }

                                var morp = stringToMorph[m.Key];
                                morp.Vertices.Add(new MorphVertex()
                                {
                                    VertexIndex = mesh.Vertices.Count - 1,
                                    Vertex = m.Value[i]
                                });
                            }
                        }
                    }
                    offset += mo.VertexCount;

                    if (mo.PrimitiveType == 4)
                    {
                        mesh.PrimitiveType = PrimitiveType.Triangles;
                        ml.Add(mesh);
                    }
                    if (mo.PrimitiveType == 5)
                    {
                        mesh.PrimitiveType = PrimitiveType.TriangleStrip;
                        ml.Add(mesh);
                    }
                    foreach (var boneindex in mo.BoneIndices)
                        MaxBoneIndex = Math.Max(boneindex, MaxBoneIndex);
                }

            }

            Console.WriteLine("Bones: " + MaxBoneIndex + " " + Skeleton.Bones.Count);
            Console.WriteLine("Vertices: " + offset + " " + vertices.Count);

            var model = new GenericModel() { Skeleton = Skeleton, Meshes = ml };
            model.MaterialBank.Add("material", new GenericMaterial());

            return model;
        }

        public GenericModel ToGenericModel()
        {
            return GetGenericModel();
        }

        public void Open(byte[] Data)
        {
            ParseOBE(Data);
        }
    }
}
