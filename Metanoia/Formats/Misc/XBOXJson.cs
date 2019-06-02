using Metanoia.Modeling;
using Newtonsoft.Json.Linq;

namespace Metanoia.Formats.Misc
{
    [Format(Extension = ".json", Description = "XBOX json")]
    public class XBOXJson : IModelFormat
    {
        private GenericModel Model;

        public void Open(byte[] Data)
        {
            dynamic stuff = JObject.Parse(System.Text.Encoding.UTF8.GetString(Data));
            Model = new GenericModel();
            Model.Skeleton = new GenericSkeleton();
            
            var skeletons = stuff.skeletons;

            foreach(var skel in skeletons)
            {
                if (((string)skel.name).Contains("carryable"))
                {
                    foreach(var bone in skel.bones)
                    {
                        var gBone = new GenericBone();
                        Model.Skeleton.Bones.Add(gBone);
                        gBone.Name = "Bone_" + bone.index;
                        gBone.ParentIndex = bone.parent;
                        gBone.Position = new OpenTK.Vector3((float)bone.local.position[0], (float)bone.local.position[1], (float)bone.local.position[2]);
                        gBone.QuaternionRotation =new OpenTK.Quaternion((float)bone.local.rotation[0], (float)bone.local.rotation[1], (float)bone.local.rotation[2], (float)bone.local.rotation[3]).Inverted();
                        gBone.Scale = new OpenTK.Vector3((float)bone.local.scale[0], (float)bone.local.scale[1], (float)bone.local.scale[2]);
                    }
                    break;
                }
            }

            var models = stuff.models;

            foreach (var modl in models)
            {
                foreach(var batch in modl.batches)
                {
                    GenericMesh mesh = new GenericMesh();
                    Model.Meshes.Add(mesh);
                    mesh.Name = batch.id;

                    foreach(int index in batch.indices)
                    {
                        var vertex = new GenericVertex();
                        vertex.Pos = new OpenTK.Vector3((float)batch.positions[index * 3 + 0], (float)batch.positions[index * 3 + 1], (float)batch.positions[index * 3 + 2]);
                        vertex.Nrm = new OpenTK.Vector3((float)batch.normals[index * 3 + 0], (float)batch.normals[index * 3 + 1], (float)batch.normals[index * 3 + 2]);
                        vertex.UV0 = new OpenTK.Vector2((float)batch.uvs[index * 2 + 0], (float)batch.uvs[index * 2 + 1]);
                        vertex.UV1 = new OpenTK.Vector2((float)batch.uvs2[index * 2 + 0], (float)batch.uvs2[index * 2 + 1]);
                        vertex.UV2 = new OpenTK.Vector2((float)batch.uvs3[index * 2 + 0], (float)batch.uvs3[index * 2 + 1]);
                        vertex.Clr = new OpenTK.Vector4((float)batch.colors[index * 4 + 0] * 255, (float)batch.colors[index * 4 + 1] * 255, (float)batch.colors[index * 4 + 2] * 255, (float)batch.colors[index * 4 + 3] * 255);
                        vertex.Bones = new OpenTK.Vector4((int)batch.bindings[index * 4 + 0], (int)batch.bindings[index * 4 + 1], (int)batch.bindings[index * 4 + 2], (int)batch.bindings[index * 4 + 3]);
                        vertex.Weights = new OpenTK.Vector4(((int)batch.weights[index * 4 + 0]) / 255f, ((int)batch.weights[index * 4 + 1]) / 255f, ((int)batch.weights[index * 4 + 2]) / 255f, ((int)batch.weights[index * 4 + 3]) / 255f);
                        mesh.Vertices.Add(vertex);
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
