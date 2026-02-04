using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SEU_AutoConnect
{
    public class Config
    {
        [JsonProperty("username")]
        public string? Username { get; set; }
        
        [JsonProperty("password")]
        public string? Password { get; set; }
        
        [JsonProperty("wifi_ssid")]
        public string WifiSsid { get; set; } = "SEU-WLAN";
        
        [JsonProperty("login_ip")]
        public string LoginIp { get; set; } = "http://202.119.25.2";
        
        [JsonProperty("not_sign_in_title")]
        public string NotSignInTitle { get; set; } = "上网登录页";
        
        [JsonProperty("result_return")]
        public string ResultReturn { get; set; } = "\"result\":\"1\"";
        
        [JsonProperty("signed_in_title")]
        public string SignedInTitle { get; set; } = "注销页";
        
        [JsonProperty("check_interval")]
        public int CheckInterval { get; set; } = 5;
        
        [JsonProperty("reconnect_delay")]
        public int ReconnectDelay { get; set; } = 3;
        
        [JsonProperty("max_retry")]
        public int MaxRetry { get; set; } = 3;

        [JsonProperty("auto_start_service")]
        public bool AutoStartService { get; set; } = false;

        [JsonProperty("start_minimized")]
        public bool StartMinimized { get; set; } = false;
    }
    
    public class ConfigManager
    {
        private readonly string _configPath;
        
        public ConfigManager()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _configPath = Path.Combine(appDir, "config.json");
        }
        
        public Config? LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    return CreateDefaultConfig();
                }
                
                string json = File.ReadAllText(_configPath);
                var jObject = JObject.Parse(json);
                
                Config config;
                
                // 支持原Python版本的嵌套seu格式
                if (jObject.ContainsKey("seu"))
                {
                    var seuObj = jObject["seu"];
                    config = new Config
                    {
                        Username = seuObj?["username"]?.ToString(),
                        Password = seuObj?["password"]?.ToString(),
                        LoginIp = seuObj?["login_ip"]?.ToString() ?? "http://202.119.25.2",
                        NotSignInTitle = seuObj?["not_sign_in_title"]?.ToString() ?? "上网登录页",
                        ResultReturn = seuObj?["result_return"]?.ToString() ?? "\"result\":\"1\"",
                        SignedInTitle = seuObj?["signed_in_title"]?.ToString() ?? "注销页",
                        WifiSsid = jObject["wifi_ssid"]?.ToString() ?? "SEU-WLAN",
                        CheckInterval = jObject["check_interval"]?.ToObject<int>() ?? 5,
                        ReconnectDelay = jObject["reconnect_delay"]?.ToObject<int>() ?? 3,
                        MaxRetry = jObject["max_retry"]?.ToObject<int>() ?? 3,
                        AutoStartService = jObject["auto_start_service"]?.ToObject<bool>() ?? false,
                        StartMinimized = jObject["start_minimized"]?.ToObject<bool>() ?? false
                    };
                }
                else
                {
                    config = JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                }
                
                return config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载配置失败: {ex.Message}");
                return null;
            }
        }
        
        public void SaveConfig(Config config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置失败: {ex.Message}");
            }
        }
        
        public Config? ImportConfig(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var jObject = JObject.Parse(json);
                
                Config config;
                
                // 支持原Python版本的嵌套seu格式
                if (jObject.ContainsKey("seu"))
                {
                    var seuObj = jObject["seu"];
                    config = new Config
                    {
                        Username = seuObj?["username"]?.ToString(),
                        Password = seuObj?["password"]?.ToString(),
                        LoginIp = seuObj?["login_ip"]?.ToString() ?? "http://202.119.25.2",
                        NotSignInTitle = seuObj?["not_sign_in_title"]?.ToString() ?? "上网登录页",
                        ResultReturn = seuObj?["result_return"]?.ToString() ?? "\"result\":\"1\"",
                        SignedInTitle = seuObj?["signed_in_title"]?.ToString() ?? "注销页",
                        WifiSsid = jObject["wifi_ssid"]?.ToString() ?? "SEU-WLAN",
                        CheckInterval = jObject["check_interval"]?.ToObject<int>() ?? 5,
                        ReconnectDelay = jObject["reconnect_delay"]?.ToObject<int>() ?? 3,
                        MaxRetry = jObject["max_retry"]?.ToObject<int>() ?? 3,
                        AutoStartService = jObject["auto_start_service"]?.ToObject<bool>() ?? false,
                        StartMinimized = jObject["start_minimized"]?.ToObject<bool>() ?? false
                    };
                }
                else
                {
                    config = JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                }
                
                // 保存到本地
                SaveConfig(config);
                
                return config;
            }
            catch (Exception ex)
            {
                throw new Exception($"导入配置失败: {ex.Message}");
            }
        }
        
        private Config CreateDefaultConfig()
        {
            var config = new Config();
            SaveConfig(config);
            return config;
        }
    }
}
