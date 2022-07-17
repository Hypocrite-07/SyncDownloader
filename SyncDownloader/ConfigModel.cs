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
        public Config.Info ConfigInfo { get; set; }
        private Config.DownloadInfo _configDI { get; set; }
        private Config.ProjectInfo _configPI { get; set; }
        private bool _tempedPatch = false;
        private string _configFile;
 
        
        private string origPathPI;
        private string tempPathPI;
        private bool _isNeedUpdate = true;

        public ConfigModel(string filename)
        {
            this._configFile = filename;
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
                OtherUtils.EndApp(true, 404);
            }
        }

        public bool HasUpdate()
        {
            DownloadPatchInfo();
            if (Equals(ConfigInfo.NeedlyUpdate, _isNeedUpdate)){}
            else
                ConfigInfo.NeedlyUpdate = _isNeedUpdate;
           return ConfigInfo.NeedlyUpdate;
        }

        private void ParseDIConfig()
        {
            var jsonConfig = File.ReadAllText(_configFile, Encoding.UTF8);
            _configDI = JsonSerializer.Deserialize<Config.DownloadInfo>(jsonConfig)!;
            if(Program.IsDebug)
            {
                Console.WriteLine(
                    $"\nConfig Download Info Exists: {File.Exists(jsonConfig)}\n" +
                    $"WebHome Url: {_configDI.UrlWebServer}\n" +
                    $"WebDir: {_configDI.WebDir}\n" +
                    $"DirInstallation: {_configDI.DirInstallation}\n");
            }
        }

        private void CreateDirInstall()
        {
            bool isExists = Directory.Exists($"{_configDI.DirInstallation}");
            if (isExists == false)
            {
                Directory.CreateDirectory($"{_configDI.DirInstallation}");
                isExists = true;
            }
            if (Program.IsDebug)
            {
                Console.WriteLine($"Dir Installation Info Exists: {isExists}\n");
            }
        }

        private void DownloadPatchInfo()
        {
            var path = $"{_configDI.DirInstallation}\\patch-info";
            origPathPI = path;
            if (File.Exists($"{path}"))
            {
                path = $"{path}.temp";
                tempPathPI = path;
                _tempedPatch = true;
            }
            var url = new Uri(@$"{_configDI.UrlWebServer}{_configDI.WebDir}/patch-info");
            Console.WriteLine(url);
            Downloader.Download(url, path);
            if (Program.IsDebug)
            {
                Console.WriteLine(
                    "\nPatch Info Installed.\n" +
                    $"Temped Patch: {_tempedPatch}\n");
            }
            if (_tempedPatch)
            { 
                if(Verifyer.isNewerVersion(ParsePatch(origPathPI), ParsePatch(tempPathPI)))
                {
                    ToDeletePath(origPathPI, "Installed new verison already.");
                    FileSystem.Rename(tempPathPI, origPathPI);
                    _isNeedUpdate = true;
                }
                else
                {
                    if(Equals(CipherUtils.Get256HashSumFile(origPathPI), CipherUtils.Get256HashSumFile(tempPathPI)))
                    {
                        ToDeletePath(tempPathPI, "Installed this verison already.");
                        _isNeedUpdate = false;
                    }
                    else
                    {
                        ToDeletePath(origPathPI, "Hash's not match.");
                        FileSystem.Rename(tempPathPI, origPathPI);
                        _isNeedUpdate = true;
                    }
                }
            }
            _configPI = ParsePatch(origPathPI);
        }

        

        private Config.ProjectInfo ParsePatch(string path)
        {
            var jsonConfig = File.ReadAllText($"{path}", Encoding.UTF8);
            var ConfigPI = JsonSerializer.Deserialize<Config.ProjectInfo>(jsonConfig)!;
            return ConfigPI;
        }

        private void SyncParse()
        {
            ConfigInfo = new Config.Info { DownloadInfo = _configDI, ProjectInfo = _configPI, NeedlyUpdate = _isNeedUpdate };
        }
    }
}
