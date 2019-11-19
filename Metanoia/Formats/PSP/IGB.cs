using System;
using System.Collections.Generic;
using Metanoia.Modeling;
using DivaAlchemy;
using DivaAlchemy.Helpers;
using OpenTK;

namespace Metanoia.Formats.PSP
{
    public class IGB : I3DModelFormat
    {
        public string Name => "Intrinsic Games Binary";
        public string Extension => ".igb";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool Verify(FileItem file)
        {
            return file.Extension.Equals(Extension);
        }

        private GenericSkeleton Skeleton = new GenericSkeleton();
        private GenericModel Model = new GenericModel();

        public void Open(FileItem File)
        {
            var igb = new IGBFile(File.GetFileBinary());

            var igbSkel = new IGSkeleton(igb);

            Matrix4[] transforms = new Matrix4[igbSkel.Bones.Count];
            Matrix4[] inverts = new Matrix4[igbSkel.Bones.Count];
            int boneIndex = 0;
            foreach (var v in igbSkel.Bones)
            {
                if (v == null)
                    continue;
                GenericBone b = new GenericBone();
                b.Name = v.Name;
                b.ParentIndex = v.ParentIndex;
                transforms[boneIndex] = new Matrix4(
                    v.WorldInverseMatrix.M11, v.WorldInverseMatrix.M12, v.WorldInverseMatrix.M13, v.WorldInverseMatrix.M14,
                    v.WorldInverseMatrix.M21, v.WorldInverseMatrix.M22, v.WorldInverseMatrix.M23, v.WorldInverseMatrix.M24,
                    v.WorldInverseMatrix.M31, v.WorldInverseMatrix.M32, v.WorldInverseMatrix.M33, v.WorldInverseMatrix.M34,
                    v.WorldInverseMatrix.M41, v.WorldInverseMatrix.M42, v.WorldInverseMatrix.M43, v.WorldInverseMatrix.M44);
                inverts[boneIndex] = transforms[boneIndex].Inverted();
                b.Transform = inverts[boneIndex];

                boneIndex++;
                Skeleton.Bones.Add(b);
            }
            
            foreach (var b in Skeleton.Bones)
            {
                if (b.ParentIndex != -1)
                {
                    b.Transform = inverts[b.ParentIndex] * transforms[Skeleton.Bones.IndexOf(b)];
                    b.Transform = b.Transform.Inverted();
                    var position = igbSkel.Bones[Skeleton.Bones.IndexOf(b)].Position;
                    b.Position = new Vector3(position.X, position.Y, position.Z);
                }
            }

            var vertexBuffers = IGVertexAccessor.ToDivaModels(igb);

            Model.Skeleton = Skeleton;

            Console.WriteLine(vertexBuffers.Count);
            foreach (var model in vertexBuffers)
            {
                GenericMesh m = new GenericMesh();
                m.Name = model.Name;
                if (m.Name.Equals(""))
                    m.Name = "Mesh_" + vertexBuffers.IndexOf(model);
                Console.WriteLine(m.Name + " " + !(model.Texture==null));

                if(model.Texture != null)
                {
                    GenericTexture t = new GenericTexture();
                    t.Name = System.IO.Path.GetFileNameWithoutExtension(model.Texture.Name);
                    t.Width = (uint)model.Texture.Width;
                    t.Height = (uint)model.Texture.Height;
                    t.Mipmaps.Add(model.Texture.RGBA);

                    if(!Model.MaterialBank.ContainsKey(t.Name))
                        Model.TextureBank.Add(t.Name, t);

                    GenericMaterial mat = new GenericMaterial();
                    mat.TextureDiffuse = t.Name;

                    if (!Model.MaterialBank.ContainsKey(t.Name))
                        Model.MaterialBank.Add(t.Name, mat);

                    m.MaterialName = t.Name;
                }
                
                foreach (var mesh in model.Mesh)
                {
                    var vertices = ToGenericVertices(mesh.Vertices);
                    foreach (var dl in mesh.DisplayList)
                    {
                        if (dl.PrimitiveType == PrimType.Triangles)
                        {
                            foreach(var f in dl.Indices)
                                m.Vertices.Add(vertices[f]);
                        }
                        if (dl.PrimitiveType == PrimType.TriangleStrip)
                        {
                            var tempList = new List<GenericVertex>();
                            foreach (var f in dl.Indices)
                                tempList.Add(vertices[f]);
                            Tools.TriangleConverter.StripToList(tempList, out tempList);
                            m.Vertices.AddRange(tempList);
                        }
                    }
                }
                

                if(model.SingleBindBone != null && model.SingleBindBone != "")
                {
                    var singleBone = Skeleton.Bones.Find(e => e.Name.Equals(model.SingleBindBone));
                    var singleBindTransform = Skeleton.GetWorldTransform(singleBone);
                    var singleBindIndex = Skeleton.Bones.IndexOf(singleBone);
                    for (int i = 0; i < m.VertexCount;i++)
                    {
                        var vert = m.Vertices[i];
                        vert.Pos = Vector3.TransformPosition(vert.Pos, singleBindTransform);
                        vert.Nrm = Vector3.TransformNormal(vert.Nrm, singleBindTransform);
                        vert.Bones = new Vector4(singleBindIndex, 0, 0, 0);
                        vert.Weights = new Vector4(1, 0, 0, 0);
                        m.Vertices[i] = vert;
                    }
                }

                m.Optimize();

                Model.Meshes.Add(m);
            }
        }

