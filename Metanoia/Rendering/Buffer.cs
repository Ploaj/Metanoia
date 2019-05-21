using System;
using OpenTK.Graphics.OpenGL;

namespace Metanoia.Rendering
{
    public class Buffer : IDisposable
    {
        public int ID { get; internal set; }

        public BufferTarget BufferTarget { get; internal set; }

        public Buffer(BufferTarget BufferTarget)
        {
            int id;
            GL.GenBuffers(1, out id);
            ID = id;
            this.BufferTarget = BufferTarget;
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget, ID);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
            ID = -1;
        }
    }
}
