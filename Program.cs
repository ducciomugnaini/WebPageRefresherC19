using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace WebPageRefresherC19
{
    public class Config
    {
        public int NotificationToAdminFrequency { get; set; }
        public double RefreshIntervalMinutes { get; set; }
        public string AdminEmail { get; set; }

        public string ChromeDriverPath { get; set; }

        public MailCredential MailCredential { get; set; }

        public List<string> Recipients { get; set; }
    }

    public class MailCredential
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class Program
    {
        static string logDir = "";
        static string logPath = "";
        static string configPath = "";
        static string configDir = "";

        static string logHistory = "";

        static Config config;

        static DateTime _lastTimeFileWatcherEventRaised;

        static Program()
        {
            SetupDir();
            ReadConfig();
            InitConfigWatcher();
        }

        static void Main(string[] args)
        {
            RefreshPage();

            SendSuccessMail();            
        }

        public static void RefreshPage()
        {
            IWebDriver driver = new ChromeDriver(config.ChromeDriverPath);
            ProcessHelper.OpenVsCode(logPath);

            Log("Inizio ricerca vaccino");

            var retry = true;
            var retryNum = 0;
            while (retry)
            {
                retryNum = retryNum + 1;
                Console.WriteLine($"\n\nReload page {DateTime.Now} \n\n");

                driver.Navigate().GoToUrl("https://prenotavaccino.sanita.toscana.it/#/home");
                Thread.Sleep(5000);

                var btns = driver.FindElements(By.XPath("//button[@class='bg-transparent border-0 col-md-4 col-sm-12']"));
                var txt = btns[4].Text;
                retry = txt.Contains("CHIUSO");

                if (retry)
                {
                    Log("Vaccino non ancora disponibile");

                    Thread.Sleep(1000 * 60 * (int)config.RefreshIntervalMinutes);

                    if (retryNum % config.NotificationToAdminFrequency == 0)
                    {
                        SendLogMail(config.AdminEmail);
                    }
                }
            }

            driver.Close();
        }

        private static void SetupDir()
        {
            logDir = $@"{Directory.GetCurrentDirectory()}\CovidCrawler";
            logPath = $@"{logDir}\covidLog.txt";

            configDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.ToString() + "\\config";
            configPath = $@"{configDir}\\config.json";
        }

        private static void SendMail(string receiver, string subject, string body)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(config.MailCredential.Username);
                mail.To.Add(receiver);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(config.MailCredential.Username, config.MailCredential.Password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        private static void SendSuccessMail()
        {
            foreach (var recipients in config.Recipients)
            {
                SendSuccessMail(recipients);
            }
        }

        private static void SendSuccessMail(string receiver)
        {
            Log("Vaccino disponibile => invio mail");

            SendMail(receiver,
                "Vaccino disponibile",
                $"Il vaccino è disponibile per la prenotazione dalle {DateTime.Now}\n\n{logHistory}");
        }

        private static void SendLogMail(string receiver)
        {
            SendMail(receiver,
                "Log WebRefresherC19",
                logHistory);
        }

        private static void Log(string log)
        {
            Directory.CreateDirectory(logDir);
            using (StreamWriter sw = File.AppendText(logPath))
            {
                var logPhrase = $"{DateTime.Now} - {log}";
                sw.WriteLine(logPhrase);
                logHistory = logHistory + logPhrase + "\n";
            }
        }

        public static class ProcessHelper
        {
            public static void Open(string app, string args)
            {
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = app;
                    myProcess.StartInfo.Arguments = args;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.Start();
                }
            }

            public static void OpenVsCode(string filePath)
            {
                Open("code", filePath);
            }
        }

        private static void ReadConfig()
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        }

        private static void InitConfigWatcher()
        {
            var filSystemeWatcher = new FileSystemWatcher
            {
                Filter = "*.json",
                Path = configDir,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            filSystemeWatcher.Changed += OnChanged;
            filSystemeWatcher.Error += OnError;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {                
                if (DateTime.Now.Subtract(_lastTimeFileWatcherEventRaised).TotalMilliseconds < 500)
                {
                    return;
                }
            }

            _lastTimeFileWatcherEventRaised = DateTime.Now;

            Log("Configuration changed");
            ReadConfig();
            Log("Configuration updated");
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Log("Error happened on file config change");
        }
    }
}
