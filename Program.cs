using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace WebPageRefresherC19
{
    class Program
    {
        static string covidLogDir = $@"{Directory.GetCurrentDirectory()}\CovidCrawler";
        static string covidLogPath = $@"{covidLogDir}\covidLog.txt";
        static string chromeDriverPath = "C:\\ChromeDriver";
        static int refreshIntervalMinutes = 10;
        static string logHistory = "";

        static void Main(string[] args)
        {
            RefreshPage();

            SendMail("...@gmail.com");
            SendMail("...@gmail.com");
            SendMail("...@gmail.com");
        }

        public static void RefreshPage()
        {
            IWebDriver driver = new ChromeDriver(chromeDriverPath);
            var retry = true;

            ProcessHelper.OpenVsCode(covidLogPath);

            Log("Inizio ricerca vaccino");

            while (retry)
            {
                Console.WriteLine($"\n\nReload page {DateTime.Now} \n\n");

                driver.Navigate().GoToUrl("https://prenotavaccino.sanita.toscana.it/#/home");
                Thread.Sleep(5000);

                var btns = driver.FindElements(By.XPath("//button[@class='bg-transparent border-0 col-md-4 col-sm-12']"));
                var txt = btns[4].Text;
                retry = txt.Contains("CHIUSO");

                if (retry)
                {
                    Log("Vaccino non ancora disponibile");

                    Thread.Sleep(1000 * 60 * refreshIntervalMinutes);
                    //Thread.Sleep(1000);
                }
            }

            driver.Close();
        }

        private static void SendMail(string receiver)
        {
            Log("Vaccino disponibile => invio mail");

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("...@gmail.com");
                mail.To.Add(receiver);
                mail.Subject = "Vaccino disponibile";
                mail.Body = $"Il vaccino è disponibile per la prenotazione dalle {DateTime.Now}\n\n{logHistory}";
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("...@gmail.com", "password");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
        
        private static void Log(string log)
        {

            Directory.CreateDirectory(covidLogDir);
            using (StreamWriter sw = File.AppendText(covidLogPath))
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
    }
}
