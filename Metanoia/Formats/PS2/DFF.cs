using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using Metanoia.Formats.Misc.Renderware;
using OpenTK;

namespace Metanoia.Formats.PS2
{
    public class DFF : I3DModelFormat
    {
        public string Name => "";
        public string Extension => ".dff";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        public GenericModel Model = new GenericModel();

        public void Open(FileItem file)
        {
            using (RenderWareBinaryStreamReader r = new RenderWareBinaryStreamReader(file))
            {
                if(r.ReadSection() is RWClump clump)
                {
                    Model.Skeleton = new GenericSkeleton();

                    foreach(var skel in clump.FrameList.Frames)
                    {
                        var bone = new GenericBone();
                        bone.Transform = new Matrix4(skel.Transform);
                        bone.Position = skel.Position;
                        bone.ParentIndex = skel.Parent;
                        Model.Skeleton.Bones.Add(bone);
                    }

                    Console.WriteLine("Clump " + clump.FrameList.Frames.Length);
                }
            }
        }

        public void Save(string filePath)
        {
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }

        public bool Verify(FileItem file)
        {
            return file.Extension == ".dff";
        }
    }

    public class DFFSection
    {

    }

    public class DFFChunk
    {

    }
}
