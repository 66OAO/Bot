using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Help
{
    public class HelpData
    {
        public string MainHelpKey;
        public List<SubHelpItem> SubHelps;
    }

    public class SubHelpItem
    {
        public string Title;
        public string SubHelpKey;
    }
}
