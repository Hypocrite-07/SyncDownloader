using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SyncDownloader.Utils
{
    static class OtherUtils
    {
        public static bool SetIsDebug(string[] args)
        {
            if (args.Length == 1)
            {
                if (args[0] == "-D" || args[0] == "--debug")
                {
                    return true;
                    Console.WriteLine("Debug Mode is Activated");
                }
                else
                {
                    Console.WriteLine("Args was be entered incorrect.\nExamples args:\n\nDebug Mode: -D or --debug");
                    EndApp(true);
                }
            }
            return false;
        }

        public static void EndApp(bool sure)
        {
            if (sure == false)
            {
                Console.WriteLine("\nEnter any key for exit from app.");
                Console.ReadKey();
            }
            Environment.Exit(0);
        }

        public static void EndApp(bool sure, int exitCode)
        {
            if (sure == false  || Program.IsDebug)
            {
                Console.WriteLine("\nEnter any key for exit from app.");
                if (Program.IsDebug)
                    Console.WriteLine($"Sure Crash: {sure}");
                Console.ReadKey();
            }
            Environment.Exit(exitCode);
        }

        


    }
}
