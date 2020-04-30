using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using Metanoia.Tools;

namespace Metanoia.Formats.Misc
{
    public class STPK //: IContainerFormat
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

        List<VTX> VTXs = new List<VTX>();
        List<TXR> TXRs = new List<TXR>();
        private GenericSkeleton Skeleton = new GenericSkeleton();
        
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
            string name = r.ReadString(r.Position, -1);
            r.Position += (uint)name.Length;
            r.Align(0x10);

            Console.WriteLine(name);

            VTX prevVTX = null;
            TXR prevTXR = null;

            while (true)
            {
                var start = r.Position;

                if (r.Position >= r.Length)
                    break;

                r.BigEndian = true;
                string key = r.ReadString(4);
                var size = r.ReadUInt32();
                var flag1 = r.ReadInt32();
                var flag2 = r.ReadInt32();

                var sectionStart = r.Position;

                r.BigEndian = false;

                Console.WriteLine(start.ToString("X") + " " + key + " " + size.ToString("X"));

                switch (key)
                {
                    case "$CT0":
                        r.Position += 12;
                        break;
                    case "$VTX":
                        prevVTX = new VTX(r);
                        VTXs.Add(prevVTX);
                        break;
                    case "$TXR":
                        prevTXR = new TXR(r);
                        TXRs.Add(prevTXR);
                        break;
                    case "$TXI":
                        break;
                    case "$RSI":
                        if (prevVTX != null)
                            prevVTX.ReadResource(r);
                        if (prevTXR != null)
                            prevTXR.ReadResource(r);
                        prevVTX = null;
                        prevTXR = null;
                        break;
                    case "$SKL":
                        {
                            var f = r.ReadInt32();
                            r.ReadInt16(); // dunno
                            var boneCount = r.ReadInt16();
                            r.ReadInt16(); // dunno
                            r.ReadInt16(); // dunno
                            Stack<int> boneStack = new Stack<int>();
                            for(int i = 0; i < boneCount; i++)
                            {
                                GenericBone bone = new GenericBone();
                                bone.Name = r.ReadString(sectionStart + r.ReadUInt32(), -1);
                                bone.ParentIndex = boneStack.Count > 0 ? boneStack.Peek() : -1;
                                r.ReadByte();
                                var depth = r.ReadByte();
                                var pop = r.ReadSByte() + 1;
                                r.ReadByte();
                                boneStack.Push(i);
                                for(int j = 0; j < pop; j++)
                                        boneStack.Pop();
                                bone.Transform = new OpenTK.Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0,
                                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0,
                                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0,
                                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 1);
                                r.Skip(0x48);
                                Skeleton.Bones.Add(bone);
                            }
                        }
                        break;
                    case "$SCN":

                        break;
                    case "$MSH":

                        break;
                    case "$TRE":

                        break;
                    case "$CLT":

                        break;
                }

                r.Seek(start + size + 0x10);

