namespace DirectoryCleaner
{
    //All dates are NOT in UTC, this is subject to change.
    public interface IOSEntity
    {
        public DateTime GetLastAccessDate();

        public DateTime GetLastWriteTimeDate();

        /// <summary>
        /// Throws IOException on an error
        /// </summary>
        public void MoveToDirectory(string directoryToMoveTo);

        public string Name { get; }

        public bool IsValid();
    }

    public class DirectoryEntity : IOSEntity
    {
        private readonly DirectoryInfo thisDirectory;
        public DirectoryEntity(string path)
        {
            thisDirectory = new(path);
        }

        public DateTime GetLastAccessDate() => thisDirectory.LastAccessTime;

        public DateTime GetLastWriteTimeDate() => thisDirectory.LastWriteTime;

        public void MoveToDirectory(string directoryToMoveTo) => thisDirectory.MoveTo(directoryToMoveTo + "/" + Name);

        public string Name { get => thisDirectory.Name; }

        public bool IsValid() => !thisDirectory.Attributes.HasFlag(FileAttributes.Hidden);
    }

    public class FileEntity : IOSEntity
    {
        private readonly FileInfo thisFile;

        public FileEntity(string path)
        {
            thisFile = new(path);
        }
        public DateTime GetLastAccessDate() => thisFile.LastAccessTime;

        public DateTime GetLastWriteTimeDate() => thisFile.LastWriteTime;
        public void MoveToDirectory(string directoryToMoveTo) => thisFile.MoveTo(directoryToMoveTo + "/" + thisFile.Name);

        public string Name { get => thisFile.Name; }

        public bool IsValid() => !thisFile.Attributes.HasFlag(FileAttributes.Hidden);
    }

    public static class OSObjectFactory
    {
        public static IOSEntity CreateIOSEntityFromPath(string path)
        {
            if (Directory.Exists(path)) return new DirectoryEntity(path);
            else if (File.Exists(path)) return new FileEntity(path);

            throw new InvalidOperationException("Could not determine if this object was a file or a directory.");
        }
    }

}
