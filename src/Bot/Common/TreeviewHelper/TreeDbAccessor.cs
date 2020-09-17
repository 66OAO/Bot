using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib;
using BotLib.Extensions;
using Bot.AssistWindow.Widget.Right;
using DbEntity;

namespace Bot.Common.TreeviewHelper
{
    public class TreeDbAccessor
    {
        private Type _catalogType;

        private Type _leafType;

        private string _dbAccount;

        public string DbAccount
        {
            get { return _dbAccount; }
            private set { _dbAccount = value; }
        }

        private TreeNode _root;

        private const string RootCatalogHeader = "Root!@#$Catalog$%&Header*&()";

        public TreeDbAccessor(Type catalogType, Type leafType, string dbAccount)
        {
            this._catalogType = catalogType;
            this._leafType = leafType;
            this._dbAccount = dbAccount;
        }

        public TreeNode Root
        {
            get
            {
                if (this._root == null)
                {
                    this._root = this.ReadRootFromDb();
                    if (this._root == null)
                    {
                        this._root = this.DowRootFromServerAndSave();
                    }
                    Util.Assert(this._root != null);
                }
                return this._root;
            }
        }

        public TreeCatalog CreateRootAndSaveToDb(string entityId)
        {
            TreeCatalog TreeCatalog = EntityHelper.Create(this._catalogType, this._dbAccount) as TreeCatalog;
            TreeCatalog.Name = "Root!@#$Catalog$%&Header*&()";
            TreeCatalog.EntityId = entityId;
            this.SaveNodeToDb(TreeCatalog);
            return TreeCatalog;
        }

        private TreeNode DowRootFromServerAndSave()
        {
            TreeNode TreeNode = null;
            string entityId = StringEx.xGenGuidB32Str();
            if (!string.IsNullOrEmpty(entityId))
            {
                TreeNode = this.CreateRootAndSaveToDb(entityId);
            }
            Util.Assert(TreeNode != null);
            return TreeNode;
        }

        private List<TreeNode> TakeNodesThatParentNotInList(List<TreeNode> nodes)
        {
            var idset = new HashSet<string>(nodes.Select(k => k.EntityId));
            return nodes.xRemove(k => idset.Contains(k.ParentId));
        }

        public void AddNodeToTargetNode(TreeNode n, string targetNodeId)
        {
            n.ParentId = targetNodeId;
            n.NextId = null;
            TreeNode lastNode = this.ReadLastChildNode(targetNodeId);
            if (lastNode != null)
            {
                n.PrevId = lastNode.EntityId;
                lastNode.NextId = n.EntityId;
            }
            else
            {
                n.PrevId = null;
            }
            DbHelper.BatchSaveOrUpdateToDb(new EntityBase[]
	        {
		        n,
		        lastNode
	        });
        }

        private TreeNode ReadLastChildNode(string entityId)
        {
            List<TreeNode> list = this.ReadDescendantList(entityId, true);
            TreeNode result;
            if (list != null && list.Count > 0)
            {
                result = list.Last();
            }
            else
            {
                result = null;
            }
            return result;
        }

        private List<TreeNode> FixAllNodeInTheTree(out HashSet<string> nodeIds)
        {
            var invalidNodes = new List<TreeNode>();
            nodeIds = new HashSet<string>();
            var nodes = this.ReadAllNodeFromDb();
            if (!nodes.xIsNullOrEmpty())
            {
                nodeIds = (this.ReadNodeIdInTheTree() ?? new HashSet<string>());
                foreach (var et in nodes)
                {
                    if (!nodeIds.Contains(et.EntityId))
                    {
                        var etNode = et as TreeNode;
                        if (etNode != null)
                        {
                            invalidNodes.Add(etNode);
                        }
                    }
                }
            }
            return invalidNodes;
        }

        private HashSet<string> ReadNodeIdInTheTree()
        {
            var nodes = this.ReadNodeInTheTree();
            return new HashSet<string>(nodes.Select(k => k.EntityId));
        }

        private List<TreeNode> ReadNodeInTheTree()
        {
            var nodes = new List<TreeNode>();
            nodes.Add(this.Root);
            this.MakeNodeSort(this.Root.EntityId, nodes);
            return nodes;
        }

        private void MakeNodeSort(string entityId, List<TreeNode> list_0)
        {
            var cNodes = this.ReadDescendantList(entityId, false);
            foreach (var node in cNodes)
            {
                list_0.Add(node);
                if (this.IsCatalogType(node))
                {
                    this.MakeNodeSort(node.EntityId, list_0);
                }
            }
        }

