using System;
using System.Threading.Tasks;
using BotLib;
using BotLib.Wpf.Extensions;
using Bot.Automation.ChatDeskNs;
using Bot.Common;
using System.Threading;
using Bot.Automation.ChatDeskNs.Automators;

namespace Bot.AssistWindow.NotifyIcon.WorkMode
{
    public class AssistModeHelper
    {
        public static void Create(string nick, int delayMs=1000)
        {
            Task.Factory.StartNew(() => {
                Start(nick,delayMs);
            }, TaskCreationOptions.LongRunning);
        }

        public static void Close(string nick)
        {
            var desk = ChatDesk.GetDeskFromCache(nick);
            if (desk == null)
            {
                Log.Error("AssistModeHelper.Close,desk不存在,nick=" + nick);
            }
            else
            {
                desk.Dispose();
            }
        }


        private static void Start(string nick, int delayMs)
        {
	        if (delayMs > 0)
	        {
		        Thread.Sleep(delayMs);
	        }
	        var loginedSeller = QnHelper.Detected.GetSellerFromCache(nick);
	        string arg;
	        var desk = ChatDesk.Create(loginedSeller, nick, out arg);
	        if (desk != null)
	        {
		        DispatcherEx.xInvoke(() =>
                {
                    WndAssist.CreateAndAttachToDesk(desk);
                });
                //写入登录数据
                
	        }
	        else
	        {
                MsgBox.ShowErrDialog(string.Format("无法为{0}创建辅助面板，原因是{1}", nick, arg));
	        }
        }
    }
}
