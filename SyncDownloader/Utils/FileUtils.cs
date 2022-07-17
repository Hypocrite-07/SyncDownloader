using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDownloader.Utils
{
    static class FileUtils
    {
        public static string GetFormatedPath(string path)
        {
            return path.Replace(Program.Dir + "\\", "");
        }

        public static void ToDeletePath(string path, string reason)
        {
            if(Program.IsDebug)
            {
                Console.WriteLine($"\n{path} will be deleted. Reason: {reason}\n");
            }
            File.Delete(path);
        }
        
        public static string GetFormatedPath(string path, string directory)
        {
            return path.Replace(directory + "\\", "");
        }

        public static long GetSize(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.Length;
        }

    }
}
