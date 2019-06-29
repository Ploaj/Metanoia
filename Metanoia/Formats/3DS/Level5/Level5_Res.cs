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

        public string[] TextureNames;

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
                r.Seek(0x3E); // I dunno header stuff
                int numOfMesh = r.ReadInt16();

                r.Seek(0x12); // I dunno what this is
                var numOfSomething1 = r.ReadUInt16();
                r.Skip(0x02);
                var numOfSomething2 = r.ReadUInt16() * (uint)2;

                r.Seek(0x26);
                int numOfTex = r.ReadInt16();
                r.Skip(0x06);
                int numOfMat = r.ReadInt16();

                r.Skip(12);
                r.Skip(numOfSomething1 * (uint)8);
                r.Skip(numOfSomething2 * (uint)8);

                // Textures
                uint[] texcrc = new uint[numOfTex];
                for (int i = 0; i < numOfTex; i++)
                {
                    texcrc[i] = r.ReadUInt32();
                    r.Skip(16);
                }

                // Materials
                Materials = new List<Level5_Material>(numOfMat);
                for (int i = 0; i < numOfMat; i++)
                {
                    var mat = new Level5_Material();
                    r.Skip(16);
                    uint c = r.ReadUInt32();
                    for (int j = 0; j < texcrc.Length; j++)
                        if (c == texcrc[j])
                        {
                            mat.Index = j;
                            break;
                        }
                    r.Skip(0xCC);
                    Materials.Add(mat);
                }

                // hacky skip....

                while (true)
                {
                    r.ReadInt32();
                    int i2 = r.ReadInt32();
                    if (i2 != 0)
                    {
                        r.Seek(r.Position - 8);
                        break;
                    }
                }

                for (int i = 0; i < numOfMat; i++)
                {
                    Materials[i].Name = r.ReadString();
                }

                // skip through textures
                TextureNames = new string[numOfTex];
                for (int i = 0; i < numOfTex; i++)
                {
                    TextureNames[i] = r.ReadString();
                    if (!ResourceNames.ContainsKey(CRC32.Crc32C(TextureNames[i])))
                        ResourceNames.Add(CRC32.Crc32C(TextureNames[i]), TextureNames[i]);
                }

                for (int i = 0; i < numOfMat; i++)
                {
                    Materials[i].TexName = TextureNames[Materials[i].Index];
                }

                for (int i = 0; i < numOfMesh; i++)
                {
                    string mname = r.ReadString();
                    ResourceNames.Add(CRC32.Crc32C(mname), mname);
                }

                ModelName = r.ReadString(); // model name

                r.ReadString();
                
                while (r.Position < r.BaseStream.Length)
                {
                    string mname = r.ReadString();
                    if (!ResourceNames.ContainsKey(CRC32.Crc32C(mname)))
                        ResourceNames.Add(CRC32.Crc32C(mname), mname);
                }
            }
        }
    }
}
