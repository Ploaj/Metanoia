using System;
using System.Collections.Generic;

namespace Metanoia.Modeling
{
    public class GenericAnimation
    {
        public string Name { get; set; }
        
        public List<GenericAnimationTransform> TransformNodes = new List<GenericAnimationTransform>();

        public int FrameCount { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="skeleton"></param>
        public void UpdateSkeleton(float frame, GenericSkeleton skeleton)
        {
            foreach(var v in TransformNodes)
            {
                GenericBone bone = null;
                switch (v.HashType)
                {
                    case AnimNodeHashType.Name:
                        bone = skeleton.Bones.Find(e => e.Name == v.Name);
                        break;
                    case AnimNodeHashType.CRC32C:
                        bone = skeleton.Bones.Find(e => e.NameHash == v.Hash);
                        break;
                }
                if(bone != null)
                {
                    bone.AnimatedTransform = v.GetTransformAt(frame, bone);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
