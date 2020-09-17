using BotLib;
using Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbEntity;

namespace Bot.Robot.Rule.Importer.Legacy
{
    public class FullMatchRule
	{
		public List<string> Questions;
		public List<string> Answers;
		public FullMatchRule(List<string> qs, List<string> ans)
		{
			this.Questions = qs.Select(x=> UniformedString.Convert(x).Text).ToList<string>();
			this.Answers = ans;
		}
		public static List<string> SplitQuestion(string question)
		{
			var qs = new List<string>();
			try
			{
				var lines = question.Split(new string[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
				if (lines != null && lines.Length > 0)
				{
					qs.AddRange(lines);
				}
			}
			catch (Exception e)
			{
                Log.Exception(e);
			}
			return qs;
		}
	}
}
