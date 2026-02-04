using System;
using Microsoft.Win32;

namespace SEU_AutoConnect
{
    public static class AutoStartManager
    {
        private const string AppName = "SEU-AutoConnect";
        private const string RegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        
        /// <summary>
        /// 检查是否已启用开机自启动
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
                var value = key?.GetValue(AppName);
                return value != null;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 设置开机自启动
        /// </summary>
        public static bool SetAutoStart(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
                
                if (key == null)
                    return false;
                
                if (enable)
                {
                    string? exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (string.IsNullOrEmpty(exePath))
                        return false;
                    
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置自启动失败: {ex.Message}");
                return false;
            }
        }
    }
}
