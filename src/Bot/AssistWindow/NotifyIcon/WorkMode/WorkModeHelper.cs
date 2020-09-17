using Bot.AssistWindow.NotifyIcon.MenuCreator;
using Bot.Common.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbEntity;

namespace Bot.AssistWindow.NotifyIcon.WorkMode
{
      public class WorkModeHelper
    {
        private static string _key;
        static WorkModeHelper()
        {
            WorkModeHelper._key = HybridKey.WorkMode.ToString();
        }

        public static WorkModeEnum GetWorkMode(string nick)
        {
            WorkModeEnum workMode = HybridHelper.GetValue<WorkModeEnum>(nick, WorkModeHelper._key, WorkModeEnum.Unknown);
            if (workMode == WorkModeEnum.Unknown)
            {
                workMode = WorkModeEnum.NoUse;
            }
            if (workMode == WorkModeEnum.Assist)
            {

            }
            return workMode;
        }

        public static void SetWorkMode(string nick, WorkModeEnum mode)
        {
            HybridHelper.Save<WorkModeEnum>(nick, WorkModeHelper._key, mode);
        }

        public static string GetDesc(WorkModeEnum mode)
        {
            string wmDesc;
            switch (mode)
            {
                case WorkModeEnum.Assist:
                    wmDesc = "聊天辅助";
                    break;
                case WorkModeEnum.NoUse:
                    wmDesc = "不使用";
                    break;
                default:
                    wmDesc = "Unknown";
                    break;
            }
            return wmDesc;
        }
    }
}
