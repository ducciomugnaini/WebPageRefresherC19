using System;
using System.IO;

namespace WebPageRefresherC19.Facilities
{
    public static class Logger
    {
        private static string logDir = "";
        private static string logPath = "";       

        public static string logHistory = "";

        static Logger()
        {
            logDir = $@"{Directory.GetCurrentDirectory()}\CovidCrawler";
            logPath = $@"{logDir}\covidLog.txt";            
        }

        public static void OpenLog()
        {
            ProcessHelper.OpenVsCode(logPath);
        }

        public static void Log(string log)
        {
            Directory.CreateDirectory(logDir);
            using (StreamWriter sw = File.AppendText(logPath))
            {
                var logPhrase = $"{DateTime.Now} - {log}";
                sw.WriteLine(logPhrase);
                logHistory = logHistory + logPhrase + "\n";
            }
        }
    }
}
