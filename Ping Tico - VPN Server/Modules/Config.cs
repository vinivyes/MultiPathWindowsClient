using System;
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
        public int mp_bridge_timeout_min { get; set; } = 5;             //How often bridges need to be renewed
        public int mp_bridge_timeout_interval_sec { get; set; } = 5;    //Interval between each bridge timeout (Prevents all bridges from going down at once)
        public LogLevel log_level { get; set; } = LogLevel.INFO;        //Max level of Log Output.
        public List<MultiPathConnection> mp_servers { get; set; } = new List<MultiPathConnection>();          //All running MP Servers

        public bool MultiPathPortFree(int server_port)
        {
            foreach(MultiPathConnection c in mp_servers) { if(c.serverPort == server_port) return false; }
            return true;
        }
    }

    public static class Config
    {
        public static ConfigData config = new ConfigData();

        //Loads configuration from config.json
        public static void LoadConfig()
        {
            try
            {
                Log.ToConsole(LogLevel.INFO, "Loading Configuration...");

                if (File.Exists("./config.json"))
                {
                    config = JsonSerializer.Deserialize<ConfigData>(File.ReadAllBytes("./config.json"));
                    foreach(MultiPathConnection mp in config.mp_servers)
                    {
                        mp.StartMultiPath();
                    }
                }
                else
                {
                    Log.ToConsole(LogLevel.INFO, "No configuration file was found, writting a new one with defalt values.");
                    SaveConfig();
                }

                Log.ToConsole(LogLevel.INFO, "Loaded Configuration.");
            }
            catch (Exception ex)
            {
                Log.ToConsole(LogLevel.ERROR, "Unable to load configuration file. Using Default Configuration.");
                Log.HandleError(ex);
            }            
        }

        //Saves latest config to config.json
        public static void SaveConfig()
        {
            File.WriteAllText("./config.json", Encoding.UTF8.GetString(JsonSerializer.Serialize<ConfigData>(config)));
            Log.ToConsole(LogLevel.INFO, "Saved Configuration File");
        }
        
    }
}
