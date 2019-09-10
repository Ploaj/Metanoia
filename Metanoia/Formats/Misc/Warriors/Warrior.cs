using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Metanoia.Modeling;
using OpenTK;

namespace Metanoia.Formats.Misc
{
    public class Warrior : I3DModelFormat
    {
        private GenericSkeleton Skeleton { get; set; } = new GenericSkeleton();

        public string Name => "";

        public string Extension => ".g1m";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        private List<G1M> Models = new List<G1M>();

        private List<GenericTexture> Textures = new List<GenericTexture>();

        public void Open(FileItem File)
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(File.FilePath));

            foreach(var v in files)
            {
                Console.WriteLine(v);
                if (Path.GetExtension(v) == ".g1m")
                    ParseData(v);
                if (Path.GetExtension(v) == ".g1t")
                    ParseTextures(v);
                if (Path.GetExtension(v) == ".smc")
                    ParseSMC(v);
            }
        }

        private void ParseTextures(string path)
        {
            using (DataReader r = new DataReader(path))
            {
                r.BigEndian = false;

                r.Seek(0x0C);
                var offset = r.ReadUInt32();
                var count = r.ReadInt32();

                r.Seek(offset);

                for(int i =0; i < count; i++)
                {
                    var off = offset + r.ReadUInt32();
                    var temp = r.Position;
                    r.Seek(off);

                    int mipMap = r.ReadByte();
                    int T = r.ReadByte();
                    int W = r.ReadByte();
                    r.Skip(1);

                    var flags = r.ReadInt32();

                    if((flags >> 24) == 0x10)
                    {
                        r.Skip(0xC);
                    }

                    int H = (int)Math.Pow(2, (W >> 4));
                    W = (int)Math.Pow(2, (W & 0xF));

                    Console.WriteLine(W + " " + H + " " + T + " " + r.Position.ToString("X"));

                    GenericTexture t = new GenericTexture();

                    var size = (int)r.Length - (int)r.Position;
                    if (T == 0x59)
                    {
                        size = W * H / 2;

                        t.InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
                    }
                    else
                    if (T == 0x5B)
                    {
                        size = W * H;

                        t.InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    }
                    else
                    {
                        continue;
                    }

                    t.Width = (uint)W;
                    t.Height = (uint)H;
                    t.Mipmaps.Add(r.GetSection(r.Position, size));
                    Textures.Add(t);

                    r.Seek(temp);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void ParseSMC(string path)
        {
            using (DataReader r = new DataReader(path))
            {
                r.BigEndian = false;
                r.ReadInt32(); // magic
                var ver = r.ReadInt32();

                // 14 counts for the sections
                var unk1 = r.ReadInt32();
                var unk2 = r.ReadInt32();
                var texCount = r.ReadInt32();
                var unk3 = r.ReadInt32();
                var unk4 = r.ReadInt32();
                var skelCount = r.ReadInt32();

                r.Seek(0x40);

                if(skelCount > 0)
                {
                    r.ReadInt16();
                    var skelNameLength = r.ReadUInt32();
                    r.Skip(skelNameLength);

                    var boneCount = r.ReadInt32();
                    for(int i = 0; i < boneCount; i++)
                    {
                        var boneNameLength = r.ReadInt32();
                        GenericBone b = new GenericBone();
                        b.Name = new string(r.ReadChars(boneNameLength - 1));
                        r.Skip(1);
                        var index = r.ReadInt32();
                        b.ParentIndex = r.ReadInt32();
                        b.Transform = new OpenTK.Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                            r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                            r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                            r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        b.Position = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        r.ReadSingle();
                        Skeleton.Bones.Add(b);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void ParseData(string path)
        {
            using (DataReader r = new DataReader(path))
            {
                r.BigEndian = false;

                while(r.Position < r.Length)
                {
                    string flag = new string(r.ReadChars(8));
                    if (r.Position + 4 >= r.Length)
                        break;
                    var sectionEnd = r.Position + r.ReadUInt32() - 8;
                    Console.WriteLine(flag + " " + sectionEnd.ToString("X8"));

                    switch (flag.Substring(0, 4))
                    {
                        case "_M1G":
                            var G1M = new G1M(r);
                            Models.Add(G1M);
                            break;
                    }

                    r.Seek(sectionEnd);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GenericModel ToGenericModel()
        {
            var model = new GenericModel();
            model.Skeleton = Skeleton;

            var tindex = 0;
            foreach(var t in Textures)
            {
                model.TextureBank.Add("T" + tindex.ToString("X3"), t);
                
                tindex++;
            }

            foreach (var g1m in Models)
            {
                var mod = g1m.G1MG;

                var nuno = g1m.NUNO;
                var nunv = g1m.NUNV;

                model.Skeleton = g1m.Skeleton.Skeleton;

                var idTonunoBoneToBone33 = new List<Dictionary<int, int>>();
                List<int> nunoParents = new List<int>();

                //var idTonunoBoneToBone32 = new List<int>();

                var entries = new List<NUNO.NUNOv33Entry>();
                var nunoIndexOffset = nunv.V33Entries.Count;
                if (nunv != null)
                    entries.AddRange(nunv.V33Entries);
                if (nuno != null)
                    entries.AddRange(nuno.V33Entries);

                foreach (var entry in entries)
                {
                    var boneStart = model.Skeleton.Bones.Count;
                    var parentBone = mod.BindMatches[entry.ParentBoneID - 1][0];
                    nunoParents.Add(parentBone);

                    Dictionary<int, int> nunoBoneToBone = new Dictionary<int, int>();
                    idTonunoBoneToBone33.Add(nunoBoneToBone);

                    GenericMesh driverMesh = new GenericMesh();
                    driverMesh.Name = "driver_" + (idTonunoBoneToBone33.Count - 1) + "_" + model.Skeleton.Bones[parentBone].Name;
                    model.Meshes.Add(driverMesh);
                    driverMesh.Visible = false;

                    for (int pointIndex = 0; pointIndex < entry.Points.Count; pointIndex++)
                    {
                        // fake bone generation

                        var p = entry.Points[pointIndex];
                        var link = entry.Influences[pointIndex];
                        GenericBone b = new GenericBone();
                        b.Name = $"CP_{idTonunoBoneToBone33.Count}_{model.Skeleton.Bones[parentBone].Name}_{pointIndex}";
                        b.Transform = Matrix4.Identity;

                        nunoBoneToBone.Add(pointIndex, model.Skeleton.Bones.Count);
                        model.Skeleton.Bones.Add(b);
                        b.Position = p.Xyz;
                        b.ParentIndex = link.P3;
                        if (b.ParentIndex == -1)
                            b.ParentIndex = parentBone;
                        else
                        {
                            b.ParentIndex += boneStart;
                            b.Position = Vector3.TransformPosition(p.Xyz, model.Skeleton.GetWorldTransform(parentBone) * model.Skeleton.GetWorldTransform(b.ParentIndex).Inverted());
                        }

                        // fake driver mesh generation

                        driverMesh.Vertices.Add(new GenericVertex() {
                            Pos = Vector3.TransformPosition(Vector3.Zero, model.Skeleton.GetWorldTransform(model.Skeleton.Bones.Count - 1)),
                            Weights = new Vector4(1, 0, 0, 0),
                            Bones = new Vector4(model.Skeleton.Bones.Count - 1, 0, 0, 0)
                        });

                        if(link.P1 > 0 && link.P3 > 0)
                        {
                            driverMesh.Triangles.Add((uint)pointIndex);
                            driverMesh.Triangles.Add((uint)link.P1);
                            driverMesh.Triangles.Add((uint)link.P3);
                        }
                        if (link.P2 > 0 && link.P4 > 0)
                        {
                            driverMesh.Triangles.Add((uint)pointIndex);
                            driverMesh.Triangles.Add((uint)link.P2);
                            driverMesh.Triangles.Add((uint)link.P4);
                        }
                    }
                }
                
                foreach(var group in mod.Lods[0].Groups)
                {
                    var isCloth = (group.ID & 0xF) == 1;
                    var isPoint = (group.ID & 0xF) == 2;
                    var NunSection = (group.ID2 - (group.ID2 % 10000)) / 10000;
                    Console.WriteLine(group.Name + " " + group.ID + " " + group.ID2 + " " + NunSection);

                    foreach (var polyindex in group.Indices)
                    {
                        if (polyindex > mod.Polygons.Count)
                            continue;
                        var poly = mod.Polygons[polyindex];

                        GenericMesh mesh = new GenericMesh();
                        mesh.Name = System.Text.RegularExpressions.Regex.Replace(group.Name, @"\p{C}+", string.Empty) + model.Meshes.Count;//  + "_" + polyindex;

                        model.MaterialBank.Add("MP_" + polyindex, 
                            new GenericMaterial() {
                                MaterialInfo = mod.Materials[poly.MaterialIndex].ToString(),
                                TextureDiffuse = "T" + mod.TextureBanks[poly.TextureBankIndex].DiffuseTextureIndex.ToString("X3"),
                                EnableBlend = false,
                            });
                        mesh.MaterialName = "MP_" + polyindex;

                        mesh.Vertices = mod.GetVertices(poly, out mesh.Triangles, !isCloth);

                        if (isPoint)
                        {
                            for (int i = 0; i < mesh.VertexCount; i++)
                            {
                                var vert = mesh.Vertices[i];
                                vert.Nrm = Vector3.TransformNormal(vert.Nrm, model.Skeleton.GetWorldTransform(mod.BindMatches[poly.BoneTableIndex][0])); 
                                vert.Pos = Vector3.TransformPosition(vert.Pos, model.Skeleton.GetWorldTransform(mod.BindMatches[poly.BoneTableIndex][0])); 
                                mesh.Vertices[i] = vert;
                            }
                        }
                        if(isCloth)
                        {
                            Dictionary<int, int> nunoBoneToBone = idTonunoBoneToBone33[group.ID2 & 0xF];
                            if (NunSection == 2 && nunv != null && nuno != null)
                            {
                                foreach(var v in idTonunoBoneToBone33)
                                {
                                    Console.WriteLine(string.Join(", ", v.Keys.ToArray()));
                                }
                                nunoBoneToBone = idTonunoBoneToBone33[(group.ID2 & 0xF) + nunoIndexOffset];

                                Console.WriteLine(string.Join(", ", nunoBoneToBone.Keys.ToArray()));
                            }
                                //mesh.Name += "_driver_" + (group.ID2 & 0xF);

                            for (int i = 0; i < mesh.VertexCount; i++)
                            {
                                var vert = mesh.Vertices[i];
                                if (vert.Bit == Vector4.Zero)
                                {
                                    //Console.WriteLine(vert.Weights.ToString() + " " + vert.Bones.ToString());
                                    vert.Pos = Vector3.TransformPosition(vert.Pos, model.Skeleton.GetWorldTransform(mod.BindMatches[poly.BoneTableIndex][0]));
                                    vert.Nrm = Vector3.TransformNormal(vert.Nrm, model.Skeleton.GetWorldTransform(mod.BindMatches[poly.BoneTableIndex][0]));
                                    //vert.Weights = new Vector4(1, 0, 0, 0);
                                    vert.Bones = new Vector4(mod.BindMatches[poly.BoneTableIndex][(int)vert.Bones.X / 3],
                                        mod.BindMatches[poly.BoneTableIndex][(int)vert.Bones.Y / 3],
                                        mod.BindMatches[poly.BoneTableIndex][(int)vert.Bones.Z / 3],
                                        mod.BindMatches[poly.BoneTableIndex][(int)vert.Bones.W / 3]);
                                    mesh.Vertices[i] = vert;
                                    continue;
                                }

                                Vector3 finpos = Vector3.Zero;

                                //Console.WriteLine(vert.Nrm + " " + vert.Nrm.Normalized());

                                var tempb = new GenericBone();
                                tempb.Transform = Matrix4.Identity;
                                tempb.Rotation = vert.Clr1.Xyz;

                                var weights1 = new Vector4(vert.Pos, vert.Extra3.X);

                                {
                                    var pos = Vector3.Zero;
                                    var d4 = Vector3.Zero;
                                    d4 += Weight(pos, model.Skeleton, weights1, vert.Bones, nunoBoneToBone) * vert.Weights.X;
                                    d4 += Weight(pos, model.Skeleton, weights1, vert.Extra, nunoBoneToBone) * vert.Weights.Y;
                                    d4 += Weight(pos, model.Skeleton, weights1, vert.Fog, nunoBoneToBone) * vert.Weights.Z;
                                    d4 += Weight(pos, model.Skeleton, weights1, vert.Extra2, nunoBoneToBone) * vert.Weights.W;

                                    var d5 = Vector3.Zero;
                                    d5 += Weight(pos, model.Skeleton, weights1, vert.Bones, nunoBoneToBone) * vert.Clr1.X;
                                    d5 += Weight(pos, model.Skeleton, weights1, vert.Extra, nunoBoneToBone) * vert.Clr1.Y;
                                    d5 += Weight(pos, model.Skeleton, weights1, vert.Fog, nunoBoneToBone) * vert.Clr1.Z;
                                    d5 += Weight(pos, model.Skeleton, weights1, vert.Extra2, nunoBoneToBone) * vert.Clr1.W;

                                    var d6 = Vector3.Zero;
                                    d6 += Weight(pos, model.Skeleton, vert.Bit, vert.Bones, nunoBoneToBone) * vert.Weights.X;
                                    d6 += Weight(pos, model.Skeleton, vert.Bit, vert.Extra, nunoBoneToBone) * vert.Weights.Y;
                                    d6 += Weight(pos, model.Skeleton, vert.Bit, vert.Fog, nunoBoneToBone) * vert.Weights.Z;
                                    d6 += Weight(pos, model.Skeleton, vert.Bit, vert.Extra2, nunoBoneToBone) * vert.Weights.W;

                                    vert.Pos = Vector3.Cross(d5, d6) * (vert.Extra3.Y) + d4;
                                }

                                vert.Weights = Vector4.Zero; // new Vector4(1, 0, 0, 0);
                                vert.Bones = Vector4.Zero; // new Vector4(mod.BindMatches[poly.BoneTableIndex][0], 0, 0, 0);

                                //RigMe(vert.Pos, model.Skeleton, nunoBoneToBone, out vert.Bones, out vert.Weights);

                                mesh.Vertices[i] = vert;
                            }
                        }

                        mesh.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip;

                        model.Meshes.Add(mesh);
                    }
                }
            }

            return model;
        }

        private void RigMe(Vector3 position, GenericSkeleton skel, Dictionary<int, int> nuno, out Vector4 bone, out Vector4 weight)
        {
            weight = new Vector4();
            bone = new Vector4();

            foreach(var v in nuno)
            {
                var t = skel.GetWorldTransform(v.Value);
                var p = Vector3.TransformPosition(Vector3.Zero, t);

                var dis = Vector3.Distance(position, p);
                for(int i = 0; i < 4; i++)
                {
                    if(dis < weight[i] || weight[i] == 0)
                    {
                        weight[i] = dis;
                        bone[i] = v.Value;
                        break;
                    }
                }

            }
            weight = weight.Normalized();
        }

        private GenericVertex SingleBind(GenericVertex vert, GenericSkeleton s)
        {
            Vector3 p = Vector3.Zero;
            Vector3 n = Vector3.Zero;

            for (int i = 0; i < 1; i++)
            {
                if(vert.Weights[i] > 0)
                {
                    n += Vector3.TransformNormal(vert.Nrm, s.GetWorldTransform((int)vert.Bones[i])); // * vert.Weights[i];
                    p += Vector3.TransformPosition(vert.Pos, s.GetWorldTransform((int)vert.Bones[i])); // * vert.Weights[i];
                }
            }

            vert.Pos = p;
            vert.Nrm = n;
            return vert;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="skel"></param>
        /// <param name="weights"></param>
        /// <param name="bones"></param>
        /// <param name="nunoBoneToBone"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        private Vector3 Weight(Vector3 pos, GenericSkeleton skel, Vector4 weights, Vector4 bones, Dictionary<int, int> nunoBoneToBone, bool normal = false)
        {
            Vector3 temp = Vector3.Zero;
            if (normal)
            {
                temp += Vector3.TransformNormal(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.X])) * weights.X;
                temp += Vector3.TransformNormal(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.Y])) * weights.Y;
                temp += Vector3.TransformNormal(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.Z])) * weights.Z;
                temp += Vector3.TransformNormal(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.W])) * weights.W;
            }
            else
            {
                temp += Vector3.TransformPosition(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.X])) * weights.X;
                temp += Vector3.TransformPosition(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.Y])) * weights.Y;
                temp += Vector3.TransformPosition(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.Z])) * weights.Z;
                temp += Vector3.TransformPosition(pos, skel.GetWorldTransform(nunoBoneToBone[(int)bones.W])) * weights.W;
            }
            return temp;
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "_M1G";
            // TODO: can also check version
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Matrices
    /// </summary>
    public class G1MM
    {
        public List<Matrix4> Matrices = new List<Matrix4>();

        public G1MM(DataReader r)
        {
            var count = r.ReadInt32();
            for(int i = 0; i < count; i++)
            {
                Matrices.Add(new Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            }
        }
    }

    /// <summary>
    /// Skeleton data
    /// </summary>
    public class G1MS
    {
        public short[] BoneIndices;
        public GenericSkeleton Skeleton = new GenericSkeleton();

        public G1MS(DataReader r)
        {
            var myStart = 0;

            var start = r.Position - 12;
            var dataoffset = start + r.ReadUInt32();
            var skelCount = r.ReadInt32(); // 0? some other offset
            var boneCount = r.ReadInt16();
            var boneTableCount = r.ReadInt16();
            r.ReadInt16();
            r.ReadInt16();

            BoneIndices = new short[boneTableCount];
            for(int i = 0; i < boneTableCount; i++)
            {
                BoneIndices[i] = r.ReadInt16();

                if (BoneIndices[i] != -1)
                {
                    var temp = r.Position;
                    r.Seek(dataoffset + (uint)(0x30 * BoneIndices[i]));
                    
                    GenericBone b = new GenericBone();
                    b.Transform = Matrix4.Identity;
                    b.Scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    b.ParentIndex = r.ReadInt32();
                    if ((b.ParentIndex & 0x80000000) > 0 && b.ParentIndex != -1)
                    {
                        b.ParentIndex = b.ParentIndex & 0x7FFFFFFF;
                    }
                    else
                    {
                        b.ParentIndex = myStart + b.ParentIndex;
                    }
                    Quaternion q = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    b.QuaternionRotation = q.Inverted();

                    b.Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    r.Skip(4);
                    b.Name = "Bone_" + i.ToString();
                    Skeleton.Bones.Add(b);
                    
                    r.Seek(temp);
                }

            }

        }
    }
}
