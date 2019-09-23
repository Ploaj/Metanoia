using Metanoia.Exporting;
using Metanoia.Formats;
using Metanoia.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metanoia
{
    public class FormatManager
    {
        public static readonly FormatManager Instance = new FormatManager();

        private Dictionary<string, List<Type>> ExtensionToType = new Dictionary<string, List<Type>>();
        
        private List<IModelExporter> ModelExporters = new List<IModelExporter>();

        private List<Type> AllTypes = new List<Type>();

        public FormatManager()
        {
            var modelExportTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                         from assemblyType in domainAssembly.GetTypes()
                         where typeof(IModelExporter).IsAssignableFrom(assemblyType)
                         select assemblyType).ToArray();

            foreach (var t in modelExportTypes)
            {
                if(t != typeof(IModelExporter))
                    ModelExporters.Add((IModelExporter)Activator.CreateInstance(t));
            }

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

        public string GetModelExportFilter()
        {
            StringBuilder filter = new StringBuilder();

            foreach (var v in ModelExporters)
                filter.Append($"{v.Name()} (*{v.Extension()})|*{v.Extension()}|");
            filter.Append("All files (*.*)|*.*");

            return filter.ToString();
        }

        public bool ExportModel(string filePath, GenericModel m)
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLower();

            foreach (var v in ModelExporters)
            {
                if (v.Extension().Equals(ext))
                {
                    v.Export(filePath, m);
                    return true;
                }
            }
            return false;
        }

        public string GetExtensionFilter()
        {
            return "Supported Files |*" + string.Join(";*", ExtensionToType.Keys.ToArray());
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
