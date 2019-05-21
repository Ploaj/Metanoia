using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Text;

namespace Metanoia.Rendering
{
    public class Shader : IDisposable
    {
        public int ProgramID;

        public bool Linked = false;

        private StringBuilder ErrorLog = new StringBuilder();

        private int VertexID = -1;
        private int FragmentID = -1;

        public Shader()
        {
            ProgramID = GL.CreateProgram();
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramID);
        }

        public void LoadShader(string FilePath, ShaderType Type)
        {
            var shaderid = GL.CreateShader(Type);
            GL.ShaderSource(shaderid, System.IO.File.ReadAllText(FilePath));
            GL.CompileShader(shaderid);

            switch (Type)
            {
                case ShaderType.VertexShader: VertexID = shaderid; break;
                case ShaderType.FragmentShader: FragmentID = shaderid; break;
            }

            ErrorLog.Append(GL.GetShaderInfoLog(shaderid));
        }

        public void CompileProgram()
        {
            if (Linked) return;

            if (VertexID != -1)
            {
                GL.AttachShader(ProgramID, VertexID);
            }
            if (FragmentID != -1)
            {
                GL.AttachShader(ProgramID, FragmentID);
            }

            GL.LinkProgram(ProgramID);

            ErrorLog.Append(GL.GetShaderInfoLog(ProgramID));

            if (VertexID != -1)
            {
                GL.DetachShader(ProgramID, VertexID);
                GL.DeleteShader(VertexID);
                VertexID = -1;
            }
            if (FragmentID != -1)
            {
                GL.DetachShader(FragmentID, FragmentID);
                GL.DeleteShader(FragmentID);
                FragmentID = -1;
            }

            Linked = true;
        }

        public int GetAttributeLocation(string Name)
        {
            int uniformloc = GL.GetUniformLocation(ProgramID, Name);
            return uniformloc != -1 ? uniformloc : GL.GetAttribLocation(ProgramID, Name);
        }

        public string GetErrorLog()
        {
            return ErrorLog.ToString();
        }
    }
}
