using System.Diagnostics;
using System.Threading;

namespace ECView
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        // ReSharper disable once NotAccessedField.Local
        private Mutex _mut;
        public App()
        {
            //禁用重复开启
            /*bool createNew = false;
            string targetExeName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string productName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().GetName().Name);

            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, productName, out createNew))
            {
                if (createNew)
                {
                    StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
                    Run();
                }
                else
                {
                    //PTMCWin32API.SendMessage(targetExeName, "Protocol Testing Management Console", "/v:true");
                    Environment.Exit(1);
                }
            }*/
            const bool requestInitialOwnership = true;
            bool mutexWasCreated;
            _mut = new Mutex(requestInitialOwnership, "com.ECView.Ding", out mutexWasCreated);
            if (mutexWasCreated) return;
            // 随意什么操作啦~
            //Current.Shutdown();
            //当前运行WPF程序的进程实例
            var process = Process.GetCurrentProcess();
            process.Kill();
        }
    }
}
