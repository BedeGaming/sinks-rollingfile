using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.RollingFileAlternate
{
    /// <summary>
    /// Provides access to the file system.
    /// </summary>
    public class FileSystem : IFileSystem
    {
        /// <summary>
        /// Returns whether the specified directory exists.
        /// </summary>
        /// <returns></returns>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Creates the specified directory.
        /// </summary>
        /// <param name="path"></param>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Opens the specified file in the mode necessary to append information only.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public System.IO.Stream OpenFileForAppend(string fullPath)
        {
            return File.Open(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        }

        /// <summary>
        /// Returns the names of the files in the specified directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFiles(string directory)
        {
            return Directory.GetFiles(directory);
        }
    }
}
