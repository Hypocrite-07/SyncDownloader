using Microsoft.VisualBasic;
using SyncDownloader.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static SyncDownloader.Config;

namespace SyncDownloader
{
    static class Downloader
    {
        public static bool NeedUpdateBrokedFiles => needUpdateBrokedFiles;
        public static bool HasTroubles => hasTrouble;

        private static DownloadInfo downloadInfo;
        private static ProjectInfo projectInfo;
        private static jsonFileInfo[] brokedFiles;
        private static bool hasTrouble = false;
        private static bool needUpdateBrokedFiles = false;
        private static Stopwatch sw;

        public static void Start()
        {
            projectInfo = Program.Configuration.ConfigInfo.ProjectInfo;
            downloadInfo = Program.Configuration.ConfigInfo.DownloadInfo;
            sw = new Stopwatch();
            CheckSumsAndDownload();
        }

        //Возвращает значенме неправильные файлов.
        private static jsonFileInfo[] CheckHashSums()
        {
            ArrayList _brokedFiles = new ArrayList();
            foreach (jsonFileInfo file in projectInfo.Entries)
            {
                var _path = $"{downloadInfo.DirInstallation}\\{file.Path}";
                if (File.Exists(_path))
                {
                    var localHash = Utils.CipherUtils.Get256HashSumFile(_path);
                    var localSize = Utils.FileUtils.GetSize(_path);
                    if (Equals(file.Hash, localHash) && Equals(file.Size, localSize))
                    {
                        if (Program.IsDebug)
                        {
                            Console.WriteLine($"UPD: {_path} is ignore. All right.");
                        }
                        continue;
                    }
                }
                try
                {
                    _brokedFiles.Add(file);
                }
                catch (Exception ex)
                {
                    hasTrouble = true;
                    if (Program.IsDebug)
                        Console.WriteLine($"{_path} has trouble.\n{ex.Message}\n");
                }
            }
            jsonFileInfo[] __brokedFiles = new jsonFileInfo[_brokedFiles.Count];
            for(int x = 0; x < _brokedFiles.Count; x++)
            {
                __brokedFiles[x] = _brokedFiles.ToArray().GetValue(x) as jsonFileInfo;
            }
            return __brokedFiles;
        }

        public static void CheckSumsAndDownload()
        {
            CheckFiles();
            if (needUpdateBrokedFiles)
                DownloadFiles();
        }

        public static void CheckFiles()
        {
            var bf = CheckHashSums();
            if (bf.Length > 0)
                needUpdateBrokedFiles = true;
            else
                needUpdateBrokedFiles = false;
            brokedFiles = bf;
        }

        public static void DownloadFiles()
        {
            if (Program.NeedlyUpdate)
            {
                foreach (var file in brokedFiles)
                {
                    DownloadFile(file);
                }
                if (needUpdateBrokedFiles)
                    CheckFiles();

            }
            if (needUpdateBrokedFiles)
            {
                foreach (var file in brokedFiles)
                {
                    DownloadFile(file);
                }
            }
            
            if (hasTrouble)
                return;
            needUpdateBrokedFiles = false;
        }


    

        private static void DownloadFile(jsonFileInfo file)
        {
            if (file == null) return;
            bool temp = false;
            var path = $"{downloadInfo.DirInstallation}\\{file.Path}";
            var dir = $"{path}".Remove(path.LastIndexOf('\\'));           
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);        
            var origPathPI = path;
            string tempPathPI = "";
            if (File.Exists($"{path}"))
            {
                path = $"{path}.temp";
                tempPathPI = path;
                temp = true;
            }
            Console.WriteLine($"Download: {file.Path}");
            Uri uri = new Uri(@$"{downloadInfo.UrlWebServer}{downloadInfo.WebDir}/{file.Path.Replace("\\\\", "/")}");
            Download(uri, path);
            if (Equals(Utils.CipherUtils.Get256HashSumFile(path), file.Hash) && Equals(Utils.FileUtils.GetSize(path), file.Size))
            {
                if (temp)
                {
                    if (tempPathPI == null)
                        return;
                    Utils.FileUtils.ToDeletePath(origPathPI, "Installed new verison already.");
                    FileSystem.Rename(tempPathPI, origPathPI);
                }
            }
            else
            {
                throw new Exception("Not Matched Downloaded Files.");
            }
                
        }

        public static void Download(Uri url, string path)
        {
            sw = new Stopwatch();
            WebClient webClient = new WebClient();
            sw.Start();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(@$"{url}"), $"{path}");
            while (webClient.IsBusy)
            {
                Thread.Sleep(200);
            }
        }

        private static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));

            var percent = e.ProgressPercentage.ToString() + "%";

            Console.WriteLine(string.Format("{0} MB's / {1} MB's | {2} | {3}",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"),
                percent,
                speed));
        }

        private static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();

            if (e.Cancelled == true)
            {
                if (Program.IsDebug)
                    Console.WriteLine("Download has been canceled.");
            }
            else
            {
                if (Program.IsDebug)
                    Console.WriteLine("Download completed!");
            }
        }
    }
}
