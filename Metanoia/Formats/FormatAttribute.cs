using System;

namespace Metanoia.Formats
{
    public class FormatAttribute : Attribute
    {
        public string Extension;
        public string Magic;
        public string Description;

        public FormatAttribute(string Extension = "", string Magic = "", string Description = "")
        {
            this.Extension = Extension;
            this.Magic = Magic;
            this.Description = Description;
        }
    }
}
