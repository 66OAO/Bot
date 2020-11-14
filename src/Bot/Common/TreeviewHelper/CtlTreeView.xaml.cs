using BotLib;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using Bot.Common.Trivial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using BotLib.Extensions;
using Bot.Common.Account;
using Bot.AssistWindow.Widget.Right.ShortCut;
using Bot.Common.Windows;
using Bot.AssistWindow.Widget.Right;
using DbEntity;

namespace Bot.Common.TreeviewHelper
{
    public partial class CtlTreeView : UserControl
    {
        private TreeViewController _ctl;
        private bool _isShowRoot;
        private bool _isReadOnly;
        private Func<TreeNode, bool> _showFilter;
        private Action<TreeViewItem, TreeNode> _leafChildrenCreater;
        private TreeNode _showFrom;
        private Func<bool> _candrag;
        private Window _parentWidnow;
        private DelayCaller _treeUpdater;
        private bool _triggerTreeUpdate;
        private string[] _filterKeys;
        private object _synobj;
        private bool? _IsPublicDbAccount;
        private bool _isDragingStart;
        private Point _dragStartPoint;
        private TreeViewItem _dragSource;
        public enum ItemTypeEnum
        {
            Nothing,
            Catalog,
            Leaf
        }

        private enum InsertTypeEnum
        {
            Chidren,
            PrevSibling,
            NextSibling
        }

        private bool IsPublicDbAccount
        {
            get
            {
                if (!_IsPublicDbAccount.HasValue)
                {
                    _IsPublicDbAccount = new bool?(AccountHelper.GetMainPart(_ctl.Seller) == _ctl.DbAccount);
                }
                bool? isPublicDbAccount = _IsPublicDbAccount;
                return isPublicDbAccount.GetValueOrDefault() & isPublicDbAccount.HasValue;
            }
        }
        private Window ParentWindow
        {
            get
            {
                if (_parentWidnow == null)
                {
                    _parentWidnow = this.xFindParentWindow();
                }
                return _parentWidnow;
            }
        }

        public event EventHandler<MouseButtonEventArgs> EvDoubleClickLeafNode;
        public event EventHandler<CommonEventArgs<TreeViewItem>> EvDrop;
        public CtlTreeView()
        {
            _isReadOnly = false;
            _parentWidnow = null;
            _triggerTreeUpdate = true;
            _synobj = new object();
            _IsPublicDbAccount = null;
            _isDragingStart = false;
            InitializeComponent();
        }

        public void Init(TreeViewController controller, Func<TreeNode, bool> showFilter = null, Action<TreeViewItem, TreeNode> leafChildrenCreater = null, bool isReadOnly = false, bool isShowRoot = false, string targetNodeId = null, TreeNode showFrom = null, Func<bool> canDrag = null)
        {
            _ctl = controller;
            _showFilter = showFilter;
            _leafChildrenCreater = leafChildrenCreater;
            _showFrom = (showFrom ?? _ctl.DbAccessor.Root);
            _isReadOnly = isReadOnly;
            _isShowRoot = isShowRoot;
            _candrag = canDrag;
            _treeUpdater = new DelayCaller(() =>
            {
                LoadTreeViewData(null);
            }, 500, true);
            LoadTreeViewData(targetNodeId);
        }

        public void ShowMessage(string msg, string title = "提示")
        {
            MsgBox.ShowTip(msg, title, this, null);
        }

        private void CreateNode(bool isCatalog, object data, Action<TreeNode> cb = null)
        {
            var selectedItem = (tvMain.SelectedItem as TreeViewItem);
            if (selectedItem == null)
            {
                ShowMessage("需要选中一个条目，才能增加分组", "提示");
                return;
            }
            RenderItem(selectedItem);
            var insertIndex = GetInsertIndexOfTreeView(selectedItem);
            var pcat = GetCataNode(selectedItem);
            Util.Assert(pcat != null);
            var act = new Action<TreeNode>((node) =>
            {
                TreeViewItem newIt = null;
                if (isCatalog)
                {
                    var items = GetChildrenOrBrotherItems(selectedItem);
                    newIt = CreateCataItem(items, node, null, insertIndex);
                }
                else
                {
                    var items = GetChildrenOrBrotherItems(selectedItem);
                    newIt = CreateLeafItem(items, node, insertIndex);
                }

                FocusItem(newIt);

                var pre = GetPreIndexNode(newIt);
                var next = GetNextIndexNode(newIt);
                _ctl.DbAccessor.InsertNewNodeBetween(node, pcat.EntityId, pre, next);
                if (cb != null) cb(node);
            });
            if (isCatalog)
            {
                _ctl.CreateCata(pcat, ParentWindow, act, data);
            }
            else
            {
                _ctl.Create(pcat, ParentWindow, act, data);
            }
        }

