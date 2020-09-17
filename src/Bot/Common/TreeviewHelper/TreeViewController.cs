using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bot.Common.TreeviewHelper
{
    public abstract class TreeViewController
    {
        public TreeDbAccessor DbAccessor;
        protected Type CatalogType
        {
            get;
            private set;
        }
        protected Type LeafType
        {
            get;
            private set;
        }
        public string DbAccount
        {
            get;
            private set;
        }
        public string Seller
        {
            get;
            private set;
        }
        public abstract void CreateCata(TreeNode n, Window wnd, Action<TreeNode> callback, object obj = null);
        public abstract void EditCata(TreeNode cp, Window wnd, Action<TreeNode> callback);
        public abstract void Create(TreeNode pn, Window wnd, Action<TreeNode> callback, object obj = null);
        public abstract void Edit(TreeNode pre, Window wnd, Action<TreeNode> callback);
        public abstract string ReadCataNodeName(TreeNode cn);
        public string ReadCataName(TreeNode n)
        {
            string result;
            if (n.EntityId == this.DbAccessor.Root.EntityId)
            {
                result = "根分组";
            }
            else
            {
                result = this.ReadCataNodeName(n);
            }
            return result;
        }
        public abstract string ReadNodeCode(TreeNode n);
        public abstract string ReadNodeTitle(TreeNode n);
        public abstract string ReadNodeImageName(TreeNode n);
        public abstract string ControllerName();
        public TreeViewController(Type catalogType, Type leafType, string dbAccount, string seller, TreeDbAccessor dbAccessor)
		{
			this.CatalogType = catalogType;
			this.LeafType = leafType;
			this.DbAccount = dbAccount;
			this.Seller = seller;
			this.DbAccessor = dbAccessor;
		}
    }
}
