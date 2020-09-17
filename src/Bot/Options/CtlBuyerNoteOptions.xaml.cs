using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.Help;

namespace Bot.Options
{
	public partial class CtlBuyerNoteOptions : UserControl, IOptions
    {
        private string _seller;

        public CtlBuyerNoteOptions(string seller)
		{
			InitializeComponent();
            InitUI(seller);
		}

		public OptionEnum OptionType
		{
			get
			{
				return OptionEnum.BuyerNote;
			}
		}

		public void InitUI(string seller)
		{
			_seller = seller;
			if (Params.BuyerNote.GetSetIsPreferSelfNote(_seller))
			{
				rbtSelf.IsChecked = true;
				rbtNewest.IsChecked =false;
			}
			else
			{
				rbtSelf.IsChecked = false;
				rbtNewest.IsChecked = true;
			}
			cboxShowDetailAsTooltip.IsChecked = new bool?(Params.BuyerNote.GetIsShowDetailAsTooltip(_seller));
		}


		public void RestoreDefault()
		{
			Params.BuyerNote.SetIsPreferSelfNote(_seller, Params.BuyerNote.IsPreferSelfNoteDefault);
			Params.BuyerNote.SetIsShowDetailAsTooltip(_seller, Params.BuyerNote.IsShowDetailAsTooltipDefault);
			InitUI(_seller);
		}

		public void Save(string seller)
		{
			bool? isChecked = rbtSelf.IsChecked;
			Params.BuyerNote.SetIsPreferSelfNote(seller, isChecked.Value & isChecked != null);
			isChecked = cboxShowDetailAsTooltip.IsChecked;
			Params.BuyerNote.SetIsPreferSelfNote(seller, isChecked.Value & isChecked != null);
		}

		public HelpData GetHelpData()
		{
            return null;
		}

		public void NavHelp()
		{
		}
	}
}
