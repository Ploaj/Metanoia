using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using Metanoia.Tools;

namespace Metanoia.Formats.GameCube
{
    [FormatAttribute(Extension = ".mdg", Description = "Jimmy Jet Fusion")]
    public class JetFusion : IModelFormat
    {
        private GenericModel Model { get; set; }

        public void Open(FileItem File)
        {
            var skelPath = File.FilePath.Replace(".mdg", ".anm");
            var skelAngPath = File.FilePath.Replace(".mdg", ".ang");
            var modelPath = File.FilePath.Replace(".mdg", ".mdl");
            var texPath = File.FilePath.Replace(".mdg", ".tex");

            if(!System.IO.File.Exists(skelPath))
            {
                System.Windows.Forms.MessageBox.Show("Missing Skeleton File " + Path.GetFileName(skelPath));
                return;
            }
            if (!System.IO.File.Exists(skelAngPath))
            {
                System.Windows.Forms.MessageBox.Show("Missing Skeleton File " + Path.GetFileName(skelAngPath));
                return;
            }
            if (!System.IO.File.Exists(modelPath))
            {
                System.Windows.Forms.MessageBox.Show("Missing Model File " + Path.GetFileName(modelPath));
                return;
            }

            Model = new GenericModel();
            Model.Skeleton = new GenericSkeleton();

            if (System.IO.File.Exists(texPath))
            {
                using (DataReader r = new DataReader(new FileStream(texPath, FileMode.Open)))
                {
                    r.BigEndian = true;

                    var unk = r.ReadInt32();
                    var width = r.ReadInt32();
                    var height = r.ReadInt32();
                    var dataLength = r.ReadInt32();
                    var padding = r.ReadInt32();
                    var format = r.ReadByte();
                    var data = r.GetSection(0x20, (int)(r.BaseStream.Length - 0x20));
                    
                    var bmp = HSDLib.Helpers.TPL.ConvertFromTextureMelee(data, width, height, (int)TPL_TextureFormat.CMP, null, 0, 0);
                    
                    GenericTexture t = new GenericTexture();
                    t.FromBitmap(bmp);

                    bmp.Dispose();

                    Model.TextureBank.Add("texture", t);
                    Model.MaterialBank.Add("material", new GenericMaterial() { TextureDiffuse = "texture" });
                }
            }

            using (DataReader r = new DataReader(new FileStream(skelPath, FileMode.Open)))
            {
                r.BigEndian = false;

                r.ReadInt32(); // magic
                r.ReadInt32(); // header
                var boneCount = r.ReadInt32();
                var boneOffset = r.ReadUInt32();

                using (DataReader angr = new DataReader(new FileStream(skelAngPath, FileMode.Open)))
                {
                    angr.BigEndian = false;
                    for (int i = 0; i < boneCount; i++)
                    {
                        r.Seek(boneOffset + (uint)(i * 0x20));
                        var bone = new GenericBone();
                        bone.Position = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        r.ReadSingle();
                        //r.Skip(0x10); // inverted world position
                        bone.Name = r.ReadString(r.ReadUInt32(), -1);
                        bone.ParentIndex = r.ReadInt32();
                        var flags = r.ReadInt32();
                        var angOffset = r.ReadUInt32();

                        /*angr.Seek(angOffset);
                        angr.Skip(4);
                        var posOff = angr.ReadUInt32();
                        var rotOff = angr.ReadUInt32();
                        var scaOff = angr.ReadUInt32();
                        angr.Seek(posOff);
                        bone.Position = new OpenTK.Vector3(angr.ReadSingle(), angr.ReadSingle(), angr.ReadSingle());
                        angr.Seek(rotOff);
                        bone.Rotation = new OpenTK.Vector3(angr.ReadSingle(), angr.ReadSingle(), angr.ReadSingle());
                        angr.Seek(scaOff);
                        bone.Scale = new OpenTK.Vector3(angr.ReadSingle(), angr.ReadSingle(), angr.ReadSingle());*/

                        Model.Skeleton.Bones.Add(bone);
                    }
                }
            }

            Model.Skeleton.TransformWorldToRelative();

            using (DataReader r = new DataReader(new FileStream(modelPath, FileMode.Open)))
            {
                r.BigEndian = true;

                r.ReadInt32(); // magic
                int boneCount = r.ReadInt16();
                var meshCount = r.ReadInt16();
                boneCount = r.ReadInt32();
                var meshOffset = r.ReadUInt32();
                r.ReadUInt32();
                var boneOffset = r.ReadUInt32();

                for (int i = 0; i < meshCount; i++)
                {
                    r.Seek(meshOffset + (uint)(i * 80));
                    r.Skip(0x30); // bounding stuff
                    var mesh = new GenericMesh();
                    mesh.MaterialName = "material";
                    mesh.Name = r.ReadString(r.ReadUInt32(), -1);
                    r.Skip(4*4);
                    var datInfoOffset = r.ReadUInt32();
                    Model.Meshes.Add(mesh);

                    r.Seek(datInfoOffset);
                    var flag = r.ReadInt32();
                    var bufferOffset = r.ReadUInt32();
                    var someCount = r.ReadInt32();
                    var primCount = r.ReadInt32();
                    
                    using (DataReader buffer = new DataReader(new FileStream(File.FilePath, FileMode.Open)))
                    {
                        buffer.BigEndian = true;
                        if (new string(buffer.ReadChars(4)) == "MDG5")
                            buffer.Seek(0x10);
                        else
                            buffer.Seek(0);
                        buffer.Skip(bufferOffset);
                        for(int p = 0; p < primCount; p++)
                        {
                            var primitiveType = buffer.ReadInt16();
                            var pcount = buffer.ReadInt16();

                            if(primitiveType != 0x98)
                                throw new NotSupportedException("Unknown prim type " + primitiveType.ToString("X"));

                            var strip = new List<GenericVertex>();
                            for (int v = 0; v < pcount; v++)
                            {
                                var vert = new GenericVertex();
                                vert.Pos = new OpenTK.Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
                                vert.Nrm = new OpenTK.Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
                                vert.Nrm.Normalize();

                                // Color?
                                int col = buffer.ReadInt16();
                                var R = (col >> 12) & 0xF;
                                var G = (col >> 8) & 0xF;
                                var B = (col >> 4) & 0xF;
                                var A = (col) & 0xF;
                                vert.Clr = new OpenTK.Vector4(
                                    (R | R << 4) / (float)0xFF,
                                    (G | G << 4) / (float)0xFF,
                                    (B | B << 4) / (float)0xFF,
                                    (A | A << 4) / (float)0xFF);

                                vert.UV0 = new OpenTK.Vector2(buffer.ReadInt16() / (float)0xFFF, buffer.ReadInt16() / (float)0xFFF);
                                
                                var weight1 = buffer.ReadByte() / (float)0xFF;
                                float weight2 = 0;
                                if (weight1 != 1)
                                    weight2 = 1 - weight1;
                                var bone1 = buffer.ReadByte();
                                var bone2 = buffer.ReadByte();
                                vert.Bones = new OpenTK.Vector4(bone1, bone2, 0, 0);
                                vert.Weights = new OpenTK.Vector4(weight1, weight2, 0, 0);
                                
                                strip.Add(vert);
                            }

                            TriangleConverter.StripToList(strip, out strip);

                            mesh.Vertices.AddRange(strip);
                        }
                    }

                    mesh.Optimize();
                }
            }
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }
    }
}
