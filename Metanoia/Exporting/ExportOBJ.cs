using Metanoia.Modeling;
using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Metanoia.Exporting
{
    public class ExportOBJ : IModelExporter
    {
        public static void Save(string FilePath, GenericModel Model)
        {
            new ExportOBJ().Export(FilePath, Model);
        }

        public string Name()
        {
            return "Waveform OBJ";
        }

        public string Extension()
        {
            return ".obj";
        }

        public void Export(string FilePath, GenericModel Model)
        {
            bool UseMaterial = false;
            DialogResult dialogResult = MessageBox.Show("Export Materials and Textures?", "OBJ Exporter", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                UseMaterial = true;
                using (StreamWriter writer = new StreamWriter(new FileStream(FilePath.Replace(".obj", ".mtl"), FileMode.Create)))
                {
                    foreach(var mat in Model.MaterialBank)
                    {
                        writer.WriteLine($"newmtl {mat.Key}");
                        writer.WriteLine("Ns -3.921569");
                        writer.WriteLine("Ka 1.000000 1.000000 1.000000");
                        writer.WriteLine("Kd 1.000000 1.000000 1.000000");
                        writer.WriteLine("Ks 0.500000 0.500000 0.500000");
                        writer.WriteLine("Ke 0.000000 0.000000 0.000000");
                        writer.WriteLine("Ni -1.000000");
                        writer.WriteLine("d 1.000000");
                        writer.WriteLine("illum 2");
                        writer.WriteLine($"map_Kd {mat.Value.TextureDiffuse}");
                        writer.WriteLine($"");
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(new FileStream(FilePath, FileMode.Create)))
            {
                if (UseMaterial)
                    writer.WriteLine($"mtllib {Path.GetFileName(FilePath.Replace(".obj", ".mtl"))}");

                Dictionary<Vector3, int> positionToIndex = new Dictionary<Vector3, int>();
                Dictionary<Vector3, int> normalToIndex = new Dictionary<Vector3, int>();
                Dictionary<Vector2, int> uvToIndex = new Dictionary<Vector2, int>();
                
                foreach (var mesh in Model.Meshes)
                {
                    foreach (var v in mesh.Vertices)
                    {
                        if (!positionToIndex.ContainsKey(v.Pos))
                        {
                            writer.WriteLine($"v {v.Pos.X} {v.Pos.Y} {v.Pos.Z}");
                            positionToIndex.Add(v.Pos, positionToIndex.Count);
                        }
                    }

                    foreach (var v in mesh.Vertices)
                    {
                        if (!uvToIndex.ContainsKey(v.UV0))
                        {
                            uvToIndex.Add(v.UV0, uvToIndex.Count);
                            writer.WriteLine($"vt {v.UV0.X} {1-v.UV0.Y}");
                        }
                    }

                    foreach (var v in mesh.Vertices)
                    {
                        if (!normalToIndex.ContainsKey(v.Nrm))
                        {
                            normalToIndex.Add(v.Nrm, normalToIndex.Count);
                            writer.WriteLine($"vn {v.Nrm.X} {v.Nrm.Y} {v.Nrm.Z}");
                        }
                    }

                    if (UseMaterial)
                        writer.WriteLine($"usemtl {mesh.MaterialName}");

                    writer.WriteLine($"g {mesh.Name}");

                    for (int i = 0; i < mesh.TriangleCount; i += 3)
                    {
                        writer.Write("f");
                        var v1 = mesh.Vertices[(int)mesh.Triangles[i + 0]];
                        var v2 = mesh.Vertices[(int)mesh.Triangles[i + 1]];
                        var v3 = mesh.Vertices[(int)mesh.Triangles[i + 2]];
                        writer.Write($" {positionToIndex[v1.Pos] + 1}/{uvToIndex[v1.UV0] + 1}/{normalToIndex[v1.Nrm] + 1}");
                        writer.Write($" {positionToIndex[v2.Pos] + 1}/{uvToIndex[v2.UV0] + 1}/{normalToIndex[v2.Nrm] + 1}");
                        writer.Write($" {positionToIndex[v3.Pos] + 1}/{uvToIndex[v3.UV0] + 1}/{normalToIndex[v3.Nrm] + 1}");
                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
