using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BotLib;
using BotLib.Extensions;
using Bot.Common.Account;
using Bot.AssistWindow.Widget.Bottom;

namespace Bot.Robot.Rule.QaCiteTableV2
{
	public class CiteTableManagerV2
	{
        private static ConcurrentDictionary<string, CiteTableV2> _dict;
        private static object _syn4CreateTable;

		static CiteTableManagerV2()
		{
			_dict = new ConcurrentDictionary<string, CiteTableV2>();
			_syn4CreateTable = new object();
		}

        public static List<CtlAnswer.Item4Show> GetInputSugestion(string input, string dbAccount, Dictionary<long, double> contextNumiid = null, int count = 5)
		{
            var sugs = new List<CtlAnswer.Item4Show>();
			try
			{
                sugs = GetCiteTable(dbAccount).GetInputSugestion(input, contextNumiid, count);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
            return sugs;
		}

        private static CiteTableV2 GetCiteTable(string dbAccount)
		{
			var citeTb = _dict.xTryGetValue(dbAccount, null);
			if (citeTb == null)
			{
                lock (_syn4CreateTable)
				{
					citeTb = _dict.xTryGetValue(dbAccount, null);
					if (citeTb == null)
					{
						citeTb = new CiteTableV2(dbAccount);
						_dict[dbAccount] = citeTb;
					}
				}
			}
			return citeTb;
		}

        public static void InitCiteTables(string dbAccount)
		{
            var citeTb = GetCiteTable(dbAccount);
			citeTb.LoadFromDb(false);
		}

        public static void ReInitCiteTables()
        {
            try
            {
                var dict = _dict;
                _dict = new ConcurrentDictionary<string, CiteTableV2>();
                foreach (string dbAccount in dict.Keys)
                {
                    var citeTb = GetCiteTable(dbAccount);
                    citeTb.LoadFromDb(true);
                }
                GC.Collect();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}
