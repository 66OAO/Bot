using Bot.ChromeNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChromeDebugger
{
    public interface IChatRecordChrome
    {
        event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        event EventHandler<BuyerEventArgs> EvBuyerClosed;
        event EventHandler<RecieveNewMessageEventArgs> EvRecieveNewMessage;
        event EventHandler<ChromeAdapterEventArgs> EvChromeDetached;
        event EventHandler<ChromeAdapterEventArgs> EvChromeConnected;
        event EventHandler<HtmlChangedEventArgs> EvHtmlUpdated;
        string CachedHtml { get; }
        string PreBuyer { get; }
        string CurBuyer { get; }
        bool IsChromeOk { get; }
        void Dispose();
    }
}
