using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bot.Common;
using Bot.Common.Account;
using Bot.Common.TreeviewHelper;
using Bot.Common.Trivial;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using Xceed.Wpf.Toolkit;
using DbEntity;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
    public partial class CtlShortcut : UserControl
    {
        private ContextMenu _menuShortcut;
        private bool _isDoubleClick;
        private DateTime _doubleClickTime;
        private string _seller;
        private ShortcutTreeviewController _pubTvController;
        private ShortcutTreeviewController _prvTvController;
        private ChatDesk _desk;
        private RightPanel _rightPanel;
        private bool _needInitTvSearch;
        private DateTime _prvSearchMouseDownTime;
        private DateTime _pubSearchMouseDownTime;
        private TreeNode _nodeToBeCopy;
        private class TabItemTag
        {
            public TabItemTag(ShortcutCatalogEntity cataEntity, bool isShopShortcut)
            {
                this.CatEntity = cataEntity;
                this.IsShopShortcut = isShopShortcut;
            }

            public ShortcutCatalogEntity CatEntity;

            public bool IsShopShortcut;
        }

        public CtlShortcut(ChatDesk desk, RightPanel rp)
        {
            this._isDoubleClick = false;
            this._doubleClickTime = DateTime.MinValue;
            this._needInitTvSearch = true;
            this._prvSearchMouseDownTime = DateTime.MinValue;
            this._pubSearchMouseDownTime = DateTime.MinValue;
            this.InitializeComponent();
            this._desk = desk;
            this._rightPanel = rp;
            this._seller = this._desk.Seller;
            this.SetContentVisible();
            this.InitTabControl(null, null, null);
            this.LoadDatas();
            this.SetTitleButtonsVisible();
            this.ShowTitleButtons();
        }

        public void SetTitleButtonsVisible()
        {
            this.grdTitleButtons.xIsVisible(Params.Shortcut.GetIsShowTitleButtons(this._seller));
        }

        public void SetContentVisible()
        {
            Params.Shortcut.ShowType showType = Params.Shortcut.GetShowType(this._seller);
            if (showType != Params.Shortcut.ShowType.ShopOnly)
            {
                if (showType != Params.Shortcut.ShowType.SelfOnly)
                {
                    Grid.SetRow(this.gboxPub, 0);
                    Grid.SetRow(this.gboxPrv, 1);
                    this.rd2.Height = new GridLength(1.0, GridUnitType.Star);
                    this.gboxPrv.Visibility = Visibility.Visible;
                    this.gboxPub.Visibility = Visibility.Visible;
                }
                else
                {
                    Grid.SetRow(this.gboxPrv, 0);
                    this.rd2.Height = GridLength.Auto;
                    this.gboxPrv.Visibility = Visibility.Visible;
                    this.gboxPub.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Grid.SetRow(this.gboxPub, 0);
                this.rd2.Height = GridLength.Auto;
                this.gboxPub.Visibility = Visibility.Visible;
                this.gboxPrv.Visibility = Visibility.Collapsed;
            }
            if (this.IsNotShopOnly())
            {
                this.tvPrvSearch.CollapseSearch();
                this.BindEvent(this.tvPrvSearch, false);
            }
            if (this.IsNotSelfOnly())
            {
                this.tvPubSearch.CollapseSearch();
                this.BindEvent(this.tvPubSearch, false);
            }
        }

        private void BindEvent(CtlTreeView treeview, bool useDrop)
        {
            if (useDrop)
            {
                treeview.EvDrop -= this.OnEvDrop;
                treeview.EvDrop += this.OnEvDrop; ;
            }
            treeview.tvMain.MouseDoubleClick -= this.OnTreeviewDoubleClick;
            treeview.tvMain.MouseDoubleClick += this.OnTreeviewDoubleClick;
            treeview.tvMain.MouseLeftButtonUp -= this.OnMouseLeftButtonUp;
            treeview.tvMain.MouseLeftButtonUp += this.OnMouseLeftButtonUp;
            treeview.tvMain.ContextMenu = this.MenuShortcut;
        }

        private void OnEvDrop(object sender, CommonEventArgs<TreeViewItem> e)
        {
            this.LoadDatas(sender as CtlTreeView, e.Data.Tag as string);
        }

        public ContextMenu MenuShortcut
        {
            get
            {
                if (this._menuShortcut == null)
                {
                    this._menuShortcut = (ContextMenu)base.FindResource("menuShortcut");
                }
                return this._menuShortcut;
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - this._doubleClickTime).TotalMilliseconds > 500.0)
            {
                this._isDoubleClick = false;
                this._doubleClickTime = DateTime.Now;
                TreeView tv = sender as TreeView;
                CtlTreeView ctv = tv.xFindAncestor<CtlTreeView>();
                bool isCatalog;
                if (ctv.IsPointInsideItem(false, out isCatalog))
                {
                    if (!isCatalog)
                    {
                        DelayCaller.CallAfterDelayInUIThread(() =>
                        {
                            var shortcut = ctv.ReadNodeFromSelectedItem() as ShortcutEntity;
                            this.SetOrSendShortcut(shortcut, false);
                        }, 80);
                    }
                }

            }
        }

        private void OnTreeviewDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this._isDoubleClick = true;
            this._doubleClickTime = DateTime.Now;
            TreeView tv = sender as TreeView;
            CtlTreeView ctv = tv.xFindAncestor<CtlTreeView>();
            bool isCatalog;
            if (ctv.IsPointInsideItem(false, out isCatalog))
            {
                if (!isCatalog)
                {
                    ShortcutEntity shortcut = ctv.ReadNodeFromSelectedItem() as ShortcutEntity;
                    this.SetOrSendShortcut(shortcut, true);
                    e.Handled = true;
                }
                //else
                //{
                //    Util.Assert(false, "节点为空", "OnTreeviewDoubleClick");
                //}
            }
        }

        private void SetOrSendShortcut(ShortcutEntity shortcut, bool isSend)
        {
            if (shortcut != null)
            {
                shortcut.SetOrSendShortcutAsync(this._desk, isSend, true);
            }
        }

        private void InitTabControl(string tabRootNodeId = null, string subTabRootNodeId = null, string targetNodeId = null)
        {
            try
            {
                this.tabMain.Items.Clear();
                if (this.IsNotSelfOnly())
                {
                    this.InitPubTvController();
                }
                if (this.IsNotShopOnly())
                {
                    this.InitPrvTvController();
                }
                this.InitTabData(tabRootNodeId, subTabRootNodeId, targetNodeId);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                MsgBox.ShowErrTip(ex.Message,null);
            }
        }

        private void InitTabData(string rootId, string subRootId, string targetNodeId)
        {
            if (string.IsNullOrEmpty(rootId)) return;

            TabItem mainTabItem = this.FindCatalogTabItemById(this.tabMain, rootId);
            if (mainTabItem == null) return;
            if (!string.IsNullOrEmpty(subRootId))
            {
                TabControl tabControl = mainTabItem.Content as TabControl;
                if (tabControl != null)
                {
                    TabItem tabItem = this.FindCatalogTabItemById(tabControl, subRootId);
                    if (tabItem != null)
                    {
                        if (targetNodeId != null)
                        {
                            CtlTreeView ctv = tabItem.Content as CtlTreeView;
                            if (ctv != null)
                            {
                                ctv.ReloadTreeViewData(subRootId);
                            }
                            else
                            {
                                this.ReloadTabItemData(tabItem, targetNodeId);
                            }
                        }
                        tabControl.SelectedItem = tabItem;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(targetNodeId))
            {
                var ctv = mainTabItem.Content as CtlTreeView;
                if (ctv != null)
                {
                    ctv.ReloadTreeViewData(subRootId);
                }
                else
                {
                    ReloadTabItemData(mainTabItem, targetNodeId);
                }
            }
            this.tabMain.SelectedItem = mainTabItem;
        }

        private bool IsNotShopOnly()
        {
            return Params.Shortcut.GetShowType(this._seller) != Params.Shortcut.ShowType.ShopOnly;
        }

        private bool IsNotSelfOnly()
        {
            return Params.Shortcut.GetShowType(this._seller) != Params.Shortcut.ShowType.SelfOnly;
        }

        private void InitPrvTvController()
        {
            if (this._prvTvController == null)
            {
                this._prvTvController = new ShortcutTreeviewController(AccountHelper.GetPrvDbAccount(this._seller), this._seller);
                this.tvPrvSearch.Init(this._prvTvController, null, null, false, false, null, null, null);
            }
            this.LoadTvControllerData(this._prvTvController, false);
        }

        private void InitPubTvController()
        {
            if (this._pubTvController == null)
            {
                this._pubTvController = new ShortcutTreeviewController(AccountHelper.GetPubDbAccount(this._seller), this._seller);
                this.tvPubSearch.Init(this._pubTvController, null, null, false, false, null, null, null);
            }
            this.LoadTvControllerData(this._pubTvController, true);
        }

        private void LoadTvControllerData(ShortcutTreeviewController tvController, bool isShopShortcut)
        {
            List<TreeNode> cNodes = tvController.DbAccessor.ReadAllCataNodes(tvController.DbAccessor.Root.EntityId);
            foreach (var node in cNodes)
            {
                var cataEt = node as ShortcutCatalogEntity;
                var tabItem = this.CreateTabItem(this.tabMain, cataEt.Name, cataEt, true, isShopShortcut);
                var subCataNodes = tvController.DbAccessor.ReadAllCataNodes(cataEt.EntityId);
                if (subCataNodes.xCount() > 0)
                {
                    var tabControl = new TabControl();
                    this.CreateTabItem(tabControl, cataEt.Name + "*", cataEt, false, isShopShortcut);
                    foreach (var catNode in subCataNodes)
                    {
                        var subCataEt = catNode as ShortcutCatalogEntity;
                        this.CreateTabItem(tabControl, subCataEt.Name, subCataEt, false, isShopShortcut);
                    }
                    tabControl.SelectionChanged += this.tabMain_SelectionChanged;
                    tabItem.Content = tabControl;
                }
            }
            this.tabMain.SelectionChanged -= this.tabMain_SelectionChanged;
            this.tabMain.SelectionChanged += this.tabMain_SelectionChanged;
            this.CreateTabItem(this.tabMain, this.ShortcutTitle(isShopShortcut), tvController.DbAccessor.Root as ShortcutCatalogEntity, true, isShopShortcut);
        }

        private string ShortcutTitle(bool isShopShortcut)
        {
            return isShopShortcut ? "所有公用短语" : "所有私人短语";
        }

        private TabItem CreateTabItem(TabControl tabControl, string header, ShortcutCatalogEntity cataEt, bool level1Style, bool isShopShortcut)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = header;
            tabItem.Tag = new TabItemTag(cataEt, isShopShortcut);
            tabItem.xHoverSelect();
            tabItem.Style = this.GetTabStyle(level1Style, isShopShortcut);
            tabControl.Items.Add(tabItem);
            return tabItem;
        }

        private Style GetTabStyle(bool level1Style, bool isShopShortcut)
        {
            Style s;
            if (isShopShortcut)
            {
                if (level1Style)
                {
                    s = (Style)base.FindResource("tabPubLevel1");
                }
                else
                {
                    s = (Style)base.FindResource("tabPubLevel2");
                }
            }
            else if (level1Style)
            {
                s = (Style)base.FindResource("tabPrvLevel1");
            }
            else
            {
                s = (Style)base.FindResource("tabPrvLevel2");
            }
            return s;
        }

        private TabItem FindCatalogTabItemById(TabControl tab, string cateId)
        {
            TabItem tabItem = null;
            foreach (TabItem item in tab.Items)
            {
                TabItemTag tabItemTag = item.Tag as TabItemTag;
                ShortcutCatalogEntity catEntity = tabItemTag.CatEntity;
                if (catEntity.EntityId == cateId)
                {
                    tabItem = item;
                    break;
                }
            }
            return tabItem;
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == e.OriginalSource)
            {
                var tabControl = sender as TabControl;
                if (tabControl != null && tabControl.Items != null && tabControl.Items.Count != 0 && tabControl.SelectedIndex >= 0)
                {
                    var tabItem = tabControl.Items[tabControl.SelectedIndex] as TabItem;
                    if (tabItem.Content == null)
                    {
                        this.ReloadTabItemData(tabItem, null);
                    }
                }
            }
        }

        private void ReloadTabItemData(TabItem tabItem, string targetNodeId = null)
        {
            TabItemTag tag = this.GetTag(tabItem);
            var catEt = tag.CatEntity;
            if (catEt != null)
            {
                var ctv = new CtlTreeView();
                ctv.CollapseSearch();
                if (tag.IsShopShortcut)
                {
                    TreeNode showFrom = catEt;
                    var entityId = catEt.EntityId;
                    ctv.Init(_pubTvController, null, null, false, entityId == ((_pubTvController != null) ? _pubTvController.DbAccessor.Root.EntityId : null), targetNodeId, showFrom, this.HasAuth);
                }
                else
                {
                    var showFrom = catEt;
                    var entityId = catEt.EntityId;
                    ctv.Init(_prvTvController, null, null, false, entityId == ((_prvTvController != null) ? _prvTvController.DbAccessor.Root.EntityId : null), targetNodeId, showFrom, this.HasAuth);
                }
                this.BindEvent(ctv, true);
                this.SetTooltip(ctv);
                tabItem.Content = ctv;
            }
        }

        private void SetTooltip(CtlTreeView ctv)
        {
            ctv.tvMain.xTraverse((it) =>
            {
                var ctlLeaf = it.Header as CtlLeaf;
                if (ctlLeaf == null) return;
                var shortcut = ctv.ReadNode(it) as ShortcutEntity;
                var text = shortcut.Text.xLimitCharCountPerLine(70);
                it.ToolTip = text;
                if (text.Length > 50)
                {
                    it.SetValue(ToolTipService.ShowDurationProperty, text);
                }
            });
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            this.Create();
        }

        private void Create()
        {
            var ctv = this.GetShowingTreeview(null);
            if (this.CanEdit(ctv))
            {
                ctv.Create(null, (node) =>
                {
                    this.LoadDatas(ctv, node.EntityId);
                });
            }
        }

        private bool CanEdit(CtlTreeView ctv)
        {
            return this.IsInPrvTabItem(ctv);
        }

        private bool IsInPrvTabItem(CtlTreeView ctv)
        {
            bool result;
            if (this.grdSearch.IsVisible)
            {
                result = (this._prvSearchMouseDownTime > this._pubSearchMouseDownTime);
            }
            else
            {
                TabItemTag tag = this.FindRootTabItemTag(ctv);
                result = !tag.IsShopShortcut;
            }
            return result;
        }

        private TabItemTag FindRootTabItemTag(CtlTreeView ctv)
        {
            TabItemTag tag = null;
            if (ctv != null)
            {
                tag = (ctv.xFindAncestor<TabItem>().Tag as TabItemTag);
            }
            return tag;
        }

        private TabItemTag GetTag(TabItem tabItem)
        {
            return tabItem.Tag as TabItemTag;
        }

        private void ClearTreeViewExcept(CtlTreeView tv)
        {
            TabItem tabItem = tv.xFindAncestor<TabItem>();
            TabControl tabControl = tabItem.xFindAncestor<TabControl>();
            if (tabControl != null)
            {
                Util.Assert(tabControl.SelectedItem == tabItem);
                this.ClearTreeViewExcept(tabControl, tabItem);
            }
            if (tabControl != this.tabMain)
            {
                TabItem tabItem_ = this.tabMain.SelectedItem as TabItem;
                this.ClearTreeViewExcept(this.tabMain, tabItem_);
            }
            this._needInitTvSearch = true;
        }

        private void ClearTreeViewExcept(TabControl tabControl, TabItem exceptTabItem)
        {
            if (tabControl != null && tabControl.Items != null)
            {
                foreach (TabItem tabItem in tabControl.Items)
                {
                    if (tabItem != exceptTabItem)
                    {
                        tabItem.Content = null;
                    }
                }
            }
        }

        private CtlTreeView GetShowingTreeview(TabControl tc = null)
        {
            CtlTreeView ctv = null;
            try
            {
                if (this.grdSearch.IsVisible)
                {
                    ctv = this.GetShowingSearchTreeview();
                }
                else
                {
                    tc = (tc ?? this.tabMain);
                    TabItem tabItem = tc.SelectedItem as TabItem;
                    if (tabItem != null)
                    {
                        if (tabItem.Content is TabControl)
                        {
                            ctv = this.GetShowingTreeview(tabItem.Content as TabControl);
                        }
                        else
                        {
                            Util.Assert(tabItem.Content is CtlTreeView);
                            ctv = (tabItem.Content as CtlTreeView);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return ctv;
        }

        private CtlTreeView GetShowingSearchTreeview()
        {
            Params.Shortcut.ShowType showType = Params.Shortcut.GetShowType(this._seller);
            CtlTreeView ctv = null;
            if (showType != Params.Shortcut.ShowType.ShopOnly)
            {
                if (showType != Params.Shortcut.ShowType.SelfOnly)
                {
                    ctv = ((this._pubSearchMouseDownTime > this._prvSearchMouseDownTime) ? this.tvPubSearch : this.tvPrvSearch);
                }
                else
                {
                    ctv = this.tvPrvSearch;
                }
            }
            else
            {
                ctv = this.tvPubSearch;
            }
            return ctv;
        }

        public void LoadDatas(CtlTreeView ctv = null, string targetNodeId = null)
        {
            if (this.grdSearch.IsVisible)
            {
                this.InitTvController();
                this.InitTabControl(null, null, null);
            }
            else
            {
                string parentId = null;
                string rootId = this.GetCtvRoot(ctv, targetNodeId, out parentId);
                this.InitTabControl(rootId, parentId, targetNodeId);
            }
        }

        private string GetCtvRoot(CtlTreeView ctv, string targetNodeId, out string parentId)
        {
            string rootId = null;
            parentId = null;
            ctv = (ctv ?? this.GetShowingTreeview(null));
            if (ctv != null && ctv != this.tvPrvSearch && ctv != this.tvPubSearch)
            {
                var rootTag = this.FindRootTabItemTag(ctv);
                if (rootTag != null)
                {
                    rootId = this.FindParentNode(this.GetShortcutTreeviewController(rootTag), rootTag.CatEntity, out parentId);
                }
            }
            return rootId;
        }

        private ShortcutTreeviewController GetShortcutTreeviewController(TabItemTag tag)
        {
            return tag.IsShopShortcut ? this._pubTvController : this._prvTvController;
        }

        private string FindParentNode(ShortcutTreeviewController tvController, TreeNode n, out string parentId)
        {
            var selfId = string.Empty;
            parentId = null;
            var ancestorList = tvController.DbAccessor.ReadAncestorList(n, true, false);
            if (ancestorList.Count > 0)
            {
                selfId = ancestorList[0].EntityId;
                if (ancestorList.Count > 1)
                {
                    parentId = ancestorList[1].EntityId;
                }
                else if (tvController.DbAccessor.ReadChildCatalogById(selfId).xCount() > 0)
                {
                    parentId = selfId;
                }
            }
            else
            {
                bool isRoot = false;
                if (n.EntityId != ((_prvTvController != null) ? _prvTvController.DbAccessor.Root.EntityId : null))
                {
                    isRoot = (n.EntityId == ((_pubTvController != null) ? _pubTvController.DbAccessor.Root.EntityId : null));
                }
                else
                {
                    isRoot = true;
                }
                if (isRoot)
                {
                    selfId = n.EntityId;
                }
            }
            return selfId;
        }

        private void btnCreateCat_Click(object sender, RoutedEventArgs e)
        {
            this.CreateCat();
        }

        private void CreateCat()
        {
            var ctv = this.GetShowingTreeview(null);
            if (this.CanEdit(ctv))
            {
                ctv.CreateCatalog(null, (node) =>
                {
                    this.LoadDatas(ctv, node.EntityId);
                });
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Edit();
        }

        private void Edit()
        {
            var ctv = this.GetShowingTreeview(null);
            if (this.CanEdit(ctv))
            {
                ctv.EditAsync((node) =>
                {
                    LoadDatas(ctv, node.EntityId);
                });
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            this.Delete();
        }

        private void Delete()
        {
            CtlTreeView ctv = this.GetShowingTreeview(null);
            if (this.CanEdit(ctv))
            {
                var tn = ctv.Delete();
                if (tn != null)
                {
                    this.LoadDatas(ctv, null);
                    if (!ctv.IsCatalogType(tn))
                    {
                        ShortcutEntity shortcutEntity = tn as ShortcutEntity;
                        if (shortcutEntity != null && !string.IsNullOrEmpty(shortcutEntity.ImageName))
                        {
                            ShortcutImageHelper.DeleteImage(shortcutEntity.ImageName);
                        }
                    }
                }
            }
        }

        private void Help()
        {
        }

        private void tbxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = this.tbxSearch.Text.Trim();
            this.ShowSearchGrid(text == "");
            if (text != "")
            {
                if (this._needInitTvSearch)
                {
                    this._needInitTvSearch = false;
                    this.InitTvController();
                }
                if (this.IsNotSelfOnly())
                {
                    this.tvPubSearch.tbxSearch.Text = text;
                }
                if (this.IsNotShopOnly())
                {
                    this.tvPrvSearch.tbxSearch.Text = text;
                }
            }
        }

        public void InitTvController()
        {
            if (this._pubTvController != null)
            {
                this.tvPubSearch.Init(this._pubTvController, null, null, false, false, null, null, null);
            }
            if (this._prvTvController != null)
            {
                this.tvPrvSearch.Init(this._prvTvController, null, null, false, false, null, null, null);
            }
        }

        private bool HasAuth()
        {
            return true; //QnHelper.Auth.HasAuth(this._seller);
        }

        private void ShowSearchGrid(bool showSearchGrd)
        {
            if (showSearchGrd)
            {
                this.tabMain.Visibility = Visibility.Visible;
                this.grdSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.tabMain.Visibility = Visibility.Collapsed;
                this.grdSearch.Visibility = Visibility.Visible;
            }
        }

        private void OnClearCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.tbxSearch.Text = "";
        }

        private void mCreate_Click(object sender, RoutedEventArgs e)
        {
            this.Create();
        }

        private void OnInsertShortcutToInputbox(object sender, RoutedEventArgs e)
        {
            ShortcutEntity shortcut = this.GetSelectedShortcut();
            if (shortcut != null)
            {
                this.SetOrSendShortcut(shortcut, true);
            }
        }

        private void OnSendShortcut(object sender, RoutedEventArgs e)
        {
            ShortcutEntity shortcut = this.GetSelectedShortcut();
            if (shortcut != null)
            {
                this.SetOrSendShortcut(shortcut, false);
            }
        }

        private ShortcutEntity GetSelectedShortcut()
        {
            ShortcutEntity sc = null;
            CtlTreeView showingTreeview = this.GetShowingTreeview(null);
            TreeNode n = showingTreeview.ReadNodeFromSelectedItem();
            if (n is ShortcutEntity)
            {
                sc = (n as ShortcutEntity);
            }
            return sc;
        }

        private TreeNode ReadSelectedNode()
        {
            CtlTreeView showingTreeview = this.GetShowingTreeview(null);
            return (showingTreeview != null) ? showingTreeview.ReadNodeFromSelectedItem() : null;
        }

        private string GetSelectedNodeText()
        {
            string rtText = null;
            TreeNode n = this.ReadSelectedNode();
            if (n is ShortcutEntity)
            {
                ShortcutEntity shortcutEntity = n as ShortcutEntity;
                rtText = shortcutEntity.Text;
            }
            else if (n is ShortcutCatalogEntity)
            {
                ShortcutCatalogEntity shortcutCatalogEntity = n as ShortcutCatalogEntity;
                rtText = shortcutCatalogEntity.Name;
            }
            return rtText;
        }

        private void mCopy_Click(object sender, RoutedEventArgs e)
        {
            string text = this.GetSelectedNodeText();
            if (!string.IsNullOrEmpty(text))
            {
                ClipboardEx.SetTextSafe(text);
            }
        }

        private void mHelp_Click(object sender, RoutedEventArgs e)
        {
            this.Help();
        }

        private void mCreateCata_Click(object sender, RoutedEventArgs e)
        {
            this.CreateCat();
        }

        private void mDelete_Click(object sender, RoutedEventArgs e)
        {
            this.Delete();
        }

        private void mEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Edit();
        }

        private void mImport_Click(object sender, RoutedEventArgs e)
        {
            //WndShortcutImporter.MyShow(this._seller, this.xFindParentWindow(), ()=>{
            //    this.ShowTip("正在导入分类短语");
            //}, ()=>{
            //    DispatcherEx.xInvoke(()=>{
            //    this.LoadDatas(null, null);
            //    this.HideTip();
            //    };
            //});
        }

        private void ShowTip(string msg)
        {
            this.tboxTip.Text = msg;
            this.spTip.Visibility = Visibility.Visible;
            this.grdMain.IsEnabled = false;
        }

        private void HideTip()
        {
            this.spTip.Visibility = Visibility.Collapsed;
            this.grdMain.IsEnabled = true;
        }

        private bool IsTipVisible()
        {
            return this.spTip.Visibility == Visibility.Visible;
        }

        private void mExport_Click(object sender, RoutedEventArgs e)
        {
            string mainPart = TbNickHelper.GetMainPart(this._seller);
            ShortcutTreeviewController pubTvController = this._pubTvController;
            //TreeDbAccessor pubDbAccessor = (pubTvController != null) ? pubTvController.DbAccessor : null;
            //ShortcutTreeviewController prvTvController = this._prvTvController;
            //ExporterV2.Export(mainPart, pubDbAccessor, (prvTvController != null) ? prvTvController.DbAccessor : null);
        }

        private void OnOpenContextMenu(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.ContextMenu.IsOpen = true;
        }

        public void ShowTitleButtons()
        {
            ContextMenu contextMenu = FindResource("menuShortcut") as ContextMenu;
            if (contextMenu != null)
            {
                bool isShowTitleButtons = Params.Shortcut.GetIsShowTitleButtons(this._desk.Seller);
                contextMenu.xSetMenuItemVisibilityByTag("ShowTitle", !isShowTitleButtons);
                contextMenu.xSetMenuItemVisibilityByTag("HideTitle", isShowTitleButtons);
            }
        }

        private void mHideTitle_Click(object sender, RoutedEventArgs e)
        {
            //MsgBox.ShowTip("确定要隐藏第一行的按钮？", isYesButtonClicked =>
            //{
            //    if (isYesButtonClicked)
            //    {
            //        string message = "右击任意的分类短语，弹出的菜单中，可以恢复显示这些按钮！";
            //        MsgBox.ShowTip(message,null,null,null);
            //        this.grdTitleButtons.xIsVisible(false);
            //        Params.Shortcut.SetIsShowTitleButtons(this._desk.Seller, false);
            //        this.ShowTitleButtons();
            //    }
            //}, null, null);
        }

        private void mShowTitle_Click(object sender, RoutedEventArgs e)
        {
            this.grdTitleButtons.xIsVisible(true);
            Params.Shortcut.SetIsShowTitleButtons(this._desk.Seller, true);
            this.ShowTitleButtons();
        }

        private void btnClearSearchText_Click(object sender, RoutedEventArgs e)
        {
            this.tbxSearch.Text = "";
        }

        private void tvPrvSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this._prvSearchMouseDownTime = DateTime.Now;
        }

        private void tvPubSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this._pubSearchMouseDownTime = DateTime.Now;
        }

        private void mSetting_Click(object sender, RoutedEventArgs e)
        {
            //WndOption.MyShow(this._seller, null, OptionEnum.Shortcut, null);
        }

        private void mPasteNode_Click(object sender, RoutedEventArgs e)
        {
            CtlTreeView showingTreeview = this.GetShowingTreeview(null);
            if (this.CanEdit(showingTreeview))
            {
                if (this._nodeToBeCopy == null)
                {
                    MsgBox.ShowErrTip("请先复制节点");
                }
                else
                {
                    bool isPub = this.IsPub(this._nodeToBeCopy);
                    var targetNode = this.ReadSelectedNode() ?? this.GetTabItemCatalog();
                    if (targetNode == null)
                    {
                        Util.Beep();
                    }
                    else if (isPub == this.IsPub(targetNode))
                    {
                        MsgBox.ShowErrTip(string.Format("只能将【{0}】节点，粘贴成【{1}】短语。请先复制【{0}】节点", isPub ? "店铺公用" : "个人私用", isPub ? "个人私用" : "店铺公用"));
                    }
                    else
                    {
                        this.PasteNode(targetNode);
                        this._nodeToBeCopy = null;
                    }
                }
            }
        }

        private TreeNode GetTabItemCatalog()
        {
            CtlTreeView showingTreeview = this.GetShowingTreeview(null);
            return this.FindRootTabItemTag(showingTreeview).CatEntity;
        }

        private void PasteNode(TreeNode targetNode)
        {
            TreeDbAccessor toDbAccessor = this.GetDbAccessor(targetNode);
            TreeDbAccessor fromDbAccessor = this.GetDbAccessor(this._nodeToBeCopy);
            TreeNode n;
            if (this.IsCatalog(targetNode))
            {
                n = this.PasteCataNode(targetNode, this._nodeToBeCopy, toDbAccessor, fromDbAccessor);
            }
            else
            {
                n = this.PasteShortcutNode(targetNode, toDbAccessor, fromDbAccessor);
            }
            this.LoadDatas(null, n.EntityId);
        }

        private TreeNode PasteShortcutNode(TreeNode targetNode, TreeDbAccessor toDbAccessor, TreeDbAccessor fromDbAccessor)
        {
            TreeNode newNode = this.CreateNode(this._nodeToBeCopy, toDbAccessor.DbAccount);
            toDbAccessor.AddNext(newNode, targetNode);
            if (this.IsCatalog(newNode))
            {
                this.PasteChildNodes(newNode, fromDbAccessor.ReadChildNode(this._nodeToBeCopy.EntityId, true), toDbAccessor, fromDbAccessor);
            }
            return newNode;
        }

        private void PasteChildNodes(TreeNode cat, List<TreeNode> childNodes, TreeDbAccessor toDbAccessor, TreeDbAccessor fromDbAccessor)
        {
            foreach (TreeNode node in childNodes)
            {
                this.PasteCataNode(cat, node, toDbAccessor, fromDbAccessor);
            }
        }

        private TreeNode CreateNode(TreeNode n, string dbAccount)
        {
            TreeNode toNode;
            if (n is ShortcutCatalogEntity)
            {
                var cat = n as ShortcutCatalogEntity;
                toNode = this.CreateShortcutCatalog(cat, dbAccount);
            }
            else
            {
                var shortcut = n as ShortcutEntity;
                toNode = this.CreateShortcut(shortcut, dbAccount);
            }
            return toNode;
        }

        private TreeNode CreateShortcut(ShortcutEntity fromShortcut, string dbAccount)
        {
            ShortcutEntity shortcutEntity = EntityHelper.Create<ShortcutEntity>(dbAccount);
            shortcutEntity.Text = fromShortcut.Text;
            shortcutEntity.ImageName = ShortcutImageHelper.AddNewImage(fromShortcut.ImageName);
            shortcutEntity.Code = fromShortcut.Code;
            return shortcutEntity;
        }

        private TreeNode CreateShortcutCatalog(ShortcutCatalogEntity fromCat, string dbAccount)
        {
            var shortcutCatalog = EntityHelper.Create<ShortcutCatalogEntity>(dbAccount);
            shortcutCatalog.Name = fromCat.Name;
            return shortcutCatalog;
        }

        private TreeNode PasteCataNode(TreeNode toNode, TreeNode fromNode, TreeDbAccessor toDbAccessor, TreeDbAccessor fromDbAccessor)
        {
            TreeNode pasteNode = this.CreateNode(fromNode, toDbAccessor.DbAccount);
            toDbAccessor.AddNodeToTargetNode(pasteNode, toNode.EntityId);
            if (this.IsCatalog(pasteNode))
            {
                this.PasteChildNodes(pasteNode, fromDbAccessor.ReadChildNode(fromNode.EntityId, true), toDbAccessor, fromDbAccessor);
            }
            return pasteNode;
        }

        private bool IsCatalog(TreeNode n)
        {
            return n is ShortcutCatalogEntity;
        }

        private TreeDbAccessor GetDbAccessor(TreeNode n)
        {
            TreeDbAccessor dbAccessor = null;
            if (n.DbAccount == ((_pubTvController != null) ? _pubTvController.DbAccount : null))
            {
                dbAccessor = ((_pubTvController != null) ? _pubTvController.DbAccessor : null);
            }
            else
            {
                dbAccessor = ((_prvTvController != null) ? _prvTvController.DbAccessor : null);
            }
            return dbAccessor;
        }

        private bool IsPub(TreeNode n)
        {
            return n.DbAccount == this._pubTvController.DbAccount;
        }

        private void mCopyNode_Click(object sender, RoutedEventArgs e)
        {
            this._nodeToBeCopy = this.ReadSelectedNode();
        }

    }
}
