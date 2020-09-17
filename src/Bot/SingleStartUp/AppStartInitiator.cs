using BotLib;
using BotLib.Extensions;
using Bot.ControllerNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Common.Db;

namespace Bot
{
    public class AppStartInitiator
    {
        public static void Init()
        {
            AppStartInitiator.WaitForInit();
            AppStartInitiator.ClearTmpPathFiles();
            DeskScanner.LoopScan();
            DbSyner.LoopSyn();
        }

        private static void ClearTmpPathFiles()
        {
            try
            {
                if (Directory.GetFiles(PathEx.TmpPath).Length > 0)
                {
                    DirectoryEx.DeleteC(PathEx.TmpPath, true);
                    Directory.CreateDirectory(PathEx.TmpPath);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private static void WaitForInit()
        {
            DateTime now = DateTime.Now;
            BatTime.WaitForInit(2000);
        }
    }
}
