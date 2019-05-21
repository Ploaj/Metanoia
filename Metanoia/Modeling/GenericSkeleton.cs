using System.Collections.Generic;
using OpenTK;
using System;
using System.Linq;
using System.ComponentModel;

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
            if (Bone == null)
                return Matrix4.Identity;
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


        /// <summary>
        /// Changes the bone transform to their relative version
        /// </summary>
        public void TransformWorldToRelative()
        {
            foreach(var root in GetRoots())
            {
                BoneWorldToRelativeTransform(root, null);
            }
        }

        private void BoneWorldToRelativeTransform(GenericBone b, GenericBone parent)
        {
            if (parent != null)
            {
                Console.WriteLine(b.Name + " " + parent.Name);
                Matrix4 worldT = b.Transform;
                Matrix4 parentT = GetBoneTransform(parent);
                parentT.Invert();
                b.Transform = parentT * b.Transform;
                if (!worldT.Equals(GetBoneTransform(b)))
                {
                    Console.WriteLine("\t" + worldT.ToString());
                    Console.WriteLine("\t" + GetBoneTransform(b).ToString());
                    Console.WriteLine("\t" + GetBoneTransform(parent).ToString());
                    Console.WriteLine("\t" + parentT.ToString());
                }
            }

            foreach (var child in GetChildren(b))
                BoneWorldToRelativeTransform(child, b);
        }

        public GenericBone[] GetChildren(GenericBone b)
        {
            var index = Bones.IndexOf(b);
            if (index == -1) return null;
            return Bones.Where(e => e.ParentIndex == index).ToArray();
        }

        public GenericBone[] GetRoots()
        {
            return Bones.Where(e => e.ParentIndex == -1).ToArray();
        }
    }

    public class GenericBone
    {
        [ReadOnly(true), Category("Properties")]
        public string Name { get; set; }
        public int ID;
        public int ParentIndex = -1;
        
        public bool Selected = false;

        [ReadOnly(true), Category("Transforms")]
        public Vector3 Position { get; set; }

        [ReadOnly(true), Category("Transforms")]
        public Vector3 Rotation { get; set; }

        [ReadOnly(true), Category("Transforms")]
        public Vector3 Scale { get; set; } = Vector3.One;

        [Browsable(false)]
        public Quaternion QuaternionRotation {
            get
            {
                Quaternion x = Quaternion.FromAxisAngle(Vector3.UnitX, Rotation.X);
                Quaternion y = Quaternion.FromAxisAngle(Vector3.UnitY, Rotation.Y);
                Quaternion z = Quaternion.FromAxisAngle(Vector3.UnitZ, Rotation.Z);
                return z * y * x;
            }
            set
            {
                Rotation = ToEulerAngles(value);
            }
        }

        public Matrix4 GetTransform()
        {
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(QuaternionRotation) * Matrix4.CreateTranslation(Position);
        }

        public Matrix4 Transform
        {
            get
            {
                return GetTransform();
            }
            set
            {
                Position = value.ExtractTranslation();
                Scale = value.ExtractScale();
                QuaternionRotation = value.ExtractRotation();
            }
        }


        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static Vector3 ToEulerAngles(Quaternion q)
        {
            Matrix4 mat = Matrix4.CreateFromQuaternion(q);
            float x, y, z;

            y = (float)Math.Asin(-Clamp(mat.M31, -1, 1));

            if (Math.Abs(mat.M31) < 0.99999)
            {
                x = (float)Math.Atan2(mat.M32, mat.M33);
                z = (float)Math.Atan2(mat.M21, mat.M11);
            }
            else
            {
                x = 0;
                z = (float)Math.Atan2(-mat.M12, mat.M22);
            }
            return new Vector3(x, y, z);
        }
    }
}
