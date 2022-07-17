namespace SyncDownloader
{
    internal class Program
    {

        public static string directory;
        public static ConfigModel Configuration;
        public static bool IsDebug;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Downloader Yeaaaah!\n");
                IsDebug = Utils.OtherUtils.SetIsDebug(args);
                directory = Directory.GetCurrentDirectory();
                Configuration = new ConfigModel("sdcfg.json");
                if (Configuration.ConfigInfo.NeedlyUpdate || IsDebug)
                {
                    Downloader.Start();
                }
                Utils.OtherUtils.EndApp(false);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.OtherUtils.EndApp(true, 303);
            }
        }
    }
}