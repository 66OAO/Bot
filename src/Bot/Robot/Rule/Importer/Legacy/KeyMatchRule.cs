using Bot.Common;
using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Robot.Rule.Importer.Legacy
{
    public class KeyMatchRule
    {
        public List<List<List<string>>> Keys;
        public List<string> Answers;
        public KeyMatchRule(List<List<List<string>>> qs, List<string> ans)
		{
			this.Answers = ans;
			if (qs == null)
			{
				qs = new List<List<List<string>>>();
			}
			for (int i = 0; i < qs.Count; i++)
			{
				for (int j = 0; j < qs[i].Count; j++)
				{
					for (int k = 0; k < qs[i][j].Count; k++)
					{
						qs[i][j][k] = UniformedString.Convert(qs[i][j][k]).Text;
						if (qs[i][j][k] == "")
						{
							qs[i][j][k] = "%$^&$SDFG^RV我BS";
						}
					}
				}
			}
			this.Keys = qs;
		}
    }
}
