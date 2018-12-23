using System.Collections.Generic;
using OpenTK;

namespace Metanoia.Modeling
{
    public enum RotationOrder
    {
        XYZ,
        ZYX
    }

    public class GenericSkeleton
    {
        public List<GenericBone> Bones = new List<GenericBone>();

        public RotationOrder RotationOrder = RotationOrder.XYZ;

        public Matrix4 GetWorldTransform(int BoneIndex)
        {
            if (BoneIndex > Bones.Count || BoneIndex < 0) return Matrix4.Identity;

            GenericBone b = Bones[BoneIndex];

            return GetBoneTransform(b);
        }

        public Matrix4 GetBoneTransform(GenericBone Bone)
        {
            if (Bone.ParentIndex == -1)
                return Bone.GetTransform();
            else
                return Bone.GetTransform() * GetBoneTransform(Bones[Bone.ParentIndex]);
        }

        public int IndexOf(GenericBone Bone)
        {
            return Bones.IndexOf(Bone);
        }

        public GenericBone GetBoneByID(int id)
        {
            foreach (GenericBone b in Bones)
                if (b.ID == id)
                    return b;
            return null;
        }
    }

    public class GenericBone
    {
        public string Name;
        public int ID;
        public int ParentIndex;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public Quaternion QuaternionRotation {
            get
            {
                Quaternion x = Quaternion.FromAxisAngle(Vector3.UnitX, Rotation.X);
                Quaternion y = Quaternion.FromAxisAngle(Vector3.UnitY, Rotation.Y);
                Quaternion z = Quaternion.FromAxisAngle(Vector3.UnitZ, Rotation.Z);
                return z * y * x;
            }
        }

        public Matrix4 GetTransform()
        {
            return Matrix4.CreateFromQuaternion(QuaternionRotation) * Matrix4.CreateTranslation(Position);
        }
    }
}
