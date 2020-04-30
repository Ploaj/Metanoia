using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.Misc
{
    /// <summary>
    ///  TODO: incomplete
    /// </summary>
    public class G4PKM : I3DModelFormat
    {
        public string Name => "Yokai watch model";

        public string Extension => ".g4pkm";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        private GenericModel model = new GenericModel();

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = false;
                r.Seek(0x48);
                var skelLength = r.ReadInt32();
                var modLength = r.ReadInt32();

                r.Seek(0x80);
                var skelData = r.ReadBytes(skelLength);
                if (r.Position % 0x20 != 0)
                    r.Position += 0x20 - (r.Position % 0x20);
                var modData = r.ReadBytes(modLength);

                ReadSkelData(skelData);

                ReadModelData(modData);
            }
        }

        private void ReadSkelData(byte[] data)
        {
            model.Skeleton = new GenericSkeleton();
            using (DataReader r = new DataReader(data))
            {
                r.BigEndian = false;

                r.Seek(4);
                var dataOff = r.ReadUInt16();
                r.Seek(0x20);
                var boneCount = r.ReadInt16();

                r.Seek(dataOff);

                r.Skip(0x30 * (uint)boneCount); // transforms
                r.Skip(0x30 * (uint)boneCount); // transforms


                for (int i = 0; i < boneCount; i++)
                {
                    var bone = new GenericBone();
                    bone.Scale = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    r.ReadSingle();
                    bone.QuaternionRotation = new OpenTK.Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()).Inverted();
                    bone.Position = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    r.ReadSingle();
                    model.Skeleton.Bones.Add(bone);
                }

                for (int i = 0; i < boneCount; i++)
                {
                    var hash = r.ReadUInt32();
                }

                for (int i = 0; i < boneCount; i++)
                {
                    var parent = r.ReadInt16() - 1;
                    if (parent == boneCount - 1)
                        parent = -1;
                    model.Skeleton.Bones[i].ParentIndex = parent;
                }

                r.Skip((uint)boneCount * 2);
                r.Skip((uint)boneCount);
                r.Skip((uint)boneCount * 4); // more hashes?
                
                var strStart = r.Position;
                for (int i = 0; i < boneCount; i++)
                {
                    model.Skeleton.Bones[i].Name = r.ReadString(r.ReadUInt16() + strStart, -1);
                }
            }
        }

        private void ReadModelData(byte[] data)
        {

        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            return model;
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "G4PK";
        }
    }
}
