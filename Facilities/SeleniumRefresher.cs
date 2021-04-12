using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using WebPageRefresherC19.config;

namespace WebPageRefresherC19.Facilities
{
    class SeleniumRefresher
    {
        public static string RefreshPageLoop()
        {
            IWebDriver driver = new ChromeDriver(ConfigManager.Config.ChromeDriverPath);

            Logger.Log("Inizio ricerca vaccino");

            var retry = true;
            var retryNum = 0;
            string stopTryReasonPhrase = "";
            while (retry)
            {
                retryNum = retryNum + 1;
                Console.WriteLine($"\n\nReload page {DateTime.Now} \n\n");

                driver.Navigate().GoToUrl(ConfigManager.Config.VaccineUrl);
                Thread.Sleep(5000);

                var btns = driver.FindElements(By.XPath(ConfigManager.Config.ElementClassToFind));

                if (btns.Count < 5)
                {
                    retry = false;
                    stopTryReasonPhrase = "Il sito è stato aggiornato. Controllare la nuova struttura";
                }
                else
                {
                    var txt = btns[4].Text;
                    retry = txt.Contains(ConfigManager.Config.TextToFind);
                    stopTryReasonPhrase = retry ? 
                        $"Vaccino non ancora disponibile" :
                        $"Il vaccino è disponibile per la prenotazione online @{ConfigManager.Config.VaccineUrl}";
                }

                if (retry)
                {
                    Logger.Log(stopTryReasonPhrase);

                    Thread.Sleep(1000 * 60 * (int)ConfigManager.Config.RefreshIntervalMinutes);

                    if (retryNum % ConfigManager.Config.NotificationToAdminFrequency == 0)
                    {
                        Mail.SendLogMail(ConfigManager.Config);
                    }
                }
            }

            driver.Close();
            return stopTryReasonPhrase;
        }
    }
}
