using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators
{
    public static class QnAccountFinderFactory
    {
        private static QnAccountFinder _finder;
        public static QnAccountFinder Finder
        {
            get
            {
                if (QnAccountFinderFactory._finder == null)
                {
                    QnAccountFinderFactory._finder = new QnAccountFinder();
                }
                return QnAccountFinderFactory._finder;
            }
        }
    }
}
