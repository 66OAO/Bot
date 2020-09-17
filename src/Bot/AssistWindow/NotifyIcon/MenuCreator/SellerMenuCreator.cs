using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bot.Asset;
using Bot.AssistWindow.NotifyIcon.WorkMode;
using Bot.Common;
using BotLib;
using BotLib.Extensions;
using DbEntity;

namespace Bot.AssistWindow.NotifyIcon.MenuCreator
{
    public class SellerMenuCreator
    {
        private static Bitmap _imgAssist;
        private static Bitmap _imgNoUse;

        static SellerMenuCreator()
        {
            _imgAssist = AssetImageHelper.GetImageFromWinFormCache(AssetImageEnum.iconYellow);
            _imgNoUse = AssetImageHelper.GetImageFromWinFormCache(AssetImageEnum.iconGray);
        }

        public static void Create(CtlNotifyIcon ctlNotifyIcon, string nick)
        {
            var it = ctlNotifyIcon.CreateItem(nick, null, null, true, nick);
            ctlNotifyIcon.InsertItem(2, it);
            var workMode = WorkModeHelper.GetWorkMode(nick);
            CreateMenuItem(ctlNotifyIcon, it, nick, workMode);
        }

        private static void CreateMenuItem(CtlNotifyIcon ctlNotifyIcon, ToolStripMenuItem toolStripMenuItem, string nick, WorkModeEnum workMode)
        {
            var assistItem = ctlNotifyIcon.CreateItem("聊天辅助", ToolStripMenuItem_Click, _imgAssist, true, WorkModeEnum.Assist);
            toolStripMenuItem.DropDownItems.Add(assistItem);
            toolStripMenuItem.DropDownItems.Add(ctlNotifyIcon.CreateSeparator());
            var noUseItem = ctlNotifyIcon.CreateItem("不使用", ToolStripMenuItem_Click, _imgNoUse, true, WorkModeEnum.NoUse);
            toolStripMenuItem.DropDownItems.Add(noUseItem);
            if (workMode == WorkModeEnum.NoUse)
            {
                ToolStripMenuItem_Click(noUseItem, null);
            }
            else
            {
                ToolStripMenuItem_Click(assistItem, null);
            }
        }

        private static void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WorkModeMenuItemHelper.WorkModeMenuItem_Click(sender, e == null);
        }

        private class WorkModeMenuItemHelper
		{
			public static void WorkModeMenuItem_Click(object sender, bool bySoft)
			{
				var it = sender as ToolStripMenuItem;
				var parentIt = it.OwnerItem as ToolStripMenuItem;
				var mode = (WorkModeEnum)it.Tag;
				var nick = parentIt.Tag.ToString();
				if (bySoft)
				{
					WorkModeMenuItemHelper.SetWorkMode(it, parentIt, mode, nick);
				}
				else
				{
					var premode = WorkModeHelper.GetWorkMode(nick);
					Util.Assert(premode > WorkModeEnum.Unknown);
					if (premode != mode)
					{
						if (premode == WorkModeEnum.NoUse)
						{
                            WorkModeMenuItemHelper.SetWorkMode(it, parentIt, mode, nick);
						}
						else
						{
							WorkModeMenuItemHelper.CloseWorkMode(premode, nick);
							WorkModeMenuItemHelper.SetWorkMode(it, parentIt, mode, nick);
						}
					}
				}
			}

			private static void CloseWorkMode(WorkModeEnum workMode, string nick)
			{
				if (workMode == WorkModeEnum.Assist)
				{
					AssistModeHelper.Close(nick);
				}
			}

			private static void SetWorkMode(ToolStripMenuItem mi, ToolStripMenuItem parent, WorkModeEnum mode, string nick)
			{
				parent.Image = WorkModeMenuItemHelper.GetWorkModeImage(mode);
				WorkModeMenuItemHelper.SelectedMenuItem(parent, mi, mode);
				WorkModeHelper.SetWorkMode(nick, mode);
				switch (mode)
				{
				    case WorkModeEnum.Assist:
					    AssistModeHelper.Create(nick);
					    break;
				    case WorkModeEnum.NoUse:
					    break;
				    default:
					    Util.ThrowException("WorkMode为Unknow");
					    break;
				}
			}

			private static void SelectedMenuItem(ToolStripMenuItem parentMenuItem, ToolStripMenuItem selectedMenuItem, WorkModeEnum workMode)
			{
				for (int i = 0; i < parentMenuItem.DropDownItems.Count; i++)
				{
					var it = parentMenuItem.DropDownItems[i] as ToolStripMenuItem;
					if (it != null)
					{
						it.Checked = (it == selectedMenuItem);
					}
				}
			}

			private static Image GetWorkModeImage(WorkModeEnum workMode)
			{
				Image workModeImage;
				switch (workMode)
				{
				    case WorkModeEnum.Assist:
					    workModeImage = _imgAssist;
					    break;
				    case WorkModeEnum.NoUse:
					    workModeImage = _imgNoUse;
					    break;
				    default:
					    workModeImage = null;
					    break;
				}
				return workModeImage;
			}

		}

        public static void RemoveMenuItem(CtlNotifyIcon ctlNotifyIcon, string nick)
        {
            ctlNotifyIcon.RemoveItem(nick);
        }
    }
}
