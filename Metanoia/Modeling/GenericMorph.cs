using System.Collections.Generic;

namespace Metanoia.Modeling
{
    public class MorphVertex
    {
        public int VertexIndex;
        public GenericVertex Vertex;
    }

    public class GenericMorph
    {
        public string Name;

        public List<MorphVertex> Vertices = new List<MorphVertex>();
    }
}
