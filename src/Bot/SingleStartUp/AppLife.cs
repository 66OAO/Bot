using BotLib;
using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class AppLife
    {
        public static void OnStart()
        {
            AppLife.InitLogFile();
        }

        private static void InitLogFile()
        {
            Log.Initiate(PathEx.ParentOfExePath + "运行日志.txt", false, (int)Math.Pow(2.0, 23.0), true);
            Log.WriteEnvironmentString(Params.SystemInfo);
        }

    }
}
