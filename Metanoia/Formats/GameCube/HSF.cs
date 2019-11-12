using System;
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
    public class HSF : I3DModelFormat
    {
        public string Name => "Mario Party Model";
        public string Extension => ".hsf";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

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

            public int TriCount = 0;
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
            public int Unk7;
            public int MaterialCount;
            public int MaterialIndex;
        }

        private Dictionary<string, MeshObject> MeshObjects = new Dictionary<string, MeshObject>();
        private List<GenericTexture> Textures = new List<GenericTexture> ();

        private List<NodeObject> Nodes = new List<NodeObject>();

        private List<Material1Object> Materials1 = new List<Material1Object>();
        private List<MaterialObject> Materials = new List<MaterialObject>();

        public void Open(FileItem File)
        {
            using (DataReader reader = new DataReader(new FileStream(File.FilePath, FileMode.Open)))
            {
                reader.BigEndian = true;

                reader.Position = 0xA8;

                var stringTableOffset = reader.ReadUInt32();
                var stringTableSize = reader.ReadInt32();

                reader.Position = 0x14;

                var flag = reader.ReadInt32();

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
                ReadNormals(reader, reader.ReadStructArray<AttributeHeader>(normalCount), stringTableOffset, flag);

                reader.Position = uvOffset;
                ReadUVs(reader, reader.ReadStructArray<AttributeHeader>(uvCount), stringTableOffset);

                ReadTextures(reader, textureOffset, textureCount, paletteOffset, paletteCount, stringTableOffset);

                reader.Position = boneOffset;
                ReadNodes(reader, boneCount, stringTableOffset);

                uint endOffset = rigOffset + (uint)(rigCount * 0x24);
                reader.Position = rigOffset;
                var meshName = MeshObjects.Keys.ToArray();
                for (int i = 0; i < rigCount; i++)
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
                    
                    if(i != rigCount - 1)
                        reader.Position = temp;
                }

                var weightStart = reader.Position;
                for (int i = 0; i < rigCount; i++)
                {
                    var mo = MeshObjects[meshName[i]];

                    foreach (var mb in mo.DoubleBinds)
                    {
                        reader.Position = (uint)(weightStart + mb.WeightOffset);
                        mo.DoubleWeights.AddRange(reader.ReadStructArray<RiggingDoubleWeight>(mb.Count));
                    }
                }
                
                weightStart = reader.Position;
                for (int i = 0; i < rigCount; i++)
                {
                    var mo = MeshObjects[meshName[i]];

                    foreach (var mb in mo.MultiBinds)
                    {
                        reader.Position = (uint)(weightStart + mb.WeightOffset);
                        mo.MultiWeights.AddRange(reader.ReadStructArray<RiggingMultiWeight>(mb.Count));
                    }
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
                Console.WriteLine(textureName + " " + (Tools.TPL_TextureFormat)format + " " + dataLength.ToString("X"));
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
                        
                        var temp = reader.Position;
                        reader.Position = ExtOffset + offset * 8;
                        var verts = reader.ReadStructArray<VertexGroup>(primCount);
                        reader.Position = temp;

                        primOb.TriCount = primOb.Vertices.Length;
                        var newVert = new VertexGroup[primOb.Vertices.Length + primCount + 1];
                        Array.Copy(primOb.Vertices, 0, newVert, 0, primOb.Vertices.Length);
                        newVert[3] = newVert[1];
                        Array.Copy(verts, 0, newVert, primOb.Vertices.Length + 1, verts.Length);
                        primOb.Vertices = newVert;
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

        private void ReadNormals(DataReader reader, AttributeHeader[] headers, uint stringTableOffset, int flag)
        {
            reader.PrintPosition();
            var startingOffset = reader.Position;
            flag = 0;

            if(headers.Length >= 2)
            {
                var pos = startingOffset + headers[0].DataOffset + headers[0].DataCount * 3;
                if (pos % 0x20 != 0)
                    pos += 0x20 - (pos % 0x20);
                if (headers[1].DataOffset == pos - startingOffset)
                    flag = 4;
            }

            foreach (var att in headers)
            {
                reader.Position = startingOffset + att.DataOffset;

                var nrmList = new List<Vector3>();
                for (int i = 0; i < att.DataCount; i++)
                {
                    if (flag == 4)
                        nrmList.Add(new Vector3(reader.ReadSByte() / (float)sbyte.MaxValue, reader.ReadSByte() / (float)sbyte.MaxValue, reader.ReadSByte() / (float)sbyte.MaxValue));
                    else
                        nrmList.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }

                MeshObjects[reader.ReadString(stringTableOffset + att.StringOffset, -1)].Normals = nrmList;
            }
        }

        private void ReadColors(DataReader reader, AttributeHeader[] headers, uint stringTableOffset)
        {
            var startingOffset = reader.Position;
            foreach (var att in headers)
            {
                reader.Position = startingOffset + att.DataOffset;

                var clrList = new List<Vector4>();
                for (int i = 0; i < att.DataCount; i++)
                {
                    clrList.Add(new Vector4(reader.ReadSByte() / (float)sbyte.MaxValue, reader.ReadSByte() / (float)sbyte.MaxValue, reader.ReadSByte() / (float)sbyte.MaxValue, 1));
                }
                MeshObjects[reader.ReadString(stringTableOffset + att.StringOffset, -1)].Colors = clrList;
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


        private GenericVertex GetVertex(MeshObject mesh, VertexGroup g, GenericSkeleton skeleton)
        {
            // Rigging
            Vector4 boneIndices = new Vector4(mesh.SingleBind, 0, 0, 0);
            Vector4 weight = new Vector4(1, 0, 0, 0);

            var Position = mesh.Positions[g.PositionIndex];
            
            var bone = skeleton.Bones.Find(e => e.Name.Equals(mesh.Name));
            if (bone != null)
            {
                boneIndices = new Vector4(skeleton.Bones.IndexOf(bone), 0, 0, 0);
                Position = Vector3.TransformPosition(Position, skeleton.GetBoneTransform(bone));
            }
            
            
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

            var Normal = Vector3.Zero;
            var Color = Vector4.One;

            if (mesh.Normals.Count > 0)
                Normal = mesh.Normals[g.NormalIndex];

            if (mesh.Colors.Count > 0)
                Normal = mesh.Colors[g.NormalIndex].Xyz;

            if (g.UVIndex > 0 && mesh.UVs.Count > 0)
                uv = mesh.UVs[g.UVIndex];

            return new GenericVertex()
            {
                Pos = Position,
                Nrm = Normal, // TODO: single bind normal
                Clr = Color,
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

            // Textures
            foreach (var tex in Textures)
            {
                if (m.TextureBank.ContainsKey(tex.Name))
                {
                    tex.Name += "_+";
                }
                m.TextureBank.Add(tex.Name, tex);
            }

            int index = -1;
            foreach(var meshObject in MeshObjects)
            {
                index++;
                Dictionary<int, List<GenericVertex>> MaterialToVertexBank = new Dictionary<int, List<GenericVertex>>();
                //Console.WriteLine($"{meshObject.Key} {skel.Bones[meshObject.Value.SingleBind].Name}");
                foreach (var d in meshObject.Value.Primitives)
                {
                    if (!MaterialToVertexBank.ContainsKey(d.Material))
                        MaterialToVertexBank.Add(d.Material, new List<GenericVertex>());

                    var vertices = MaterialToVertexBank[d.Material];

                    switch (d.PrimitiveType)
                    {
                        case 0x02: // Triangle
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[0], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[1], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[2], skel));
                            break;
                        case 0x03: // Quad
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[0], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[1], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[2], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[1], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[3], skel));
                            vertices.Add(GetVertex(meshObject.Value, d.Vertices[2], skel));
                            break;
                        case 0x04: // Triangle Strip
                            var verts = new List<GenericVertex>();
                            foreach (var dv in d.Vertices)
                                verts.Add(GetVertex(meshObject.Value, dv, skel));
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
                        mesh.Name += "_" + m.Meshes.Count;// Textures[Materials[Materials1[v.Key].MaterialIndex].TextureIndex].Name;

                    GenericMaterial mat = new GenericMaterial();

                    mesh.MaterialName = "material_" + m.MaterialBank.Count;

                    m.MaterialBank.Add(mesh.MaterialName, mat);

                    var mat1Index = Materials1[v.Key].MaterialIndex;
                    mat1Index = Math.Min(mat1Index, Materials.Count - 1);
                    var textureIndex = Materials[mat1Index].TextureIndex;
                    mat.TextureDiffuse = Textures[textureIndex].Name;
                        
                    m.Meshes.Add(mesh);

                    mesh.Vertices.AddRange(v.Value);

                    //Console.WriteLine(mesh.Name + " " + v.Key + " " + Materials[v.Key].TextureIndex + " " + Textures.Count + " " + Materials.Count);

                    mesh.Optimize();
                }

            }


            return m;
        }

        public bool Verify(FileItem file)
        {
            return file.Extension == Extension;
        }

        public void Save(string filePath)
        {
        }
    }
}
