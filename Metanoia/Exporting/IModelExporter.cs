using Metanoia.Modeling;

namespace Metanoia.Exporting
{
    public interface IModelExporter
    {
        string Name();

        string Extension();

        void Export(string filePath, GenericModel model);

    }
}
