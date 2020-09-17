using BotLib;
using BotLib.FileSerializer;
using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;

namespace Bot
{
    public class AppCloseEnder
    {
        public static void EndApp()
        {
            Params.IsAppClosing = true;
            SeriableObject.DealAppClose();
            NoReEnterTimer.IsAppClosed = true;
        }
    }
}
