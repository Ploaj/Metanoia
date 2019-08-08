using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using OpenTK;

namespace Metanoia.Formats.Misc
{
    [FormatAttribute(Extension = ".hgo", Description = "Crash")]
    public class Hgof : IModelFormat
    {
        private GenericSkeleton Skeleton = new GenericSkeleton();

        public void Open(FileItem File)
        {
            using (DataReader reader = new DataReader(new FileStream(File.FilePath, FileMode.Open)))
            {
                reader.BigEndian = true;
                reader.Seek(8);

                // 0MXT - Textures
                // LBTN - name table
                // 0TST - texture bank
                // 0OGH - object

                while (reader.Position < reader.BaseStream.Length)
                {
                    reader.PrintPosition();
                    string magic = reader.ReadString(4);
                    Console.WriteLine(magic);
                    uint sectionSize = reader.ReadUInt32();
                    var next = reader.Position + sectionSize - 8;

                    if (magic.Equals("0OGH"))
                    {
                        ReadHGO(reader);
                    }

                    reader.Position = next;
                }
            }
        }

        public void ReadHGO(DataReader reader)
        {
            {
                int count = reader.ReadByte();
                int code = reader.ReadByte(); // 0x08
                ReadSkeleton(reader, count);
            }
            {
                int count = reader.ReadByte();
                int code = reader.ReadByte(); // 0x08
                reader.ReadBytes(count);
                reader.ReadInt32();
            }
            reader.PrintPosition();

        }

        private void ReadSkeleton(DataReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // three matrices
                Matrix4 m1 = new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                reader.Position += 0xC;
                GenericBone b = new GenericBone();
                b.ParentIndex = reader.ReadSByte();
                b.Transform = m1;
                Skeleton.Bones.Add(b);
                reader.Position += 5;
            }
            Skeleton.Bones[0].Scale = new Vector3(90, 90, 90); // TODO: Remove
        }

        public GenericModel ToGenericModel()
        {
            var model = new GenericModel();
            model.Skeleton = Skeleton;

            return model;
        }
    }
}
