using Metanoia.Modeling;

namespace Metanoia.Formats
{
    public interface IModelFormat : IFileFormat
    {
        GenericModel ToGenericModel();
    }
}
