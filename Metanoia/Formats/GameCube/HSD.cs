using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using HSDLib;
using HSDLib.Common;
using System.Diagnostics;
using OpenTK;
using HSDLib.Helpers;
using HSDLib.GX;
using System.Drawing;

namespace Metanoia.Formats.GameCube
{
    [FormatAttribute(Extension = ".dat", Description = "HAL DAT")]
    public class HSD : IModelFormat
    {
        private GenericSkeleton skeleton = new GenericSkeleton();

        private GenericModel outModel = new GenericModel();

        private Dictionary<IHSDNode, int> jobjToIndex = new Dictionary<IHSDNode, int>();

        private Dictionary<byte[], int> tobjToIndex = new Dictionary<byte[], int>();

        public void Open(byte[] Data)
        {
            var r = new HSDFile();
            System.IO.File.WriteAllBytes("temp.bin", Data);
            r.Decompile("temp.bin");
            
            foreach(var root in r.Roots)
            {
                Debug.WriteLine(root.Name);

                ParseJOBJs(root.Node, null);

                if(root.Node is HSD_JOBJ jobj)
                {
                    List<HSD_JOBJ> BoneList = jobj.DepthFirstList;

                    ParseDOBJs(root.Node, null, BoneList);
                }
            }
        }
        
        private void ParseJOBJs(IHSDNode node, IHSDNode parent)
        {
            //Debug.WriteLine(node.GetType());
            if(node is HSD_JOBJ jobj)
            {
                var bone = new GenericBone();
                bone.Name = "JOBJ_" + skeleton.Bones.Count;
                jobjToIndex.Add(node, skeleton.Bones.Count);
                skeleton.Bones.Add(bone);
                bone.Position = new Vector3(jobj.Transforms.TX, jobj.Transforms.TY, jobj.Transforms.TZ);
                bone.Rotation = new Vector3(jobj.Transforms.RX, jobj.Transforms.RY, jobj.Transforms.RZ);
                bone.Scale = new Vector3(jobj.Transforms.SX, jobj.Transforms.SY, jobj.Transforms.SZ);
                if(parent != null)
                    bone.ParentIndex = jobjToIndex[parent];
            }
            
            foreach(var child in node.Children)
                ParseJOBJs(child, node);
        }

        private void ParseDOBJs(IHSDNode node, IHSDNode parent, List<HSD_JOBJ> BoneList)
        {
            if (node is HSD_DOBJ dobj)
            {
                GenericMesh mesh = new GenericMesh();
                mesh.Name = "Mesh_" + outModel.Meshes.Count;

                GenericMaterial mat = new GenericMaterial();
                mesh.MaterialName = "material_" + outModel.MaterialBank.Count;
                outModel.MaterialBank.Add(mesh.MaterialName, mat);

                var Xscale = 1;
                var Yscale = 1;
                
                if(dobj.MOBJ != null)
                {
                    if(dobj.MOBJ.Textures != null)
                    {
                        var tobj = dobj.MOBJ.Textures;

                        mat.SWrap = GXTranslator.toWrapMode(tobj.WrapS);
                        mat.TWrap = GXTranslator.toWrapMode(tobj.WrapT);

                        Xscale = tobj.WScale;
                        Yscale = tobj.HScale;

                        if (!tobjToIndex.ContainsKey(tobj.ImageData.Data))
                        {
                            Bitmap B = null;
                            if (tobj.ImageData != null)
                            {
                                if (tobj.Tlut != null)
                                    B = TPL.ConvertFromTextureMelee(tobj.ImageData.Data, tobj.ImageData.Width, tobj.ImageData.Height, (int)tobj.ImageData.Format, tobj.Tlut.Data, tobj.Tlut.ColorCount, (int)tobj.Tlut.Format);
                                else
                                    B = TPL.ConvertFromTextureMelee(tobj.ImageData.Data, tobj.ImageData.Width, tobj.ImageData.Height, (int)tobj.ImageData.Format, null, 0, 0);

                            }
                            GenericTexture t = new GenericTexture();
                            t.FromBitmap(B);
                            B.Dispose();

                            tobjToIndex.Add(tobj.ImageData.Data, outModel.TextureBank.Count);
                            outModel.TextureBank.Add("texture_" + outModel.TextureBank.Count, t);
                        }

                        mat.TextureDiffuse = outModel.TextureBank.Keys.ToArray()[tobjToIndex[tobj.ImageData.Data]];
                    }
                }

                outModel.Meshes.Add(mesh);
                if (dobj.POBJ != null)
                    foreach (HSD_POBJ pobj in dobj.POBJ.List)
                    {
                        // Decode the Display List Data
                        GXDisplayList DisplayList = new GXDisplayList(pobj.DisplayListBuffer, pobj.VertexAttributes);
                        var Vertices = ToGenericVertex(VertexAccessor.GetDecodedVertices(DisplayList, pobj), BoneList, pobj.BindGroups != null ? new List<HSD_JOBJWeight>(pobj.BindGroups.Elements) : null, parent);
                        int bufferOffset = 0;
                        foreach (GXPrimitiveGroup g in DisplayList.Primitives)
                        {
                            var primitiveType = GXTranslator.toPrimitiveType(g.PrimitiveType);
                            
                            var strip = new List<GenericVertex>();
                            for (int i = bufferOffset; i < bufferOffset + g.Count; i++)
                                strip.Add(Vertices[i]);
                            bufferOffset += g.Count;

                            switch (primitiveType)
                            {
                                case OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip:
                                    Tools.TriangleConverter.StripToList(strip, out strip);
                                    break;
                                case OpenTK.Graphics.OpenGL.PrimitiveType.Quads:
                                    Tools.TriangleConverter.QuadToList(strip, out strip);
                                    break;
                                case OpenTK.Graphics.OpenGL.PrimitiveType.Triangles:
                                    break;
                                default:
                                    Debug.WriteLine("Error converting primitive type " + primitiveType);
                                    break;
                            }

                            mesh.Vertices.AddRange(strip);
                        }
                    }

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var vert = mesh.Vertices[i];
                    vert.UV0 = new Vector2(vert.UV0.X * Xscale, vert.UV0.Y * Yscale);
                    mesh.Vertices[i] = vert;
                }
                mesh.Optimize();
                Tools.TriangleConverter.ReverseFaces(mesh.Triangles, out mesh.Triangles);
            }

