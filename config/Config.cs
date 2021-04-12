using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using WebPageRefresherC19.Facilities;

namespace WebPageRefresherC19.config
{
    public class Config
    {
        public int NotificationToAdminFrequency { get; set; }
        public double RefreshIntervalMinutes { get; set; }
        public string AdminEmail { get; set; }

        public string ChromeDriverPath { get; set; }

        public string VaccineUrl { get; set; }
        public string ElementClassToFind { get; set; }
        public string TextToFind { get; set; }

        public MailCredential MailCredential { get; set; }

        public List<string> Recipients { get; set; }
    }

    public class MailCredential
    {
        public string SenderUsername { get; set; }
        public string SenderPassword { get; set; }
    }

    public static class ConfigManager
    {
        public static Config Config { get; private set; }

        private static string _configPath = "";
        private static string _configDir = "";

        private static DateTime _lastTimeFileWatcherEventRaised;
        private static int updateIndex = 0;

        public static void SetupConfigPaths()
        {
            _configDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.ToString() + "\\config";
            _configPath = $@"{_configDir}\\config.json";
        }

        public static void ReadConfig()
        {
            Thread.Sleep(1000);
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_configPath));
        }

        public static void InitConfigWatcher()
        {
            var filSystemeWatcher = new FileSystemWatcher
            {
                Filter = "*.json",
                Path = _configDir,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            filSystemeWatcher.Changed += OnChanged;
            filSystemeWatcher.Error += OnError;
        }

        public static string PrettyJson(string unPrettyJson)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(unPrettyJson);
            return System.Text.Json.JsonSerializer.Serialize(jsonElement, options);
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                if (DateTime.Now.Subtract(_lastTimeFileWatcherEventRaised).TotalMilliseconds < 10000)
                {
                    return;
                }
            }

            _lastTimeFileWatcherEventRaised = DateTime.Now;

            Logger.Log($"Configuration changed detected #{updateIndex}");
            ReadConfig();
            Mail.SendMailConfigIsChanged(Config, updateIndex);
            Logger.Log($"Configuration updated #{updateIndex}");

            updateIndex++;
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Logger.Log("Error happened on file config change");
        }        
    }
}
