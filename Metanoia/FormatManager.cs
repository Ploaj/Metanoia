using Metanoia.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metanoia
{
    public class FormatManager
    {
        public static readonly FormatManager Instance = new FormatManager();

        private Dictionary<string, List<Type>> ExtensionToType = new Dictionary<string, List<Type>>();

        private List<Type> AllTypes = new List<Type>();

        public string GetExtensionFilter()
        {
            return "Supported Files |*" + string.Join(";*", ExtensionToType.Keys.ToArray());
        }

        public FormatManager()
        {
            var Types = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                         from assemblyType in domainAssembly.GetTypes()
                         where typeof(IFormat).IsAssignableFrom(assemblyType)
                         select assemblyType).ToArray();
            
            foreach(var t in Types)
            {
                if (t.IsInterface)
                    continue;

                AllTypes.Add(t);

                IFormat format = Activator.CreateInstance(t) as IFormat;

                var extension = format.Extension;
                if (!ExtensionToType.ContainsKey(extension))
                    ExtensionToType.Add(extension, new List<Type>());
                ExtensionToType[extension].Add(t);
            }
        }

        public IFormat Open(FileItem f)
        {
            // check extensions
            if (f.Extension != null)
            if (ExtensionToType.ContainsKey(f.Extension))
            {
                foreach(var type in ExtensionToType[f.Extension])
                {
                    IFormat format = Activator.CreateInstance(type) as IFormat;
                    if(format.Verify(f) && format.CanOpen)
                    {
                        format.Open(f);
                        return format;
                    }
                }
            }

            // check verify
            foreach(var type in AllTypes)
            {
                IFormat format = Activator.CreateInstance(type) as IFormat;

                if (format.Verify(f) && format.CanOpen)
                {
                    format.Open(f);
                    return format;
                }
            }

            // not supported
            return null;
        }
        
    }
}
