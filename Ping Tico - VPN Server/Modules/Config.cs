﻿using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;
using System.IO;

namespace PingTicoVPNServer.Modules
{
    /// <summary>
    /// When starting, the properties will be populated from the config.json file.
    /// </summary>

    public class ConfigData {
        public int mp_bridge_timeout_min { get; set; } = 5;
        public int mp_bridge_timeout_interval_sec { get; set; } = 5;
        public LogLevel log_level { get; set; } = LogLevel.INFO;
        public List<MultiPathServer> mp_servers { get; set; } = new List<MultiPathServer>();
    }

    public static class Config
    {
        public static ConfigData config = new ConfigData();

        //Loads configuration from config.json
        public static void LoadConfig()
        {
            try
            {
                Log.LogToConsole(LogLevel.INFO, "Loading Configuration...");

                if (File.Exists("./config.json"))
                {
                    config = JsonSerializer.Deserialize<ConfigData>(File.ReadAllBytes("./config.json"));
                }
                else
                {
                    Log.LogToConsole(LogLevel.INFO, "No configuration file was found, writting a new one with defalt values.");
                    SaveConfig();
                }

                Log.LogToConsole(LogLevel.INFO, "Loaded Configuration.");
            }
            catch (Exception ex)
            {
                Log.LogToConsole(LogLevel.ERROR, "Unable to load configuration file. Using Default Configuration.");
                Log.HandleError(ex);
            }            
        }

        public static void SaveConfig()
        {
            File.WriteAllText("./config.json", Encoding.UTF8.GetString(JsonSerializer.Serialize<ConfigData>(config)));
            Log.LogToConsole(LogLevel.INFO, "Saved Configuration File");
        }
    }
}
