using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;

namespace BotLib.Misc
{
    public class ArabNumberConverter
    {
        public static string GetNumberString(string txt, int startIndex = 0, bool toBanJiao = false)
        {
            string result;
            if (string.IsNullOrEmpty(txt) || startIndex < 0 || startIndex > txt.Length) return null;
            if (toBanJiao)
            {
                txt = txt.xToBanJiao();
            }
            bool flag2 = false;
            int i = startIndex;
            if (txt[i] == '+' || txt[i] == '-')
            {
                i++;
            }
            while (i < txt.Length)
            {
                char c = txt[i];
                if (! ((c >= '0' && c <= '9') || c == ','))
                {
                    if (c != '.')
                    {
                        break;
                    }
                    if (flag2)
                    {
                        break;
                    }
                    flag2 = true;
                }
                i++;
            }
            string text = txt.Substring(startIndex, i - startIndex);
            if (string.IsNullOrEmpty(text) || text[0] == ',')
            {
                result = null;
            }
            else
            {
                while (text.Length > 0)
                {
                    if (!text.EndsWith(",") && !text.EndsWith("."))
                    {
                        break;
                    }
                    text = text.Substring(0, text.Length - 1);
                }
                result = text;
            }

            return result;
        }
    }
}
