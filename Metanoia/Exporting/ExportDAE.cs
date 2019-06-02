using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using OpenTK;

namespace Metanoia.Exporting
{
    [ExportAttribute(Name = "Collada", Extension = ".dae", ExportType = ExportType.Model)]
    public class ExportDAE
    {
        public static void Save(string FilePath, GenericModel Model)
        {
            using (DAEWriter writer = new DAEWriter(FilePath))
            {
                writer.WriteAsset();

                writer.WriteLibraryImages();

                if(Model.Skeleton.Bones != null)
                foreach (var bone in Model.Skeleton.Bones)
                {
                    float[] Transform = new float[] { bone.Transform.M11, bone.Transform.M21, bone.Transform.M31, bone.Transform.M41,
                    bone.Transform.M12, bone.Transform.M22, bone.Transform.M32, bone.Transform.M42,
                    bone.Transform.M13, bone.Transform.M23, bone.Transform.M33, bone.Transform.M43,
                    bone.Transform.M14, bone.Transform.M24, bone.Transform.M34, bone.Transform.M44 };
                        Matrix4 InvWorldTransform = Model.Skeleton.GetBoneTransform(bone).Inverted();
                        float[] InvTransform = new float[] { InvWorldTransform.M11, InvWorldTransform.M21, InvWorldTransform.M31, InvWorldTransform.M41,
                    InvWorldTransform.M12, InvWorldTransform.M22, InvWorldTransform.M32, InvWorldTransform.M42,
                    InvWorldTransform.M13, InvWorldTransform.M23, InvWorldTransform.M33, InvWorldTransform.M43,
                    InvWorldTransform.M14, InvWorldTransform.M24, InvWorldTransform.M34, InvWorldTransform.M44 };
                        writer.AddJoint(bone.Name, bone.ParentIndex == -1 ? "" : Model.Skeleton.Bones[bone.ParentIndex].Name, Transform, InvTransform);
                }
                
                writer.StartGeometrySection();
                foreach(var mesh in Model.Meshes)
                {
                    writer.StartGeometryMesh(mesh.Name);

                    List<float> Positions = new List<float>();
                    List<float> Normals = new List<float>();
                    List<float> UV0s = new List<float>();
                    List<float> Colors = new List<float>();
                    List<float[]> BoneWeights = new List<float[]>();
                    List<int[]> BoneIndices = new List<int[]>();

                    foreach (var vert in mesh.Vertices)
                    {
                        Positions.AddRange(new float[] { vert.Pos.X, vert.Pos.Y, vert.Pos.Z });
                        Normals.AddRange(new float[] { vert.Nrm.X, vert.Nrm.Y, vert.Nrm.Z });
                        Colors.AddRange(new float[] { vert.Clr.X, vert.Clr.Y, vert.Clr.Z, vert.Clr.W });
                        UV0s.AddRange(new float[] { vert.UV0.X, vert.UV0.Y });
                        List<float> weights = new List<float>();
                        List<int> bones = new List<int>();
                        for(int i =0; i < 4; i++)
                        {
                            if (vert.Weights[i] > 0)
                            {
                                weights.Add(vert.Weights[i]);
                                bones.Add((int)vert.Bones[i]);
                            }
                        }
                        BoneWeights.Add(weights.ToArray());
                        BoneIndices.Add(bones.ToArray());
                    }

                    writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.POSITION, Positions.ToArray(), mesh.Triangles.ToArray());

                    writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.NORMAL, Normals.ToArray(), mesh.Triangles.ToArray());

                    writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.TEXCOORD, UV0s.ToArray(), mesh.Triangles.ToArray());

                    writer.WriteGeometrySource(mesh.Name, DAEWriter.VERTEX_SEMANTIC.COLOR, Colors.ToArray(), mesh.Triangles.ToArray());

                    writer.AttachGeometryController(BoneIndices, BoneWeights);

                    writer.EndGeometryMesh();
                }
                writer.EndGeometrySection();
            }
            
        }
    }
}