        private TreeViewItem GetPreIndexItem(TreeViewItem item)
        {
            TreeViewItem it = null;
            if (item != null)
            {
                int indexAtParent = GetIndexAtParent(item);
                it = GetItemByIndex(item, indexAtParent - 1);
            }
            return it;
        }

        private TreeViewItem GetNextIndexItem(TreeViewItem item)
        {
            TreeViewItem it = null;
            if (item != null)
            {
                int indexAtParent = GetIndexAtParent(item);
                it = GetItemByIndex(item, indexAtParent + 1);
            }
            return it;
        }

        private TreeViewItem GetItemByIndex(TreeViewItem it, int idx)
        {
            if (idx < 0) return null;
            var brothers = GetBrotherItems(it);
            return ((brothers.Count > idx) ? (brothers[idx] as TreeViewItem) : null);
        }

        private TreeNode GetNextIndexNode(TreeViewItem it)
        {
            return ReadNode(GetNextIndexItem(it));
        }

        private TreeNode GetPreIndexNode(TreeViewItem it)
        {
            return ReadNode(GetPreIndexItem(it));
        }

        private TreeNode GetCataNode(TreeViewItem it)
        {
            if (it == null) return _showFrom;
            return IsCatalogItem(it) ? ReadNode(it) : ReadLeafNode(it);
        }

        private TreeNode ReadLeafNode(TreeViewItem it)
        {
            if (it == null) return _showFrom;
            return it.Parent == tvMain ? _showFrom : ReadNode(it.Parent as TreeViewItem);
        }

        private ItemCollection GetChildrenOrBrotherItems(TreeViewItem item)
        {
            if (item == null) return tvMain.Items;
            return IsCatalogItem(item) ? item.Items : GetBrotherItems(item);
        }

        private int GetInsertIndexOfTreeView(TreeViewItem item)
        {
            if (item == null) return tvMain.Items.Count;
            return IsCatalogItem(item) ? item.Items.Count : GetBrotherItems(item).IndexOf(item) + 1;
        }

        public void Create(object data = null, Action<TreeNode> cb = null)
        {
            CreateNode(false, data, cb);
        }

        public void CreateCatalog(object data = null, Action<TreeNode> cb = null)
        {
            CreateNode(true, data, cb);
        }

        public void EditAsync(Action<TreeNode> cb = null)
        {
            var selectedItem = (tvMain.SelectedItem as TreeViewItem);
            if (selectedItem == null)
            {
                ShowMessage("需要选中一个条目，才能编辑", "提示");
                return;
            }
            var tvNode = ReadNode(selectedItem);

            if (_ctl.DbAccessor.IsRoot(tvNode))
            {
                ShowMessage("不允许编辑“根分组”", "提示");
                return;
            }
            Util.Assert(!string.IsNullOrEmpty(tvNode.ParentId));
            TreeNode TreeNode_ = tvNode.Clone<TreeNode>(false);
            var act = new Action<TreeNode>((newNode) =>
            {
                newNode.CopyTreeNodeDataFrom(tvNode);
                SetDbNodeToTvItem(selectedItem, newNode);
                _ctl.DbAccessor.SaveNodeToDb(newNode);
                cb(newNode);
            });
            if (IsCatalogType(tvNode))
            {
                _ctl.EditCata(TreeNode_, ParentWindow, act);
            }
            else
            {
                _ctl.Edit(TreeNode_, ParentWindow, act);
            }
        }

        private void SetDbNodeToTvItem(TreeViewItem it, TreeNode node)
        {
            it.Tag = node.EntityId;
            if (IsCatalogType(node))
            {
                it.Header = new CtlCatalog(_ctl.ReadCataName(node), _filterKeys);
            }
            else
            {
                it.Header = new CtlLeaf(_ctl.ReadNodeTitle(node), _ctl.ReadNodeCode(node), _filterKeys, null, null, GetBackgroundColorBrush());
            }
        }

