using Metanoia.Modeling;
namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_MBN
    {
        public static GenericBone ToBone(byte[] data)
        {
            GenericBone bone = new GenericBone();
            using (DataReader r = new DataReader(new System.IO.MemoryStream(data)))
            {
                bone.ID = r.ReadInt32();
                bone.ParentIndex = r.ReadInt32();
                r.Skip(4);

                r.Seek(0xC);
                bone.Position = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                var rot = new OpenTK.Matrix3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                bone.QuaternionRotation = rot.ExtractRotation().Inverted();
                bone.Scale = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                
            }
            return bone;
        }
    }
}
