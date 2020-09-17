using Bot.Automation.ChromeDebugger;
using Bot.Automation.ChromeDebugger;
using Bot.ChromeNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Component
{
    public class ChatDeskContact
    {
        private ChatRecordChromeV2 _chatRecordChrome;
        public event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        public ChatDeskContact(ChatRecordChromeV2 chatrecChrome)
        {
            this._chatRecordChrome = chatrecChrome;
            this._chatRecordChrome.EvBuyerSwitched += _chatRecordChrome_EvBuyerSwitched;
        }

        void _chatRecordChrome_EvBuyerSwitched(object sender, BuyerSwitchedEventArgs e)
        {
            if (EvBuyerSwitched != null) EvBuyerSwitched(sender, e);
        }
    }
}
