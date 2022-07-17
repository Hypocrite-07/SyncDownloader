using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDownloader
{
    static class Verifyer
    {
        public static bool isNewerVersion(Config.ProjectInfo _old, Config.ProjectInfo _new)
        {
            if (new Version(_new.Version) > new Version(_old.Version))
            {
                if(Program.IsDebug)
                {
                    Console.WriteLine(
                        $"\nFound New Version!\n" +
                        $"Current Version: {new Version(_old.Version)}\n" +
                        $"Current Entries Count: {_old.Entries.Length}\n" +
                        $"New Version: {new Version(_new.Version)}\n" +
                        $"New Entries Count: {_new.Entries.Length}\n");
                }
                DeleteOlderPaths(_old, _new);
                return true;
            }
            return false;
        }

        private static void DeleteOlderPaths(Config.ProjectInfo _old, Config.ProjectInfo _new)
        {
            var oldPaths = _old.Entries;
            var newPaths = _new.Entries;
            foreach (var oldPath in oldPaths)
            {
                if (newPaths.Contains(oldPath))
                    continue;
                Utils.FileUtils.ToDeletePath(oldPath.Path, "Not Actually.");
            }
        }

        public static bool isMatchFile(string filename_old, string filename_new)
        {
            var hash_old = Utils.CipherUtils.Get256HashSumFile(filename_old);
            var hash_new = Utils.CipherUtils.Get256HashSumFile(filename_new);
            if(Program.IsDebug)
            {
                Console.WriteLine(
                    $"\nCurrent File: {filename_old}\n" +
                    $"Current Hash: {hash_old}\n" +
                    $"New File: {filename_new}\n" +
                    $"New Hash: {hash_new}\n");
            }
            if (hash_old.Equals(hash_new))
                return true;
            return false;
        }
    }
}
