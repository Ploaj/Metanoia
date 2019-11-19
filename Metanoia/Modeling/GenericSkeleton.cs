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

        public GenericBone[] BreathFirst
        {
            get
            {
                List<GenericBone> bones = new List<GenericBone>();

                foreach(var b in Bones)
                {
                    if(b.ParentIndex == -1)
                    {
                        GetBreathFirst(b, bones);
                    }
                }

                return bones.ToArray();
            }
        }

        public GenericBone GetBoneByName(string name)
        {
            return Bones.Find(e => e.Name == name);
        }

        private void GetBreathFirst(GenericBone bone, List<GenericBone> bones)
        {
            bones.Add(bone);

            var index = Bones.IndexOf(bone);
            foreach(var v in Bones.FindAll(e=>e.ParentIndex == index))
            {
                GetBreathFirst(v, bones);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="BoneIndex"></param>
        /// <returns></returns>
        public Matrix4 GetWorldTransform(int BoneIndex)
        {
            if (BoneIndex > Bones.Count || BoneIndex < 0) return Matrix4.Identity;

            GenericBone b = Bones[BoneIndex];

            return GetWorldTransform(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Matrix4[] GetBindTransforms()
        {
            Matrix4[] inv = new Matrix4[Bones.Count];

            for (int i = 0; i < Bones.Count; i++)
                inv[i] = GetWorldTransform(Bones[i], false).Inverted() * GetWorldTransform(Bones[i], true);

            return inv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Bone"></param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Matrix4 GetWorldTransform(GenericBone Bone, bool animated = false)
        {
            if (Bone == null)
                return Matrix4.Identity;
            if (Bone.ParentIndex == -1)
                return Bone.GetTransform(animated);
            else
                return Bone.GetTransform(animated) * GetWorldTransform(Bones[Bone.ParentIndex], animated);
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
                BoneWorldToRelativeTransform(root);
            }
        }

        private void BoneWorldToRelativeTransform(GenericBone b)
        {
            if(b.ParentIndex != -1)
            {
                Matrix4 parentT = GetWorldTransform(b.ParentIndex).Inverted();
                b.Transform = parentT * b.Transform;
            }

            foreach (var child in GetChildren(b))
                BoneWorldToRelativeTransform(child);
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

        public uint NameHash
        {
            get
            {
                return Tools.CRC32.Crc32C(Name);
            }
        }
        
        public bool Selected = false;

        [ReadOnly(true), Category("Animated")]
        public Matrix4 AnimatedTransform { get; set; } = Matrix4.Identity;

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
                Rotation = ToEulerAngles(Matrix4.CreateFromQuaternion(value));
            }
        }

        public Matrix4 GetTransform(bool animated)
        {
            if (animated && AnimatedTransform != Matrix4.Identity)
                return AnimatedTransform;
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(QuaternionRotation) * Matrix4.CreateTranslation(Position);
        }

        public Matrix4 Transform
        {
            get
            {
                return GetTransform(false);
            }
            set
            {
                Position = value.ExtractTranslation();
                Scale = value.ExtractScale();
                Rotation = ToEulerAngles(value.Inverted());
            }
        }


        private static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public static Vector3 ToEulerAngles(Quaternion q)
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

        public static Vector3 ToEulerAngles(Matrix4 mat)
        {
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
