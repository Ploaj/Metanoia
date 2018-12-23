using System;

namespace Metanoia.Exporting
{
    public enum ExportType
    {
        Model,
        Texture,
        Animation
    }

    public class ExportAttribute : Attribute
    {
        public string Name;
        public string Extension;
        public ExportType ExportType;

        public ExportAttribute(string Name = "", string Extension = "", ExportType ExportType = ExportType.Model)
        {
            this.Name = Name;
            this.ExportType = ExportType;
            this.Extension = Extension;
        }
    }
}
