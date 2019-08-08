using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;

namespace Metanoia.Formats.GameCube
{
    /// <summary>
    /// TODO: incomplete
    /// </summary>
    [Format(Extension = ".mdr", Description = "Mario DDR")]
    public class MarioDDR : IModelFormat
    {
        private HSF mod;

        public void Open(FileItem File)
        {
            var chunks = GetChunks(File.GetFileBinary());

            var outputpath = Path.GetDirectoryName(File.FilePath);
            int i = 0; 
            foreach(var c in chunks)
            {
                System.IO.File.WriteAllBytes(outputpath + "\\" + i + "_" + c.Flags.ToString("X8"), c.Data);
                //HSF h = new HSF();
               // h.Open(new FileItem(outputpath + "\\" + i + "_" + c.Flags.ToString("X8")));
                //mod = h;
                i++;
            }
        }

        private List<MarioDDRChunks> GetChunks(byte[] data)
        {
            List<MarioDDRChunks> Chunks = new List<MarioDDRChunks>();
            using (DataReader r = new DataReader(new MemoryStream(data)))
            {
                r.BigEndian = true;
                int count = r.ReadInt32();

                for(int i = 0; i < count; i++)
                {
                    var off = r.ReadUInt32();

                    var temp = r.Position;
                    r.Seek(off);
                    MarioDDRChunks chunk = new MarioDDRChunks();

                    var decomSize = r.ReadInt32();
                    chunk.Flags = r.ReadInt32();
                    r.ReadInt32();
                    var compSize = r.ReadInt32();
                    chunk.Data = Tools.Decompress.ZLIB(r.GetSection(r.Position, compSize));
                    Chunks.Add(chunk);

                    r.Seek(temp);
                }
            }
            return Chunks;
        }

        public GenericModel ToGenericModel()
        {
            return mod.ToGenericModel();
        }

        private class MarioDDRChunks
        {
            public int Flags;
            public byte[] Data;
        }
    }
}
