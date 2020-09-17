using BotLib;
using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bot.Common
{
    public class CUtil
    {
        public static string GetProcessPath(string processName)
        {
            var qnPath = string.Empty;
            var ps = Process.GetProcessesByName(processName);
            if (ps.Length != 0)
            {
                qnPath = ps[0].MainModule.FileName;
                int idx = qnPath.LastIndexOf('\\');
                if (idx > 0)
                {
                    qnPath = qnPath.Substring(0, idx);
                }
                if (!string.IsNullOrEmpty(qnPath))
                {
                    Params.SetProcessPath(processName, qnPath);
                }
            }
            if (string.IsNullOrEmpty(qnPath))
            {
                qnPath = Params.GetProcessPath(processName);
            }
            return qnPath;
        }
    }

}
