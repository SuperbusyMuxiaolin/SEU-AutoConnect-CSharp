using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SEU_AutoConnect
{
    public partial class MainWindow : Window
    {
        private readonly ConfigManager _configManager;
        private readonly NetworkService _networkService;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;
        
        public MainWindow()
        {
            InitializeComponent();
            
            _configManager = new ConfigManager();
            _networkService = new NetworkService();
            
            // 加载配置
            LoadConfiguration();
            
            // 检查自启动状态
            UpdateAutoStartMenu();
            
            // 添加日志
            LogMessage("程序已启动，请配置后点击\"启动服务\"");
        }
        
        private void LoadConfiguration()
        {
            var config = _configManager.LoadConfig();
            if (config != null)
            {
                UsernameTextBox.Text = config.Username ?? "";
                PasswordBox.Password = config.Password ?? "";
                WifiSsidTextBox.Text = config.WifiSsid;
                CheckIntervalTextBox.Text = config.CheckInterval.ToString();
                LogMessage("已加载配置文件");
            }
            else
            {
                // 使用默认值
                WifiSsidTextBox.Text = "SEU-WLAN";
                CheckIntervalTextBox.Text = "5";
                LogMessage("使用默认配置");
            }
        }
        
        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = new Config
                {
                    Username = UsernameTextBox.Text.Trim(),
                    Password = PasswordBox.Password,
                    WifiSsid = WifiSsidTextBox.Text.Trim(),
                    CheckInterval = int.TryParse(CheckIntervalTextBox.Text, out int interval) ? interval : 5
                };
                
                if (string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
                {
                    MessageBox.Show("请填写用户名和密码！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                _configManager.SaveConfig(config);
                LogMessage("配置已保存");
                MessageBox.Show("配置保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"保存配置失败: {ex.Message}", true);
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "选择配置文件",
                    Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                };
                
                if (dialog.ShowDialog() == true)
                {
                    var config = _configManager.ImportConfig(dialog.FileName);
                    if (config != null)
                    {
                        UsernameTextBox.Text = config.Username ?? "";
                        PasswordBox.Password = config.Password ?? "";
                        WifiSsidTextBox.Text = config.WifiSsid;
                        CheckIntervalTextBox.Text = config.CheckInterval.ToString();
                        LogMessage($"已导入配置: {Path.GetFileName(dialog.FileName)}");
                        MessageBox.Show("配置导入成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"导入配置失败: {ex.Message}", true);
                MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void StartService_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;
            
            // 验证配置
            if (string.IsNullOrEmpty(UsernameTextBox.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("请先填写用户名和密码！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var config = new Config
            {
                Username = UsernameTextBox.Text.Trim(),
                Password = PasswordBox.Password,
                WifiSsid = WifiSsidTextBox.Text.Trim(),
                CheckInterval = int.TryParse(CheckIntervalTextBox.Text, out int interval) ? interval : 5
            };
            
            _isRunning = true;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StatusText.Text = "运行中";
            StatusText.Foreground = System.Windows.Media.Brushes.Green;
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            LogMessage("服务已启动，开始监控网络状态...");
            TrayIcon.ShowBalloonTip("SEU-AutoConnect", "服务已启动", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            
            await Task.Run(() => RunServiceLoop(config, _cancellationTokenSource.Token));
        }
        
        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning) return;
            
            _cancellationTokenSource?.Cancel();
            _isRunning = false;
            
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusText.Text = "已停止";
            StatusText.Foreground = System.Windows.Media.Brushes.Gray;
            
            LogMessage("服务已停止");
            TrayIcon.ShowBalloonTip("SEU-AutoConnect", "服务已停止", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
        
        private async void ConnectNow_Click(object sender, RoutedEventArgs e)
        {
            var config = _configManager.LoadConfig();
            if (config == null || string.IsNullOrEmpty(config.Username))
            {
                LogMessage("请先配置用户名和密码", true);
                return;
            }
            
            LogMessage("手动触发连接...");
            await Task.Run(() => _networkService.ConnectAndAuthenticate(config, LogMessage));
        }
        
        private async void RunServiceLoop(Config config, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 更新当前WiFi状态
                    Dispatcher.Invoke(() =>
                    {
                        var currentWifi = _networkService.GetCurrentWifi();
                        CurrentWifiText.Text = string.IsNullOrEmpty(currentWifi) ? "未连接" : currentWifi;
                    });
                    
                    // 检查网络并尝试连接
                    await Task.Run(() => _networkService.ConnectAndAuthenticate(config, LogMessage));
                    
                    // 等待下一次检查
                    await Task.Delay(TimeSpan.FromSeconds(config.CheckInterval), token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogMessage($"服务循环异常: {ex.Message}", true);
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
        }
        
        private void LogMessage(string message, bool isError = false)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string prefix = isError ? "[错误]" : "[信息]";
                LogTextBox.AppendText($"[{timestamp}] {prefix} {message}\r\n");
                LogTextBox.ScrollToEnd();
            });
        }
        
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }
        
        private void AutoStart_Changed(object sender, RoutedEventArgs e)
        {
            bool enabled = AutoStartMenuItem.IsChecked;
            AutoStartManager.SetAutoStart(enabled);
            LogMessage(enabled ? "已启用开机自启动" : "已禁用开机自启动");
        }
        
        private void UpdateAutoStartMenu()
        {
            AutoStartMenuItem.IsChecked = AutoStartManager.IsAutoStartEnabled();
        }
        
        private void ShowWindow_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            TrayIcon.Dispose();
            Application.Current.Shutdown();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 最小化到托盘而不是关闭
            e.Cancel = true;
            Hide();
            TrayIcon.ShowBalloonTip("SEU-AutoConnect", "程序已最小化到系统托盘", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
        
        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
    }
}
