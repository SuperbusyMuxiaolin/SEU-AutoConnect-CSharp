using System;
using System.IO;
using System.Text;

namespace SEU_AutoConnect
{
    public static class LogManager
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SEU-AutoConnect",
            "logs");

        public static string GetCurrentLogFilePath()
        {
            return Path.Combine(_logDir, $"app-{DateTime.Now:yyyyMMdd}.log");
        }

        public static void EnsureLogFileExists()
        {
            lock (_lock)
            {
                Directory.CreateDirectory(_logDir);
                var path = GetCurrentLogFilePath();
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, string.Empty, Encoding.UTF8);
                }
            }
        }

        public static void Write(string message, bool isError)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var prefix = isError ? "[错误]" : "[信息]";
                var line = $"[{timestamp}] {prefix} {message}{Environment.NewLine}";

                lock (_lock)
                {
                    Directory.CreateDirectory(_logDir);
                    File.AppendAllText(GetCurrentLogFilePath(), line, Encoding.UTF8);
                }
            }
            catch
            {
                // 忽略日志写入异常
            }
        }
    }
}