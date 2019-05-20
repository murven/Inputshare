using System;
using System.Configuration;

namespace InputshareLib
{
    public static class ConfigManager
    {
        public static string ReadConfig(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key] ?? null;
            }catch(Exception ex)
            {
                ISLogger.Write($"ConfigManager->Failed to read config: {ex.Message}");
                return null;
            }
        }

        public static void WriteConfig(string key, string value)
        {
            try
            {
                var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (conf.AppSettings.Settings[key] == null)
                    conf.AppSettings.Settings.Add(key, value);
                else
                    conf.AppSettings.Settings[key].Value = value;
                conf.Save();
                ISLogger.Write($"ConfigManager->{key} = {value}");
                ConfigurationManager.RefreshSection(conf.AppSettings.SectionInformation.Name);
            }catch(Exception ex)
            {
                ISLogger.Write($"ConfigManager->Failed to write to config: {ex.Message}");
            }
        }

    }
}
