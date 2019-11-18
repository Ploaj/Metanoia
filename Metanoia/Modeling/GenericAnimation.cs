using System;
using System.Collections.Generic;

namespace Metanoia.Modeling
{
    public class GenericAnimation
    {
        public string Name { get; set; }
        
        public List<GenericAnimationTransform> TransformNodes = new List<GenericAnimationTransform>();

        public int FrameCount { get; set; } = 0;

        public override string ToString()
        {
            return Name;
        }
    }
}
