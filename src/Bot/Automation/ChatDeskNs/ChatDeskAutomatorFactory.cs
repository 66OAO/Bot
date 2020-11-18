using Bot.Common;
using Bot.Automation.ChatDeskNs.Automators;
using Bot.Automation.ChatDeskNs.Automators.DeskAutomators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs
{
    public class ChatDeskAutomatorFactory
    {
        public static DeskAutomator Create(HwndInfo hwndInfo, string seller)
        {
            DeskAutomator automator;
            if (QnHelper.IsGreaterV7_30_67N())
            {
                automator = new DeskAutomatorV7_30_67N(hwndInfo, seller);
            }
            else if (QnHelper.IsGreaterV7_21_00N())
            {
                automator = new DeskAutomatorV7_21_00N(hwndInfo, seller);
            }
            else if (QnHelper.IsGreaterV7_20_00N())
            {
                automator = new DeskAutomatorV7_20_00N(hwndInfo, seller);
            }
            else if (QnHelper.IsGreaterV6_07_00N())
            {
                automator = new DeskAutomatorV6_07_00N(hwndInfo, seller);
            }
            else if (QnHelper.IsGreaterV6_02_00N())
            {
                automator = new DeskAutomatorV6_02_00N(hwndInfo, seller);
            }
            else
            {
                automator = new DeskAutomator(hwndInfo, seller);
            }
            return automator;
        }
    }
}
