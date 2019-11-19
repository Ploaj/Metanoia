using Metanoia.Modeling;

namespace Metanoia.Exporting
{
    public interface IAnimationExporter
    {
        string Name();

        string Extension();

        void Export(string filePath, GenericSkeleton skeleton, GenericAnimation animation);
    }
}
