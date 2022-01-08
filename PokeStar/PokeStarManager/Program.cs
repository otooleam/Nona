using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace PokeStarManager
{
   class Program
   {
      static ConsoleEventDelegate handler;
      private delegate bool ConsoleEventDelegate(int eventType);
      [DllImport("kernel32.dll", SetLastError = true)]
      private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

      [DllImport("user32.dll")]
      static extern int SetWindowText(IntPtr hWnd, string text);


      static void Main(string[] args)
      {
         int errorStartCounter = 0;
         int MAX_RESTARTS = 3;
         bool resetFlag = false;
         bool end = false;

         handler = new ConsoleEventDelegate(ConsoleEventCallback);
         SetConsoleCtrlHandler(handler, true);

         string program_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         JObject env_file = JObject.Parse(File.ReadAllText($"{program_path}\\manager_env.json"));

         string pokestar_location = env_file.GetValue("pokestar_location").ToString();
         string version = env_file.GetValue("version").ToString();
         string server = env_file.GetValue("home_server").ToString();


         Console.Title = $"{server} Manager";

         Console.WriteLine($"{DateTime.Now:MM/dd/yyyy hh:mm tt} \t Starting Pokestar Manager {version}.");
         /**/
         while (!end)
         {
            Console.WriteLine($"{DateTime.Now:MM/dd/yyyy hh:mm tt} \t Checking Pokestar...");

            if (Process.GetProcessesByName("Pokestar").Length == 0)
            {
               if (!resetFlag)
               {
                  Console.WriteLine($"{DateTime.Now:MM/dd/yyyy hh:mm tt} \t Starting Pokestar...");
                  ProcessStartInfo p_info = new ProcessStartInfo
                  {
                     UseShellExecute = true,
                     CreateNoWindow = false,
                     WindowStyle = ProcessWindowStyle.Normal,
                     FileName = "Pokestar.exe",
                     WorkingDirectory = pokestar_location
                  };
                  Process p = Process.Start(p_info);
                  Thread.Sleep(100);  // <-- ugly hack
                  SetWindowText(p.MainWindowHandle, server);

                  Thread.Sleep(60000 * 5);
                  errorStartCounter++;
                  Console.WriteLine($"{DateTime.Now:MM/dd/yyyy hh:mm tt} \t Pokestar Start Up Complete.");
               }
               else
               {
                  end = true;
               }
            }
            else if (errorStartCounter != 0)
            {
               errorStartCounter = 0;
            }

            if (errorStartCounter > MAX_RESTARTS)
            {
               Console.WriteLine($"{DateTime.Now:MM/dd/yyyy hh:mm tt} \t ERROR: Maximum Timeout Attempts Reached.");
               resetFlag = true;
            }
            else
            {
               Console.WriteLine($"{DateTime.Now:MM/dd/yyyy hh:mm tt} \t Pokestar Is Running.");
            }

            Thread.Sleep(60000);
         }
         Console.Write("Press Any Key To Exit:");
         string _ = Console.ReadLine();
      }

      static bool ConsoleEventCallback(int eventType)
      {
         try
         {
            Process.GetProcessesByName("Pokestar")[0].Kill();
            return true;
         }
         catch 
         {
            return false;
         }
      }
   }
}
