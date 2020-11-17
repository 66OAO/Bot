using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
    public class QnShortcutImporter : Importer
    {        
        protected override void AssertFileFormatOk()
        {
            throw new NotImplementedException();
        }

        public static bool IsFileType(string txt)
        {
            return txt == "快捷短语编码,快捷短语,快捷短语分组";
        }

        public QnShortcutImporter(string fn, bool isAllReplace, string dbAccount)
            : base(fn, isAllReplace, dbAccount)
		{
			
		}

        protected override List<Node> ReadNodesFromFile(List<List<string>> dlist, out string importDbAccount)
        {
            importDbAccount = null;
            var nlist = new List<Node>();
            var ndict = new Dictionary<string, List<Node>>();
            var catas = new List<string>();
            for (int i = 1; i < dlist.Count; i++)
            {
                var cataName = (dlist[i].Count > 2) ? (dlist[i][2] ?? "") : "";
                cataName = cataName.Trim();
                Node item = new Node
                {
                    Code = dlist[i][0].Trim(),
                    Answer = dlist[i][1].Trim(),
                    IsCatalog = false,
                    Level = ((cataName == "") ? 1 : 2)
                };
                if (!ndict.ContainsKey(cataName))
                {
                    ndict[cataName] = new List<Node>();
                    if (cataName != "")
                    {
                        catas.Add(cataName);
                    }
                }
                ndict[cataName].Add(item);
            }
            foreach (string c in catas)
            {
                nlist.Add(new Node
                {
                    Answer = c,
                    IsCatalog = true,
                    Level = 1
                });
                nlist.AddRange(ndict[c]);
            }
            if (ndict.ContainsKey(""))
            {
                nlist.AddRange(ndict[""]);
            }
            return nlist;
        }
    }
}
