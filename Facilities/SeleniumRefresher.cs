using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using WebPageRefresherC19.config;

namespace WebPageRefresherC19.Facilities
{
    class SeleniumRefresher
    {
        public static void RefreshPageLoop()
        {
            IWebDriver driver = new ChromeDriver(ConfigManager.Config.ChromeDriverPath);

            Logger.Log("Inizio ricerca vaccino");

            var retry = true;
            var retryNum = 0;
            while (retry)
            {
                retryNum = retryNum + 1;
                Console.WriteLine($"\n\nReload page {DateTime.Now} \n\n");

                driver.Navigate().GoToUrl(ConfigManager.Config.VaccineUrl);
                Thread.Sleep(5000);

                var btns = driver.FindElements(By.XPath(ConfigManager.Config.ElementClassToFind));
                var txt = btns[4].Text;
                retry = txt.Contains(ConfigManager.Config.TextToFind);

                if (retry)
                {
                    Logger.Log("Vaccino non ancora disponibile");

                    Thread.Sleep(1000 * 60 * (int)ConfigManager.Config.RefreshIntervalMinutes);
                    
                    if (retryNum % ConfigManager.Config.NotificationToAdminFrequency == 0)
                    {
                        Mail.SendLogMail(ConfigManager.Config);
                    }
                }
            }

            driver.Close();
        }
    }
}
