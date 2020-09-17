using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation
{
    public class HwndInfo
    {
        public HwndInfo(int handle, string description)
        {
            this.Handle = handle;
            this.Description = description;
        }
        public string Description { get; set; }

        public int Handle { get; set; }
    }
}