            foreach (var child in node.Children)
                ParseDOBJs(child, node, BoneList);
        }

        private List<GenericVertex> ToGenericVertex(GXVertex[] InVerts, List<HSD_JOBJ> BoneList, List<HSD_JOBJWeight> WeightList, IHSDNode parent)
        {
            var finalList = new List<GenericVertex>(InVerts.Length);

            var transform = skeleton.GetBoneTransform(skeleton.Bones[jobjToIndex[parent]]);

            foreach (var inVert in InVerts)
            {
                GenericVertex vertex = new GenericVertex();
                vertex.Pos = Vector3.TransformPosition(new Vector3(inVert.Pos.X, inVert.Pos.Y, inVert.Pos.Z), transform);
                vertex.Nrm = Vector3.TransformNormal(new Vector3(inVert.Nrm.X, inVert.Nrm.Y, inVert.Nrm.Z), transform);
                vertex.UV0 = new Vector2(inVert.TEX0.X, inVert.TEX0.Y);
                if (inVert.Clr0.A != 0 || inVert.Clr0.R != 0 || inVert.Clr0.G != 0 || inVert.Clr0.B != 0)
                    vertex.Clr = new Vector4(inVert.Clr0.R, inVert.Clr0.G, inVert.Clr0.B, inVert.Clr0.A);

                vertex.Bones = new Vector4(jobjToIndex[parent], 0, 0, 0);
                vertex.Weights = new Vector4(1, 0, 0, 0);

                if (WeightList != null)
                {
                    var weightList = WeightList[inVert.PMXID / 3];
                    // single bind fix
                    if (weightList != null && weightList.JOBJs.Count == 1)
                    {
                        vertex.Pos = Vector3.TransformPosition(vertex.Pos, skeleton.GetBoneTransform(skeleton.Bones[jobjToIndex[weightList.JOBJs[0]]]));
                        vertex.Nrm = Vector3.TransformNormal(vertex.Nrm, skeleton.GetBoneTransform(skeleton.Bones[jobjToIndex[weightList.JOBJs[0]]]));
                    }

                    //Bone Weights
                    for(int i = 0; i < weightList.Weights.Count; i++)
                    {
                        vertex.Bones[i] = jobjToIndex[weightList.JOBJs[i]];
                        vertex.Weights[i] = weightList.Weights[i];
                    }
                }

                finalList.Add(vertex);
            }

            return finalList;
        }

        public GenericModel ToGenericModel()
        {
            outModel.Skeleton = skeleton;

            return outModel;
        }
    }
}