        private List<EntityBase> ReadAllNodeFromDb()
        {
            var lst = new List<EntityBase>();
            lst.AddRange(DbHelper.Fetch(this.DbAccount, this._catalogType, null));
            lst.AddRange(DbHelper.Fetch(this.DbAccount, this._leafType, null));
            return lst;
        }

        private TreeNode ReadRootFromDb()
        {
            List<TreeNode> list = DbHelper.Fetch<TreeNode>(this._catalogType, this._dbAccount, (TreeNode x) =>
            {
                TreeCatalog TreeCatalog = (TreeCatalog)x;
                return TreeCatalog.Name == "Root!@#$Catalog$%&Header*&()";
            });
            TreeNode result = null;
            int cnt = list.xCount();
            if (cnt == 1)
            {
                result = list[0];
            }
            else if (cnt > 1)
            {
                result = this.FixExtraRoot(list);
            }
            return result;
        }

        private TreeNode FixExtraRoot(List<TreeNode> nodes)
        {
            TreeNode node = null;
            int descendantCnt = 0;
            foreach (TreeNode n in nodes)
            {
                var descendants = this.ReadDescendant(n.EntityId, false);
                if (descendants != null && descendants.Count >= descendantCnt)
                {
                    descendantCnt = descendants.Count;
                    node = n;
                }
            }
            nodes.Remove(node);
            foreach (TreeNode n in nodes)
            {
                n.IsDeleted = true;
            }
            DbHelper.BatchSaveOrUpdateToDb(nodes.ToArray());
            Log.Error(string.Format("删除多余的root,count={0},dba={1}", nodes.Count, this._dbAccount));
            return node;
        }

        public bool VerifyCatalogName(string parentId, string name)
        {
            return DbHelper.FirstOrDefault(this._catalogType, this.DbAccount, (k) =>
            {
                var et = k as TreeCatalog;
                return et.ParentId == parentId && et.Name == name;
            }) != null;
        }

        public bool HasSameNameCatalogChild(string parentId, string name)
        {
            return DbHelper.FirstOrDefault(this._catalogType, this._dbAccount,  x =>
            {
                TreeCatalog TreeCatalog = (TreeCatalog)x;
                return TreeCatalog.ParentId == parentId && TreeCatalog.Name == name && TreeCatalog.PrevId != TreeCatalog.ParentId;
            }) != null;
        }

        private List<TreeNode> ReadChildNodes(Type t, string dbAccount, string parentId)
        {
            return DbHelper.Fetch<TreeNode>(t, dbAccount, (x) => x.ParentId == parentId);
        }

        public void SaveNodeToDb(TreeNode n)
        {
            DbHelper.SaveToDb(n, true);
        }

        public void SaveRecordsInTransaction(params EntityBase[] arr)
        {
            DbHelper.BatchSaveOrUpdateToDb(arr);
        }

        public List<TreeNode> ReadAncestorList(TreeNode targetNode, bool includeSelf = true, bool includeRoot = false)
        {
            List<TreeNode> ancestors = new List<TreeNode>();
            if (includeSelf)
            {
                ancestors.Add(targetNode);
            }
            while (!string.IsNullOrEmpty(targetNode.ParentId))
            {
                targetNode = this.ReadCatalogById(targetNode.ParentId);
                Util.Assert(targetNode != null);
                ancestors.Add(targetNode);
            }
            if (!includeRoot && ancestors.Count > 0)
            {
                ancestors.RemoveAt(ancestors.Count - 1);
            }
            ancestors.Reverse();
            return ancestors;
        }

        private TreeNode ReadCatalogById(string entityId)
        {
            return this.ReadNodeByIdAndType(entityId, this._catalogType);
        }

        public TreeNode ReadNodeByIdAndType(string entityId, Type type)
        {
            return (TreeNode)DbHelper.FirstOrDefault(type, this._dbAccount, entityId);
        }

        public TreeNode ReadNodeById(string entityId)
        {
            TreeNode result;
            if (entityId == null)
            {
                result = null;
            }
            else
            {
                result = (this.ReadNodeByIdAndType(entityId, this._leafType) ?? this.ReadNodeByIdAndType(entityId, this._catalogType));
            }
            return result;
        }

