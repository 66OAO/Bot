using BotLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Robot.Rule.Importer.Legacy
{
    public class ForAllApp
    {
        public static string Convert2B64String(string text)
        {
            var rt = "";
            try
            {
                byte[] bytes = Encoding.GetEncoding("gb2312").GetBytes(text);
                rt = Convert.ToBase64String(bytes);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }
        public static string ConvertFromB64String(string b64)
        {
            var rt = "";
            try
            {
                byte[] bytes = Convert.FromBase64String(b64);
                rt = Encoding.GetEncoding("gb2312").GetString(bytes);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }
        public static int ConvertStringToInt(string s)
        {
            int rt = 0;
            try
            {
                s = s.Trim();
                rt = Convert.ToInt32(s);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }
        public static DateTime ConvertStringToDateTime(string s)
        {
            var dt = DateTime.MinValue;
            try
            {
                s = s.Trim();
                dt = Convert.ToDateTime(s);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return dt;
        }
        public static string ToLocString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
