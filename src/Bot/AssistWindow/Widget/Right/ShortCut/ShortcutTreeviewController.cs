using Bot.Common.TreeviewHelper;
using Bot.Common.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BotLib.Extensions;
using Bot.Common;
using DbEntity;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
    public class ShortcutTreeviewController : TreeViewController
    {
        private string _catalogHeaderCache;
        private string _catalogHeaderCacheOriText;
        public ShortcutTreeviewController(string dbAccount, string seller)
            :base(typeof(ShortcutCatalogEntity), typeof(ShortcutEntity), dbAccount, seller, new TreeDbAccessor(typeof(ShortcutCatalogEntity), typeof(ShortcutEntity), dbAccount))
		{
			
		}
        public override void CreateCata(TreeNode p, Window wnd, Action<TreeNode> callback, object obj = null)
		{
            ShortcutCatalogEntity cata = null;
			this.ShowWndInput(p.EntityId, wnd, null, (text)=>{
                if(text.xIsNullOrEmptyOrSpace()) return;
                cata = EntityHelper.Create<ShortcutCatalogEntity>(this.DbAccount);
                cata.ParentId = p.EntityId;
                cata.Name = text;
                if (callback != null) callback(cata);
            });
		}
        private void ShowWndInput(string parentId, Window wnd, string old, Action<string> callback)
		{
			WndInput.MyShow("请输入“短语分组”的名称：", "新建：短语分组", callback, old, null, (text)=>{
                 if(text.xIsNullOrEmptyOrSpace()) return "必填";
                if(text.Equals(old)) return "分组名字已存在，请换一个名字。";
                if(DbAccessor.VerifyCatalogName(parentId,text)) return "分组名字已存在，请换一个名字。";
                return null;
            }, wnd, false);
		}
        public override void EditCata(TreeNode cp, Window wnd, Action<TreeNode> callback)
		{
            var cata = (cp as ShortcutCatalogEntity);
			this.ShowWndInput(cp.ParentId, wnd, cata.Name, (text)=>{
                if(text.xIsNullOrEmptyOrSpace()) return;
                cata.Name = text;
                if(callback !=null) callback(cata);
            });
		}
        public override void Create(TreeNode pn, Window wnd, Action<TreeNode> callback, object obj = null)
        {
            WndShortcutEditor.CreateNew(pn.EntityId, DbAccount, Seller, wnd, callback);
        }
        public override void Edit(TreeNode pre, Window wnd, Action<TreeNode> callback)
        {
            WndShortcutEditor.Edit(pre as ShortcutEntity, Seller, DbAccount, wnd, callback);
        }
        public override string ReadCataNodeName(TreeNode cn)
        {
            return (cn as ShortcutCatalogEntity).Name.Replace("\r\n", " ").Replace("\n", " ");
        }
        public override string ReadNodeTitle(TreeNode node)
        {
            string text = string.Empty;
            var se = node as ShortcutEntity;
            if (se != null)
            {
                if (string.IsNullOrEmpty(se.Title))
                {
                    text = se.Text;
                }
                else
                {
                    text = se.Title;
                }
            }
            return text.Replace("\r\n", " ").Replace("\n", " ");
        }
        public override string ReadNodeCode(TreeNode node)
        {
            var se = node as ShortcutEntity;
            string code = se.Code;
            return code.Replace("\r\n", " ").Replace("\n", " ").Trim();
        }
        public override string ControllerName()
        {
            return "话术";
        }
        public override string ReadNodeImageName(TreeNode node)
        {
            return (node as ShortcutEntity).ImageName;
        }
    }
}