        private void tbxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_triggerTreeUpdate)
            {
                if (_treeUpdater != null)
                {
                    _treeUpdater.CallAfterDelay();
                }
            }
        }

        private void commands_Clear(object sender, ExecutedRoutedEventArgs e)
        {
            tbxSearch.Text = "";
            LoadTreeViewData(null);
            tbxSearch.Focus();
        }

        private void tvMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EvDoubleClickLeafNode != null)
            {
                var tvItem = tvMain.SelectedItem as TreeViewItem;
                if (tvItem != null)
                {
                    Point position = e.GetPosition(tvMain);
                    if (IsPointInsideHeaderItem(position, tvItem, false)
                        && !tvItem.HasItems
                        && IsCatalogType(tvItem))
                    {
                        if (EvDoubleClickLeafNode != null)
                        {
                            EvDoubleClickLeafNode(tvItem, e);
                        }
                    }
                }
            }
        }

        public bool IsCatalogType(TreeViewItem it)
        {
            string entityId = it.Tag as string;
            var TreeNode = _ctl.DbAccessor.ReadNodeById(entityId);
            return TreeNode != null && _ctl.DbAccessor.IsCatalogType(TreeNode);
        }

        private void tvMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isReadOnly && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(tvMain);
                if (_isDragingStart)
                {
                    if (IsDoDragDrop(position))
                    {
                        DragDrop.DoDragDrop(_dragSource, _dragSource, DragDropEffects.Move);
                        _isDragingStart = false;
                        _dragSource = null;
                    }
                }
                else
                {
                    TreeViewItem tvItem = tvMain.SelectedItem as TreeViewItem;
                    if (IsPointInsideHeaderItem(position, tvItem, true))
                    {
                        _isDragingStart = true;
                        _dragSource = tvItem;
                        _dragStartPoint = position;
                    }
                }
            }
        }

        public TreeNode ReadNode(TreeViewItem it)
        {
            string entityId = ((it != null) ? it.Tag : null) as string;
            return (entityId == null) ? null : _ctl.DbAccessor.ReadNodeById(entityId);
        }

        private bool IsPointInsideItem(Point p, TreeViewItem it, bool writeLog = true)
        {
            bool rt = false;
            if (it == null) return rt;
            Point point = it.TranslatePoint(new Point(0.0, 0.0), tvMain);
            rt = p.X >= point.X && p.X < point.X + it.ActualWidth && p.Y >= point.Y && p.Y < point.Y + it.ActualHeight;
            return rt;
        }

        private bool IsPointInsideHeaderItem(Point p, TreeViewItem it, bool writeLog = true)
        {
            bool isPointInsideItem = false;
            if (it == null) return isPointInsideItem;
            HitTestResult hitRlt = VisualTreeHelper.HitTest(tvMain, p);
            FrameworkElement fe = null;
            if (hitRlt != null && hitRlt.VisualHit != null)
            {
                TreeViewItem item = DependencyObjectEx.xFindAncestorFromMe<TreeViewItem>(hitRlt.VisualHit);
                fe = ((item != null) ? item.Header : null) as FrameworkElement;
            }
            if (fe == null) return isPointInsideItem;
            Point point = fe.TranslatePoint(new Point(0.0, 0.0), tvMain);
            isPointInsideItem = p.X >= point.X && p.X < point.X + fe.ActualWidth && p.Y >= point.Y && p.Y < point.Y + fe.ActualHeight;
            
            return isPointInsideItem;
        }

        private bool IsDoDragDrop(Point p)
        {
            return Math.Abs(p.X - _dragStartPoint.X) > 10.0 || Math.Abs(p.Y - _dragStartPoint.Y) > 10.0;
        }

        private void tvMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragingStart = false;
            _dragSource = null;
        }

        private TreeViewItem GetTreeViewItemByHitPosi(Point p)
        {
            HitTestResult hit = VisualTreeHelper.HitTest(tvMain, p);
            TreeViewItem tvItem = null;
            if (hit != null)
            {
                tvItem = ((hit.VisualHit != null) ? DependencyObjectEx.xFindAncestorFromMe<TreeViewItem>(hit.VisualHit) : null);
            }
            return tvItem;
        }

        private TreeViewItem GetDragItemParent(Point p)
        {
            var it = GetTreeViewItemByPosi(p);
            if (it == null)
            {
                double distance;
                it = GetNearestTreeViewItem(tvMain.Items, p, out distance);
            }
            while (it.Parent != tvMain)
            {
                var header = it.Header as FrameworkElement;
                var point = header.TranslatePoint(new Point(0.0, 0.0), tvMain);
                if (p.X >= point.X - 3.0)
                {
                    break;
                }
                it = (it.Parent as TreeViewItem);
            }
            return it;
        }

        private TreeViewItem GetTreeViewItemByPosi(Point p)
        {
            var it = GetTreeViewItemByHitPosi(p);
            if (it != null)
            {
                var fe = it.Header as FrameworkElement;
                var point = fe.TranslatePoint(new Point(0.0, fe.ActualHeight / 2.0), tvMain);
                TreeViewItem tmpItem = null;
                if (p.Y <= point.Y)
                {
                    tmpItem = GetPrevItem(it);
                }
                else
                {
                    tmpItem = GetNextItem(it);
                }
                if (tmpItem != null && GetTreeViewItemDistance(tmpItem, p) < GetTreeViewItemDistance(it, p))
                {
                    it = tmpItem;
                }
            }
            return it;
        }

        private TreeViewItem GetNextItem(TreeViewItem it)
        {
            TreeViewItem nextItem = null;
            if (it.Items.Count > 0 && it.IsExpanded)
            {
                nextItem = (it.Items[0] as TreeViewItem);
                return nextItem;
            }
            ItemCollection items = GetBrotherItems(it);
            int idx = items.IndexOf(it);
            if (idx == items.Count - 1)
            {
                nextItem = GetSubNextItem(it);
            }
            else
            {
                nextItem = (items[idx + 1] as TreeViewItem);
            }
            return nextItem;
        }

        private TreeViewItem GetSubNextItem(TreeViewItem it)
        {
            TreeViewItem nextItem = null;
            if (it.Parent == tvMain) return nextItem;
            var parentItem = it.Parent as TreeViewItem;
            var items = GetBrotherItems(parentItem);
            int idx = items.IndexOf(parentItem);
            if (idx < items.Count - 1)
            {
                nextItem = (items[idx + 1] as TreeViewItem);
            }
            return nextItem;
        }

        public bool IsPointInsideItem(bool findInHeader, out bool isCatalog)
        {
            isCatalog = false;
            var item = tvMain.SelectedItem as TreeViewItem;
            var isPointInsideItem = false;
            if (item == null) return isPointInsideItem;
            Point p = Mouse.GetPosition(tvMain);
            if (findInHeader)
            {
                isPointInsideItem = IsPointInsideHeaderItem(p, item, false);
            }
            else
            {
                isPointInsideItem = IsPointInsideHeaderItem(p, item, false);
            }
            if (isPointInsideItem)
            {
                isCatalog = IsCatalogItem(item);
            }
            return isPointInsideItem;
        }

        private TreeViewItem GetPrevItem(TreeViewItem tvItem)
        {
            int indexAtParent = GetIndexAtParent(tvItem);
            TreeViewItem preItem = null;
            if (indexAtParent == 0)
            {
                preItem = (tvItem.Parent as TreeViewItem);
                return preItem;
            }
            preItem = GetBrotherItems(tvItem)[indexAtParent - 1] as TreeViewItem;
            if (preItem.Items.Count > 0 && preItem.IsExpanded)
            {
                preItem = (preItem.Items[preItem.Items.Count - 1] as TreeViewItem);
            }
            return preItem;
        }

        private TreeViewItem GetNearestTreeViewItem(ItemCollection items, Point p, out double distance)
        {
            distance = Double.MaxValue;
            TreeViewItem distItem = null;
            foreach (TreeViewItem item in items)
            {
                var dst = GetTreeViewItemDistance(item, p);
                if (dst < distance)
                {
                    distance = dst;
                    distItem = item;
                }
                if (item.IsExpanded)
                {
                    double subDist;
                    var subItem = GetNearestTreeViewItem(item.Items, p, out subDist);
                    if (subDist < distance)
                    {
                        distance = subDist;
                        distItem = subItem;
                    }
                }
            }
            return distItem;
        }

        private double GetTreeViewItemDistance(TreeViewItem it, Point p)
        {
            var fe = it.Header as FrameworkElement;
            Point point = fe.TranslatePoint(new Point(0.0, fe.ActualHeight / 2.0), tvMain);
            double diffX = point.X - p.X;
            double diffY = point.Y - p.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        private ItemCollection GetBrotherItems(TreeViewItem it)
        {
            ItemCollection items = null;
            if (it == null || it.Parent == null)
            {
                items = tvMain.Items;
            }
            else if (it.Parent is TreeViewItem)
            {
                items = (it.Parent as TreeViewItem).Items;
            }
            else if (it.Parent is TreeView)
            {
                items = (it.Parent as TreeView).Items;
            }
            return items;
        }

        private int GetIndexAtParent(TreeViewItem item)
        {
            int idx = GetBrotherItems(item).IndexOf(item);
            Util.Assert(idx >= 0);
            return idx;
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            Point posi = e.GetPosition(tvMain);
            var item = GetDragItemParent(posi);
            if (item == _dragSource || _dragSource.IsAncestorOf(item))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        public TreeNode ReadNodeFromSelectedItem()
        {
            if (tvMain.SelectedItem == null) return null;
            return ReadNode(tvMain.SelectedItem as TreeViewItem);
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (_dragSource != null)
                {
                    if (_candrag != null && _candrag())
                    {
                        Point posi = e.GetPosition(tvMain);
                        var parentIt = GetDragItemParent(posi);
                        if (parentIt == null)
                        {
                            return;
                        }
                        if (parentIt.Parent == null)
                        {
                            Log.Error("还真出现：treeView_Drop,itm.Parent == null");
                            return;
                        }
                        var insertType = GetInsertType(posi, parentIt);
                        if (insertType != InsertTypeEnum.Chidren && IsRoot(parentIt))
                        {
                            return;
                        }
                        var modifiedSet = new HashSet<TreeNode>();
                        bool draged;
                        if (draged = DetachNode(_dragSource, modifiedSet))
                        {
                            if (insertType == InsertTypeEnum.Chidren)
                            {
                                draged = MoveNodeAsChildren(_dragSource, parentIt, modifiedSet);
                            }
                            else
                            {
                                draged = MoveNodeAsSibling(_dragSource, parentIt, insertType, modifiedSet);
                            }
                        }
                        if (draged)
                        {
                            var modifiedNodes = modifiedSet.Where(k => k != null).ToArray<TreeNode>();
                            this._ctl.DbAccessor.SaveRecordsInTransaction(modifiedNodes);
                        }
                        FocusItem(_dragSource);
                        if (EvDrop != null)
                        {
                            EvDrop(this, new CommonEventArgs<TreeViewItem>
                            {
                                Data = _dragSource
                            });
                        }
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowErrTip(ex.Message);
                Log.Exception(ex);
            }
        }

        private bool IsRoot(TreeViewItem it)
        {
            return this._isShowRoot && this.ReadNode(it).EntityId == this._showFrom.EntityId;
        }

        private InsertTypeEnum GetInsertType(Point p, TreeViewItem parentIt)
        {
            var fe = parentIt.Header as FrameworkElement;
            Point point = fe.TranslatePoint(new Point(0.0, 0.0), this.tvMain);
            InsertTypeEnum insertType;
            if (this.IsCatalogType(parentIt))
            {
                if (p.Y - point.Y < 3.0)
                {
                    insertType = InsertTypeEnum.PrevSibling;
                }
                else if (point.Y + fe.ActualHeight - p.Y < 3.0)
                {
                    insertType = InsertTypeEnum.NextSibling;
                }
                else
                {
                    insertType = InsertTypeEnum.Chidren;
                }
            }
            else if (p.Y > point.Y + fe.ActualHeight / 2.0)
            {
                insertType = InsertTypeEnum.NextSibling;
            }
            else
            {
                insertType = InsertTypeEnum.PrevSibling;
            }
            return insertType;
        }

        private bool MoveNodeAsChildren(TreeViewItem dragSource, TreeViewItem parentIt, HashSet<TreeNode> modifiedSet)
        {
            bool rt = false;
            try
            {
                this.RenderItem(parentIt);
                var pNode = this.ReadNode(parentIt);
                Util.Assert(parentIt != null && pNode != null && pNode.EntityId != null && this.IsCatalogType(pNode));
                var n = this.ReadNode(dragSource);
                n.ParentId = pNode.EntityId;
                List<TreeNode> list = this._ctl.DbAccessor.ReadDescendantNode(pNode.EntityId, true);
                TreeNode lastNode = (list != null) ? list.LastOrDefault<TreeNode>() : null;
                if (lastNode != null)
                {
                    lastNode.NextId = n.EntityId;
                    n.PrevId = lastNode.EntityId;
                }
                modifiedSet.Add(n);
                modifiedSet.Add(lastNode);
                RemoveFromStartPosition(dragSource);
                parentIt.Items.Add(dragSource);
                rt = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        private bool MoveNodeAsSibling(TreeViewItem dragSource, TreeViewItem parentIt, InsertTypeEnum itype, HashSet<TreeNode> modifiedSet)
        {
            bool rt = false;
            try
            {
                var n = this.ReadNode(dragSource);
                var pNode = this.ReadNode(parentIt);
                if (itype == CtlTreeView.InsertTypeEnum.NextSibling)
                {
                    this._ctl.DbAccessor.InsertNewNodeBetween(n, pNode.ParentId, pNode, null, modifiedSet);
                }
                else if (itype == CtlTreeView.InsertTypeEnum.PrevSibling)
                {
                    this._ctl.DbAccessor.InsertNewNodeBetween(n, pNode.ParentId, null, pNode, modifiedSet);
                }
                else
                {
                    Util.Assert(false);
                }
                this.RemoveFromStartPosition(dragSource);
                int idx = this.GetIndexAtParent(parentIt);
                if (itype == InsertTypeEnum.NextSibling)
                {
                    idx++;
                }
                this.GetBrotherItems(parentIt).Insert(idx, dragSource);
                rt = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        private void RemoveFromStartPosition(TreeViewItem it)
        {
            if (it != null && it.Parent != null)
            {
                var brotherItems = this.GetBrotherItems(it);
                if (brotherItems != null && brotherItems.Contains(it))
                {
                    if (brotherItems.Count == 1 && it.Parent != this.tvMain)
                    {
                        var pItem = it.Parent as TreeViewItem;
                        if (pItem != null)
                        {
                            pItem.IsExpanded = false;
                        }
                    }
                    brotherItems.Remove(it);
                }
            }
        }

        private bool DetachNode(TreeViewItem dragSource, HashSet<TreeNode> modifiedSet)
        {
            bool rt = false;
            try
            {
                var dragNode = this.ReadNode(dragSource);
                var prevNode = this._ctl.DbAccessor.ReadNodeById(dragNode.PrevId);
                var nextNode = this._ctl.DbAccessor.ReadNodeById(dragNode.NextId);
                dragNode.PrevId = null;
                dragNode.NextId = null;
                if (prevNode != null)
                {
                    prevNode.NextId = ((nextNode != null) ? nextNode.EntityId : null);
                }
                if (nextNode != null)
                {
                    nextNode.PrevId = ((prevNode != null) ? prevNode.EntityId : null);
                }
                if (dragNode != null)
                {
                    modifiedSet.Add(dragNode);
                }
                if (prevNode != null)
                {
                    modifiedSet.Add(prevNode);
                }
                if (nextNode != null)
                {
                    modifiedSet.Add(nextNode);
                }
                rt = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        public void ReloadTreeViewData(string entityId)
        {
            if (!string.IsNullOrEmpty(entityId))
            {
                LoadTreeViewData(entityId);
            }
        }

        public TreeNode Delete()
        {
            var it = tvMain.SelectedItem as TreeViewItem;
            if (it == null)
            {
                ShowMessage("需要选中一个条目，才能删除", "提示");
                return null;
            }
            if (IsCatalogItem(it))
            {
                return DeleteCatalog(it);
            }
            else
            {
                return Delete(it);
            }
        }

        private TreeNode Delete(TreeViewItem it)
        {
            TreeNode deletedNode = null;
            var node = ReadNode(it);
            var text = _ctl.ReadNodeTitle(node);
            if (text.xIsNullOrEmptyOrSpace())
            {
                text = _ctl.ReadNodeTitle(node);
            }
            var message = string.Format("确定要删除 {0}:{1}?", _ctl.ControllerName(), text);
            if (WndNotTipAgain.MyShowDialog(message, "删除" + ParentWindow.GetType().Name, null, null, true))
            {
                _ctl.DbAccessor.DeleteLeaf(node);
                FocusNearest(it);
                deletedNode = node;
            }
            return deletedNode;
        }

        private TreeNode DeleteCatalog(TreeViewItem cataIt)
        {
            var node = ReadNode(cataIt);
            var message = string.Format("确定要删除分组?\r\n\r\n【{0}】内的节点，将全部被删除！", _ctl.ReadCataName(node));
            if (!_ctl.DbAccessor.HasChildren(node) || MsgBox.ShowDialog(message, "删除分组", this, null, null))
            {
                _ctl.DbAccessor.DeleteCatalog(node);
                if (_ctl.DbAccessor.IsRoot(node))
                {
                    cataIt.Items.Clear();
                    cataIt.Focus();
                }
                else
                {
                    FocusNearest(cataIt);
                }
            }
            return node;
        }

        private void FocusNearest(TreeViewItem it)
        {
            var brothers = GetBrotherItems(it);
            int idx = brothers.IndexOf(it);
            brothers.Remove(it);
            if (idx >= brothers.Count)
            {
                idx = brothers.Count - 1;
            }
            if (idx >= 0)
            {
                (brothers[idx] as TreeViewItem).Focus();
            }
        }

        private void LoadTreeViewData(string entityId = null)
        {
            lock (_synobj)
            {
                tvMain.Items.Clear();
                if (_showFrom != null)
                {
                    var ancestorNodes = new List<TreeNode>();
                    var node = string.IsNullOrEmpty(entityId) ? null : _ctl.DbAccessor.ReadNodeById(entityId);
                    if (node != null)
                    {
                        ancestorNodes = _ctl.DbAccessor.ReadAncestorList(node, true, false);
                        if (!ancestorNodes.xIsNullOrEmpty() && !_ctl.DbAccessor.IsRoot(_showFrom))
                        {
                            ancestorNodes.RemoveAt(0);
                        }
                        _triggerTreeUpdate = false;
                        tbxSearch.Text = string.Empty;
                    }
                    else
                    {
                        _filterKeys = SplitFilterKeys();
                    }
                    if (_isShowRoot)
                    {
                        var it = CreateRootItem(_showFrom);
                        RenderNode(it.Items, _showFrom, ancestorNodes);
                    }
                    else
                    {
                        RenderNode(tvMain.Items, _showFrom, ancestorNodes);
                    }
                    if (node != null)
                    {
                        _triggerTreeUpdate = true;
                        var it = GetTreeViewItem(node);
                        FocusItem(it);
                    }
                    else
                    {
                        if (_isShowRoot)
                        {
                            var it = tvMain.Items[0] as TreeViewItem;
                            it.IsExpanded = true;
                        }
                    }
                    if (FilterKeysNotBlank())
                    {
                        ExpandedAll(tvMain.Items);
                    }
                }
            }
        }

        private void ExpandedAll(ItemCollection its)
        {
            if (its != null && its.Count > 0)
            {
                foreach (TreeViewItem treeViewItem in its)
                {
                    if (IsCatalogItem(treeViewItem))
                    {
                        treeViewItem.IsExpanded = true;
                        ExpandedAll(treeViewItem.Items);
                    }
                }
            }
        }

        public bool IsCatalogItem(TreeViewItem it)
        {
            var entityId = it.Tag.ToString();
            var node = _ctl.DbAccessor.ReadNodeById(entityId);
            return node != null && _ctl.DbAccessor.IsCatalogType(node);
        }

        private bool FilterKeysNotBlank()
        {
            return _filterKeys != null && _filterKeys.Length > 0;
        }

        private void FocusItem(TreeViewItem item)
        {
            if (item != null)
            {
                ExpandParentItem(item);
                item.BringIntoView();
                item.IsSelected = true;
                item.Focus();
            }
        }

        private void ExpandParentItem(TreeViewItem it)
        {
            TreeViewItem pItem = it.Parent as TreeViewItem;
            if (pItem != null && !pItem.IsExpanded)
            {
                pItem.IsExpanded = true;
                tvMain.UpdateLayout();
            }
        }

        private TreeViewItem GetTreeViewItem(TreeNode node)
        {
            return GetTreeViewItemFromDescendantItem(node, tvMain.Items);
        }

        private TreeViewItem GetTreeViewItemFromDescendantItem(TreeNode node, ItemCollection items)
        {
            TreeViewItem rt = null;
            foreach (TreeViewItem it in items)
            {
                var n = ReadNode(it);
                if (((n != null) ? n.EntityId : null) == node.EntityId)
                {
                    rt = it;
                    break;
                }
                if (it.Items.Count > 0)
                {
                    var item = GetTreeViewItemFromDescendantItem(node, it.Items);
                    if (item != null)
                    {
                        rt = item;
                        break;
                    }
                }
            }
            return rt;
        }

        private string[] SplitFilterKeys()
        {
            var fkArr = tbxSearch.Text.Trim().xSplitBySpace(StringSplitOptions.RemoveEmptyEntries);
            var fks = new List<string>();
            for (int i = 0; i < fkArr.Length; i++)
            {
                string text = fkArr[i];
                if (!text.xIsNullOrEmptyOrSpace())
                {
                    fks.Add(text.Trim().ToLower());
                }
            }
            return fks.ToArray();
        }

        private TreeViewItem CreateRootItem(TreeNode n)
        {
            var it = CreateCataItem(n, new string[0], true);
            it.Expanded += new RoutedEventHandler(OnExpanded);
            it.Collapsed += new RoutedEventHandler(OnCollapsed);
            tvMain.Items.Add(it);
            return it;
        }

        private TreeViewItem CreateCataItem(ItemCollection brotherItems, TreeNode n, List<TreeNode> ancestors = null, int insertIndex = -1)
        {
            var it = CreateCataItem(n, _filterKeys, false);
            it.Expanded += new RoutedEventHandler(OnExpanded);
            it.Collapsed += new RoutedEventHandler(OnCollapsed);
            if (insertIndex >= 0)
            {
                brotherItems.Insert(insertIndex, it);
            }
            else
            {
                brotherItems.Add(it);
            }
            if (_ctl.DbAccessor.HasChildren(n) && NeedShowInFilter(n))
            {
                if (ancestors != null && ancestors.Count > 0 && ancestors[0].EntityId == n.EntityId)
                {
                    ancestors.RemoveAt(0);
                    RenderNode(it.Items, n, ancestors);
                }
                else
                {
                    var leafIt = new TreeViewItem();
                    leafIt.Header = "*";
                    it.Items.Add(leafIt);
                }
            }
            return it;
        }

        private bool NeedShowInFilter(TreeNode n)
        {
            var cNodes = _ctl.DbAccessor.ReadDescendantList(n.EntityId, true);
            bool showInFilter = false;
            foreach (var node in cNodes)
            {
                if (ShowNodeIfNeed(node) || (IsCatalogType(node) && NeedShowInFilter(node)))
                {
                    showInFilter = true;
                    break;
                }
            }
            return showInFilter;
        }

        private TreeViewItem CreateCataItem(TreeNode n, string[] highlightKeys, bool isRoot = false)
        {
            return new TreeViewItem
            {
                Header = new CtlCatalog(isRoot ? "根分组" : _ctl.ReadCataName(n), highlightKeys),
                Tag = n.EntityId
            };
        }

        private void OnExpanded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            TreeViewItem it = sender as TreeViewItem;
            Toggle(it, true);
            RenderItem(it);
        }

        private void RenderItem(TreeViewItem item)
        {
            if (item != null && item.Items.Count == 1)
            {
                var it = item.Items[0] as TreeViewItem;
                if (it.Header.ToString() == "*")
                {
                    item.Items.Clear();
                    var node = ReadNode(item);
                    if (node != null)
                    {
                        RenderNode(item.Items, node, null);
                    }
                }
            }
        }

        private void RenderNode(ItemCollection items, TreeNode node, List<TreeNode> ancestors = null)
        {
            List<TreeNode> childNodes = _ctl.DbAccessor.ReadDescendantList(node.EntityId, true);
            foreach (var n in childNodes)
            {
                if (ShowNodeIfNeed(n))
                {
                    if (IsCatalogType(n))
                    {
                        CreateCataItem(items, n, ancestors, -1);
                    }
                    else
                    {
                        CreateLeafItem(items, n, -1);
                    }
                }
            }
        }

        private TreeViewItem CreateLeafItem(ItemCollection items, TreeNode n, int insertIndex = -1)
        {
            var it = new TreeViewItem();
            if (n is ShortcutEntity)
            {
                it.Header = new CtlShortcutEntity(n as ShortcutEntity, _filterKeys, GetBackgroundColorBrush());
            }
            else
            {
                it.Header = new CtlLeaf(_ctl.ReadNodeTitle(n),
                    _ctl.ReadNodeCode(n),
                    _filterKeys, _ctl.ReadNodeImageName(n),
                    ShortcutImageHelper.UseImage,
                    GetBackgroundColorBrush());
            }
            it.Tag = n.EntityId;
            if (_leafChildrenCreater != null)
            {
                _leafChildrenCreater(it, n);
            }
            if (insertIndex >= 0)
            {
                items.Insert(insertIndex, it);
            }
            else
            {
                items.Add(it);
            }
            return it;
        }

        private SolidColorBrush GetBackgroundColorBrush()
        {
            return IsPublicDbAccount ? Brushes.Goldenrod : Brushes.SeaGreen;
        }

        public bool IsCatalogType(TreeNode n)
        {
            return _ctl.DbAccessor.IsCatalogType(n);
        }

        private bool ShowNodeIfNeed(TreeNode n)
        {
            bool isShowNode = true;
            if (_showFilter != null)
            {
                isShowNode = _showFilter(n);
            }
            if (isShowNode && FilterKeysNotBlank() && (!(isShowNode = NodeNameContainsFilterKey(n)) && IsCatalogType(n)))
            {
                var childNodes = _ctl.DbAccessor.ReadAllDescendantList(n.EntityId, true);
                foreach (var node in childNodes)
                {
                    if (NodeNameContainsFilterKey(node))
                    {
                        isShowNode = true;
                        break;
                    }
                }
            }
            return isShowNode;
        }

        private bool NodeNameContainsFilterKey(TreeNode n)
        {
            string text = GetShowText(n);
            string[] filterKeys = _filterKeys;
            bool containsFilterKey = false;
            for (int i = 0; i < filterKeys.Length; i++)
            {
                string value = filterKeys[i];
                if (text.Contains(value))
                {
                    containsFilterKey = true;
                    break;
                }
            }
            return containsFilterKey;
        }

        private string GetShowText(TreeNode n)
        {
            string nodeName = string.Empty;
            if (IsCatalogType(n))
            {
                nodeName = _ctl.ReadCataName(n);
            }
            else
            {
                nodeName = _ctl.ReadNodeCode(n) + " " + _ctl.ReadNodeTitle(n);
            }
            return nodeName.ToLower();
        }

        private void OnCollapsed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Toggle(sender as TreeViewItem, false);
        }

        private void Toggle(TreeViewItem it, bool isExpand)
        {
            CtlCatalog ctlCatalog = it.Header as CtlCatalog;
            ctlCatalog.Toggle(isExpand);
        }

        public void CollapseSearch()
        {
            grdSearch.Visibility = Visibility.Collapsed;
        }
    }
}
