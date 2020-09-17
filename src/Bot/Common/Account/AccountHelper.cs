using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;
using Bot.Common.Db;
using DbEntity;

namespace Bot.Common.Account
{
    public class AccountHelper
    {
        public static bool IsPubAccountEqual(string dbAccount, string seller)
        {
            return AccountHelper.GetPubDbAccount(seller) == dbAccount;
        }

        public static string GetMainPart(string seller)
        {
            return TbNickHelper.GetMainPart(seller);
        }

        public static string GetPubDbAccount(string seller)
        {
            return TbNickHelper.ConvertNickToPubDbAccount(seller);
        }

        public static string GetShopDbAccount(string seller)
        {
            return TbNickHelper.ConvertNickToShopDbAccount(seller);
        }

        public static string GetWwMainNick(string seller)
        {
            string pubDba = AccountHelper.GetPubDbAccount(seller);
            return TbNickHelper.GetWwMainNickFromPubDbAccount(pubDba);
        }

        public static string GetPrvDbAccount(string seller)
        {
            return HybridHelper.GetValue<string>(seller, HybridKey.PrvDbAccount.ToString(), TbNickHelper.ConvertNickToPrvDbAccount(seller));
        }

        public static void GetPubDbAccount(string mainNick, string pubDbAccount)
        {
            //TbNickHelper.AssertMainNick(mainNick);
            //TbNickHelper.AssertPubDbAccount(pubDbAccount);
            HybridHelper.GetValue<string>(mainNick, HybridKey.PubDbAccount.ToString(), pubDbAccount);
        }

        public static HashSet<string> GetDbAccounts()
        {
            return AccountHelper.ConvertNicksToDbAccount(QnHelper.Detected.GetNicks());
        }

        public static HashSet<string> ConvertNicksToDbAccount(string[] nicks)
        {
            var set = new HashSet<string>();
            foreach (var nick in nicks.xSafeForEach())
            {
                set.Add(AccountHelper.GetPubDbAccount(nick));
                set.Add(AccountHelper.GetPrvDbAccount(nick));
                set.Add(nick);
                set.Add(TbNickHelper.GetMainPart(nick));
                set.Add(AccountHelper.GetShopDbAccount(nick));
            }
            return set;
        }
    }

}
