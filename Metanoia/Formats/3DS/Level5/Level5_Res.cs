using Metanoia.Tools;
using System;
using System.Collections.Generic;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_Resource
    {
        public class Level5_Material
        {
            public string Name { get; set; }
            public int Index { get; set; } = -1;
            public string TexName { get; set; }
        }

        public string ModelName { get; set; }
        Dictionary<uint, string> ResourceNames { get; set; } = new Dictionary<uint, string>();

        public List<string> TextureNames = new List<string>();

        public List<Level5_Material> Materials = new List<Level5_Material>();

        public string GetResourceName(uint crc)
        {
            if (ResourceNames.ContainsKey(crc))
                return ResourceNames[crc];

            return "";
        }

        public Level5_Resource(byte[] data)
        {
            data = Decompress.Level5Decom(data);
            using (DataReader r = new DataReader(new System.IO.MemoryStream(data)))
            {
                var magic = new string(r.ReadChars(6));
                if (magic != "CHRC00" && magic != "CHRN01")
                    throw new FormatException("RES file is corrupt");

                // -----------------------
                var unknown0 = r.ReadInt16();
                var stringTableOffset = r.ReadInt16() << 2;
                var unknown1 = r.ReadInt16();
                var materialTableOffset = r.ReadInt16() << 2;
                var materialTableSectionCount = r.ReadInt16();
                var resourceNodeOffsets = r.ReadInt16() << 2;
                var resourceNodeCount = r.ReadInt16();

                r.Seek((uint)stringTableOffset);
                while (r.Position < r.BaseStream.Length)
                {
                    string mname = r.ReadString();
                    if (mname == "")
                        break;
                    if (!ResourceNames.ContainsKey(CRC32.Crc32C(mname)))
                        ResourceNames.Add(CRC32.Crc32C(mname), mname);
                }

                r.Seek((uint)materialTableOffset);
                for (int i = 0; i < materialTableSectionCount; i++)
                {
                    var offset = r.ReadInt16() << 2;
                    var count = r.ReadInt16();
                    var unknown = r.ReadInt16();
                    var size = r.ReadInt16();

                    if (unknown == 0x270F)
                        continue;

                    var temp = r.Position;
                    for(int j = 0; j <count; j++)
                    {
                        r.Position = (uint)(offset + j * size);
                        var key = r.ReadUInt32();
                        string resourceName = (ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X"));
                        //Console.WriteLine(resourceName + " " + unknown.ToString("X") + " " + size.ToString("X"));

                        if (unknown == 0xF0)
                        {
                            TextureNames.Add(resourceName);
                        }
                        if (unknown == 0x122)
                        {
                            Level5_Material mat = new Level5_Material();
                            mat.Name = resourceName;
                            r.Skip(12);
                            key = r.ReadUInt32();
                            resourceName = (ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X"));
                            mat.TexName = resourceName;
                            Console.WriteLine(resourceName + " " + unknown.ToString("X") + " " + size.ToString("X"));
                            Materials.Add(mat);
                        }
                    }

                    r.Seek(temp);
                }

                r.Seek((uint)resourceNodeOffsets);
                for (int i = 0; i < resourceNodeCount; i++)
                {
                    var offset = r.ReadInt16() << 2;
                    var count = r.ReadInt16();
                    var unknown = r.ReadInt16();
                    var size = r.ReadInt16();

                    if (unknown == 0x270F)
                        continue;

                    var temp = r.Position;
                    r.Seek((uint)offset);
                    for (int j = 0; j < count; j++)
                    {
                        var key = r.ReadUInt32();
                        //Console.WriteLine((ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X")) + " " + unknown.ToString("X") + " " + size.ToString("X"));
                        r.Position += (uint)(size - 4);
                    }

                    r.Seek(temp);
                }

            }
        }
    }
}
