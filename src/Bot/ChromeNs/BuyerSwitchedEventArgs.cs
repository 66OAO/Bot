using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.ChromeNs
{
    public class BuyerSwitchedEventArgs : ChromeAdapterEventArgs
    {
        public string PreBuyer;
        public string CurBuyer;
        public bool FromTaskWindow;
    }
}