        public void DeleteLeaf(TreeNode et)
        {
            var uptEts = new List<TreeNode>();
            et.IsDeleted = true;
            uptEts.Add(et);
            var prev = this.ReadNodeById(et.PrevId);
            var next = this.ReadNodeById(et.NextId);
            if (prev != null)
            {
                prev.NextId = ((next != null) ? next.EntityId : null);
                uptEts.Add(prev);
            }
            if (next != null)
            {
                next.PrevId = ((prev != null) ? prev.EntityId : null);
                uptEts.Add(next);
            }
            DbHelper.BatchSaveOrUpdateToDb(uptEts.ToArray());
        }

        public void AddNext(TreeNode n, TreeNode prev)
        {
            this.InsertNewNodeBetween(n, prev.ParentId, prev, null, null);
        }

        public void InsertNewNodeBetween(TreeNode n, string parentId, TreeNode prev, TreeNode next, HashSet<TreeNode> modifiedSet = null)
        {
            Util.Assert(!string.IsNullOrEmpty(parentId));
            n.ParentId = parentId;
            if (string.IsNullOrEmpty(n.DbAccount))
            {
                if (prev != null && !string.IsNullOrEmpty(prev.DbAccount))
                {
                    n.DbAccount = prev.DbAccount;
                }
                else
                {
                    n.DbAccount = next.DbAccount;
                }
            }
            if (prev != null)
            {
                n.PrevId = prev.EntityId;
                n.NextId = prev.NextId;
                prev.NextId = n.EntityId;
                next = this.ReadNodeById(n.NextId);
                if (next != null)
                {
                    next.PrevId = n.EntityId;
                }
            }
            else
            {
                if (next != null)
                {
                    n.NextId = next.EntityId;
                    n.PrevId = next.PrevId;
                    next.PrevId = n.EntityId;
                    prev = this.ReadNodeById(n.PrevId);
                    if (prev != null)
                    {
                        prev.NextId = n.EntityId;
                    }
                }
                else
                {
                    n.PrevId = null;
                    n.NextId = null;
                    Util.Assert(!this.HasChildren(n.ParentId));
                }
            }
            if (modifiedSet == null)
            {
                DbHelper.BatchSaveOrUpdateToDb(n,prev,next);
            }
            else
            {
                if (n != null)
                {
                    modifiedSet.Add(n);
                }
                if (prev != null)
                {
                    modifiedSet.Add(prev);
                }
                if (next != null)
                {
                    modifiedSet.Add(next);
                }
            }
        }

        public void InsertNewNodeBetween(TreeNode n, string parentId, TreeNode prev, TreeNode next)
        {
            n.ParentId = parentId;
            if (string.IsNullOrEmpty(n.DbAccount))
            {
                string dbAccount;
                if (prev != null)
                {
                    if ((dbAccount = prev.DbAccount) != null)
                    {
                        goto IL_2C;
                    }
                }
                dbAccount = next.DbAccount;
            IL_2C:
                n.DbAccount = dbAccount;
            }
            if (prev != null)
            {
                n.PrevId = prev.EntityId;
                n.NextId = prev.NextId;
                prev.NextId = n.EntityId;
                next = this.ReadNodeById(n.NextId);
                if (next != null)
                {
                    next.PrevId = n.EntityId;
                }
            }
            else if (next != null)
            {
                n.NextId = next.EntityId;
                n.PrevId = next.PrevId;
                next.PrevId = n.EntityId;
                prev = this.ReadNodeById(n.PrevId);
                if (prev != null)
                {
                    prev.NextId = n.EntityId;
                }
            }
            else
            {
                n.PrevId = null;
                n.NextId = null;
                Util.Assert(!this.HasChildren(n.ParentId));
            }
            DbHelper.BatchSaveOrUpdateToDb(new EntityBase[]
            {
                n,
                prev,
                next
            });
        }

        public bool IsRoot(TreeNode n)
        {
            return n.EntityId == this.Root.EntityId;
        }

