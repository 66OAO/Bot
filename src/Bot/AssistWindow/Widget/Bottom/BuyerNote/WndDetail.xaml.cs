using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.Common.Windows;
using BotLib.Extensions;
using System.Linq;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom.BuyerNote
{
	public partial class WndDetail : EtWindow
	{
		public WndDetail()
		{
			this.InitializeComponent();
		}

        private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
		}

		public static void MyShow(string buyerMain, string seller)
		{
			new WndDetail
			{
				tboxContent = 
				{
					Text = WndDetail.GetBuyerNotes(buyerMain, seller)
				}
			}.FirstShow(null, null, null, false);
		}

        public static string GetBuyerNotes(string buyerMain, string seller)
		{
            var ets = BuyerNoteHelper.GetBuyerNotesOnlyBuyer(buyerMain, seller);
			return WndDetail.GetDesc(ets);
		}

		private static string GetDesc(List<BuyerNoteEntity> ets)
		{
            ets = ets.OrderByDescending(k => k.ModifyTick).ToList();
			var builder = new StringBuilder();
			foreach (var note in ets.xSafeForEach())
			{
				builder.AppendLine(string.Format("记录者：{0},更新时间：{1}", note.Recorder, note.RecordTime.xToString()));
				builder.AppendLine("便签内容：" + note.Note);
				builder.AppendLine("-------------------------------------");
			}
			return builder.ToString();
		}
	}
}
