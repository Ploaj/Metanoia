using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using System.Runtime.InteropServices;

namespace Metanoia.Formats.PS4
{
    public class GravityRush2GFX : I3DModelFormat
    {
        public string Name => "Gravity Rush 2 GFX";

        public string Extension => ".gfx";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        private GenericModel Model;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            public uint Hash;
            public ushort ID;
            public ushort Flags;
            public uint Offset;
            public int Size;
        }

        public void Open(FileItem file)
        {
            Model = new GenericModel();

            var skelPath = file.FilePath.Replace(".gfx", ".skel");

            if (File.Exists(skelPath))
            {
                Model.Skeleton = ReadSkel(skelPath);
            }

            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = false;

                r.Seek(0x10);
                var mCount = r.ReadInt32();
                var infoOffset = r.ReadUInt32();
                var dataOffset = r.ReadUInt32();

                for(uint i = 0; i < mCount; i++)
                {
                    r.Seek(0x30 + i * 16);
                    var entry = r.ReadStruct<Entry>();

                    //Console.WriteLine(i + " " + hash.ToString("X") + " " + index);

                    r.Seek(infoOffset + entry.Offset);

                    if(entry.ID == 2)
                    {
                        var b = new GenericBone();
                        b.Rotation = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        r.ReadSingle();
                        b.Position = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        r.ReadSingle();
                        b.Scale = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                        r.ReadSingle();

                        var unk = r.ReadInt32();
                        var flag = r.ReadInt16();
                        var meshCount = r.ReadInt16();
                        var unk2 = r.ReadInt32();
                        var hash2 = r.ReadInt32();

                        uint[] bt = new uint[meshCount];
                        for (int j = 0; j < meshCount; j++)
                        {
                            bt[j] = r.ReadUInt32();
                            var temp = r.Position;
                            r.Seek(0x30 + bt[j] * 0x10);
                            var m = ReadMesh(r, infoOffset, dataOffset);
                            if(m != null)
                                Model.Meshes.Add(m);
                            r.Seek(temp);
                        }
                    }
                    // material
                    /*if(index == 0x08 && (flags & 0x00FF) == 0x0022)
                    {
                        var fl = r.ReadInt32();
                        var index1 = r.ReadUInt32();
                        var index2 = r.ReadUInt32();
                        var index3 = r.ReadUInt32();
                        var index4 = r.ReadUInt32();
                        var index5 = r.ReadUInt32();
                        var index6 = r.ReadUInt32();
                        var index7 = r.ReadUInt32();

                        GetBuffer(r, index7, infoOffset, dataOffset);
                    }*/


                }
            }
        }

        private static GenericMesh ReadMesh(DataReader r, uint infoOffset, uint bufferOffset)
        {
            GenericMesh mesh = new GenericMesh();

            var meshEntry = r.ReadStruct<Entry>();

            if(meshEntry.ID == 4)
            {

                r.Seek(infoOffset + meshEntry.Offset);
                var lodCount = r.ReadInt32();

                r.Seek(infoOffset + meshEntry.Offset + 0x10);
                r.PrintPosition();
                var indexEntryIndex = r.ReadUInt32();
                
                r.Seek(0x30 + indexEntryIndex * 0x10);
                var indexInfoEntry = r.ReadStruct<Entry>();

                r.Seek(infoOffset + indexInfoEntry.Offset + 4);
                var indexBufferEntryIndex = r.ReadUInt32();

                r.Seek(0x30 + indexBufferEntryIndex * 0x10);
                var indexBufferEntry = r.ReadStruct<Entry>();

                r.Seek(infoOffset + indexBufferEntry.Offset);
                r.PrintPosition();
                var indexDataBufferEntry = r.ReadStruct<Entry>();
                
                if (indexBufferEntry.Flags == 0x010A || lodCount > 1)
                    return null;

                r.Seek(bufferOffset + indexDataBufferEntry.Offset);
                for(uint j = 0; j < indexDataBufferEntry.Size / 0x20; j++)
                {
                    GenericVertex v = new GenericVertex();
                    v.Pos = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    r.Skip(0x14);
                    mesh.Vertices.Add(v);
                    mesh.Triangles.Add(j);
                }

                Console.WriteLine(meshEntry.Hash.ToString("X") + " " + meshEntry.ID);
                Console.WriteLine("\t" + lodCount + " " + indexBufferEntry.Flags.ToString("X") + " " + (bufferOffset + indexDataBufferEntry.Offset).ToString("X"));


                /*r.Seek(infoOffset + meshEntry.Offset + 0x30);
                var vertexEntryIndex = r.ReadUInt32();

                r.Seek(0x30 + vertexEntryIndex * 0x10 + 0x08);
                var vertexBufferEntryIndex = r.ReadUInt32();
                
                r.Seek(0x30 + indexBufferEntryIndex * 0x10);
                r.Seek(0x30 + indexBufferEntryIndex * 0x10);*/
                return mesh;
            }

            return null;
        }

        private static byte[] GetBuffer(DataReader r, uint index, uint infoOffset, uint bufferOffset)
        {
            r.Seek(0x30 + index * 16);
            var hash = r.ReadUInt32();
            var id = r.ReadInt16();
            var flags = r.ReadInt16();
            var iOffset = r.ReadUInt32();
            var iSize = r.ReadInt32();

            r.Seek(iOffset + infoOffset);
            r.PrintPosition();
            return GetDataBuffer(r, bufferOffset);
        }

        private static byte[] GetDataBuffer(DataReader r, uint bufferOffset)
        {
            var hash = r.ReadUInt32();
            var id = r.ReadInt16();
            var flags = r.ReadInt16();
            var iOffset = r.ReadUInt32();
            var iSize = r.ReadInt32();

            r.Seek(iOffset + bufferOffset);
            r.PrintPosition();
            Console.WriteLine(id + " " + r.ReadInt32().ToString("X"));

            return null;
        }

        private static GenericSkeleton ReadSkel(string path)
        {
            GenericSkeleton skel = new GenericSkeleton();
            using (DataReader r = new DataReader(path))
            {
                r.BigEndian = false;

                r.Seek(0x10);
                var count = r.ReadInt16();
                var count2 = r.ReadInt16();
                var count3 = r.ReadInt32();

                var boneInfoOffset = r.Position + r.ReadUInt32();
                var boneParentInfoOffset = r.Position + r.ReadUInt32();
                var hashOffset = r.Position + r.ReadUInt32();
                // various hash table offsets

                for (uint i = 0; i < count; i++)
                {
                    GenericBone b = new GenericBone();

                    r.Seek(boneInfoOffset + 48 * i);
                    var rot = new OpenTK.Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()).Inverted();
                    rot.Normalize();
                    b.QuaternionRotation = rot;
                    b.Position = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    r.ReadSingle();
                    b.Scale = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    r.ReadSingle();

                    r.Seek(boneParentInfoOffset + 2 * i);
                    b.ParentIndex = r.ReadInt16();

                    r.Seek(hashOffset + 4 * i);
                    b.Name = "B_"+r.ReadInt32().ToString("X8");

                    skel.Bones.Add(b);
                }

            }
            return skel;
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString == "GFX2";
        }
    }
}
