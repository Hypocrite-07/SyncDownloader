using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SyncDownloader.Utils
{
    static class CipherUtils
    {
        public static string Get256HashSumFile(string filename)
        {
            if (filename == null) return null;
            FileStream fs = File.OpenRead(filename);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, (int)fs.Length);
            fs.Close();
            return Convert.ToHexString(SHA256.Create().ComputeHash(fileData)).ToLower();
        }
    }
}
