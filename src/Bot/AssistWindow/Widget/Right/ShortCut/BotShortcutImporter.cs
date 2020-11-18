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
    public class BotShortcutImporter : Importer
    {
        public BotShortcutImporter(string fn, bool isAllReplace, string dbAccount)
            : base(fn, isAllReplace, dbAccount)
		{
			
		}

        public static bool IsFileType(string txt)
        {
            return txt == "最初3行的内容请勿改动(V3)！！！";
        }

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
                isOk = (tmplist[0][0] == "最初3行的内容请勿改动(V3)！！！" && tmplist[2].xToCsvStringWithoutEscape() == "分组,快捷编码,标题,内容,图片名,程序数据（手工新增词条时，本字段留空即可）");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return isOk;
        }

        protected override List<Node> ReadNodesFromFile(List<List<string>> dlist, out string importDbAccount)
        {
            importDbAccount = dlist[1][0];
            var nlist = new List<Node>();
            var ndict = new Dictionary<string, int>();
            var catas = new List<string>();
            for (int i = 3; i < dlist.Count; i++)
            {
                Util.Assert(dlist[i].Count >= 3);
                while (dlist[i].Count < 5)
                {
                    dlist[i].Add("");
                }
                var cName = dlist[i][0].Trim();
                if (!string.IsNullOrEmpty(cName) && !ndict.ContainsKey(cName))
                {
                    this.FindDescendantNodes(cName, nlist, ndict, this.GetCataId(dlist[i][4]), catas);
                }
                int level = ndict.ContainsKey(cName) ? ndict[cName] : 1;
                nlist.Add(new Node
                {
                    Level = level,
                    IsCatalog = false,
                    Code = dlist[i][1],
                    Question = dlist[i][2],
                    Answer = dlist[i][3],
                    ImageName = ((dlist[i].Count < 5 || string.IsNullOrEmpty(dlist[i][4])) ? null : dlist[i][4]),
                    Id = ((dlist[i].Count > 5) ? this.GetEnityId(dlist[i][5]) : "")
                });
            }
            return nlist;
        }

        private string GetEnityId(string idValue)
        {
            var id = string.Empty;
            var vals = idValue.xSplitBySpace(StringSplitOptions.None);
            if (vals.Length != 0)
            {
                id = vals[0];
            }
            return id;
        }

        private string GetCataId(string idValue)
        {
            var id = string.Empty;
            var vals = idValue.xSplitBySpace(StringSplitOptions.None);
            if (vals.Length > 1)
            {
                id = vals[1];
            }
            return id;
        }

        private void FindDescendantNodes(string cName, List<Node> nlist, Dictionary<string, int> ndict, string cid, List<string> catas)
        {
            var splitCats = cName.Split(new string[]{"=>","＝>"}, StringSplitOptions.RemoveEmptyEntries);
            if (splitCats.Length > 0)
            {
                ndict[cName] = splitCats.Length + 1;
                int level = 0;
                foreach (var cat in splitCats)
                {
                    level++;
                    var lvCat = splitCats.Take(level).xToString("!@#$", true);
                    if (cat.Trim() != "" && !catas.Contains(lvCat))
                    {
                        var n = new Node
                        {
                            Level = level,
                            IsCatalog = true,
                            Code = "",
                            Answer = cat.Trim(),
                            ImageName = null,
                            Id = ((level == splitCats.Length) ? cid : "")
                        };
                        nlist.Add(n);
                        catas.Add(lvCat);
                    }
                }
            }
        }

        protected override void AssertFileFormatOk()
        {
            var nlist = CsvFileHelper.ReadCsvFile(this._filename, 3);
            Util.Assert(nlist.Count == 3 && BotShortcutImporter.IsFileFormatOk(nlist));
        }
    }
}
