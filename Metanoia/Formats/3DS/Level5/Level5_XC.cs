using System;
using System.Drawing;
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
    public class Level5_XC : IContainerFormat, I3DModelFormat
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
                    nameTable = Decompress.LZ10Decompress(nameTable);
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

            // fix textures
            if (textureList.Count > 0 && textureList.Count % 2 == 0)
            {
                for (int i = 0; i < textureList.Count/2+1; i+=2)
                {
                    _3DSImageTools.Tex_Formats pixelFormat1 = (_3DSImageTools.Tex_Formats)textureList[i].PixelFormatTrue;
                    _3DSImageTools.Tex_Formats pixelFormat2 = (_3DSImageTools.Tex_Formats)textureList[i+1].PixelFormatTrue;

                    if (pixelFormat1 == _3DSImageTools.Tex_Formats.ETC1 && pixelFormat2 == _3DSImageTools.Tex_Formats.RGB565)
                    {
                        Bitmap etc1 = GenericTexture.GetBitmap((int)textureList[i].Width, (int)textureList[i].Height, textureList[i].Mipmaps[0]);
                        Bitmap rgb = GenericTexture.GetBitmap((int)textureList[i+1].Width, (int)textureList[i+1].Height, textureList[i+1].Mipmaps[0]);

                        ImageCleaner imageCleaner = new ImageCleaner(rgb, etc1);
                        imageCleaner.Cleaner();

                        textureList[i + 1].Mipmaps.Clear();
                        textureList[i + 1].FromBitmap(imageCleaner.Result);
                    }
                }
            }

            return model;
        }

        public GenericAnimation[] ToGenericAnimation()
        {
            int motionCount = Files.Count(x => x.Key.EndsWith("mtn2") || x.Key.EndsWith("mtn3"));
            int subMotionCount = Files.Count(x => x.Key.EndsWith("mtninf") || x.Key.EndsWith("mtninf2"));
            
            if (motionCount + subMotionCount > 0)
            {
                Level5_Resource resourceFile = null;

                List<GenericAnimation> animations = new List<GenericAnimation>();
                Dictionary<uint, GenericAnimation> animDict = new Dictionary<uint, GenericAnimation>();
                Dictionary<uint, List<Level5_MINF>> subAnimDict = new Dictionary<uint, List<Level5_MINF>>();
                Dictionary<uint, List<Level5_MINF2.SubAnimation>> subAnimDict2 = new Dictionary<uint, List<Level5_MINF2.SubAnimation>>();

                foreach (var f in Files)
                {
                    if (f.Key.EndsWith("RES.bin"))
                    {
                        resourceFile = new Level5_Resource(f.Value);
                    }
                    else if (f.Key.EndsWith(".mtn2"))
                    {
                        var anim = new Level5_MTN2();
                        anim.Open(f.Key, f.Value);
                        GenericAnimation newAnimation = anim.ToGenericAnimation();
                        animDict.Add(CRC32.Crc32C(newAnimation.Name), newAnimation);
                    }
                    else if (f.Key.EndsWith(".mtn3"))
                    {
                        var anim = new Level5_MTN3();
                        anim.Open(f.Key, f.Value);
                        GenericAnimation newAnimation = anim.ToGenericAnimation();
                        animDict.Add(CRC32.Crc32C(newAnimation.Name), newAnimation);
                    }
                    else if (f.Key.EndsWith(".mtninf"))
                    {
                        var subAnim = new Level5_MINF();
                        subAnim.Open(f.Value);
                        
                        if (!subAnimDict.ContainsKey(subAnim.AnimationName))
                        {
                            subAnimDict.Add(subAnim.AnimationName, new List<Level5_MINF>());
                        }

                        subAnimDict[subAnim.AnimationName].Add(subAnim);
                    }
                    else if (f.Key.EndsWith(".mtninf2"))
                    {
                        var mtninf2 = new Level5_MINF2();
                        mtninf2.Open(f.Value);

                        Console.WriteLine("mtninf2 " + mtninf2.SubAnimations.Count);

                        foreach (Level5_MINF2.SubAnimation subAnimation in mtninf2.SubAnimations)
                        {
                            Console.WriteLine(subAnimation.AnimationName.ToString("X8"));

                            if (!subAnimDict2.ContainsKey(subAnimation.AnimationName))
                            {
                                subAnimDict2.Add(subAnimation.AnimationName, new List<Level5_MINF2.SubAnimation>());
                            }

                            subAnimDict2[subAnimation.AnimationName].Add(subAnimation);
                        }                      
                    }
                }

                foreach (KeyValuePair<uint, GenericAnimation> kvp in animDict)
                {
                    animations.Add(kvp.Value);

                    if (subAnimDict.ContainsKey(kvp.Key))
                    {
                        foreach (Level5_MINF minf in subAnimDict[kvp.Key])
                        {
                            GenericAnimation newAnimation = kvp.Value.TrimAnimation(minf.FrameStart, minf.FrameEnd);

                            string animationSubName = minf.AnimationSubName.ToString("X8");
                            if (resourceFile != null)
                            {
                                animationSubName = resourceFile.GetResourceName(minf.AnimationSubName);

                                if (animationSubName == "")
                                {
                                    animationSubName = minf.AnimationSubName.ToString("X8");
                                }
                            }

                            newAnimation.Name = kvp.Value.Name + "_" + animationSubName;                         
                            animations.Add(newAnimation);
                        }
                    }

                    if (subAnimDict2.ContainsKey(kvp.Key))
                    {
                        foreach (Level5_MINF2.SubAnimation subAnimations in subAnimDict2[kvp.Key])
                        {
                            GenericAnimation newAnimation = kvp.Value.TrimAnimation(subAnimations.FrameStart, subAnimations.FrameEnd);
                            newAnimation.Name = kvp.Value.Name + "_" + subAnimations.AnimationSubName;
                            animations.Add(newAnimation);
                        }
                    }


                }

                return animations.ToArray();
            } else
            {
                return null;
            }
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "XPCK";
        }
    }
}
