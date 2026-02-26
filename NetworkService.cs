using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ManagedNativeWifi;

namespace SEU_AutoConnect
{
    public class NetworkService
    {
        private readonly HttpClient _httpClient;
        private static readonly Regex _metaCharsetRegex = new Regex("charset=([\\w-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public NetworkService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }
        
        /// <summary>
        /// 获取当前连接的WiFi名称
        /// </summary>
        public string? GetCurrentWifi()
        {
            try
            {
                var connectedProfile = NativeWifi.EnumerateConnectedNetworkSsids()
                    .FirstOrDefault();
                return connectedProfile?.ToString();
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 连接到指定WiFi
        /// </summary>
        public bool ConnectToWifi(string ssid, Action<string, bool> log)
        {
            try
            {
                // 检查是否已连接
                var currentSsid = GetCurrentWifi();
                if (currentSsid == ssid)
                {
                    log($"已连接到 {ssid}", false);
                    return true;
                }
                
                // 扫描可用网络
                var availableNetwork = NativeWifi.EnumerateAvailableNetworkSsids()
                    .FirstOrDefault(n => n.ToString() == ssid);
                
                if (availableNetwork == null)
                {
                    log($"未找到WiFi: {ssid}", true);
                    return false;
                }
                
                log($"正在连接到 {ssid}...", false);
                
                // 尝试使用NativeWifi连接（需要已保存的配置文件）
                var profiles = NativeWifi.EnumerateProfiles()
                    .Where(p => p.Name == ssid);
                
                foreach (var profile in profiles)
                {
                    bool connected = NativeWifi.ConnectNetwork(profile.Interface.Id, profile.Name, BssType.Infrastructure);
                    if (connected)
                    {
                        System.Threading.Thread.Sleep(3000); // 等待连接建立
                        if (GetCurrentWifi() == ssid)
                        {
                            log($"成功连接到 {ssid}", false);
                            return true;
                        }
                    }
                }
                
                // 如果NativeWifi失败，尝试使用netsh命令
                return ConnectToWifiUsingNetsh(ssid, log);
            }
            catch (Exception ex)
            {
                log($"连接WiFi异常: {ex.Message}", true);
                return false;
            }
        }
        
        private bool ConnectToWifiUsingNetsh(string ssid, Action<string, bool> log)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"wlan connect name=\"{ssid}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding("GBK")
                    }
                };
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                if (output.Contains("已成功完成") || output.ToLower().Contains("successfully"))
                {
                    System.Threading.Thread.Sleep(3000);
                    if (GetCurrentWifi() == ssid)
                    {
                        log($"成功连接到 {ssid}", false);
                        return true;
                    }
                }
                
                log($"连接 {ssid} 失败", true);
                return false;
            }
            catch (Exception ex)
            {
                log($"netsh连接失败: {ex.Message}", true);
                return false;
            }
        }
        
        /// <summary>
        /// 检查互联网连接
        /// </summary>
        public bool CheckInternetConnection()
        {
            string[] testHosts = { "8.8.8.8", "114.114.114.114", "223.5.5.5" };
            
            foreach (var host in testHosts)
            {
                try
                {
                    using var client = new TcpClient();
                    var result = client.BeginConnect(host, 53, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    
                    if (success)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        public string? GetLocalIpAddress()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint?.Address.ToString();
            }
            catch
            {
                return null;
            }
        }
        
        private static async Task<string> ReadContentAsStringSafe(HttpResponseMessage response, params string?[] keywords)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();

            try
            {
                // 使用GBK编码（GB2312的超集，兼容性更好）
                return Encoding.GetEncoding("GBK").GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 进行校园网认证
        /// </summary>
        public async Task<(bool success, string message)> AuthenticateCampusNetwork(Config config)
        {
            try
            {
                var localIp = GetLocalIpAddress();
                if (string.IsNullOrEmpty(localIp))
                {
                    return (false, "无法获取本机IP地址");
                }
                
                // 检查登录状态
                var response = await _httpClient.GetAsync(config.LoginIp);
                var content = await ReadContentAsStringSafe(response, config.SignedInTitle, config.NotSignInTitle);
                
                // 已登录
                if (content.Contains(config.SignedInTitle))
                {
                    return (true, $"已登录 (IP: {localIp})");
                }
                
                // 需要登录
                if (content.Contains(config.NotSignInTitle))
                {
                    var template = string.IsNullOrWhiteSpace(config.LoginRequestUrlTemplate)
                        ? "https://w.seu.edu.cn:801/eportal/?c=Portal&a=login&callback=dr1003&login_method=1&user_account=%2C0%2C{username}&user_password={password}&wlan_user_ip={local_ip}&wlan_user_ipv6=&wlan_user_mac=000000000000&wlan_ac_ip=&wlan_ac_name=SPL_NetEngine8000F8&jsVersion=3.3.2&v=3080"
                        : config.LoginRequestUrlTemplate;

                    string loginUrl = template
                        .Replace("{username}", config.Username ?? string.Empty)
                        .Replace("{password}", config.Password ?? string.Empty)
                        .Replace("{local_ip}", localIp);
                    
                    var loginResponse = await _httpClient.GetAsync(loginUrl);
                    var loginContent = await ReadContentAsStringSafe(loginResponse, config.ResultReturn, config.SignedInTitle, config.NotSignInTitle);
                    
                    if (loginContent.Contains(config.ResultReturn))
                    {
                        return (true, $"登录成功 (IP: {localIp})");
                    }
                    else
                    {
                        return (false, $"登录失败 (IP: {localIp})");
                    }
                }
                
                return (false, $"未连接校园网 (IP: {localIp})");
            }
            catch (Exception ex)
            {
                return (false, $"认证异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 连接WiFi并进行认证的完整流程
        /// </summary>
        public async void ConnectAndAuthenticate(Config config, Action<string, bool> log)
        {
            try
            {
                // 检查互联网连接
                if (CheckInternetConnection())
                {
                    log("网络正常", false);
                    return;
                }
                
                log("检测到网络断开，尝试重新连接...", false);
                
                // 连接WiFi
                if (!string.IsNullOrEmpty(config.WifiSsid))
                {
                    var currentWifi = GetCurrentWifi();
                    if (currentWifi != config.WifiSsid)
                    {
                        if (!ConnectToWifi(config.WifiSsid, log))
                        {
                            log("WiFi连接失败", true);
                            return;
                        }
                    }
                }
                
                // 等待网络稳定
                await Task.Delay(TimeSpan.FromSeconds(config.ReconnectDelay));
                
                // 进行校园网认证
                if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    var (success, message) = await AuthenticateCampusNetwork(config);
                    log(message, !success);
                    
                    if (success)
                    {
                        // 验证互联网连接
                        await Task.Delay(2000);
                        if (CheckInternetConnection())
                        {
                            log("网络连接恢复正常", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log($"连接流程异常: {ex.Message}", true);
            }
        }
    }
}
