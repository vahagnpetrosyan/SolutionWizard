using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;

namespace SolutionWizard.Utilities
{
    public static class ConfigurationHelper
    {
        private const string configFileName = "settings.ini";
        private static FileIniDataParser parser;
        private static IniParser.Model.IniData config;
        
        static ConfigurationHelper()
        {
            parser = new FileIniDataParser();
            config = parser.ReadFile(configFileName);
        }
        
        public static string GetSetting(string key)
        {
            return config["defaults"][key];               
        }

        public static void SetSetting(string key, string value)
        {
            config["defaults"][key] = value;
        }

        public static void SaveConfigs()
        {
            parser.WriteFile(configFileName, config);   
        }
    }
}