                if (r.Position % 16 != 0)
                    r.Position += 16 - (r.Position % 16);
            }
        }

        public GenericModel ToGenericModel()
        {
            var mdl = new GenericModel() { Skeleton = Skeleton }; ;

            byte[] Data = File.ReadAllBytes(@"chr0020_model.npki");
            byte[] ImageData = File.ReadAllBytes(@"chr0020_model.npkv");

            foreach (var t in TXRs)
            {
                GenericTexture tex = new GenericTexture();
                tex.Width = (uint)t.Width;
                tex.Height = (uint)t.Height;

                var buf = new byte[t.Width * t.Height / 2];
                Array.Copy(ImageData, t.BufferOffsets[0], buf, 0, buf.Length);
                buf = VitaSwizzle.UnswizzlePS4(buf, t.Width, t.Height);
                tex.Mipmaps.Add(buf);

                tex.InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.CompressedRgbS3tcDxt1Ext;

                if(!mdl.TextureBank.ContainsKey(t.Name))
                    mdl.TextureBank.Add(t.Name, tex);

                Console.WriteLine(tex.Width + " " + tex.Height + " " + t.Unknown.ToString("X") + " " + t.BufferOffsets[0].ToString("X") + " " + t.BufferSizes[0].ToString("X"));
            }

            foreach (var v in VTXs)
            {
                if (v.BufferSizes.Length < 2)
                    continue;
                GenericMesh m = new GenericMesh();
                mdl.Meshes.Add(m);
                m.Name = v.Name;

                using (DataReader r = new DataReader(Data))
                {
                    for(uint i = 0; i < v.BufferSizes[1]; i+=2)
                    {
                        r.Seek((uint)v.BufferOffsets[1] + i);
                        //r.PrintPosition();
                        var index = r.ReadInt16();

                        GenericVertex vert = new GenericVertex();

                        foreach(var attr in v.Attribtues)
                        {
                            r.Seek((uint)(v.BufferOffsets[0] + v.BufferInfos[attr.RESBufferIndex].Offset + index * v.BufferInfos[attr.RESBufferIndex].Stride + attr.BufferOffset));

                            switch (attr.AttributeName)
                            {
                                case 1:
                                    vert.Pos = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                    break;
                                case 2:
                                    vert.Nrm = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                    break;
                                case 5:
                                    vert.UV0 = new OpenTK.Vector2(r.ReadSingle(), r.ReadSingle());
                                    break;
                                case 8:
                                    var b1 = v.Bones[r.ReadInt32()];
                                    var b2 = v.Bones[r.ReadInt32()];
                                    var b3 = v.Bones[r.ReadInt32()];
                                    var b4 = v.Bones[r.ReadInt32()];
                                    vert.Bones = new OpenTK.Vector4(Skeleton.IndexOf(Skeleton.GetBoneByName(b1)),
                                        Skeleton.IndexOf(Skeleton.GetBoneByName(b2)),
                                        Skeleton.IndexOf(Skeleton.GetBoneByName(b3)),
                                        Skeleton.IndexOf(Skeleton.GetBoneByName(b4)));
                                    break;
                                case 7:
                                    vert.Weights = new OpenTK.Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                    break;
                            }

                        }
                        
                        if(v.Bones.Count == 1)
                        {
                            vert.Bones = new OpenTK.Vector4(Skeleton.IndexOf(Skeleton.GetBoneByName(v.Bones[0])), 0, 0, 0);
                            vert.Weights = new OpenTK.Vector4(1, 0, 0, 0);
                        }

                        m.Vertices.Add(vert);
                        m.Triangles.Add(i / 2);

                        //Console.WriteLine(index + " " + v.BufferOffsets[0].ToString("X") + " " + v.BufferInfos[0].Offset + " " + v.BufferInfos[0].Stride);

                    }
                    //m.Optimize();
                }
            }

            return mdl;
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "$CFH";
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public class TXR
        {
            public int Unknown;
            public int Unkown2;
            public int Width;
            public int Height;
            public int Unkown3;
            public int Unkown4;
            public TXR(DataReader r)
            {
                Unknown = r.ReadInt32();
                Unkown2 = r.ReadInt16();
                Width = r.ReadInt16();
                Height = r.ReadInt16();
                Unkown3 = r.ReadInt16();
                Unkown4 = r.ReadInt32();
            }


            public int FaceOffset;
            public string Name;
            public int[] BufferOffsets;
            public int[] BufferSizes;

            public void ReadResource(DataReader r)
            {
                var start = r.Position;
                
                var bufferCount = r.ReadInt32() >> 24;
                r.ReadInt32();
                r.ReadInt32();
                Name = r.ReadString(start + r.ReadUInt32(), -1);

                BufferOffsets = new int[bufferCount];
                BufferSizes = new int[bufferCount];

                for (int i = 0; i < bufferCount; i++)
                {
                    BufferOffsets[i] = r.ReadInt32() & 0xFFFFFF;
                    BufferSizes[i] = r.ReadInt32();
                    r.ReadInt32();
                    r.ReadInt32();
                }
            }
        }

        private class VTXAttribute
        {
            public string MaterialName;
            public byte BufferOffset;
            public byte RESBufferIndex;
            public byte AttributeName;
            public byte unk1;
            public byte AttributeCount;
            public byte unk2;
        }

        public class VTXBufferInfo
        {
            public int Offset;
            public int Stride;
        }

        private class VTX
        {
            public List<string> Bones = new List<string>();
            public List<VTXAttribute> Attribtues = new List<VTXAttribute>();
            public List<VTXBufferInfo> BufferInfos = new List<VTXBufferInfo>();

            public int VertexCount;

            public VTX(DataReader r)
            {
                var start = r.Position;
                r.ReadInt32(); //0x0A

                var attrOffset = start + r.ReadUInt16();
                var attrCount = r.ReadByte();
                r.ReadByte();
                VertexCount = r.ReadInt32();
                r.ReadByte();
                r.ReadByte();
                r.ReadByte();
                var bufferCount = r.ReadByte();
                var unkOff = start + r.ReadUInt16();
                var bufferInfoOffset = start + r.ReadUInt16();
                var floatTableOffset = start + r.ReadUInt16();
                var boneNameOffset = start + r.ReadUInt16();
                var boneNameCount = r.ReadByte();
                r.ReadByte();
                r.ReadInt16();

                r.Seek(attrOffset);
                for(int i = 0; i < attrCount; i++)
                {
                    Attribtues.Add(new VTXAttribute()
                    {
                        MaterialName = r.ReadString(start + r.ReadUInt16(), -1),
                        BufferOffset = r.ReadByte(),
                        RESBufferIndex = r.ReadByte(),
                        AttributeName = r.ReadByte(),
                        unk1 = r.ReadByte(),
                        AttributeCount = r.ReadByte(),
                        unk2 = r.ReadByte(),
                    });
                }

                r.Seek(boneNameOffset);
                for (int i = 0; i < boneNameCount; i++)
                {
                    Bones.Add(r.ReadString(start + r.ReadUInt16(), -1));
                }

                r.Seek(bufferInfoOffset);
                for (int i = 0; i < bufferCount; i++)
                {
                    BufferInfos.Add(new VTXBufferInfo()
                    {
                        Offset = r.ReadInt32(),
                        Stride = r.ReadInt32()
                    });
                }


            }


            public int FaceOffset;
            public string Name;
            public int[] BufferOffsets;
            public int[] BufferSizes;

            public void ReadResource(DataReader r)
            {
                var start = r.Position;

                FaceOffset = r.ReadInt32();
                var bufferCount = FaceOffset >> 24;
                FaceOffset &= 0xFFFFFF;

                BufferOffsets = new int[bufferCount];
                BufferSizes = new int[bufferCount];

                r.ReadInt32();
                r.ReadInt32();
                Name = r.ReadString(start + r.ReadUInt32(), -1);

                for(int i = 0; i < bufferCount; i++)
                {
                    BufferOffsets[i] = r.ReadInt32() & 0xFFFFFF;
                    BufferSizes[i] = r.ReadInt32();
                    r.ReadInt32();
                    r.ReadInt32();
                }
            }

        }
    }
}
