using Metanoia.Modeling;
using System.IO;

namespace Metanoia.Exporting
{
    [ExportAttribute(Name = "Generic Material", Extension = ".mat", ExportType = ExportType.Model)]
    public class ExportMat
    {
        public static void Save(string FilePath, GenericModel Model)
        {
            using (StreamWriter w = new StreamWriter(new FileStream(FilePath, FileMode.Create)))
            {
                foreach(var mat in Model.MaterialBank)
                {
                    w.WriteLine(mat.Key);
                    w.WriteLine("{");
                    w.WriteLine("Diffuse: " + mat.Value.TextureDiffuse);
                    w.WriteLine("WrapS: " + mat.Value.SWrap);
                    w.WriteLine("WrapT: " + mat.Value.TWrap);
                    w.WriteLine(mat.Value.MaterialInfo);
                    w.WriteLine("}");
                }
            }
        }
    }
}
