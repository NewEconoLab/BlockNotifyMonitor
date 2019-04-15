using System;

namespace BlockNotifyMonitor
{
    class Program
    {
        static string MainName = "BlockNotifyMonitor";
        static void Main(string[] args)
        {
            Console.WriteLine("{0} start...", MainName);
            TaskRunner.run("config.json", MainName);
            Console.ReadKey();
        }
    }
}
