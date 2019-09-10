using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;

namespace Metanoia.Formats.Misc
{
    public class STPK : IContainerFormat
    {
        public List<FileItem> Files = new List<FileItem>();
        
        public string Name => "JStar";
        public string Extension => ".srd";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        public FileItem[] GetFiles()
        {
            return Files.ToArray();
        }

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = true;
                r.ReadInt32(); // magic
                r.ReadInt32(); // version
                int resourceCount = r.ReadInt32();
                r.ReadUInt32();

                for (int i = 0; i < resourceCount; i++)
                {
                    var offset = r.ReadUInt32();
                    var size = r.ReadInt32();
                    r.ReadInt64(); // padding?
                    var name = r.ReadString(0x20);
                    Files.Add(new FileItem(name, r.GetSection(offset, size)));
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "STPK";
        }
    }

    public class JStar : I3DModelFormat
    {
        public string Name => "JStar Model";
        public string Extension => ".srd";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;
        
        public void Open(FileItem File)
        {
            using(DataReader r = new DataReader(File))
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

            for(int i = 0; i < 0x400; i++)
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
                    case "$SCN":

                        break;
                    case "$MSH":

                        break;
                    case "$TRE":

                        break;
                    case "$CLT":

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

        public bool Verify(FileItem file)
        {
            return file.MagicString == "$CFH";
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
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
