using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Bot.AssistWindow.NotifyIcon;
using BotLib;
using BotLib.Extensions;

namespace Bot.SingleStartUp
{
	public class StartUp
	{
		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
                bool createdNew;
                using (new Mutex(true, "niuniuservice", out createdNew))
				{
                    if (createdNew)
					{
                        KillProcess();
						AppLife.OnStart();
						App app = new App();
						app.InitializeComponent();
						app.Run(WndNotifyIcon.Inst);
					}
					else
					{
                        MessageBox.Show("千牛客服已经运行了！", "千牛客服");
						Application app = Application.Current;
						if (app != null)
						{
							app.Shutdown();
						}
					}
				}
			}
			catch (Exception ex)
			{
                MessageBox.Show("OnStartup:" + ex.Message, "千牛客服");
				Log.Exception(ex);
			}
		}

        private static void KillProcess()
		{
            var processes = Process.GetProcessesByName("Bot");
			var curProcess = Process.GetCurrentProcess();
			foreach (var p in processes.xSafeForEach())
			{
                if (p.Id != curProcess.Id)
				{
					p.Kill();
				}
			}
		}

	}
}
