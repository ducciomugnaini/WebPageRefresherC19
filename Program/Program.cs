using WebPageRefresherC19.config;
using WebPageRefresherC19.Facilities;

namespace WebPageRefresherC19
{
    class Program
    {
        static Program()
        {
            ConfigManager.SetupConfigPaths();
            ConfigManager.ReadConfig();
            ConfigManager.InitConfigWatcher();
        }

        static void Main()
        {
            Logger.OpenLog();
            var reasonPhrase = SeleniumRefresher.RefreshPageLoop();
            Mail.SendBroadcastMail(ConfigManager.Config, reasonPhrase);
        }        
    }
}
