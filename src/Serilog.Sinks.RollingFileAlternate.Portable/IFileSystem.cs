using System.Collections.Generic;
using System.IO;

namespace Serilog.Sinks.RollingFileAlternate
{
    /// <summary>
    /// Provides file system access.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Returns whether the specified directory exists.
        /// </summary>
        /// <returns></returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Creates the specified directory.
        /// </summary>
        /// <param name="path"></param>
        void CreateDirectory(string path);

        /// <summary>
        /// Opens the specified file in the mode necessary to append information only.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Stream OpenFileForAppend(string fullPath);

        /// <summary>
        /// Returns the names of the files in the specified directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        IEnumerable<string> GetFiles(string directory);
    }
}
