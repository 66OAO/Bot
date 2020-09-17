using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators
{
    public class LoginedSeller
    {
        public readonly int SellerHwnd;
        public readonly int EpHwnd;
        public readonly string Name;
        public bool IsEnterprise
        {
            get
            {
                return this.EpHwnd != 0;
            }
        }

        public LoginedSeller(string name, int sellerHwnd, int epHwnd)
		{
			this.SellerHwnd = 0;
			this.EpHwnd = 0;
			this.Name = name;
			this.SellerHwnd = sellerHwnd;
			this.EpHwnd = epHwnd;
		}

    }
}
