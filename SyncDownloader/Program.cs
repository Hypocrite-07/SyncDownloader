namespace SyncDownloader
{
    internal class Program
    {

        private static string directory;
        private static ConfigModel configuration;
        private static bool isDebug;
        private static bool needlyUpdate;
        public static string Dir => directory;
        public static bool NeedlyUpdate => needlyUpdate;
        public static ConfigModel Configuration => configuration;
        public static bool IsDebug => isDebug;
        

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Downloader Yeaaaah!\n");
                isDebug = Utils.OtherUtils.SetIsDebug(args);
                directory = Directory.GetCurrentDirectory();
                configuration = new ConfigModel("sdcfg.json");
                if (Configuration.ConfigInfo.NeedlyUpdate || IsDebug)
                {
                    Downloader.Start();
                }
                Utils.OtherUtils.EndApp(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.OtherUtils.EndApp(true, 303);
            }
        }
    }
}