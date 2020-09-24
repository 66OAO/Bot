using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Bot.Common;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
    public static class ShortcutHelper
    {
        public static void SetOrSendShortcutAsync(this ShortcutEntity shortcut, ChatDesk desk, bool isSend, bool focusEditor)
        {
            if (shortcut == null)
            {
                MsgBox.ShowErrTip("没有选中短语", null);
                return;
            }
            Task.Factory.StartNew(() =>
            {
                SetOrSendShortcut(shortcut, desk, isSend, focusEditor);
            });

        }

        private static void SetOrSendShortcut(this ShortcutEntity shortcut, ChatDesk desk, bool isSend, bool focusEditor)
        {
            if (shortcut == null) return;
            if (string.IsNullOrEmpty(shortcut.ImageName))
            {
                if (isSend)
                {
                    desk.Editor.SendPlainText(shortcut.Text);
                    desk.Editor.FocusEditor(true);
                }
                else
                {
                    desk.Editor.SetPlainText(shortcut.Text, true, focusEditor);
                }
            }
            else
            {
                ShortcutImageHelper.UseImage(shortcut.ImageName, image =>
                {
                    if (isSend)
                    {
                        desk.Editor.SendPlainTextAndImage(shortcut.Text, image);
                        desk.Editor.FocusEditor(true);
                    }
                    else
                    {
                        desk.Editor.SetPlainTextAndImage(shortcut.Text, image, focusEditor);
                    }
                });
            }
        }


        public static List<ShortcutEntity> GetShopShortcuts(string mainNick)
        {
            var ses = DbHelper.Fetch<ShortcutEntity>();
            if (!ses.xIsNullOrEmpty())
            {
                ses = ses.Where(k => k.DbAccount.Contains(mainNick)).ToList() ?? new List<EntityBase>();
            }
            return ses.Select(k => k as ShortcutEntity).ToList();
        }

    }
}
