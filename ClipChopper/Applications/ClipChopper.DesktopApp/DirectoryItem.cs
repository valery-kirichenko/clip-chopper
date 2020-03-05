namespace ClipChopper.DesktopApp
{
    public sealed class DirectoryItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public DirectoryItem(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public DirectoryItem(string path)
        {
            Name = System.IO.Path.GetFileName(path);
            Path = path;
        }
    }
}
