using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using Metanoia.Tools;
using OpenTK;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_XC : I3DModelFormat
    {
        public Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();

        public string Name => "";

        public string Extension => ".xc";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        public FileItem[] GetFiles()
        {
            FileItem[] files = new FileItem[Files.Count];

            var keys = Files.Keys.ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = new FileItem(keys[i], Files[keys[i]]);
            }

            return files;
        }

        public void Open(FileItem File)
        {
            using (DataReader r = new DataReader(new MemoryStream(File.GetFileBinary())))
            {
                if (!new string(r.ReadChars(4)).Equals("XPCK"))
                    throw new Exception("File header error");

                uint fileCount = (uint)(r.ReadUInt16() & 0xFFF);
                
                var fileInfoOffset = r.ReadUInt16() * 4;
                var fileTableOffset = r.ReadUInt16() * 4;
                var dataOffset = r.ReadUInt16() * 4;

                r.ReadUInt16();
                var filenameTableSize = r.ReadUInt16() * 4;
                
                var hashToData = new Dictionary<uint, byte[]>();
                r.Seek((uint)fileInfoOffset);
                for(int i = 0; i < fileCount; i++)
                {
                    var nameCRC = r.ReadUInt32();
                    r.ReadInt16();
                    var offset = (uint)r.ReadUInt16();
                    var size = (uint)r.ReadUInt16();
                    var offsetExt = (uint)r.ReadByte();
                    var sizeExt = (uint)r.ReadByte();

                    offset |= offsetExt << 16;
                    size |= sizeExt << 16;
                    offset = (uint)(offset * 4 + dataOffset);

                    var data = r.GetSection(offset, (int)size);
                    //Decompress.CheckLevel5Zlib(data, out data);
                    
                    hashToData.Add(nameCRC, data);
                }

                byte[] nameTable = r.GetSection((uint)fileTableOffset, filenameTableSize);
                if(!Decompress.CheckLevel5Zlib(nameTable, out nameTable))
                    nameTable = Decompress.lzss_Decompress(nameTable);
                using (DataReader nt = new DataReader(new MemoryStream(nameTable)))
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        var name = nt.ReadString();

                        if(hashToData.ContainsKey(CRC32.Crc32C(name)))
                            Files.Add(name, hashToData[CRC32.Crc32C(name)]);
                        else
                            Console.WriteLine("Couldn't find " + name + " " + CRC32.Crc32C(name).ToString("X"));

                    }
                }

            }

        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            var model = new GenericModel();
            var skel = new GenericSkeleton();
            model.Skeleton = skel;

            Level5_Resource resourceFile = null;
            var textureList = new List<GenericTexture>();
            
            foreach(var f in Files)
            {
                //Console.WriteLine(f.Key);
                if (f.Key.EndsWith("RES.bin"))
                {
                    resourceFile = new Level5_Resource(f.Value);
                    model.Name = resourceFile.ModelName;
                }
                if (f.Key.EndsWith(".mbn"))
                {
                    skel.Bones.Add(Level5_MBN.ToBone(f.Value));
                }
                if (f.Key.EndsWith(".prm"))
                {
                    model.Meshes.Add(Level5_PRM.ToGenericMesh(f.Value));
                }
                if (f.Key.EndsWith(".atr"))
                {
                }
                if (f.Key.EndsWith(".xi"))
                {
                    var tex = Level5_XI.ToGenericTexture(f.Value);
                    tex.Name = f.Key;
                    textureList.Add(tex);
                }
            }

            if (resourceFile == null)
                return model;


            // add materials
            foreach(var mat in resourceFile.Materials)
            {
                GenericMaterial material = new GenericMaterial();
                material.TextureDiffuse = mat.TexName;
                model.MaterialBank.Add(mat.Name, material);
            }

            // add textures
            for (int i = 0; i < textureList.Count; i++)
            {
                model.TextureBank.Add(resourceFile.TextureNames[i], textureList[i]);
            }

            // fix bones
            foreach (var bone in skel.Bones)
            {
                bone.Name = resourceFile.GetResourceName((uint)bone.ID);
                bone.ID = 0;
            }
            foreach (var bone in skel.Bones)
            {
                if(bone.ParentIndex == 0)
                {
                    bone.ParentIndex = -1;
                }
                else
                {
                    var parentName = resourceFile.GetResourceName((uint)bone.ParentIndex);
                    bone.ParentIndex = skel.Bones.FindIndex(e => e.Name.Equals(parentName));
                }
            }
            var boneIndex = 0;
            foreach (var bone in skel.Bones)
            {
                if (bone.Name.Equals(""))
                    bone.Name = "Bone_" + boneIndex++;
            }

            foreach(var mesh in model.Meshes)
            {
                for(int i = 0; i < mesh.VertexCount; i++)
                {
                    var vertex = mesh.Vertices[i];
                    Vector4 newBones = new Vector4();
                    for(int j = 0; j < 4; j++)
                    {
                        if(vertex.Weights[j] > 0)
                        {
                            var hash = BitConverter.ToUInt32(BitConverter.GetBytes(vertex.Bones[j]), 0);
                            newBones[j] = skel.Bones.FindIndex(e => e.Name == resourceFile.GetResourceName(hash));                        }
                    }
                    vertex.Bones = newBones;
                    mesh.Vertices[i] = vertex;
                }
            }

            return model;
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "XPCK";
        }
    }
}
