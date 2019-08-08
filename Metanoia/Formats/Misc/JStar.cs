using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;

namespace Metanoia.Formats.Misc
{
    [Format(Extension = ".stp", Description = "JStars Models")]
    public class JStar : IModelFormat
    {
        public class STPK
        {
            public Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();

            public STPK(string FileName)
            {
                using (DataReader r = new DataReader(new FileStream(FileName, FileMode.Open)))
                {
                    r.BigEndian = true;
                    r.ReadInt32(); // magic
                    r.ReadInt32(); // version
                    int resourceCount = r.ReadInt32();
                    r.ReadUInt32();

                    for(int i = 0; i < resourceCount; i++)
                    {
                        var offset = r.ReadUInt32();
                        var size = r.ReadInt32();
                        r.ReadInt64(); // padding?
                        var name = r.ReadString(0x20);
                        Files.Add(name, r.GetSection(offset, size));
                    }
                }
            }
        }

        public void Open(FileItem File)
        {
            STPK p = new STPK(File.FilePath);

            foreach(var v in p.Files)
            {
                if(v.Value.Length > 0)
                    Console.WriteLine(v.Key + " " + v.Value.Length.ToString("X"));
            }

            using(DataReader r = new DataReader(new MemoryStream(p.Files["017_gon_01p_PS3.srd"])))
            {
                r.BigEndian = true;
                ReadCFH(r);
                r.PrintPosition();
            }
        }

        private void ReadCFH(DataReader r)
        {
            if (new string(r.ReadChars(4)) != "$CFH")
                Console.WriteLine("error");

            r.ReadInt32();
            r.ReadInt32();
            int resourceCount = r.ReadInt32();

            for (int i = 0; i < resourceCount; i++)
                ReadRES(r);
        }
        
        private void ReadRES(DataReader r)
        {
            if (new string(r.ReadChars(4)) != "$RSF")
                Console.WriteLine("error");
            
            int resourceCount1 = r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            int resourceCount2 = r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            string name = r.ReadString(0x20);

            Console.WriteLine(name);

            for(int i = 0; i < 0x200; i++)
            {
                var start = r.Position;

                string key = r.ReadString(4);
                var size = r.ReadUInt32();

                Console.WriteLine(key);

                switch (key)
                {
                    case "$CT0":
                        r.Position += 12;
                        break;
                    case "$VTX":
                        r.ReadStruct<TXR>();
                        break;
                    case "$TXR":
                        r.ReadStruct<TXR>();
                        break;
                    case "$TXI":
                        r.ReadStruct<TXI>();
                        var txiStr = r.ReadString();
                        break;
                    case "$RSI":
                        r.ReadStruct<RSI>();
                        var rsiStr = r.ReadString();
                        break;
                    case "$SKL":

                        break;
                    default:
                        return;
                }

                r.Seek(start + size + 0x10);

                if (r.Position % 16 != 0 && key != "$SKL")
                    r.Position += 16 - (r.Position % 16);
            }
        }

        public GenericModel ToGenericModel()
        {
            return new GenericModel();
        }

        public struct VTX
        {
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public int Unknown5;
            public int VertexCount;
            public int Unknown6;
            public short Unknown7;
            public short Unknown8;
            public short Unknown9;
            public short Unknown10;
        }

        public struct TXR
        {
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public short Unknown5;
            public short Unknown6;
            public short Unknown7;
            public short Unknown8;
            public short Unknown9;
            public short Unknown10;
        }

        public struct TXI
        {
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public int Unknown5;
            public int Unknown6;
            public int Unknown7;
        }

        public struct RSI
        {
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public int Unknown5;
            public int Unknown6;
            public int Unknown7;
            public int Unknown8;
            public int Unknown9;
            public int Unknown10; //offset
        }
    }
}
