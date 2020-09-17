using Bot.ChromeNs;
using Bot.Automation.ChatDeskNs;
using Bot.Automation.ChromeDebugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChromeDebugger
{
    public class ChromeDebugerCreator
    {
        public static IChatRecordChrome Create(ChatDesk desk)
        {
            IChatRecordChrome chrome = null;
            if (ChromeOperator.Connectable(desk.Hwnd.Handle))
            {
                chrome = new ChatRecordChrome(desk);
            }
            else
            {
                chrome = new ChatRecordChromeV2(desk);
            }
            return chrome;
        }
    }
}
