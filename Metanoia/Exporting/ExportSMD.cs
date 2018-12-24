using System.IO;
using Metanoia.Modeling;
using System.Text;
using System.Collections.Generic;

namespace Metanoia.Exporting
{
    [ExportAttribute(Name = "Source Model", Extension = ".smd", ExportType = ExportType.Model)]
    public class ExportSMD
    {
        public static void Save(string FilePath, GenericModel Model)
        {
            using (StreamWriter w = new StreamWriter(new FileStream(FilePath, FileMode.Create)))
            {
                w.WriteLine("version 1");

                if(Model.Skeleton != null)
                {
                    w.WriteLine("nodes");
                    foreach(GenericBone bone in Model.Skeleton.Bones)
                    {
                        w.WriteLine($" {Model.Skeleton.IndexOf(bone)} \"{bone.Name}\" {bone.ParentIndex}");
                    }
                    w.WriteLine("end");
                    w.WriteLine("skeleton");
                    w.WriteLine("time 0");
                    foreach (GenericBone bone in Model.Skeleton.Bones)
                    {
                        w.WriteLine($" {Model.Skeleton.IndexOf(bone)} {bone.Position.X} {bone.Position.Y} {bone.Position.Z} {bone.Rotation.X} {bone.Rotation.Y} {bone.Rotation.Z}");
                    }
                    w.WriteLine("end");
                }

                w.WriteLine("triangles");
                Dictionary<GenericTexture, string> TextureBank = new Dictionary<GenericTexture, string>();
                foreach (GenericMesh m in Model.Meshes)
                {
                    string MaterialName = m.Name;
                    if(m.Material != null)
                    {
                        if (m.Material.TextureDiffuse != null)
                        {
                            if (TextureBank.ContainsKey(m.Material.TextureDiffuse))
                            {
                                MaterialName = TextureBank[m.Material.TextureDiffuse];
                            }
                            else
                            {
                                string TextureName = "Texture_" + TextureBank.Count + ".png";
                                Metanoia.Rendering.RenderTexture Temp = new Metanoia.Rendering.RenderTexture();
                                Temp.LoadGenericTexture(m.Material.TextureDiffuse);
                                Temp.ExportPNG(new FileInfo(FilePath).Directory.FullName + "/" + TextureName);
                                Temp.Delete();
                                TextureBank.Add(m.Material.TextureDiffuse, TextureName);
                                MaterialName = TextureName;
                            }
                        }
                    }
                    for(int i = 0; i < m.Triangles.Count; i+=3)
                    {
                        w.WriteLine(MaterialName);
                        {
                            GenericVertex v = m.Vertices[(int)m.Triangles[i]];
                            w.WriteLine($" 0 {v.Pos.X} {v.Pos.Y} {v.Pos.Z} {v.Nrm.X} {v.Nrm.Y} {v.Nrm.Z} {v.UV0.X} {v.UV0.Y} " + WriteWeights(v));
                        }
                        {
                            GenericVertex v = m.Vertices[(int)m.Triangles[i+1]];
                            w.WriteLine($" 0 {v.Pos.X} {v.Pos.Y} {v.Pos.Z} {v.Nrm.X} {v.Nrm.Y} {v.Nrm.Z} {v.UV0.X} {v.UV0.Y} " + WriteWeights(v));
                        }
                        {
                            GenericVertex v = m.Vertices[(int)m.Triangles[i+2]];
                            w.WriteLine($" 0 {v.Pos.X} {v.Pos.Y} {v.Pos.Z} {v.Nrm.X} {v.Nrm.Y} {v.Nrm.Z} {v.UV0.X} {v.UV0.Y} " + WriteWeights(v));
                        }
                    }
                }
                w.WriteLine("end");

                w.Close();
            }
        }

        public static string WriteWeights(GenericVertex v)
        {
            StringBuilder o = new StringBuilder();

            int Count = 0;

            if(v.Weights.X != 0)
            {
                Count++;
                o.Append($"{v.Bones.X} {v.Weights.X} ");
            }
            if (v.Weights.Y != 0)
            {
                Count++;
                o.Append($"{v.Bones.Y} {v.Weights.Y} ");
            }
            if (v.Weights.Z != 0)
            {
                Count++;
                o.Append($"{v.Bones.Z} {v.Weights.Z} ");
            }
            if (v.Weights.W != 0)
            {
                Count++;
                o.Append($"{v.Bones.W} {v.Weights.W} ");
            }


            return Count + " " + o.ToString();
        }

    }
}
