using Metanoia.Modeling;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Metanoia.Tools
{
    public class TriangleConverter
    {
        /// <summary>
        /// Converts a list of quads into triangles
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="outVertices"></param>
        public static void QuadToList(List<GenericVertex> vertices, out List<GenericVertex> outVertices)
        {
            outVertices = new List<GenericVertex>();

            for(int index = 0; index < vertices.Count; index+=4)
            {
                outVertices.Add(vertices[index]);
                outVertices.Add(vertices[index + 1]);
                outVertices.Add(vertices[index + 2]);
                outVertices.Add(vertices[index + 1]);
                outVertices.Add(vertices[index + 3]);
                outVertices.Add(vertices[index + 2]);
            }
        }

        /// <summary>
        /// Converts a list of triangle strips into triangles
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="outVertices"></param>
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

        /// <summary>
        /// reverses faces for triangle lists only
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="reversed"></param>
        public static void ReverseFaces(List<uint> triangles, out List<uint> reversed)
        {
            reversed = new List<uint>(triangles.Count);

            for(int i = 0; i < triangles.Count; i+=3)
            {
                reversed.Add(triangles[i + 2]);
                reversed.Add(triangles[i + 1]);
                reversed.Add(triangles[i]);
            }
        }
    }
}
