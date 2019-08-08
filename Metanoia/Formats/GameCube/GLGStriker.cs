using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;

namespace Metanoia.Formats.GameCube
{
    [Format(".glg", "Super Mario Striker")]
    public class GLGStriker : IModelFormat
    {
        public void Open(FileItem File)
        {
            using (DataReader r = new DataReader(new FileStream(File.FilePath, FileMode.Open)))
            {
                r.BigEndian = true;

                r.Seek(4);
                r.Seek(r.ReadUInt32());
                int objectCount = r.ReadInt32();
                int unknownCount = r.ReadInt32();
            }
        }

        public GenericModel ToGenericModel()
        {
            return null;
        }
    }
}
