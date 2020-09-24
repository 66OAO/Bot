using BotLib;
using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
    public class ShortcutImporter
    {
        public static string GetDbAccount(string fileName)
        {
            string dbAccount = null;
            List<List<string>> list = CsvFileHelper.ReadCsvFile(fileName, 3);
            if (IsFileFormatOk(list))
            {
                dbAccount = list[1][0];
            }
            return dbAccount;
        }

        private static bool IsFileFormatOk(List<List<string>> tmplist)
        {
            bool isOk = false;
            try
            {
                isOk = (tmplist[0][0] == "最初3行的内容请勿改动！！！" && tmplist[2].xToCsvStringWithoutEscape() == "分组,快捷编码,内容,图片名,程序数据（手工新增词条时，本字段留空即可）");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return isOk;
        }
    }
}
