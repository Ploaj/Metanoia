﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;

namespace Metanoia.Formats.GameCube
{
    [FormatAttribute(Extension = ".hsf", Description = "Mario Party Model")]
    public class HSF : IModelFormat
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AttributeHeader
        {
            public uint StringOffset;
            public uint DataCount;
            public uint DataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VertexGroup
        {
            public short PositionIndex;
            public short NormalIndex;
            public short ColorIndex;
            public short UVIndex;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RiggingSingleBind
        {
            public int BoneIndex;
            public short PositionIndex;
            public short PositionCount;
            public short NormalIndex;
            public short NormalCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RiggingDoubleBind
        {
            public int Bone1;
            public int Bone2;
            public int Count;
            public int WeightOffset;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RiggingMultiBind
        {
            public int Count;
            public short PositionIndex;
            public short PositionCount;
            public short NormalIndex;
            public short NormalCount;
            public int WeightOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RiggingDoubleWeight
        {
            public float Weight;
            public short PositionIndex;
            public short PositionCount;
            public short NormalIndex;
            public short NormalCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RiggingMultiWeight
        {
            public int BoneIndex;
            public float Weight;
        }

        private class MeshObject
        {
            public string Name;

            public int SingleBind = -1;

            public List<Vector3> Positions = new List<Vector3>();
            public List<Vector3> Normals = new List<Vector3>();
            public List<Vector2> UVs = new List<Vector2>();
            public List<Vector4> Colors = new List<Vector4>();

            public List<PrimitiveObject> Primitives = new List<PrimitiveObject>();

            public List<RiggingSingleBind> SingleBinds = new List<RiggingSingleBind>();
            public List<RiggingDoubleBind> DoubleBinds = new List<RiggingDoubleBind>();
            public List<RiggingMultiBind> MultiBinds = new List<RiggingMultiBind>();
            public List<RiggingDoubleWeight> DoubleWeights = new List<RiggingDoubleWeight>();
            public List<RiggingMultiWeight> MultiWeights = new List<RiggingMultiWeight>();

        }

        private class NodeObject
        {
            public string Name = "";

            public int ParentIndex = -1;
            public int Type = 0;

            public int MaterialIndex = 0;

            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale = Vector3.One;
        }

        private class PrimitiveObject
        {
            public int PrimitiveType;

            public int Material;

            public VertexGroup[] Vertices;

            public Vector3 UnknownVector;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MaterialObject
        {
            public long Unk1;
            public long Unk2;
            public long Unk3;
            public long Unk4;
            public long Unk5;
            public long Unk6;
            public long Unk7;
            public long Unk8;
            public long Unk9;
            public long Unk10;
            public long Unk11;
            public long Unk12;
            public long Unk13;
            public long Unk14;
            public long Unk15;
            public long Unk16;
            public int TextureIndex;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Material1Object
        {
            public long Unk1;
            public long Unk2;
            public long Unk3;
            public long Unk4;
            public long Unk5;
            public long Unk6;
            public long Unk7;
            public int MaterialIndex;
        }

        private Dictionary<string, MeshObject> MeshObjects = new Dictionary<string, MeshObject>();
        private List<GenericTexture> Textures = new List<GenericTexture> ();

        private List<NodeObject> Nodes = new List<NodeObject>();

        private List<Material1Object> Materials1 = new List<Material1Object>();
        private List<MaterialObject> Materials = new List<MaterialObject>();

        public void Open(byte[] Data)
        {
            using (DataReader reader = new DataReader(new MemoryStream(Data)))
            {
                reader.BigEndian = true;

                reader.Position = 0xA8;

                var stringTableOffset = reader.ReadUInt32();
                var stringTableSize = reader.ReadInt32();

                reader.Position = 0x18;

                var material1TableOffset = reader.ReadUInt32();
                var material1TableSize = reader.ReadInt32();

                var materialTableOffset = reader.ReadUInt32();
                var materialTableSize = reader.ReadInt32();

                var positionOffset = reader.ReadUInt32(); // positions?
                var positionCount = reader.ReadInt32();

                var normalOffset = reader.ReadUInt32(); // positions?
                var normalCount = reader.ReadInt32();

                var uvOffset = reader.ReadUInt32(); // positions?
                var uvCount = reader.ReadInt32();

                var primOffset = reader.ReadUInt32(); // positions?
                var primCount = reader.ReadInt32();

                var boneOffset = reader.ReadUInt32(); //??
                var boneCount = reader.ReadInt32();

                var textureOffset = reader.ReadUInt32(); //??
                var textureCount = reader.ReadInt32();

                var paletteOffset = reader.ReadUInt32(); //??
                var paletteCount = reader.ReadInt32();

                reader.ReadUInt32();
                reader.ReadInt32();

                var rigOffset = reader.ReadUInt32(); //??
                var rigCount = reader.ReadInt32();
                // ??

                // ??

                // ?? 0x48 per entry

                // ?? 0xCCCCCCC

                reader.Position = primOffset;
                ReadPrimitives(reader, reader.ReadStructArray<AttributeHeader>(primCount), stringTableOffset);

                reader.Position = materialTableOffset;
                Materials.AddRange(reader.ReadStructArray<MaterialObject>(materialTableSize));

                reader.Position = material1TableOffset;
                Materials1.AddRange(reader.ReadStructArray<Material1Object>(material1TableSize));

                reader.Position = positionOffset;
                ReadPositions(reader, reader.ReadStructArray<AttributeHeader>(positionCount), stringTableOffset);

                reader.Position = normalOffset;
                ReadNormals(reader, reader.ReadStructArray<AttributeHeader>(normalCount), stringTableOffset);

                reader.Position = uvOffset;
                ReadUVs(reader, reader.ReadStructArray<AttributeHeader>(uvCount), stringTableOffset);
                
                ReadTextures(reader, textureOffset, textureCount, paletteOffset, paletteCount, stringTableOffset);

                reader.Position = boneOffset;
                ReadNodes(reader, boneCount, stringTableOffset);

                uint endOffset = rigOffset + (uint)(rigCount * 0x24);
                reader.Position = rigOffset;
                var meshName = MeshObjects.Keys.ToArray();
                for(int i = 0; i < rigCount; i++)
                {
                    var mo = MeshObjects[meshName[i]];
                    reader.Position += 4; // 0xCCCCCCCC
                    var singleBindOffset = reader.ReadUInt32();
                    var doubleBindOffset = reader.ReadUInt32();
                    var multiBindOffset = reader.ReadUInt32();
                    var singleBindCount = reader.ReadInt32();
                    var doubleBindCount = reader.ReadInt32();
                    var multiBindCount = reader.ReadInt32();
                    var vertexCount = reader.ReadInt32();
                    mo.SingleBind = reader.ReadInt32();
                    //Console.WriteLine($"{mo.Name} {Nodes[mo.SingleBind].Name}");

                    var temp = reader.Position;

                    reader.Position = endOffset + singleBindOffset;
                    mo.SingleBinds.AddRange(reader.ReadStructArray<RiggingSingleBind>(singleBindCount));

                    reader.Position = endOffset + doubleBindOffset;
                    mo.DoubleBinds.AddRange(reader.ReadStructArray<RiggingDoubleBind>(doubleBindCount));

                    reader.Position = endOffset + multiBindOffset;
                    mo.MultiBinds.AddRange(reader.ReadStructArray<RiggingMultiBind>(multiBindCount));

                    var weightStart = reader.Position;
                    
                    foreach (var mb in mo.DoubleBinds)
                    {
                        reader.Position = (uint)(weightStart + mb.WeightOffset);
                        mo.DoubleWeights.AddRange(reader.ReadStructArray<RiggingDoubleWeight>(mb.Count));
                    }

                    foreach (var mb in mo.MultiBinds)
                    {
                        reader.Position = (uint)(weightStart + mb.WeightOffset);
                        mo.MultiWeights.AddRange(reader.ReadStructArray<RiggingMultiWeight>(mb.Count));
                    }

                    reader.Position = temp;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TextureInfo
        {
            public uint NameOffset;
            public uint Padding;
            public byte Type1;
            public byte Type2;
            public ushort Width;
            public ushort Height;
            public ushort Depth;
            public uint Padding1; // usually 0
            public int PaletteIndex; // -1 usually excet for paletted?
            public uint Padding3;// usually 0
            public uint DataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PaletteInfo
        {
            public uint NameOffset;
            public int Format;
            public int Count;
            public uint DataOffset;
        }

        private void ReadTextures(DataReader reader, uint offset, int count, uint paletteOffset, int paletteCount, uint stringTableOffset)
        {
            reader.Position = offset;
            var texInfo = reader.ReadStructArray<TextureInfo>(count);
            var startOffset = reader.Position;

            reader.Position = paletteOffset;
            var palInfo = reader.ReadStructArray<PaletteInfo>(paletteCount);
            var palSectionOffset = reader.Position;

            for(int i = 0; i < texInfo.Length; i++)
            {
                var textureName = reader.ReadString(stringTableOffset + texInfo[i].NameOffset, -1);
                Console.WriteLine($"{textureName} {texInfo[i].Type1} {texInfo[i].Width} {texInfo[i].Height}");

                var format = texInfo[i].Type1;

                byte[] palData = new byte[texInfo[i].Depth * 2];
                var palFormat = 0;
                var palCount = 0;

                if (texInfo[i].Type1 == 0x07) // CMP
                {
                    format = 14;
                }
                if (texInfo[i].Type1 == 0x09) // CMP
                {
                    if(texInfo[i].Type2 == 4)
                        format = 8;
                }
                if (texInfo[i].Type1 == 0x0A) // CMP
                {
                        format = 8;
                }

                //var pal = palInfo.ToList().Find(e => e.NameOffset == texInfo[i].NameOffset);
                if (texInfo[i].PaletteIndex > -1)
                {
                    var pal = palInfo[texInfo[i].PaletteIndex];
                    palCount = pal.Count;
                    palFormat = 0;
                    palData = reader.GetSection(palSectionOffset + pal.DataOffset, 2 * palCount);
                }

                var dataLength =
                    Tools.TPL.textureByteSize((Tools.TPL_TextureFormat)format, texInfo[i].Width, texInfo[i].Height);
                ;
                Console.WriteLine((Tools.TPL_TextureFormat)format + " " + dataLength.ToString("X"));
                reader.Position = startOffset + texInfo[i].DataOffset;
                var bitmap = Tools.TPL.ConvertFromTextureMelee(reader.ReadBytes(dataLength), texInfo[i].Width, texInfo[i].Height, format, palData, palCount, palFormat);//.Save(textureName + ".png");
                GenericTexture t = new GenericTexture();
                t.Name = textureName;
                t.FromBitmap(bitmap);
                bitmap.Dispose();
                Textures.Add(t);
            }
        }

        private void ReadPrimitives(DataReader reader, AttributeHeader[] headers, uint stringTableOffset)
        {
            var startingOffset = reader.Position;

            var ExtOffset = startingOffset;
            foreach (var att in headers)
            {
                ExtOffset += att.DataCount * 48;
            }
            foreach (var att in headers)
            {
                var primName = reader.ReadString(stringTableOffset + att.StringOffset, -1);
                var meshObject = new MeshObject();
                meshObject.Name = primName;
                MeshObjects.Add(primName, meshObject);

                reader.Position = startingOffset + att.DataOffset;
                for (int i = 0; i < att.DataCount; i++)
                {
                    var primOb = new PrimitiveObject();
                    meshObject.Primitives.Add(primOb);

                    primOb.PrimitiveType = reader.ReadInt16();
                    primOb.Material = reader.ReadInt16() & 0xFF;
                    
                    int primCount = 3;

                    if (primOb.PrimitiveType == 2 || primOb.PrimitiveType == 3)
                        primCount = 4;

                    primOb.Vertices = reader.ReadStructArray<VertexGroup>(primCount);
                    
                    if (primOb.PrimitiveType == 4)
                    {
                        primCount = reader.ReadInt32();
                        var offset = reader.ReadUInt32();
                        
                        var strip = new List<VertexGroup>();

                        strip.Add(primOb.Vertices[0]);
                        strip.Add(primOb.Vertices[1]);
                        strip.Add(primOb.Vertices[2]);

                        strip.Add(primOb.Vertices[1]);

                        var temp = reader.Position;
                        reader.Position = ExtOffset + offset * 8;
                        strip.AddRange(reader.ReadStructArray<VertexGroup>(primCount));
                        reader.Position = temp;

                        primOb.Vertices = strip.ToArray();
                    }

                    primOb.UnknownVector = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }
            }
        }

        private void ReadNodes(DataReader reader, int count, uint stringTableOffset)
        {
            for(int i = 0; i < count; i++)
            {
                NodeObject node = new NodeObject();
                Nodes.Add(node);

                node.Name = reader.ReadString(stringTableOffset + reader.ReadUInt32(), -1);
                node.Type = reader.ReadInt32();

                reader.Position += 0x8;

                node.ParentIndex = reader.ReadInt32();

                reader.Position += 0x8;// unknown

                node.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                node.Rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) * (float)Math.PI / 180;
                node.Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                reader.Position += 0xD4;

                node.MaterialIndex = reader.ReadInt32();

                reader.Position += 0x2C;
            }
        }

        private void ReadPositions(DataReader reader, AttributeHeader[] headers, uint stringTableOffset)
        {
            var startingOffset = reader.Position;
            foreach (var att in headers)
            {
                reader.Position = startingOffset + att.DataOffset;

                var posList = new List<Vector3>();
                for (int i = 0; i < att.DataCount; i++)
                {
                    posList.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }
                MeshObjects[reader.ReadString(stringTableOffset + att.StringOffset, -1)].Positions = posList;

                //Console.WriteLine(reader.ReadString(stringTableOffset + att.StringOffset, -1) + " " + (startingOffset + att.DataOffset).ToString("X") + " " +  reader.Position.ToString("X"));
            }
        }

        private void ReadNormals(DataReader reader, AttributeHeader[] headers, uint stringTableOffset)
        {
            var startingOffset = reader.Position;
            foreach (var att in headers)
            {
                reader.Position = startingOffset + att.DataOffset;

                var nrmList = new List<Vector3>();
                for (int i = 0; i < att.DataCount; i++)
                {
                    nrmList.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }
                MeshObjects[reader.ReadString(stringTableOffset + att.StringOffset, -1)].Normals = nrmList;
            }
        }

        private void ReadUVs(DataReader reader, AttributeHeader[] headers, uint stringTableOffset)
        {
            var startingOffset = reader.Position;
            foreach (var att in headers)
            {
                reader.Position = startingOffset + att.DataOffset;

                var posList = new List<Vector2>();
                for (int i = 0; i < att.DataCount; i++)
                {
                    posList.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
                }
                MeshObjects[reader.ReadString(stringTableOffset + att.StringOffset, -1)].UVs = posList;

                //Console.WriteLine(reader.ReadString(stringTableOffset + att.StringOffset, -1) + " " + (startingOffset + att.DataOffset).ToString("X") + " " +  reader.Position.ToString("X"));
            }
        }


        private GenericVertex GetVertex(MeshObject mesh, VertexGroup g)
        {
            // Rigging
            Vector4 boneIndices = new Vector4(mesh.SingleBind, 0, 0, 0);
            Vector4 weight = new Vector4(1, 0, 0, 0);
            
            foreach(var singleBind in mesh.SingleBinds)
            {
                if(g.PositionIndex >= singleBind.PositionIndex && g.PositionIndex < singleBind.PositionIndex + singleBind.PositionCount)
                {
                    boneIndices = new Vector4(singleBind.BoneIndex, 0, 0, 0);
                    break;
                }
            }
            int mbOffset = 0;
            foreach (var multiBind in mesh.DoubleBinds)
            {
                for (int i = mbOffset; i < mbOffset + multiBind.Count; i++)
                {
                    var w = mesh.DoubleWeights[i];
                    if (g.PositionIndex >= w.PositionIndex && g.PositionIndex < w.PositionIndex + w.PositionCount)
                    {
                        boneIndices = new Vector4(multiBind.Bone1, multiBind.Bone2, 0, 0);
                        weight = new Vector4(w.Weight, 1-w.Weight, 0, 0);
                        break;
                    }
                }
                mbOffset += multiBind.Count;
            }

            mbOffset = 0;
            foreach (var multiBind in mesh.MultiBinds)
            {
                if (g.PositionIndex >= multiBind.PositionIndex && g.PositionIndex < multiBind.PositionIndex + multiBind.PositionCount)
                {
                    boneIndices = new Vector4(0);
                    weight = new Vector4(0);
                    for (int i = mbOffset; i < mbOffset + multiBind.Count; i++)
                    {
                        boneIndices[i - mbOffset] = mesh.MultiWeights[i].BoneIndex;
                        weight[i - mbOffset] = mesh.MultiWeights[i].Weight;
                    }
                    break;
                }
                mbOffset += multiBind.Count;
            }

            var uv = Vector2.Zero;

            if (g.UVIndex > 0 && mesh.UVs.Count > 0)
                uv = mesh.UVs[g.UVIndex];

            return new GenericVertex()
            {
                Pos = mesh.Positions[g.PositionIndex],
                Nrm = mesh.Normals[g.NormalIndex],
                UV0 = uv,
                Bones = boneIndices,
                Weights = weight
            };
        }

        public GenericModel ToGenericModel()
        {
            GenericSkeleton skel = new GenericSkeleton();

            foreach(var node in Nodes)
            {
                GenericBone bone = new GenericBone()
                {
                    Name = node.Name,
                    Position = node.Position,
                    Rotation = node.Rotation,
                    Scale = node.Scale,
                    ParentIndex = node.ParentIndex
                };
                skel.Bones.Add(bone);
            }

            GenericModel m = new GenericModel();
            m.Skeleton = skel;

            int index = -1;
            foreach(var meshObject in MeshObjects)
            {
                index++;
                Dictionary<int, List<GenericVertex>> MaterialToVertexBank = new Dictionary<int, List<GenericVertex>>();

                foreach (var d in meshObject.Value.Primitives)
                {
                    if (!MaterialToVertexBank.ContainsKey(d.Material))
                        MaterialToVertexBank.Add(d.Material, new List<GenericVertex>());

                    var vertices = MaterialToVertexBank[d.Material];

                    switch (d.PrimitiveType)
                    {
                        case 0x02: // Triangle
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[0]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[1]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[2]));
                            break;
                        case 0x03: // Quad
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[0]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[1]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[2]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[1]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[3]));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[2]));
                            break;
                        case 0x04: // Triangle Strip
                            var verts = new List<GenericVertex>();
                            for (uint t = 0; t < d.Vertices.Length; t++)
                            {
                                var vert = GetVertex(meshObject.Value, d.Vertices[t]);
                                verts.Add(vert);
                            }
                            Tools.TriangleConverter.StripToList(verts, out verts);

                            vertices.AddRange(verts);
                            break;
                        default:
                            throw new Exception("Unsupported Primitive Type " + d.PrimitiveType.ToString("X"));
                    }

                }

                foreach(var v in MaterialToVertexBank)
                {
                    GenericMesh mesh = new GenericMesh();
                    mesh.Name = meshObject.Key;
                    if (MaterialToVertexBank.Count > 1)
                        mesh.Name += "_" + Textures[Materials[Materials1[v.Key].MaterialIndex].TextureIndex].Name;

                    mesh.Material = new GenericMaterial();

                    try
                    {
                        mesh.Material.TextureDiffuse = Textures[Materials[Materials1[v.Key].MaterialIndex].TextureIndex];

                    }
                    catch (Exception)
                    {

                    }
                    m.Meshes.Add(mesh);

                    mesh.Vertices.AddRange(v.Value);

                    Console.WriteLine(mesh.Name + " " + v.Key + " " + Materials[v.Key].TextureIndex + " " + Textures.Count + " " + Materials.Count);

                    mesh.Optimize();
                }

            }


            return m;
        }
    }
}