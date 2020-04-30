using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.Misc
{
    public class YO3MDL : I3DModelFormat
    {
        public string Name => "Yugioh online 3 model";
        public string Extension => ".mdl";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;
        
        private GenericModel Model;

        public void Open(FileItem file)
        {
            Model = new GenericModel();
            Model.Skeleton = new GenericSkeleton();

            using (DataReader r = new DataReader(file))
            {
                r.Seek(0x0A);

                while (r.Position < r.Length)
                {
                    var type = r.ReadByte();
                    
                    switch (type)
                    {
                        case 0: // nothing?
                            break;
                        case 1: // SomeCount at beginning of file
                            r.ReadInt16();
                            break;
                        case 2: // material
                            var materialName = r.ReadString(r.ReadByte());
                            r.Skip(0x0C);
                            break;
                        case 3: // texture
                            var textureName = r.ReadString(r.ReadByte());
                            break;
                        case 0x0A: // Bone
                            var boneName = r.ReadString(r.ReadByte());
                            Model.Skeleton.Bones.Add(new GenericBone() { Name = boneName });
                            break;
                        case 0x0C: // Appears After bone but before mesh
                            break;
                        case 0x14: // mesh
                            GenericMesh mesh = new GenericMesh();
                            Model.Meshes.Add(mesh);

                            mesh.Name = r.ReadString(r.ReadByte());
                            var c1 = r.ReadByte();

                            var numOfPositions = r.ReadInt16();
                            for(uint i = 0; i < numOfPositions; i++)
                            {
                                mesh.Vertices.Add(new GenericVertex()
                                {
                                    Pos = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle())
                                });
                                mesh.Triangles.Add(i);
                            }
                            r.Skip(4);
                            break;
                        case 0x16: // normal buffer
                            var numOfUVs = r.ReadByte();
                            r.ReadByte();
                            
                            for (uint i = 0; i < numOfUVs; i++)
                            {
                                r.Skip(8);
                            }

                            r.Skip(4);
                            break;
                        case 0x19: // mesh material entry
                            var meshMaterialName = r.ReadString(r.ReadByte());
                            r.Skip(8);
                            break;
                        default:
                            r.PrintPosition();
                            Console.WriteLine("Unknown Type 0x" + type.ToString("X2"));
                            r.Position = (uint)r.Length;
                            break;
                    }
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }

        public bool Verify(FileItem file)
        {
            return file.Magic == 0x89594F33;
        }
    }
}
