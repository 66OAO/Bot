using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.Help;
using Bot.Options.HotKey;
using Bot.Robot.Rule.QaCiteTableV2;

namespace Bot.Options
{
    public partial class CtlInputSugestionOptions : UserControl, IOptions
    {
        private Params.InputSuggestion.SourceTypeEnum _ori;
        private string _seller;
        private bool _oriIsUseDowKey;

        public CtlInputSugestionOptions(string seller)
        {
            _ori = Params.InputSuggestion.SourceType;
            _oriIsUseDowKey = Params.InputSuggestion.IsUseDownKey;
            InitializeComponent();
            InitUI(seller);
        }

        public OptionEnum OptionType
        {
            get
            {
                return OptionEnum.InputSuggestion;
            }
        }

        public HelpData GetHelpData()
        {
            throw new NotImplementedException();
        }

        public void InitUI(string seller)
        {
            _seller = seller;
            switch (Params.InputSuggestion.SourceType)
            {
                case Params.InputSuggestion.SourceTypeEnum.Shortcut:
                    rbtShortcut.IsChecked = true;
                    break;
                case Params.InputSuggestion.SourceTypeEnum.ScAndRuleAnswer:
                    rbtShortcutAndRuleAnswer.IsChecked = true;
                    break;
                case Params.InputSuggestion.SourceTypeEnum.All:
                    rbtAll.IsChecked = true;
                    break;
            }
            cboxUseDownKey.IsChecked = Params.InputSuggestion.IsUseDownKey;
        }

        public void NavHelp()
        {
            throw new NotImplementedException();
        }

        public void RestoreDefault()
        {
            Params.InputSuggestion.SourceType = Params.InputSuggestion.SourceTypeDefault;
            Params.InputSuggestion.IsUseDownKey = true;
            InitUI(_seller);
        }

        public void Save(string seller)
        {
            var sourceType = GetInputSuggestionSourceType();
            if (sourceType != _ori)
            {
                Params.InputSuggestion.SourceType = GetInputSuggestionSourceType();
                CiteTableManagerV2.ReInitCiteTables();
            }
            bool? isChecked = cboxUseDownKey.IsChecked;
            Params.InputSuggestion.IsUseDownKey = cboxUseDownKey.IsChecked.HasValue && cboxUseDownKey.IsChecked.Value;
            if (Params.InputSuggestion.IsUseDownKey != _oriIsUseDowKey)
            {
                HotKeyHelper.RegisterDownKey();
            }
        }

        private Params.InputSuggestion.SourceTypeEnum GetInputSuggestionSourceType()
        {
            var isChecked = rbtShortcut.IsChecked;
            Params.InputSuggestion.SourceTypeEnum sourceType;
            if (rbtShortcut.IsChecked.HasValue && rbtShortcut.IsChecked.Value)
            {
                sourceType = Params.InputSuggestion.SourceTypeEnum.Shortcut;
            }
            if (rbtShortcutAndRuleAnswer.IsChecked.HasValue && rbtShortcutAndRuleAnswer.IsChecked.Value)
            {
                sourceType = Params.InputSuggestion.SourceTypeEnum.ScAndRuleAnswer;
            }
            else
            {
                sourceType = Params.InputSuggestion.SourceTypeEnum.All;
            }

            return sourceType;
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
