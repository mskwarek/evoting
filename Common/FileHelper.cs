using System;
using System.IO;
using System.Text;

namespace Common
{
    public class FileHelper : IFileHelper
    {
        public bool DirectoryExists(string location) => Directory.Exists(location);
        public string ReadAllText(string location) => File.ReadAllText(location, Encoding.UTF8);
        public void WriteAllText(string location, string text) => File.WriteAllText(location, text);
    }
}