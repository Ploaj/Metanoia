using Metanoia.Tools;
using System;
using System.Text;
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

        public void RES(DataReader r)
        {
            var unknown0 = r.ReadInt16();
            var stringTableOffset = r.ReadInt16() << 2;
            var unknown1 = r.ReadInt16();
            var materialTableOffset = r.ReadInt16() << 2;
            var materialTableSectionCount = r.ReadInt16();
            var resourceNodeOffsets = r.ReadInt16() << 2;
            var resourceNodeCount = r.ReadInt16();

            r.Seek((uint)stringTableOffset);
            Encoding shiftJIS = Encoding.GetEncoding("Shift-JIS");
            while (r.Position < r.BaseStream.Length)
            {
                string mname = r.ReadString(shiftJIS);
                if (mname == "")
                    break;
                if (!ResourceNames.ContainsKey(CRC32.Crc32C(mname, shiftJIS)))
                    ResourceNames.Add(CRC32.Crc32C(mname, shiftJIS), mname);
            }

            r.Seek((uint)materialTableOffset);
            for (int i = 0; i < materialTableSectionCount; i++)
            {
                var offset = r.ReadInt16() << 2;
                var count = r.ReadInt16();
                var unknown = r.ReadInt16();
                var size = r.ReadInt16();

                Console.WriteLine("test " + offset + " " + count + unknown + " " + size);

                if (unknown == 0x270F)
                    continue;

                var temp = r.Position;
                for (int j = 0; j < count; j++)
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

        public void XRES(DataReader reader)
        {
            reader.Seek(0x04);

            short stringTableOffset = reader.ReadInt16();
            reader.Skip(0x0E);
            reader.Skip(0x04);

            short libOffset = reader.ReadInt16();
            short libCount = reader.ReadInt16();
            short lib2Offset = reader.ReadInt16();
            short lib2Count = reader.ReadInt16();
            short textureOffset = reader.ReadInt16();
            short textureCount = reader.ReadInt16();

            reader.Skip(0x08);
            short materialOffset = reader.ReadInt16();
            short materialCount = reader.ReadInt16();

            reader.Skip(0x04);
            short unkOffset = reader.ReadInt16();
            short unkCount = reader.ReadInt16();
            short boneOffset = reader.ReadInt16();
            short boneCount = reader.ReadInt16();

            reader.Seek((uint)stringTableOffset);
            while (reader.Position < reader.BaseStream.Length)
            {
                string mname = reader.ReadString();
                if (mname == "")
                    break;
                if (!ResourceNames.ContainsKey(CRC32.Crc32C(mname)))
                    ResourceNames.Add(CRC32.Crc32C(mname), mname);
            }

            reader.Seek((uint)textureOffset);
            for (int i = 0; i < textureCount; i++)
            {
                var key = reader.ReadUInt32();
                string resourceName = (ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X"));
                TextureNames.Add(resourceName);
                reader.Skip(0x1C);
            }

            reader.Seek((uint)materialOffset);
            for (int i = 0; i < materialCount; i++)
            {
                var key = reader.ReadUInt32();
                string resourceName = (ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X"));
                Level5_Material mat = new Level5_Material();
                mat.Name = resourceName;
                reader.Skip(12);
                key = reader.ReadUInt32();
                resourceName = (ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X"));
                mat.TexName = resourceName;
                Materials.Add(mat);
                reader.Skip(0xCC);
            }
        }

        public Level5_Resource(byte[] data)
        {
            data = Decompress.Level5Decom(data);
            using (DataReader r = new DataReader(new System.IO.MemoryStream(data)))
            {
                var magic = new string(r.ReadChars(4));

                if (magic == "XRES")
                {
                    XRES(r);
                } else
                {
                    r.Skip(0x02);
                    RES(r);
                }
            }
        }
    }
}
