using System;
using System.Collections.Generic;

namespace Metanoia.Modeling
{
    public class GenericAnimation
    {
        public string Name { get; set; }

        public List<GenericAnimationTransform> TransformNodes = new List<GenericAnimationTransform>();

        public int FrameCount { get; set; } = 0;

        public void UpdateSkeleton(float frame, GenericSkeleton skeleton)
        {
            foreach (var v in TransformNodes)
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
                if (bone != null)
                {
                    bone.AnimatedTransform = v.GetTransformAt(frame, bone);
                }
            }
        }

        public GenericAnimation TrimAnimation(float startFrame, float endFrame)
        {
            // Create a new animation to store the trimmed data
            var trimmedAnimation = new GenericAnimation
            {
                Name = this.Name,
                FrameCount = (int)(endFrame - startFrame + 1)
            };

            // Copy transform nodes
            foreach (var transformNode in this.TransformNodes)
            {
                var trimmedTransformNode = new GenericAnimationTransform
                {
                    Name = transformNode.Name,
                    Hash = transformNode.Hash,
                    HashType = transformNode.HashType
                };

                // Copy tracks
                foreach (var track in transformNode.Tracks)
                {
                    var trimmedTrack = new GenericTransformTrack(track.Type);

                    // Copy keys within the specified frame range
                    foreach (var key in track.Keys.Keys)
                    {
                        if (key.Frame >= startFrame && key.Frame <= endFrame || key.Frame == 0)
                        {
                            float trimmedFrame = key.Frame - startFrame;
                            trimmedTrack.AddKey(trimmedFrame, key.Value, key.InterpolationType, key.InTan, key.OutTan);
                        }
                    }

                    trimmedTransformNode.Tracks.Add(trimmedTrack);
                }

                trimmedAnimation.TransformNodes.Add(trimmedTransformNode);
            }

            return trimmedAnimation;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
