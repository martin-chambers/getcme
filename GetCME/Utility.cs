using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCME
{
    public static class Utility
    {
        public static string GetFilenameFromFilepath(string filepath)
        {
            int posLastSlash = Math.Max(filepath.LastIndexOf("\\"), filepath.LastIndexOf("/"));
            string fileName = filepath.Substring(posLastSlash + 1, filepath.Length - (posLastSlash + 1));
            return fileName;
        }
        public static string RemoveFilenameFromFilepath(string filepath)
        {
            int posLastSlash = Math.Max(filepath.LastIndexOf("\\"), filepath.LastIndexOf("/"));
            string filePath = filepath.Substring(0, posLastSlash);
            return filePath;
        }
    }
}
