using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;
using BotLib;
using Bot.Common.Account;
using Bot.Robot.Rule.QaCiteTableV2;
using Bot.AssistWindow.Widget.Bottom;

namespace Bot.Automation.ChatDeskNs.InputSuggestion
{
    public class DeskSuggestion
    {
        private ChatDesk _desk;
        private bool _isOnEditorTextChangedBusy;
        private object _synobj;


        public DeskSuggestion(ChatDesk desk)
        {
            _desk = desk;
            _synobj = new object();
            _desk.Editor.EvEditorTextChanged += Editor_EvEditorTextChanged;
        }

        private void Editor_EvEditorTextChanged(object sender, ChatDeskEventArgs e)
        {
            Task.Factory.StartNew(OnEditorTextChangedAsync, TaskCreationOptions.LongRunning);
        }

        private void OnEditorTextChangedAsync()
        {
            if (_desk.Editor.LatestSetTextTime.xElapse().TotalMilliseconds >= 200.0)
            {
                lock (_synobj)
                {
                    if (_isOnEditorTextChangedBusy)
                    {
                        return;
                    }
                    _isOnEditorTextChangedBusy = true;
                }
                try
                {
                    OnEditorTextChanged();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
                lock (_synobj)
                {
                    _isOnEditorTextChangedBusy = false;
                }
            }
        }

        private void OnEditorTextChanged()
        {
            var text = _desk.Editor.GetPlainCachedText(false);
            if (!string.IsNullOrWhiteSpace(text) && IsEditorTextChanged(text))
            {
                if (!text.xIsNullOrEmptyOrSpace())
                {
                    text = text.Trim();
                    if (!_desk.AssistWindow.ctlBottomPanel.ctlAnswer.IsShowListItem(text))
                    {
                        _desk.AssistWindow.ctlBottomPanel.ctlAnswer.ShowListItem(GetInputSugestion(text), text);
                    }
                }
            }
        }

        private bool IsEditorTextChanged(string txt)
        {
            return IsEditorTextChanged(txt, _desk.Editor.LastSetPlainText);
        }


        private bool IsEditorTextChanged(string txt, string lastPlainText)
        {
            if (string.IsNullOrEmpty(lastPlainText)) return true;
            lastPlainText = lastPlainText.Trim();
            txt = txt.Trim();
            return lastPlainText != txt;
        }

        private List<CtlAnswer.Item4Show> GetInputSugestion(string input)
        {
            string dbAccount = AccountHelper.GetPubDbAccount(_desk.Seller);
            var promptlist = CiteTableManagerV2.GetInputSugestion(input, dbAccount, null, Params.BottomPannelAnswerCount);
            return promptlist;
        }
    }
}
