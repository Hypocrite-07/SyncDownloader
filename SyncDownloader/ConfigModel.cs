using Microsoft.VisualBasic;
using SyncDownloader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SyncDownloader.Utils.FileUtils;

namespace SyncDownloader
{

    public class Config
    {

        public class Info
        {
            public DownloadInfo DownloadInfo { get; set; }
            public ProjectInfo ProjectInfo { get; set; }

            public bool NeedlyUpdate { get; set; }
        }

        public class DownloadInfo
        {
            public string UrlWebServer { get; set; }
            public string WebDir { get; set; }
            public string DirInstallation { get; set; }
        }
        
        public class ProjectInfo
        {
            public string Version { get; set; }
            public jsonFileInfo[] Entries { get; set; }
        }

        public class jsonFileInfo
        {
            public string Path { get; set; }
            public string Hash { get; set; }
            public long Size { get; set; }
        }


    }

    internal class ConfigModel
    {

        private string ConfigFile;
        public Config.Info ConfigInfo { get; set; }
        private Config.DownloadInfo ConfigDI { get; set; }
        private Config.ProjectInfo ConfigPI { get; set; }

        public bool TempedPatch = false;
        private string origPathPI;
        private string tempPathPI;
        private bool IsNeedUpdate = true;

        public ConfigModel(string filename)
        {
            this.ConfigFile = filename;
            ParseFull();
        }

        private void ParseFull()
        {
            try
            {
                ParseDIConfig();
                CreateDirInstall();
                DownloadPatchInfo();
                SyncParse();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.OtherUtils.EndApp(true, 404);
            }
        }

        private void ParseDIConfig()
        {
            var jsonConfig = File.ReadAllText(ConfigFile, Encoding.UTF8);
            ConfigDI = JsonSerializer.Deserialize<Config.DownloadInfo>(jsonConfig)!;
            if(Program.IsDebug)
            {
                Console.WriteLine(
                    $"\nConfig Download Info Exists: {File.Exists(jsonConfig)}\n" +
                    $"WebHome Url: {ConfigDI.UrlWebServer}\n" +
                    $"WebDir: {ConfigDI.WebDir}\n" +
                    $"DirInstallation: {ConfigDI.DirInstallation}\n");
            }
        }

        private void CreateDirInstall()
        {
            bool isExists = Directory.Exists($"{ConfigDI.DirInstallation}");
            if (isExists == false)
            {
                Directory.CreateDirectory($"{ConfigDI.DirInstallation}");
                isExists = true;
            }
            if (Program.IsDebug)
            {
                Console.WriteLine($"Dir Installation Info Exists: {isExists}\n");
            }
        }

        private void DownloadPatchInfo()
        {
            var path = $"{ConfigDI.DirInstallation}\\patch-info";
            origPathPI = path;
            if (File.Exists($"{path}"))
            {
                path = $"{path}.temp";
                tempPathPI = path;
                TempedPatch = true;
            }
            var url = new Uri(@$"{ConfigDI.UrlWebServer}{ConfigDI.WebDir}/patch-info");
            Console.WriteLine(url);
            Downloader.Download(url, path);
            if (Program.IsDebug)
            {
                Console.WriteLine(
                    "\nPatch Info Installed.\n" +
                    $"Temped Patch: {TempedPatch}\n");
            }
            if (TempedPatch)
            { 
                if(Verifyer.isNewerVersion(ParsePatch(origPathPI), ParsePatch(tempPathPI)))
                {
                    ToDeletePath(origPathPI, "Installed new verison already.");
                    FileSystem.Rename(tempPathPI, origPathPI);
                    IsNeedUpdate = true;
                }
                else
                {
                    if(Equals(CipherUtils.Get256HashSumFile(origPathPI), CipherUtils.Get256HashSumFile(tempPathPI)))
                    {
                        ToDeletePath(tempPathPI, "Installed this verison already.");
                        IsNeedUpdate = false;
                    }
                    else
                    {
                        ToDeletePath(origPathPI, "Hash's not match.");
                        FileSystem.Rename(tempPathPI, origPathPI);
                        IsNeedUpdate = true;
                    }
                }
            }
            ConfigPI = ParsePatch(origPathPI);
        }

        

        private Config.ProjectInfo ParsePatch(string path)
        {
            var jsonConfig = File.ReadAllText($"{path}", Encoding.UTF8);
            var ConfigPI = JsonSerializer.Deserialize<Config.ProjectInfo>(jsonConfig)!;
            return ConfigPI;
        }

        private void SyncParse()
        {
            ConfigInfo = new Config.Info { DownloadInfo = ConfigDI, ProjectInfo = ConfigPI, NeedlyUpdate = IsNeedUpdate };
        }
    }
}