        public void DeleteCatalog(TreeNode et)
        {
            var uptEts = new List<TreeNode>();
            if (!this.IsRoot(et))
            {
                et.IsDeleted = true;
                uptEts.Add(et);
                var prev = this.ReadNodeById(et.PrevId);
                var next = this.ReadNodeById(et.NextId);
                if (prev != null)
                {
                    prev.NextId = ((next != null) ? next.EntityId : null);
                    uptEts.Add(prev);
                }
                if (next != null)
                {
                    next.PrevId = ((prev != null) ? prev.EntityId : null);
                    uptEts.Add(next);
                }
            }
            var descendants = this.ReadDescendant(et.EntityId, false);
            foreach (var n in descendants)
            {
                n.IsDeleted = true;
                if (n is ShortcutEntity)
                {
                    this.DeleteShortcutImageIfHas(n as ShortcutEntity);
                }
                uptEts.Add(n);
            }
            DbHelper.BatchSaveOrUpdateToDb(uptEts.ToArray());
        }

        private void DeleteShortcutImageIfHas(ShortcutEntity se)
        {
            try
            {
                if (se != null && !string.IsNullOrEmpty(se.ImageName))
                {
                    ShortcutImageHelper.DeleteImage(se.ImageName);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public bool HasChildren(TreeNode groupEntity)
        {
            return this.HasChildren(groupEntity.EntityId);
        }

        public bool HasChildren(string parentId)
        {
            var leafs = DbHelper.Fetch<TreeNode>(this._leafType, this._dbAccount, (TreeNode x) => x.ParentId == parentId);
            var catas = DbHelper.Fetch<TreeNode>(this._catalogType, this._dbAccount, (TreeNode x) => x.ParentId == parentId);
            return (leafs != null && leafs.Count() > 0) || (catas != null && catas.Count() > 0);
        }

        public List<TreeNode> ReadDescendant(string string_0, bool bool_0 = true)
        {
            List<TreeNode> list = new List<TreeNode>();
            List<TreeNode> list2 = this.ReadChildNode(string_0, bool_0);
            list.AddRange(list2);
            foreach (TreeNode TreeNode in list2)
            {
                if (this.IsCatalogType(TreeNode))
                {
                    list.AddRange(this.ReadDescendant(TreeNode.EntityId, bool_0));
                }
            }
            return list;
        }

        public bool IsCatalogType(TreeNode x)
        {
            return x.GetType().Equals(this._catalogType);
        }

        public bool IsLeafType(TreeNode x)
        {
            return x.GetType().Equals(this._leafType);
        }

        public List<TreeNode> ReadChildNode(string parentId, bool sortByChain = true)
        {
            List<TreeNode> childNodes = new List<TreeNode>();
            var cataNodes = this.ReadChildNodes(this._catalogType, this._dbAccount, parentId);
            if (!cataNodes.xIsNullOrEmpty())
            {
                childNodes.AddRange(cataNodes);
            }
            var leafNodes = this.ReadChildNodes(this._leafType, this._dbAccount, parentId);
            if (!leafNodes.xIsNullOrEmpty())
            {
                childNodes.AddRange(leafNodes);
            }
            if (sortByChain)
            {
                childNodes = childNodes.OrderByDescending(k => k.ModifyTick).ToList();
            }
            return childNodes;
        }

        public List<TreeNode> ReadChildCatalogById(string entityId)
        {
            List<TreeNode> source = this.ReadChildNode(entityId, true);
            return source.Where(n => this.IsCatalogType(n)).ToList();
        }

        public List<TreeNode> ReadAllDescendantList(string entityId, bool sort = true)
        {
            var nodes = new List<TreeNode>();
            var cNodes = this.ReadDescendantList(entityId, sort);
            nodes.AddRange(cNodes);
            foreach (var node in cNodes)
            {
                if (this.IsCatalogType(node))
                {
                    nodes.AddRange(this.ReadAllDescendantList(node.EntityId, sort));
                }
            }
            return nodes;
        }

        public List<TreeNode> ReadDescendantList(string entityId, bool sort = true)
        {
            var nodes = new List<TreeNode>();
            var cNodes = this.ReadChildNodes(this._catalogType, this.DbAccount, entityId);
            if (!cNodes.xIsNullOrEmpty())
            {
                nodes.AddRange(cNodes);
            }
            cNodes = this.ReadChildNodes(this._leafType, this.DbAccount, entityId);
            if (!cNodes.xIsNullOrEmpty())
            {
                nodes.AddRange(cNodes);
            }
            if (sort)
            {
                nodes = nodes.OrderByDescending(k => k.ModifyTick).ToList();
            }
            return nodes;
        }

        public List<TreeNode> ReadAllCataNodes(string rootId)
        {
            var nodes = this.ReadDescendantList(rootId, true);
            return nodes.Where(k => IsCatalogType(k)).ToList();
        }


    }

}
