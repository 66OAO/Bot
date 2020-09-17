using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Automation.ChatDeskNs
{
    public class BuyerChangedEventArgs : EventArgs
    {
        public string NewBuyer
        {
            get;
            private set;
        }
        public string OldBuyer
        {
            get;
            private set;
        }
        public BuyerChangedEventArgs(string newBuyer, string oldBuyer)
		{
			this.NewBuyer = newBuyer;
			this.OldBuyer = oldBuyer;
		}
    }
}
