using Metanoia.Modeling;

namespace Metanoia.Formats
{
    public interface IFormat
    {
        string Name { get; }
        string Extension { get; }
        string Description { get; }

        bool CanOpen { get; }
        bool CanSave { get; }

        bool Verify(FileItem file);

        void Open(FileItem file);

        void Save(string filePath);
    }

    public interface IContainerFormat : IFormat
    {
        FileItem[] GetFiles();
    }

    public interface IModelContainerFormat : IContainerFormat
    {
        GenericModel ToGenericModel();
    }

    public interface I3DModelFormat : IFormat
    {
        GenericModel ToGenericModel();
    }

    public interface ITextureFormat : IFormat
    {

    }

    public interface IAnimationFormat : IFormat
    {
        GenericAnimation ToGenericAnimation();
    }
}
