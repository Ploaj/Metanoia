using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Formats.Misc.Renderware
{
    public enum RenderWareSection
    {
        NAOBJECT = 0x0000,
        STRUCT = 0x0001,
        STRING = 0x0002,
        EXTENSION = 0x0003,
        TEXTURE = 0x0006,
        MATERIAL = 0x0007,
        MATLIST = 0x0008,
        FRAMELIST = 0x000E,
        GEOMETRY = 0x000F,
        CLUMP = 0x0010,
        ATOMIC = 0x0014,
        GEOMETRYLIST = 0x001A,
    }

    public class RenderWareBinaryStreamReader : DataReader
    {
        public RenderWareBinaryStreamReader(FileItem item) : base(item)
        {

        }

        public RenderWareBinaryStreamReader(byte[] data) : base(data)
        {

        }

        public RWSection ReadSection()
        {
            var type = (RenderWareSection)ReadInt32();
            var size = ReadUInt32();
            var build = ReadInt16();
            var version = ReadInt16();

            var end = Position + size;

            RWSection section = new RWSection();

            Console.WriteLine(type + " " + Position.ToString("X"));

            switch (type)
            {
                case RenderWareSection.EXTENSION:
                    var ext = new RWExtension();
                    ext.Data = ReadBytes((int)size);
                    section = ext;
                    break;
                case RenderWareSection.STRUCT:
                    var str = new RWStruct();
                    str.StructData = ReadBytes((int)size);
                    section = str;
                    break;
                case RenderWareSection.FRAMELIST:
                    var fl = new RWFrameList();
                    fl.Structs = ReadSection() as RWStruct;
                    fl.Extension = ReadSection();
                    section = fl;
                    break;
                case RenderWareSection.GEOMETRY:
                    var geo = new RWGeometry();
                    geo.Structs = ReadSection() as RWStruct;
                    ReadSection();
                    geo.Extension = ReadSection() as RWExtension;
                    section = geo;
                    break;
                case RenderWareSection.CLUMP:
                    var clump = new RWClump();
                    clump.Structs = ReadSection() as RWStruct;
                    clump.FrameList = ReadSection() as RWFrameList;
                    clump.GeometryList = ReadSection();
                    clump.Atomic = ReadSection();
                    clump.Extension = ReadSection();
                    section = clump;
                    break;
                case RenderWareSection.GEOMETRYLIST:
                    var geom = new RWGeometryList();
                    geom.Structs = ReadSection() as RWStruct;
                    for(int i =0; i < geom.GeometryCount; i++)
                        geom.Geometries.Add(ReadSection() as RWGeometry);
                    section = geom;
                    break;
                case (RenderWareSection)0x510:
                    var v510 = new RW510();
                    v510.Struct = ReadSection() as RWStruct;
                    section = v510;
                    break;
            }

            Seek(end);
            return section;
        }
    }
}
