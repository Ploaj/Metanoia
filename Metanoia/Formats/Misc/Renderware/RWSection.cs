using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Formats.Misc.Renderware
{
    public class RWSection
    {

    }

    public class RWExtension : RWSection
    {
        public List<RWSection> Extensions = new List<RWSection>();
        
        public byte[] Data
        {
            set
            {
                using (var r = new RenderWareBinaryStreamReader(value))
                {
                    while(r.Position < r.Length)
                    {
                        Extensions.Add(r.ReadSection());
                    }
                }
            }
        }
    }

    public class RWStruct : RWSection
    {
        public byte[] StructData { get; set; }
    }

    public class RWClump : RWSection
    {
        public RWStruct Structs { get; set; }
        public RWFrameList FrameList { get; set; }
        public RWSection GeometryList { get; set; }
        public RWSection Atomic { get; set; }
        public RWSection Extension { get; set; }
    }

    public struct RWFrameStruct
    {
        public Matrix3 Transform;
        public Vector3 Position;
        public int Parent;
        public int Flags;
    }

    public class RWFrameList : RWSection
    {
        public RWStruct Structs { set
            {
                using(var r = new DataReader(value.StructData))
                {
                    Frames = r.ReadStructArray<RWFrameStruct>(r.ReadInt32());
                }
            }
        }
        public RWSection Extension { get; set; }

        public RWFrameStruct[] Frames;
    }

    public class RWGeometryList : RWSection
    {
        public RWStruct Structs
        {
            set
            {
                using (var r = new DataReader(value.StructData))
                {
                    GeometryCount = r.ReadInt32();
                }
            }
        }

        public int GeometryCount { get; set; }

        public List<RWGeometry> Geometries = new List<RWGeometry>();
    }

    [Flags]
    public enum RWGeometryFlags
    {
        TRISTRIP = 0x01,
        POSITIONS = 0x02,
        TEXTURED = 0x04,
        PRELIT = 0x08,
        NORMALS = 0x10,
        LIGHT = 0x20,
        MODULATE_MATERIAL = 0x40,
        TEXTURED2 = 0x08,
        NATIVE = 0x01000000,
        NATIVE_INSTANCE = 0x02000000,
        FLAGS_MASK = 0xFF,
        NATIVE_FLAGS_MASK = 0x0F000000
    }

    public class RWGeometry : RWSection
    {
        public RWStruct Structs
        {
            set
            {
                using (var r = new DataReader(value.StructData))
                {
                    Flags = (RWGeometryFlags)r.ReadUInt32();
                    TriangleCount = r.ReadInt32();
                    VertexCount = r.ReadInt32();
                    MorphCount = r.ReadInt32();
                    Console.WriteLine((Flags & RWGeometryFlags.FLAGS_MASK).ToString() + " " + VertexCount.ToString("X"));
                }
            }
        }

        public RWExtension Extension
        {
            set
            {
                foreach(var ext in value.Extensions)
                {

                }
                //Console.WriteLine("Extension Count: " + value.Extensions.Count);
            }
        }

        public RWGeometryFlags Flags { get; set; }
        public int TriangleCount { get; set; }
        public int VertexCount { get; set; }
        public int MorphCount { get; set; }
    }

    public class RW510 : RWSection
    {
        public RWStruct Struct
        {
            set
            {
                using (var r = new DataReader(value.StructData))
                {
                    r.Seek(0x14);
                    while(r.ReadInt32() == 0x01000104)
                    {
                        r.PrintPosition();
                        var type = r.ReadByte();
                        var unk = r.ReadByte();
                        var flag = r.ReadInt16();

                        switch (type)
                        {
                            case 0:
                                r.Skip(0x3B8);
                                break;
                            case 1:
                                r.Skip(0x168);
                                break;
                            case 2:
                                r.Skip(0xA8);
                                break;
                            case 3:
                                r.Skip(0x28);
                                break;
                            default:
                                throw new NotSupportedException("Unknown buffer struct type " + type.ToString("X"));
                        }
                    }

                }
            }
        }
    }

}