        private static List<GenericVertex> ToGenericVertices(List<DivaVertex> inVerts)
        {
            List<GenericVertex> outVerts = new List<GenericVertex>();

            foreach(var v in inVerts)
            {
                var vertex = new GenericVertex()
                {
                    Pos = new Vector3(v.Position.X, v.Position.Y, v.Position.Z),
                    Nrm = new Vector3(v.Normal.X, v.Normal.Y, v.Normal.Z),
                    UV0 = new Vector2(v.UV0.X, v.UV0.Y),
                    Clr = new Vector4(v.Color.X, v.Color.Y, v.Color.Z, v.Color.W)
                };
                int weightCount = 0;
                if(v.Weights.X > 0)
                {
                    vertex.Bones[weightCount] = v.Bones.X;
                    vertex.Weights[weightCount++] = v.Weights.X;
                }
                if (v.Weights.Y > 0)
                {
                    vertex.Bones[weightCount] = v.Bones.Y;
                    vertex.Weights[weightCount++] = v.Weights.Y;
                }
                if (v.Weights.Z > 0)
                {
                    vertex.Bones[weightCount] = v.Bones.Z;
                    vertex.Weights[weightCount++] = v.Weights.Z;
                }
                if (v.Weights.W > 0)
                {
                    vertex.Bones[weightCount] = v.Bones.W;
                    vertex.Weights[weightCount++] = v.Weights.W;
                }
                if (v.WeightsExt.X > 0)
                {
                    vertex.Bones[weightCount] = v.BonesExt.X;
                    vertex.Weights[weightCount++] = v.WeightsExt.X;
                }
                if (v.WeightsExt.Y > 0)
                {
                    vertex.Bones[weightCount] = v.BonesExt.Y;
                    vertex.Weights[weightCount++] = v.WeightsExt.Y;
                }
                if (v.WeightsExt.Z > 0)
                {
                    vertex.Bones[weightCount] = v.BonesExt.Z;
                    vertex.Weights[weightCount++] = v.WeightsExt.Z;
                }
                if (v.WeightsExt.W > 0)
                {
                    vertex.Bones[weightCount] = v.BonesExt.W;
                    vertex.Weights[weightCount++] = v.WeightsExt.W;
                }
                outVerts.Add(vertex);
            }

            return outVerts;
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }
    }
}
