using System.IO;

namespace AR.Explorer
{
    public struct FileSystemEntry
    {
        public readonly string Path;
        public readonly string Name;
        public readonly string Extension;
        public readonly FileAttributes Attributes;

        public bool IsDirectory { get { return (Attributes & FileAttributes.Directory) == FileAttributes.Directory; } }

        public FileSystemEntry(string path, string name, bool isDirectory)
        {
            Path = path;
            Name = name;
            Extension = isDirectory ? null : System.IO.Path.GetExtension(name);
            Attributes = isDirectory ? FileAttributes.Directory : FileAttributes.Normal;
        }

        public FileSystemEntry(FileSystemInfo fileInfo)
        {
            Path = fileInfo.FullName;
            Name = fileInfo.Name;
            Extension = fileInfo.Extension;
            Attributes = fileInfo.Attributes;
        }
    }
}
