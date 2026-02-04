using System;
using System.Threading;
using System.Windows;

namespace SEU_AutoConnect
{
    public partial class App : Application
    {
        private static Mutex? _mutex;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            // 单实例检查
            _mutex = new Mutex(true, "SEU-AutoConnect-Mutex-CSharp", out bool createdNew);
            
            if (!createdNew)
            {
                MessageBox.Show("程序已在运行中！", "SEU-AutoConnect", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }
            
            base.OnStartup(e);
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
