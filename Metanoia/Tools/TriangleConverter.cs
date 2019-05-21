using Metanoia.Modeling;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Metanoia.Tools
{
    public class TriangleConverter
    {
        public static void StripToList(List<GenericVertex> vertices, out List<GenericVertex> outVertices)
        {
            outVertices = new List<GenericVertex>();
            
            for (int index = 2; index < vertices.Count; index++)
            {
                bool isEven = (index % 2 != 1);

                var vert1 = vertices[index - 2];
                var vert2 = isEven ? vertices[index] : vertices[index - 1];
                var vert3 = isEven ? vertices[index - 1] : vertices[index];
                
                if(!vert1.Pos.Equals(vert2.Pos) && !vert2.Pos.Equals(vert3.Pos) && !vert3.Pos.Equals(vert1.Pos))
                {
                    outVertices.Add(vert3);
                    outVertices.Add(vert2);
                    outVertices.Add(vert1);
                }
                else
                {
                    //Console.WriteLine("ignoring degenerate triangle");
                }
            }
        }
    }
}
