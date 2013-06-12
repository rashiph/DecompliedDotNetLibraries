namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.TreeViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlValueProperty("SelectedValue"), DefaultEvent("SelectedNodeChanged"), SupportsEventValidation]
    public class TreeView : HierarchicalDataBoundControl, IPostBackEventHandler, IPostBackDataHandler, ICallbackEventHandler
    {
        private bool _accessKeyRendered;
        private Style _baseNodeStyle;
        private TreeNodeBindingCollection _bindings;
        private string _cachedClientDataObjectID;
        private string _cachedCollapseImageUrl;
        private string _cachedExpandImageUrl;
        private string _cachedExpandStateID;
        private string _cachedImageArrayID;
        private List<string> _cachedLeafNodeClassNames;
        private List<string> _cachedLeafNodeHyperLinkClassNames;
        private List<TreeNodeStyle> _cachedLeafNodeStyles;
        private Collection<int> _cachedLevelsContainingCssClass;
        private string _cachedNoExpandImageUrl;
        private List<string> _cachedParentNodeClassNames;
        private List<string> _cachedParentNodeHyperLinkClassNames;
        private List<TreeNodeStyle> _cachedParentNodeStyles;
        private string _cachedPopulateLogID;
        private string _cachedSelectedNodeFieldID;
        private string _callbackEventArgument;
        private ArrayList _checkedChangedNodes;
        private TreeNodeCollection _checkedNodes;
        private int _cssStyleIndex;
        private string _currentSiteMapNodeDataPath;
        private bool _dataBound;
        private bool _fireSelectedNodeChanged;
        private HyperLinkStyle _hoverNodeHyperLinkStyle;
        private Style _hoverNodeStyle;
        private string[] _imageUrls;
        private bool _isNotIE;
        private TreeNodeStyle _leafNodeStyle;
        private string[] _levelImageUrls;
        private TreeNodeStyleCollection _levelStyles;
        private bool _loadingNodeState;
        private TreeNodeStyle _nodeStyle;
        private TreeNodeStyle _parentNodeStyle;
        private bool _renderClientScript;
        private TreeNode _rootNode;
        private TreeNodeStyle _rootNodeStyle;
        private TreeNode _selectedNode;
        private TreeNodeStyle _selectedNodeStyle;
        private static readonly object CheckChangedEvent = new object();
        internal const int DashImageIndex = 0x10;
        internal const int DashMinusImageIndex = 0x12;
        internal const int DashPlusImageIndex = 0x11;
        private const char EscapeCharacter = '|';
        private const string EscapeSequenceForEscapeCharacter = "||";
        private const string EscapeSequenceForPathSeparator = "*|*";
        internal const int IImageIndex = 6;
        internal const int ImageUrlsCount = 0x13;
        internal const char InternalPathSeparator = '\\';
        internal const int LeafImageIndex = 2;
        internal const int LImageIndex = 13;
        internal const int LMinusImageIndex = 15;
        internal const int LPlusImageIndex = 14;
        internal const int MinusImageIndex = 5;
        internal const int NoExpandImageIndex = 3;
        internal const int ParentImageIndex = 1;
        internal const int PlusImageIndex = 4;
        private static string populateNodeScript = "\r\n    function TreeView_PopulateNodeDoCallBack(context,param) {\r\n        ";
        private static string populateNodeScriptEnd = ";\r\n    }\r\n";
        internal const int RImageIndex = 7;
        internal const int RMinusImageIndex = 9;
        internal const int RootImageIndex = 0;
        internal const int RPlusImageIndex = 8;
        private static readonly object SelectedNodeChangedEvent = new object();
        internal const int TImageIndex = 10;
        internal const int TMinusImageIndex = 12;
        internal const int TPlusImageIndex = 11;
        private static readonly object TreeNodeCollapsedEvent = new object();
        private static readonly object TreeNodeDataBoundEvent = new object();
        private static readonly object TreeNodeExpandedEvent = new object();
        private static readonly object TreeNodePopulateEvent = new object();

        [WebCategory("Behavior"), WebSysDescription("TreeView_SelectedNodeChanged")]
        public event EventHandler SelectedNodeChanged
        {
            add
            {
                base.Events.AddHandler(SelectedNodeChangedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(SelectedNodeChangedEvent, value);
            }
        }

        [WebSysDescription("TreeView_CheckChanged"), WebCategory("Behavior")]
        public event TreeNodeEventHandler TreeNodeCheckChanged
        {
            add
            {
                base.Events.AddHandler(CheckChangedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(CheckChangedEvent, value);
            }
        }

        [WebSysDescription("TreeView_TreeNodeCollapsed"), WebCategory("Behavior")]
        public event TreeNodeEventHandler TreeNodeCollapsed
        {
            add
            {
                base.Events.AddHandler(TreeNodeCollapsedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(TreeNodeCollapsedEvent, value);
            }
        }

        [WebSysDescription("TreeView_TreeNodeDataBound"), WebCategory("Behavior")]
        public event TreeNodeEventHandler TreeNodeDataBound
        {
            add
            {
                base.Events.AddHandler(TreeNodeDataBoundEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(TreeNodeDataBoundEvent, value);
            }
        }

        [WebSysDescription("TreeView_TreeNodeExpanded"), WebCategory("Behavior")]
        public event TreeNodeEventHandler TreeNodeExpanded
        {
            add
            {
                base.Events.AddHandler(TreeNodeExpandedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(TreeNodeExpandedEvent, value);
            }
        }

        [WebSysDescription("TreeView_TreeNodePopulate"), WebCategory("Behavior")]
        public event TreeNodeEventHandler TreeNodePopulate
        {
            add
            {
                base.Events.AddHandler(TreeNodePopulateEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(TreeNodePopulateEvent, value);
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            string accessKey = this.AccessKey;
            if (!string.IsNullOrEmpty(accessKey))
            {
                this.AccessKey = string.Empty;
                base.AddAttributesToRender(writer);
                this.AccessKey = accessKey;
            }
            else
            {
                base.AddAttributesToRender(writer);
            }
        }

        private static bool AppendCssClassName(StringBuilder builder, TreeNodeStyle style, bool hyperlink)
        {
            bool flag = false;
            if (style != null)
            {
                if (style.CssClass.Length != 0)
                {
                    builder.Append(style.CssClass);
                    builder.Append(' ');
                    flag = true;
                }
                string str = hyperlink ? style.HyperLinkStyle.RegisteredCssClass : style.RegisteredCssClass;
                if (str.Length > 0)
                {
                    builder.Append(str);
                    builder.Append(' ');
                }
            }
            return flag;
        }

        private static T CacheGetItem<T>(List<T> cacheList, int index) where T: class
        {
            if (index < cacheList.Count)
            {
                return cacheList[index];
            }
            return default(T);
        }

        private static void CacheSetItem<T>(List<T> cacheList, int index, T item) where T: class
        {
            if (cacheList.Count > index)
            {
                cacheList[index] = item;
            }
            else
            {
                for (int i = cacheList.Count; i < index; i++)
                {
                    T local = default(T);
                    cacheList.Add(local);
                }
                cacheList.Add(item);
            }
        }

        public void CollapseAll()
        {
            foreach (TreeNode node in this.Nodes)
            {
                node.CollapseAll();
            }
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        protected internal virtual TreeNode CreateNode()
        {
            return new TreeNode(this, false);
        }

        internal string CreateNodeId(int index)
        {
            return (this.ClientID + "n" + index);
        }

        internal string CreateNodeTextId(int index)
        {
            return (this.ClientID + "t" + index);
        }

        public sealed override void DataBind()
        {
            base.DataBind();
        }

        private void DataBindNode(TreeNode node)
        {
            if ((node.PopulateOnDemand && !base.IsBoundUsingDataSourceID) && !base.DesignMode)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_PopulateOnlyForDataSourceControls", new object[] { this.ID }));
            }
            HierarchicalDataSourceView view = this.GetData(node.DataPath);
            if (base.IsBoundUsingDataSourceID || (this.DataSource != null))
            {
                if (view == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("TreeView_DataSourceReturnedNullView", new object[] { this.ID }));
                }
                IHierarchicalEnumerable enumerable = view.Select();
                node.ChildNodes.Clear();
                if (enumerable != null)
                {
                    if (base.IsBoundUsingDataSourceID)
                    {
                        SiteMapDataSource dataSource = this.GetDataSource() as SiteMapDataSource;
                        if ((dataSource != null) && (this._currentSiteMapNodeDataPath == null))
                        {
                            IHierarchyData currentNode = dataSource.Provider.CurrentNode;
                            if (currentNode != null)
                            {
                                this._currentSiteMapNodeDataPath = currentNode.Path;
                            }
                            else
                            {
                                this._currentSiteMapNodeDataPath = string.Empty;
                            }
                        }
                    }
                    this.DataBindRecursive(node, enumerable, true);
                }
            }
        }

        private void DataBindRecursive(TreeNode node, IHierarchicalEnumerable enumerable, bool ignorePopulateOnDemand)
        {
            int depth = node.Depth + 1;
            if ((this.MaxDataBindDepth == -1) || (depth <= this.MaxDataBindDepth))
            {
                foreach (object obj2 in enumerable)
                {
                    IHierarchyData hierarchyData = enumerable.GetHierarchyData(obj2);
                    string text = null;
                    string str2 = null;
                    string navigateUrl = string.Empty;
                    string imageUrl = string.Empty;
                    string target = string.Empty;
                    string toolTip = string.Empty;
                    string imageToolTip = string.Empty;
                    TreeNodeSelectAction select = TreeNodeSelectAction.Select;
                    bool? showCheckBox = null;
                    string dataMember = string.Empty;
                    bool populateOnDemand = false;
                    dataMember = hierarchyData.Type;
                    TreeNodeBinding binding = this.DataBindings.GetBinding(dataMember, depth);
                    if (binding != null)
                    {
                        populateOnDemand = binding.PopulateOnDemand;
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj2);
                        string textField = binding.TextField;
                        if (textField.Length > 0)
                        {
                            PropertyDescriptor descriptor = properties.Find(textField, true);
                            if (descriptor == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { textField, "TextField" }));
                            }
                            object obj3 = descriptor.GetValue(obj2);
                            if (obj3 != null)
                            {
                                if (!string.IsNullOrEmpty(binding.FormatString))
                                {
                                    text = string.Format(CultureInfo.CurrentCulture, binding.FormatString, new object[] { obj3 });
                                }
                                else
                                {
                                    text = obj3.ToString();
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(text))
                        {
                            text = binding.Text;
                        }
                        string valueField = binding.ValueField;
                        if (valueField.Length > 0)
                        {
                            PropertyDescriptor descriptor2 = properties.Find(valueField, true);
                            if (descriptor2 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { valueField, "ValueField" }));
                            }
                            object obj4 = descriptor2.GetValue(obj2);
                            if (obj4 != null)
                            {
                                str2 = obj4.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(str2))
                        {
                            str2 = binding.Value;
                        }
                        string imageUrlField = binding.ImageUrlField;
                        if (imageUrlField.Length > 0)
                        {
                            PropertyDescriptor descriptor3 = properties.Find(imageUrlField, true);
                            if (descriptor3 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { imageUrlField, "ImageUrlField" }));
                            }
                            object obj5 = descriptor3.GetValue(obj2);
                            if (obj5 != null)
                            {
                                imageUrl = obj5.ToString();
                            }
                        }
                        if (imageUrl.Length == 0)
                        {
                            imageUrl = binding.ImageUrl;
                        }
                        string navigateUrlField = binding.NavigateUrlField;
                        if (navigateUrlField.Length > 0)
                        {
                            PropertyDescriptor descriptor4 = properties.Find(navigateUrlField, true);
                            if (descriptor4 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { navigateUrlField, "NavigateUrlField" }));
                            }
                            object obj6 = descriptor4.GetValue(obj2);
                            if (obj6 != null)
                            {
                                navigateUrl = obj6.ToString();
                            }
                        }
                        if (navigateUrl.Length == 0)
                        {
                            navigateUrl = binding.NavigateUrl;
                        }
                        string targetField = binding.TargetField;
                        if (targetField.Length > 0)
                        {
                            PropertyDescriptor descriptor5 = properties.Find(targetField, true);
                            if (descriptor5 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { targetField, "TargetField" }));
                            }
                            object obj7 = descriptor5.GetValue(obj2);
                            if (obj7 != null)
                            {
                                target = obj7.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(target))
                        {
                            target = binding.Target;
                        }
                        string toolTipField = binding.ToolTipField;
                        if (toolTipField.Length > 0)
                        {
                            PropertyDescriptor descriptor6 = properties.Find(toolTipField, true);
                            if (descriptor6 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { toolTipField, "ToolTipField" }));
                            }
                            object obj8 = descriptor6.GetValue(obj2);
                            if (obj8 != null)
                            {
                                toolTip = obj8.ToString();
                            }
                        }
                        if (toolTip.Length == 0)
                        {
                            toolTip = binding.ToolTip;
                        }
                        string imageToolTipField = binding.ImageToolTipField;
                        if (imageToolTipField.Length > 0)
                        {
                            PropertyDescriptor descriptor7 = properties.Find(imageToolTipField, true);
                            if (descriptor7 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("TreeView_InvalidDataBinding", new object[] { imageToolTipField, "imageToolTipField" }));
                            }
                            object obj9 = descriptor7.GetValue(obj2);
                            if (obj9 != null)
                            {
                                imageToolTip = obj9.ToString();
                            }
                        }
                        if (imageToolTip.Length == 0)
                        {
                            imageToolTip = binding.ImageToolTip;
                        }
                        select = binding.SelectAction;
                        showCheckBox = binding.ShowCheckBox;
                    }
                    else
                    {
                        if (obj2 is INavigateUIData)
                        {
                            INavigateUIData data2 = (INavigateUIData) obj2;
                            text = data2.Name;
                            str2 = data2.Value;
                            navigateUrl = data2.NavigateUrl;
                            if (string.IsNullOrEmpty(navigateUrl))
                            {
                                select = TreeNodeSelectAction.None;
                            }
                            toolTip = data2.Description;
                        }
                        if (base.IsBoundUsingDataSourceID)
                        {
                            populateOnDemand = this.PopulateNodesFromClient;
                        }
                    }
                    if (this.AutoGenerateDataBindings && (text == null))
                    {
                        text = obj2.ToString();
                    }
                    TreeNode child = null;
                    if ((text != null) || (str2 != null))
                    {
                        child = this.CreateNode();
                        if (!string.IsNullOrEmpty(text))
                        {
                            child.Text = text;
                        }
                        if (!string.IsNullOrEmpty(str2))
                        {
                            child.Value = str2;
                        }
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            child.ImageUrl = imageUrl;
                        }
                        if (!string.IsNullOrEmpty(navigateUrl))
                        {
                            child.NavigateUrl = navigateUrl;
                        }
                        if (!string.IsNullOrEmpty(target))
                        {
                            child.Target = target;
                        }
                    }
                    if (child != null)
                    {
                        if (!string.IsNullOrEmpty(toolTip))
                        {
                            child.ToolTip = toolTip;
                        }
                        if (!string.IsNullOrEmpty(imageToolTip))
                        {
                            child.ImageToolTip = imageToolTip;
                        }
                        if (select != child.SelectAction)
                        {
                            child.SelectAction = select;
                        }
                        if (showCheckBox.HasValue)
                        {
                            child.ShowCheckBox = showCheckBox;
                        }
                        child.SetDataPath(hierarchyData.Path);
                        child.SetDataBound(true);
                        node.ChildNodes.Add(child);
                        if (string.Equals(hierarchyData.Path, this._currentSiteMapNodeDataPath, StringComparison.OrdinalIgnoreCase))
                        {
                            child.Selected = true;
                            if ((this.Page == null) || !this.Page.IsCallback)
                            {
                                for (TreeNode node3 = child.Parent; node3 != null; node3 = node3.Parent)
                                {
                                    if (node3.Expanded != true)
                                    {
                                        node3.Expanded = true;
                                    }
                                }
                            }
                        }
                        child.SetDataItem(hierarchyData.Item);
                        this.OnTreeNodeDataBound(new TreeNodeEventArgs(child));
                        child.SetDataItem(null);
                        if (hierarchyData.HasChildren && ((this.MaxDataBindDepth == -1) || (depth < this.MaxDataBindDepth)))
                        {
                            if (populateOnDemand && !base.DesignMode)
                            {
                                child.PopulateOnDemand = true;
                            }
                            else
                            {
                                IHierarchicalEnumerable children = hierarchyData.GetChildren();
                                if (children != null)
                                {
                                    this.DataBindRecursive(child, children, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EnsureRenderSettings()
        {
            HttpBrowserCapabilities browser = this.Page.Request.Browser;
            this._isNotIE = this.Page.Request.Browser.MSDomVersion.Major < 4;
            this._renderClientScript = this.GetRenderClientScript(browser);
            if (((this._hoverNodeStyle != null) && (this.Page != null)) && (this.Page.Header == null))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("NeedHeader", new object[] { "TreeView.HoverStyle" }));
            }
            if ((this.Page != null) && ((this.Page.SupportsStyleSheets || this.Page.IsCallback) || ((this.Page.ScriptManager != null) && this.Page.ScriptManager.IsInAsyncPostBack)))
            {
                this.RegisterStyle(this.BaseTreeNodeStyle);
                if (this._nodeStyle != null)
                {
                    this._nodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(this._nodeStyle.HyperLinkStyle);
                    this.RegisterStyle(this._nodeStyle);
                }
                if (this._rootNodeStyle != null)
                {
                    this._rootNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(this._rootNodeStyle.HyperLinkStyle);
                    this.RegisterStyle(this._rootNodeStyle);
                }
                if (this._parentNodeStyle != null)
                {
                    this._parentNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(this._parentNodeStyle.HyperLinkStyle);
                    this.RegisterStyle(this._parentNodeStyle);
                }
                if (this._leafNodeStyle != null)
                {
                    this._leafNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(this._leafNodeStyle.HyperLinkStyle);
                    this.RegisterStyle(this._leafNodeStyle);
                }
                foreach (TreeNodeStyle style in this.LevelStyles)
                {
                    style.HyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(style.HyperLinkStyle);
                    this.RegisterStyle(style);
                }
                if (this._selectedNodeStyle != null)
                {
                    this._selectedNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(this._selectedNodeStyle.HyperLinkStyle);
                    this.RegisterStyle(this._selectedNodeStyle);
                }
                if (this._hoverNodeStyle != null)
                {
                    this._hoverNodeHyperLinkStyle = new HyperLinkStyle(this._hoverNodeStyle);
                    this._hoverNodeHyperLinkStyle.DoNotRenderDefaults = true;
                    this.RegisterStyle(this._hoverNodeHyperLinkStyle);
                    this.RegisterStyle(this._hoverNodeStyle);
                }
            }
        }

        internal static string Escape(string value)
        {
            StringBuilder builder = null;
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\\':
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append("*|*");
                        startIndex = i + 1;
                        count = 0;
                        break;

                    case '|':
                        if (builder == null)
                        {
                            builder = new StringBuilder(value.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(value, startIndex, count);
                        }
                        builder.Append("||");
                        startIndex = i + 1;
                        count = 0;
                        break;

                    default:
                        count++;
                        break;
                }
            }
            if (builder == null)
            {
                return value;
            }
            if (count > 0)
            {
                builder.Append(value, startIndex, count);
            }
            return builder.ToString();
        }

        public void ExpandAll()
        {
            foreach (TreeNode node in this.Nodes)
            {
                node.ExpandAll();
            }
        }

        private void ExpandToDepth(TreeNodeCollection nodes, int depth)
        {
            this.ViewState["NeverExpanded"] = null;
            foreach (TreeNode node in nodes)
            {
                if ((depth == -1) || (node.Depth < depth))
                {
                    if (!node.Expanded.HasValue)
                    {
                        node.Expanded = true;
                    }
                    this.ExpandToDepth(node.ChildNodes, depth);
                }
            }
        }

        public TreeNode FindNode(string valuePath)
        {
            if (valuePath == null)
            {
                return null;
            }
            return this.Nodes.FindNode(valuePath.Split(new char[] { this.PathSeparator }), 0);
        }

        protected virtual string GetCallbackResult()
        {
            if (!base.IsEnabled)
            {
                return string.Empty;
            }
            int startIndex = 0;
            int index = this._callbackEventArgument.IndexOf('|');
            string s = this._callbackEventArgument.Substring(startIndex, index);
            int num3 = int.Parse(s, CultureInfo.InvariantCulture);
            startIndex = index + 1;
            index = this._callbackEventArgument.IndexOf('|', startIndex);
            int num4 = int.Parse(this._callbackEventArgument.Substring(startIndex, index - startIndex), CultureInfo.InvariantCulture);
            bool dataBound = this._callbackEventArgument[index + 1] == 't';
            bool flag2 = this._callbackEventArgument[index + 2] == 't';
            startIndex = index + 3;
            index = this._callbackEventArgument.IndexOf('|', startIndex);
            string str2 = this._callbackEventArgument.Substring(startIndex, index - startIndex);
            startIndex = index + 1;
            index = this._callbackEventArgument.IndexOf('|', startIndex);
            int num5 = int.Parse(this._callbackEventArgument.Substring(startIndex, index - startIndex), CultureInfo.InvariantCulture);
            startIndex = index + 1;
            index = startIndex + num5;
            string str3 = this._callbackEventArgument.Substring(startIndex, index - startIndex);
            startIndex = index;
            index = this._callbackEventArgument.IndexOf('|', startIndex);
            int num6 = int.Parse(this._callbackEventArgument.Substring(startIndex, index - startIndex), CultureInfo.InvariantCulture);
            startIndex = index + 1;
            index = startIndex + num6;
            string dataPath = this._callbackEventArgument.Substring(startIndex, index - startIndex);
            startIndex = index;
            string newPath = this._callbackEventArgument.Substring(startIndex);
            startIndex = newPath.LastIndexOf('\\');
            string str6 = UnEscape(newPath.Substring(startIndex + 1));
            base.ValidateEvent(this.UniqueID, s + str3 + newPath + dataPath);
            TreeNode node = this.CreateNode();
            node.PopulateOnDemand = true;
            if ((str3 != null) && (str3.Length != 0))
            {
                node.Text = str3;
            }
            if ((str6 != null) && (str6.Length != 0))
            {
                node.Value = str6;
            }
            node.SetDataBound(dataBound);
            node.Checked = flag2;
            node.SetPath(newPath);
            node.SetDataPath(dataPath);
            this.PopulateNode(node);
            string str7 = string.Empty;
            if (node.ChildNodes.Count > 0)
            {
                StringBuilder expandState = new StringBuilder();
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    this.SaveNodeState(node.ChildNodes[i], ref num4, expandState, true);
                }
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                HtmlTextWriter writer2 = new HtmlTextWriter(writer);
                int depth = node.Depth;
                bool[] isLast = new bool[depth + 5];
                if (str2.Length > 0)
                {
                    for (int j = 0; j < str2.Length; j++)
                    {
                        if (str2[j] == 't')
                        {
                            isLast[j] = true;
                        }
                    }
                }
                this.EnsureRenderSettings();
                if (node.Expanded != true)
                {
                    writer2.AddStyleAttribute("display", "none");
                }
                writer2.AddAttribute(HtmlTextWriterAttribute.Id, this.CreateNodeId(num3) + "Nodes");
                writer2.RenderBeginTag(HtmlTextWriterTag.Div);
                node.RenderChildNodes(writer2, depth, isLast, true);
                writer2.RenderEndTag();
                writer2.Flush();
                writer2.Close();
                str7 = num4.ToString(CultureInfo.InvariantCulture) + "|" + expandState.ToString() + "|" + writer.ToString();
            }
            this._callbackEventArgument = string.Empty;
            return str7;
        }

        internal string GetCssClassName(TreeNode node, bool hyperLink)
        {
            bool flag;
            return this.GetCssClassName(node, hyperLink, out flag);
        }

        internal string GetCssClassName(TreeNode node, bool hyperLink, out bool containsClassName)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            containsClassName = false;
            int depth = node.Depth;
            bool flag = (node.ChildNodes.Count != 0) || node.PopulateOnDemand;
            List<string> cacheList = flag ? (hyperLink ? this.CachedParentNodeHyperLinkClassNames : this.CachedParentNodeClassNames) : (hyperLink ? this.CachedLeafNodeHyperLinkClassNames : this.CachedLeafNodeClassNames);
            string str = CacheGetItem<string>(cacheList, depth);
            if (this.CachedLevelsContainingCssClass.Contains(depth))
            {
                containsClassName = true;
            }
            bool flag2 = node.Selected && (this._selectedNodeStyle != null);
            if (flag2 || (str == null))
            {
                StringBuilder builder = new StringBuilder();
                if (str != null)
                {
                    builder.Append(str);
                    builder.Append(' ');
                }
                else
                {
                    if (hyperLink)
                    {
                        builder.Append(this.BaseTreeNodeStyle.RegisteredCssClass);
                        builder.Append(' ');
                    }
                    containsClassName |= AppendCssClassName(builder, this._nodeStyle, hyperLink);
                    if ((depth < this.LevelStyles.Count) && (this.LevelStyles[depth] != null))
                    {
                        containsClassName |= AppendCssClassName(builder, this.LevelStyles[depth], hyperLink);
                    }
                    if ((depth == 0) && flag)
                    {
                        containsClassName |= AppendCssClassName(builder, this._rootNodeStyle, hyperLink);
                    }
                    else if (flag)
                    {
                        containsClassName |= AppendCssClassName(builder, this._parentNodeStyle, hyperLink);
                    }
                    else
                    {
                        containsClassName |= AppendCssClassName(builder, this._leafNodeStyle, hyperLink);
                    }
                    str = builder.ToString().Trim();
                    CacheSetItem<string>(cacheList, depth, str);
                    if (containsClassName && !this.CachedLevelsContainingCssClass.Contains(depth))
                    {
                        this.CachedLevelsContainingCssClass.Add(depth);
                    }
                }
                if (flag2)
                {
                    containsClassName |= AppendCssClassName(builder, this._selectedNodeStyle, hyperLink);
                    return builder.ToString().Trim();
                }
            }
            return str;
        }

        internal string GetImageUrl(int index)
        {
            if (this.ImageUrls[index] == null)
            {
                switch (index)
                {
                    case 0:
                    {
                        string imageUrl = this.RootNodeStyle.ImageUrl;
                        if (imageUrl.Length == 0)
                        {
                            imageUrl = this.NodeStyle.ImageUrl;
                        }
                        if (imageUrl.Length == 0)
                        {
                            switch (this.ImageSet)
                            {
                                case TreeViewImageSet.XPFileExplorer:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_RootNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList_RootNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList2:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList2_RootNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList3:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList3_RootNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList4:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList4_RootNode.gif");
                                    break;

                                case TreeViewImageSet.News:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_News_RootNode.gif");
                                    break;

                                case TreeViewImageSet.Inbox:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Inbox_RootNode.gif");
                                    break;

                                case TreeViewImageSet.Events:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Events_RootNode.gif");
                                    break;

                                case TreeViewImageSet.Faq:
                                    imageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_FAQ_RootNode.gif");
                                    break;
                            }
                        }
                        if (imageUrl.Length != 0)
                        {
                            imageUrl = base.ResolveClientUrl(imageUrl);
                        }
                        this.ImageUrls[index] = imageUrl;
                        break;
                    }
                    case 1:
                    {
                        string relativeUrl = this.ParentNodeStyle.ImageUrl;
                        if (relativeUrl.Length == 0)
                        {
                            relativeUrl = this.NodeStyle.ImageUrl;
                        }
                        if (relativeUrl.Length == 0)
                        {
                            switch (this.ImageSet)
                            {
                                case TreeViewImageSet.XPFileExplorer:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList2:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList2_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList3:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList3_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList4:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList4_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.News:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_News_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.Inbox:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Inbox_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.Events:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Events_ParentNode.gif");
                                    break;

                                case TreeViewImageSet.Faq:
                                    relativeUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_FAQ_ParentNode.gif");
                                    break;
                            }
                        }
                        if (relativeUrl.Length != 0)
                        {
                            relativeUrl = base.ResolveClientUrl(relativeUrl);
                        }
                        this.ImageUrls[index] = relativeUrl;
                        break;
                    }
                    case 2:
                    {
                        string webResourceUrl = this.LeafNodeStyle.ImageUrl;
                        if (webResourceUrl.Length == 0)
                        {
                            webResourceUrl = this.NodeStyle.ImageUrl;
                        }
                        if (webResourceUrl.Length == 0)
                        {
                            switch (this.ImageSet)
                            {
                                case TreeViewImageSet.XPFileExplorer:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList2:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList2_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList3:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList3_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.BulletedList4:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList4_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.News:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_News_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.Inbox:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Inbox_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.Events:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Events_LeafNode.gif");
                                    break;

                                case TreeViewImageSet.Faq:
                                    webResourceUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_FAQ_LeafNode.gif");
                                    break;
                            }
                        }
                        if (webResourceUrl.Length != 0)
                        {
                            webResourceUrl = base.ResolveClientUrl(webResourceUrl);
                        }
                        this.ImageUrls[index] = webResourceUrl;
                        break;
                    }
                    case 3:
                        if (!this.ShowLines)
                        {
                            if (this.NoExpandImageUrlInternal.Length > 0)
                            {
                                this.ImageUrls[index] = base.ResolveClientUrl(this.NoExpandImageUrlInternal);
                            }
                            else if (this.LineImagesFolder.Length > 0)
                            {
                                this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "noexpand.gif"));
                            }
                            else
                            {
                                this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_NoExpand.gif");
                            }
                            break;
                        }
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "noexpand.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_NoExpand.gif");
                        break;

                    case 4:
                        if (!this.ShowLines)
                        {
                            if (this.ExpandImageUrlInternal.Length > 0)
                            {
                                this.ImageUrls[index] = base.ResolveClientUrl(this.ExpandImageUrlInternal);
                            }
                            else if (this.LineImagesFolder.Length > 0)
                            {
                                this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "plus.gif"));
                            }
                            else
                            {
                                this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Expand.gif");
                            }
                            break;
                        }
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "plus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Expand.gif");
                        break;

                    case 5:
                        if (!this.ShowLines)
                        {
                            if (this.CollapseImageUrlInternal.Length > 0)
                            {
                                this.ImageUrls[index] = base.ResolveClientUrl(this.CollapseImageUrlInternal);
                            }
                            else if (this.LineImagesFolder.Length > 0)
                            {
                                this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "minus.gif"));
                            }
                            else
                            {
                                this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Collapse.gif");
                            }
                            break;
                        }
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "minus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Collapse.gif");
                        break;

                    case 6:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "i.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_I.gif");
                        break;

                    case 7:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "r.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_R.gif");
                        break;

                    case 8:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "rplus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_RExpand.gif");
                        break;

                    case 9:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "rminus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_RCollapse.gif");
                        break;

                    case 10:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "t.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_T.gif");
                        break;

                    case 11:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "tplus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_TExpand.gif");
                        break;

                    case 12:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "tminus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_TCollapse.gif");
                        break;

                    case 13:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "l.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_L.gif");
                        break;

                    case 14:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "lplus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_LExpand.gif");
                        break;

                    case 15:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "lminus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_LCollapse.gif");
                        break;

                    case 0x10:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "dash.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Dash.gif");
                        break;

                    case 0x11:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "dashplus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_DashExpand.gif");
                        break;

                    case 0x12:
                        if (this.LineImagesFolder.Length != 0)
                        {
                            this.ImageUrls[index] = base.ResolveClientUrl(UrlPath.SimpleCombine(this.LineImagesFolder, "dashminus.gif"));
                            break;
                        }
                        this.ImageUrls[index] = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_DashCollapse.gif");
                        break;
                }
            }
            return this.ImageUrls[index];
        }

        internal string GetLevelImageUrl(int index)
        {
            if (this.LevelImageUrls[index] == null)
            {
                string imageUrl = this.LevelStyles[index].ImageUrl;
                if (imageUrl.Length > 0)
                {
                    this.LevelImageUrls[index] = base.ResolveClientUrl(imageUrl);
                }
                else
                {
                    this.LevelImageUrls[index] = string.Empty;
                }
            }
            return this.LevelImageUrls[index];
        }

        internal static void GetMergedStyle(Style style1, Style style2)
        {
            string cssClass = style1.CssClass;
            style1.CopyFrom(style2);
            if ((cssClass.Length != 0) && (style2.CssClass.Length != 0))
            {
                style1.CssClass = style1.CssClass + ' ' + cssClass;
            }
        }

        private bool GetRenderClientScript(HttpBrowserCapabilities caps)
        {
            return (((this.EnableClientScript && this.Enabled) && ((caps.EcmaScriptVersion.Major > 0) && (caps.W3CDomVersion.Major > 0))) && !StringUtil.EqualsIgnoreCase(caps["tagwriter"], typeof(Html32TextWriter).FullName));
        }

        internal TreeNodeStyle GetStyle(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            bool flag = (node.ChildNodes.Count != 0) || node.PopulateOnDemand;
            List<TreeNodeStyle> cacheList = flag ? this.CachedParentNodeStyles : this.CachedLeafNodeStyles;
            bool flag2 = node.Selected && (this._selectedNodeStyle != null);
            int depth = node.Depth;
            TreeNodeStyle style = CacheGetItem<TreeNodeStyle>(cacheList, depth);
            if (flag2 || (style == null))
            {
                if (style == null)
                {
                    style = new TreeNodeStyle();
                    style.CopyFrom(this.BaseTreeNodeStyle);
                    if (this._nodeStyle != null)
                    {
                        GetMergedStyle(style, this._nodeStyle);
                    }
                    if ((depth == 0) && flag)
                    {
                        if (this._rootNodeStyle != null)
                        {
                            GetMergedStyle(style, this._rootNodeStyle);
                        }
                    }
                    else if (flag)
                    {
                        if (this._parentNodeStyle != null)
                        {
                            GetMergedStyle(style, this._parentNodeStyle);
                        }
                    }
                    else if (this._leafNodeStyle != null)
                    {
                        GetMergedStyle(style, this._leafNodeStyle);
                    }
                    if ((depth < this.LevelStyles.Count) && (this.LevelStyles[depth] != null))
                    {
                        GetMergedStyle(style, this.LevelStyles[depth]);
                    }
                    CacheSetItem<TreeNodeStyle>(cacheList, depth, style);
                }
                if (flag2)
                {
                    TreeNodeStyle style2 = new TreeNodeStyle();
                    style2.CopyFrom(style);
                    GetMergedStyle(style2, this._selectedNodeStyle);
                    return style2;
                }
            }
            return style;
        }

        private int GetTrailingIndex(string s)
        {
            int num = s.Length - 1;
            while (num > 0)
            {
                if (!char.IsDigit(s[num]))
                {
                    break;
                }
                num--;
            }
            if (((num > -1) && (num < (s.Length - 1))) && ((s.Length - num) < 11))
            {
                return int.Parse(s.Substring(num + 1), CultureInfo.InvariantCulture);
            }
            return -1;
        }

        private void LoadNodeState(TreeNode node, ref int index, string expandState, IDictionary populatedNodes, int selectedNodeIndex)
        {
            if ((this.PopulateNodesFromClient && (populatedNodes != null)) && populatedNodes.Contains((int) index))
            {
                populatedNodes[(int) index] = node;
            }
            if (selectedNodeIndex != -1)
            {
                if (node.Selected && (index != selectedNodeIndex))
                {
                    node.Selected = false;
                }
                if ((index == selectedNodeIndex) && ((node.SelectAction == TreeNodeSelectAction.Select) || (node.SelectAction == TreeNodeSelectAction.SelectExpand)))
                {
                    bool selected = node.Selected;
                    node.Selected = true;
                    if (!selected)
                    {
                        this._fireSelectedNodeChanged = true;
                    }
                }
            }
            else if (node.Selected)
            {
                this.SetSelectedNode(node);
            }
            if (node.GetEffectiveShowCheckBox())
            {
                bool flag2 = node.Checked;
                string str = this.CreateNodeId(index) + "CheckBox";
                if ((this.Context.Request.Form[str] != null) || (this.Context.Request.QueryString[str] != null))
                {
                    if (!node.Checked)
                    {
                        node.Checked = true;
                    }
                    if (flag2 != node.Checked)
                    {
                        this.CheckedChangedNodes.Add(node);
                    }
                }
                else
                {
                    if ((flag2 && !node.PreserveChecked) && node.Checked)
                    {
                        node.Checked = false;
                    }
                    if (flag2 != node.Checked)
                    {
                        this.CheckedChangedNodes.Add(node);
                    }
                }
            }
            if ((((this.Page != null) && (this.Page.RequestInternal != null)) && ((expandState != null) && (expandState.Length > index))) && ((this.ShowExpandCollapse || (node.SelectAction == TreeNodeSelectAction.Expand)) || (node.SelectAction == TreeNodeSelectAction.SelectExpand)))
            {
                switch (expandState[index])
                {
                    case 'c':
                        node.Expanded = false;
                        break;

                    case 'e':
                        node.Expanded = true;
                        break;
                }
            }
            index++;
            TreeNodeCollection childNodes = node.ChildNodes;
            if (childNodes.Count > 0)
            {
                for (int i = 0; i < childNodes.Count; i++)
                {
                    this.LoadNodeState(childNodes[i], ref index, expandState, populatedNodes, selectedNodeIndex);
                }
            }
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            if (!base.IsEnabled)
            {
                return false;
            }
            int selectedNodeIndex = -1;
            string str = postCollection[this.SelectedNodeFieldID];
            if (!string.IsNullOrEmpty(str))
            {
                selectedNodeIndex = this.GetTrailingIndex(str);
            }
            this._loadingNodeState = true;
            try
            {
                Dictionary<int, TreeNode> populatedNodes = null;
                int[] numArray = null;
                int capacity = -1;
                if (this.PopulateNodesFromClient)
                {
                    string str2 = postCollection[this.PopulateLogID];
                    if (str2 != null)
                    {
                        string[] strArray = str2.Split(new char[] { ',' });
                        capacity = strArray.Length;
                        populatedNodes = new Dictionary<int, TreeNode>(capacity);
                        numArray = new int[capacity];
                        for (int j = 0; j < capacity; j++)
                        {
                            if (strArray[j].Length > 0)
                            {
                                int key = int.Parse(strArray[j], NumberStyles.Integer, CultureInfo.InvariantCulture);
                                if (!populatedNodes.ContainsKey(key))
                                {
                                    numArray[j] = key;
                                    populatedNodes.Add(key, null);
                                }
                                else
                                {
                                    numArray[j] = -1;
                                }
                            }
                            else
                            {
                                numArray[j] = -1;
                            }
                        }
                    }
                }
                string expandState = postCollection[this.ExpandStateID];
                int index = 0;
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    this.LoadNodeState(this.Nodes[i], ref index, expandState, populatedNodes, selectedNodeIndex);
                }
                if (this.PopulateNodesFromClient && (capacity > 0))
                {
                    object obj2 = this.ViewState["LastIndex"];
                    int num7 = (obj2 != null) ? ((int) obj2) : -1;
                    for (int k = 0; k < capacity; k++)
                    {
                        index = numArray[k];
                        if ((index >= 0) && populatedNodes.ContainsKey(index))
                        {
                            TreeNode node = populatedNodes[index];
                            if (node != null)
                            {
                                this.PopulateNode(node);
                                if ((node.ChildNodes.Count > 0) && (num7 != -1))
                                {
                                    TreeNodeCollection childNodes = node.ChildNodes;
                                    for (int m = 0; m < childNodes.Count; m++)
                                    {
                                        this.LoadNodeState(childNodes[m], ref num7, expandState, populatedNodes, selectedNodeIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this._loadingNodeState = false;
            }
            return (this._checkedChangedNodes != null);
        }

        protected override void LoadViewState(object state)
        {
            if (state != null)
            {
                object[] objArray = (object[]) state;
                if (objArray[0] != null)
                {
                    base.LoadViewState(objArray[0]);
                }
                if (objArray[1] != null)
                {
                    ((IStateManager) this.NodeStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.RootNodeStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.ParentNodeStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.LeafNodeStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.SelectedNodeStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.HoverNodeStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.LevelStyles).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.Nodes).LoadViewState(objArray[8]);
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.ChildControlsCreated = true;
            base.OnInit(e);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.EnsureRenderSettings();
            if (this.Page != null)
            {
                if (!this.Page.IsPostBack && !this._dataBound)
                {
                    this.ExpandToDepth(this.Nodes, this.ExpandDepth);
                }
                this.Page.RegisterRequiresPostBack(this);
                StringBuilder expandState = new StringBuilder();
                int index = 0;
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    this.SaveNodeState(this.Nodes[i], ref index, expandState, true);
                }
                if (this.RenderClientScript)
                {
                    ClientScriptManager clientScript = this.Page.ClientScript;
                    clientScript.RegisterHiddenField(this, this.ExpandStateID, expandState.ToString());
                    int num3 = 6;
                    if (this.ShowLines)
                    {
                        num3 = 0x13;
                    }
                    for (int j = 0; j < num3; j++)
                    {
                        string imageUrl = this.GetImageUrl(j);
                        if (imageUrl.Length > 0)
                        {
                            imageUrl = Util.QuoteJScriptString(imageUrl);
                        }
                        clientScript.RegisterArrayDeclaration(this, this.ImageArrayID, "'" + imageUrl + "'");
                    }
                    string hiddenFieldValue = string.Empty;
                    if (this.SelectedNode != null)
                    {
                        TreeNode selectedNode = this.SelectedNode;
                        while ((selectedNode != null) && (selectedNode != this.RootNode))
                        {
                            selectedNode = selectedNode.GetParentInternal();
                        }
                        if (selectedNode == this.RootNode)
                        {
                            hiddenFieldValue = this.SelectedNode.SelectID;
                            this.ViewState["SelectedNode"] = this.SelectedNode.SelectID;
                        }
                        else
                        {
                            this.ViewState["SelectedNode"] = null;
                        }
                    }
                    else
                    {
                        this.ViewState["SelectedNode"] = null;
                    }
                    clientScript.RegisterHiddenField(this, this.SelectedNodeFieldID, hiddenFieldValue);
                    this.Page.RegisterWebFormsScript();
                    clientScript.RegisterClientScriptResource(this, typeof(TreeView), "TreeView.js");
                    string clientDataObjectID = this.ClientDataObjectID;
                    string str4 = string.Empty;
                    if (this.PopulateNodesFromClient)
                    {
                        this.ViewState["LastIndex"] = index;
                        clientScript.RegisterHiddenField(this, this.PopulateLogID, string.Empty);
                        str4 = string.Concat(new object[] { clientDataObjectID, ".lastIndex = ", index, ";\r\n", clientDataObjectID, ".populateLog = theForm.elements['", this.PopulateLogID, "'];\r\n", clientDataObjectID, ".treeViewID = '", this.UniqueID, "';\r\n", clientDataObjectID, ".name = '", clientDataObjectID, "';\r\n" });
                        if (!clientScript.IsClientScriptBlockRegistered(base.GetType(), "PopulateNode"))
                        {
                            clientScript.RegisterClientScriptBlock(this, base.GetType(), "PopulateNode", populateNodeScript + clientScript.GetCallbackEventReference("context.data.treeViewID", "param", "TreeView_ProcessNodeData", "context", "TreeView_ProcessNodeData", false) + populateNodeScriptEnd, true);
                        }
                    }
                    string str5 = string.Empty;
                    if (this._selectedNodeStyle != null)
                    {
                        string registeredCssClass = this._selectedNodeStyle.RegisteredCssClass;
                        if (registeredCssClass.Length > 0)
                        {
                            registeredCssClass = registeredCssClass + " ";
                        }
                        string str7 = this._selectedNodeStyle.HyperLinkStyle.RegisteredCssClass;
                        if (str7.Length > 0)
                        {
                            str7 = str7 + " ";
                        }
                        if (!string.IsNullOrEmpty(this._selectedNodeStyle.CssClass))
                        {
                            string str8 = this._selectedNodeStyle.CssClass + " ";
                            registeredCssClass = registeredCssClass + str8;
                            str7 = str7 + str8;
                        }
                        str5 = clientDataObjectID + ".selectedClass = '" + registeredCssClass + "';\r\n" + clientDataObjectID + ".selectedHyperLinkClass = '" + str7 + "';\r\n";
                    }
                    string str9 = string.Empty;
                    if (this.EnableHover)
                    {
                        string str10 = this._hoverNodeStyle.RegisteredCssClass;
                        string str11 = this._hoverNodeHyperLinkStyle.RegisteredCssClass;
                        if (!string.IsNullOrEmpty(this._hoverNodeStyle.CssClass))
                        {
                            string cssClass = this._hoverNodeStyle.CssClass;
                            if (!string.IsNullOrEmpty(str10))
                            {
                                str10 = str10 + " ";
                            }
                            if (!string.IsNullOrEmpty(str11))
                            {
                                str11 = str11 + " ";
                            }
                            str10 = str10 + cssClass;
                            str11 = str11 + cssClass;
                        }
                        str5 = clientDataObjectID + ".hoverClass = '" + str10 + "';\r\n" + clientDataObjectID + ".hoverHyperLinkClass = '" + str11 + "';\r\n";
                    }
                    string script = string.Concat(new object[] { 
                        "var ", clientDataObjectID, " = new Object();\r\n", clientDataObjectID, ".images = ", this.ImageArrayID, ";\r\n", clientDataObjectID, ".collapseToolTip = \"", Util.QuoteJScriptString(this.CollapseImageToolTip), "\";\r\n", clientDataObjectID, ".expandToolTip = \"", Util.QuoteJScriptString(this.ExpandImageToolTip), "\";\r\n", clientDataObjectID, 
                        ".expandState = theForm.elements['", this.ExpandStateID, "'];\r\n", clientDataObjectID, ".selectedNodeID = theForm.elements['", this.SelectedNodeFieldID, "'];\r\n", str5, str9, "(function() {\r\n  for (var i=0;i<", num3, ";i++) {\r\n  var preLoad = new Image();\r\n  if (", this.ImageArrayID, "[i].length > 0)\r\n    preLoad.src = ", this.ImageArrayID, "[i];\r\n  }\r\n})();\r\n", 
                        str4
                     });
                    clientScript.RegisterClientScriptBlock(this, base.GetType(), this.ClientID + "_CreateDataObject1", "var " + clientDataObjectID + " = null;", true);
                    clientScript.RegisterStartupScript(this, base.GetType(), this.ClientID + "_CreateDataObject2", script, true);
                    IScriptManager scriptManager = this.Page.ScriptManager;
                    if ((scriptManager != null) && scriptManager.SupportsPartialRendering)
                    {
                        scriptManager.RegisterDispose(this, this.ImageArrayID + ".length = 0;\r\n" + clientDataObjectID + " = null;");
                    }
                }
            }
        }

        protected virtual void OnSelectedNodeChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[SelectedNodeChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeCheckChanged(TreeNodeEventArgs e)
        {
            TreeNodeEventHandler handler = (TreeNodeEventHandler) base.Events[CheckChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeCollapsed(TreeNodeEventArgs e)
        {
            TreeNodeEventHandler handler = (TreeNodeEventHandler) base.Events[TreeNodeCollapsedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeDataBound(TreeNodeEventArgs e)
        {
            TreeNodeEventHandler handler = (TreeNodeEventHandler) base.Events[TreeNodeDataBoundEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeExpanded(TreeNodeEventArgs e)
        {
            TreeNodeEventHandler handler = (TreeNodeEventHandler) base.Events[TreeNodeExpandedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodePopulate(TreeNodeEventArgs e)
        {
            TreeNodeEventHandler handler = (TreeNodeEventHandler) base.Events[TreeNodePopulateEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void PerformDataBinding()
        {
            base.PerformDataBinding();
            if ((!base.DesignMode && this._dataBound) && (string.IsNullOrEmpty(this.DataSourceID) && (this.DataSource == null)))
            {
                this.Nodes.Clear();
            }
            else
            {
                this.DataBindNode(this.RootNode);
                if (!string.IsNullOrEmpty(this.DataSourceID) || (this.DataSource != null))
                {
                    this._dataBound = true;
                }
                this.ExpandToDepth(this.Nodes, this.ExpandDepth);
            }
        }

        internal void PopulateNode(TreeNode node)
        {
            if (node.DataBound)
            {
                this.DataBindNode(node);
            }
            else
            {
                this.OnTreeNodePopulate(new TreeNodeEventArgs(node));
            }
            node.Populated = true;
            node.PopulateOnDemand = false;
        }

        protected virtual void RaiseCallbackEvent(string eventArgument)
        {
            this._callbackEventArgument = eventArgument;
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if (base.IsEnabled)
            {
                if (base.AdapterInternal != null)
                {
                    IPostBackEventHandler adapterInternal = base.AdapterInternal as IPostBackEventHandler;
                    if (adapterInternal != null)
                    {
                        adapterInternal.RaisePostBackEvent(eventArgument);
                    }
                }
                else if (eventArgument.Length != 0)
                {
                    char ch = eventArgument[0];
                    string str = HttpUtility.HtmlDecode(eventArgument.Substring(1));
                    TreeNode node = this.Nodes.FindNode(str.Split(new char[] { '\\' }), 0);
                    if (node != null)
                    {
                        switch (ch)
                        {
                            case 's':
                                if ((node.SelectAction == TreeNodeSelectAction.Expand) || (node.SelectAction == TreeNodeSelectAction.SelectExpand))
                                {
                                    if (node.Expanded == true)
                                    {
                                        if (node.SelectAction == TreeNodeSelectAction.Expand)
                                        {
                                            node.Expanded = false;
                                        }
                                    }
                                    else
                                    {
                                        node.Expanded = true;
                                    }
                                }
                                if ((node.SelectAction == TreeNodeSelectAction.Select) || (node.SelectAction == TreeNodeSelectAction.SelectExpand))
                                {
                                    bool flag = false;
                                    if (!node.Selected)
                                    {
                                        flag = true;
                                    }
                                    node.Selected = true;
                                    if (flag)
                                    {
                                        this._fireSelectedNodeChanged = true;
                                    }
                                }
                                break;

                            case 't':
                                if ((this.ShowExpandCollapse || (node.SelectAction == TreeNodeSelectAction.Expand)) || (node.SelectAction == TreeNodeSelectAction.SelectExpand))
                                {
                                    node.ToggleExpandState();
                                }
                                break;
                        }
                    }
                    if (this._fireSelectedNodeChanged)
                    {
                        try
                        {
                            this.RaiseSelectedNodeChanged();
                        }
                        finally
                        {
                            this._fireSelectedNodeChanged = false;
                        }
                    }
                }
            }
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            if (this._checkedChangedNodes != null)
            {
                foreach (TreeNode node in this._checkedChangedNodes)
                {
                    this.OnTreeNodeCheckChanged(new TreeNodeEventArgs(node));
                }
            }
        }

        internal void RaiseSelectedNodeChanged()
        {
            this.OnSelectedNodeChanged(EventArgs.Empty);
        }

        internal void RaiseTreeNodeCollapsed(TreeNode node)
        {
            this.OnTreeNodeCollapsed(new TreeNodeEventArgs(node));
        }

        internal void RaiseTreeNodeExpanded(TreeNode node)
        {
            this.OnTreeNodeExpanded(new TreeNodeEventArgs(node));
        }

        private void RegisterStyle(Style style)
        {
            if (!style.IsEmpty && ((this.Page != null) && this.Page.SupportsStyleSheets))
            {
                string cssClass = this.ClientID + "_" + this._cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                this.Page.Header.StyleSheet.CreateStyleRule(style, this, "." + cssClass);
                style.SetRegisteredCssClass(cssClass);
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if ((this.SkipLinkText.Length != 0) && !base.DesignMode)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, '#' + this.ClientID + "_SkipLink");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.SkipLinkText);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, base.SpacerImageUrl);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            base.RenderBeginTag(writer);
            if (base.DesignMode)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            base.RenderContents(writer);
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            bool isEnabled = base.IsEnabled;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                TreeNode node = this.Nodes[i];
                bool[] isLast = new bool[10];
                isLast[0] = i == (this.Nodes.Count - 1);
                node.Render(writer, i, isLast, isEnabled);
            }
            if (base.DesignMode)
            {
                if (this._nodeStyle != null)
                {
                    this._nodeStyle.ResetCachedStyles();
                }
                if (this._leafNodeStyle != null)
                {
                    this._leafNodeStyle.ResetCachedStyles();
                }
                if (this._parentNodeStyle != null)
                {
                    this._parentNodeStyle.ResetCachedStyles();
                }
                if (this._rootNodeStyle != null)
                {
                    this._rootNodeStyle.ResetCachedStyles();
                }
                if (this._selectedNodeStyle != null)
                {
                    this._selectedNodeStyle.ResetCachedStyles();
                }
                if (this._hoverNodeStyle != null)
                {
                    this._hoverNodeHyperLinkStyle = new HyperLinkStyle(this._hoverNodeStyle);
                }
                foreach (TreeNodeStyle style in this.LevelStyles)
                {
                    style.ResetCachedStyles();
                }
                if (this._imageUrls != null)
                {
                    for (int j = 0; j < this._imageUrls.Length; j++)
                    {
                        this._imageUrls[j] = null;
                    }
                }
                this._cachedExpandImageUrl = null;
                this._cachedCollapseImageUrl = null;
                this._cachedNoExpandImageUrl = null;
                this._cachedLeafNodeClassNames = null;
                this._cachedLeafNodeHyperLinkClassNames = null;
                this._cachedLeafNodeStyles = null;
                this._cachedLevelsContainingCssClass = null;
                this._cachedParentNodeClassNames = null;
                this._cachedParentNodeHyperLinkClassNames = null;
                this._cachedParentNodeStyles = null;
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (base.DesignMode)
            {
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            base.RenderEndTag(writer);
            if ((this.SkipLinkText.Length != 0) && !base.DesignMode)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_SkipLink");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.RenderEndTag();
            }
        }

        private void SaveNodeState(TreeNode node, ref int index, StringBuilder expandState, bool rendered)
        {
            node.Index = index++;
            if (node.CheckedSet)
            {
                if (!this.Enabled || ((!this.RenderClientScript && !rendered) && node.Checked))
                {
                    node.PreserveChecked = true;
                }
                else
                {
                    node.PreserveChecked = false;
                }
            }
            if (node.PopulateOnDemand)
            {
                if ((node.ChildNodes.Count == 0) || (node.Expanded != true))
                {
                    expandState.Append('c');
                }
                else
                {
                    expandState.Append('e');
                }
            }
            else if (node.ChildNodes.Count == 0)
            {
                expandState.Append('n');
            }
            else if (!node.Expanded.HasValue)
            {
                expandState.Append('u');
            }
            else if (node.Expanded == true)
            {
                expandState.Append('e');
            }
            else
            {
                expandState.Append('c');
            }
            if (node.ChildNodes.Count > 0)
            {
                TreeNodeCollection childNodes = node.ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    this.SaveNodeState(childNodes[i], ref index, expandState, (node.Expanded == true) && rendered);
                }
            }
        }

        protected override object SaveViewState()
        {
            if ((!this.Visible && (this.Page != null)) && !this.Page.IsPostBack)
            {
                this.ViewState["NeverExpanded"] = true;
            }
            object[] objArray = new object[9];
            objArray[0] = base.SaveViewState();
            bool flag = objArray[0] != null;
            if (this._nodeStyle != null)
            {
                objArray[1] = ((IStateManager) this._nodeStyle).SaveViewState();
                flag |= objArray[1] != null;
            }
            if (this._rootNodeStyle != null)
            {
                objArray[2] = ((IStateManager) this._rootNodeStyle).SaveViewState();
                flag |= objArray[2] != null;
            }
            if (this._parentNodeStyle != null)
            {
                objArray[3] = ((IStateManager) this._parentNodeStyle).SaveViewState();
                flag |= objArray[3] != null;
            }
            if (this._leafNodeStyle != null)
            {
                objArray[4] = ((IStateManager) this._leafNodeStyle).SaveViewState();
                flag |= objArray[4] != null;
            }
            if (this._selectedNodeStyle != null)
            {
                objArray[5] = ((IStateManager) this._selectedNodeStyle).SaveViewState();
                flag |= objArray[5] != null;
            }
            if (this._hoverNodeStyle != null)
            {
                objArray[6] = ((IStateManager) this._hoverNodeStyle).SaveViewState();
                flag |= objArray[6] != null;
            }
            if (this._levelStyles != null)
            {
                objArray[7] = ((IStateManager) this._levelStyles).SaveViewState();
                flag |= objArray[7] != null;
            }
            objArray[8] = ((IStateManager) this.Nodes).SaveViewState();
            if (flag | (objArray[8] != null))
            {
                return objArray;
            }
            return null;
        }

        protected void SetNodeDataBound(TreeNode node, bool dataBound)
        {
            node.SetDataBound(dataBound);
        }

        protected void SetNodeDataItem(TreeNode node, object dataItem)
        {
            node.SetDataItem(dataItem);
        }

        protected void SetNodeDataPath(TreeNode node, string dataPath)
        {
            node.SetDataPath(dataPath);
        }

        internal void SetSelectedNode(TreeNode node)
        {
            if (this._selectedNode != node)
            {
                if ((this._selectedNode != null) && this._selectedNode.Selected)
                {
                    this._selectedNode.SetSelected(false);
                }
                this._selectedNode = node;
                if ((this._selectedNode != null) && !this._selectedNode.Selected)
                {
                    this._selectedNode.SetSelected(true);
                }
            }
        }

        string ICallbackEventHandler.GetCallbackResult()
        {
            return this.GetCallbackResult();
        }

        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
        {
            this.RaiseCallbackEvent(eventArgument);
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._nodeStyle != null)
            {
                ((IStateManager) this._nodeStyle).TrackViewState();
            }
            if (this._rootNodeStyle != null)
            {
                ((IStateManager) this._rootNodeStyle).TrackViewState();
            }
            if (this._parentNodeStyle != null)
            {
                ((IStateManager) this._parentNodeStyle).TrackViewState();
            }
            if (this._leafNodeStyle != null)
            {
                ((IStateManager) this._leafNodeStyle).TrackViewState();
            }
            if (this._selectedNodeStyle != null)
            {
                ((IStateManager) this._selectedNodeStyle).TrackViewState();
            }
            if (this._hoverNodeStyle != null)
            {
                ((IStateManager) this._hoverNodeStyle).TrackViewState();
            }
            if (this._levelStyles != null)
            {
                ((IStateManager) this._levelStyles).TrackViewState();
            }
            if (this._bindings != null)
            {
                ((IStateManager) this._bindings).TrackViewState();
            }
            ((IStateManager) this.Nodes).TrackViewState();
        }

        internal static string UnEscape(string value)
        {
            char ch = '\\';
            char ch2 = '|';
            return value.Replace("*|*", ch.ToString()).Replace("||", ch2.ToString());
        }

        internal bool AccessKeyRendered
        {
            get
            {
                return this._accessKeyRendered;
            }
            set
            {
                this._accessKeyRendered = value;
            }
        }

        [WebCategory("Behavior"), DefaultValue(true), WebSysDescription("TreeView_AutoGenerateDataBindings")]
        public bool AutoGenerateDataBindings
        {
            get
            {
                object obj2 = this.ViewState["AutoGenerateDataBindings"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["AutoGenerateDataBindings"] = value;
            }
        }

        internal Style BaseTreeNodeStyle
        {
            get
            {
                if (this._baseNodeStyle == null)
                {
                    this._baseNodeStyle = new Style();
                    this._baseNodeStyle.Font.CopyFrom(this.Font);
                    if (!this.ForeColor.IsEmpty)
                    {
                        this._baseNodeStyle.ForeColor = this.ForeColor;
                    }
                    if (!base.ControlStyle.IsSet(0x2000))
                    {
                        this._baseNodeStyle.Font.Underline = false;
                    }
                }
                return this._baseNodeStyle;
            }
        }

        private List<string> CachedLeafNodeClassNames
        {
            get
            {
                if (this._cachedLeafNodeClassNames == null)
                {
                    this._cachedLeafNodeClassNames = new List<string>();
                }
                return this._cachedLeafNodeClassNames;
            }
        }

        private List<string> CachedLeafNodeHyperLinkClassNames
        {
            get
            {
                if (this._cachedLeafNodeHyperLinkClassNames == null)
                {
                    this._cachedLeafNodeHyperLinkClassNames = new List<string>();
                }
                return this._cachedLeafNodeHyperLinkClassNames;
            }
        }

        private List<TreeNodeStyle> CachedLeafNodeStyles
        {
            get
            {
                if (this._cachedLeafNodeStyles == null)
                {
                    this._cachedLeafNodeStyles = new List<TreeNodeStyle>();
                }
                return this._cachedLeafNodeStyles;
            }
        }

        private Collection<int> CachedLevelsContainingCssClass
        {
            get
            {
                if (this._cachedLevelsContainingCssClass == null)
                {
                    this._cachedLevelsContainingCssClass = new Collection<int>();
                }
                return this._cachedLevelsContainingCssClass;
            }
        }

        private List<string> CachedParentNodeClassNames
        {
            get
            {
                if (this._cachedParentNodeClassNames == null)
                {
                    this._cachedParentNodeClassNames = new List<string>();
                }
                return this._cachedParentNodeClassNames;
            }
        }

        private List<string> CachedParentNodeHyperLinkClassNames
        {
            get
            {
                if (this._cachedParentNodeHyperLinkClassNames == null)
                {
                    this._cachedParentNodeHyperLinkClassNames = new List<string>();
                }
                return this._cachedParentNodeHyperLinkClassNames;
            }
        }

        private List<TreeNodeStyle> CachedParentNodeStyles
        {
            get
            {
                if (this._cachedParentNodeStyles == null)
                {
                    this._cachedParentNodeStyles = new List<TreeNodeStyle>();
                }
                return this._cachedParentNodeStyles;
            }
        }

        private ArrayList CheckedChangedNodes
        {
            get
            {
                if (this._checkedChangedNodes == null)
                {
                    this._checkedChangedNodes = new ArrayList();
                }
                return this._checkedChangedNodes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TreeNodeCollection CheckedNodes
        {
            get
            {
                if (this._checkedNodes == null)
                {
                    this._checkedNodes = new TreeNodeCollection(null, false);
                }
                return this._checkedNodes;
            }
        }

        internal string ClientDataObjectID
        {
            get
            {
                if (this._cachedClientDataObjectID == null)
                {
                    this._cachedClientDataObjectID = this.ClientID + "_Data";
                }
                return this._cachedClientDataObjectID;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("TreeView_CollapseImageToolTip"), WebSysDefaultValue("TreeView_CollapseImageToolTipDefaultValue"), Localizable(true)]
        public string CollapseImageToolTip
        {
            get
            {
                string str = (string) this.ViewState["CollapseImageToolTip"];
                if (str == null)
                {
                    return System.Web.SR.GetString("TreeView_CollapseImageToolTipDefaultValue");
                }
                return str;
            }
            set
            {
                this.ViewState["CollapseImageToolTip"] = value;
            }
        }

        [WebSysDescription("TreeView_CollapseImageUrl"), DefaultValue(""), UrlProperty, Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Appearance")]
        public string CollapseImageUrl
        {
            get
            {
                string str = (string) this.ViewState["CollapseImageUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["CollapseImageUrl"] = value;
            }
        }

        internal string CollapseImageUrlInternal
        {
            get
            {
                if (this._cachedCollapseImageUrl == null)
                {
                    switch (this.ImageSet)
                    {
                        case TreeViewImageSet.Custom:
                            this._cachedCollapseImageUrl = this.CollapseImageUrl;
                            goto Label_0124;

                        case TreeViewImageSet.XPFileExplorer:
                            this._cachedCollapseImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_Collapse.gif");
                            goto Label_0124;

                        case TreeViewImageSet.Msdn:
                            this._cachedCollapseImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_MSDN_Collapse.gif");
                            goto Label_0124;

                        case TreeViewImageSet.WindowsHelp:
                            this._cachedCollapseImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Windows_Help_Collapse.gif");
                            goto Label_0124;

                        case TreeViewImageSet.Arrows:
                            this._cachedCollapseImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Arrows_Collapse.gif");
                            goto Label_0124;

                        case TreeViewImageSet.Contacts:
                            this._cachedCollapseImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Contacts_Collapse.gif");
                            goto Label_0124;
                    }
                    this._cachedCollapseImageUrl = string.Empty;
                }
            Label_0124:
                return this._cachedCollapseImageUrl;
            }
        }

        internal bool CustomExpandCollapseHandlerExists
        {
            get
            {
                TreeNodeEventHandler handler = (TreeNodeEventHandler) base.Events[TreeNodeCollapsedEvent];
                TreeNodeEventHandler handler2 = (TreeNodeEventHandler) base.Events[TreeNodeExpandedEvent];
                if (handler == null)
                {
                    return (handler2 != null);
                }
                return true;
            }
        }

        [DefaultValue((string) null), WebSysDescription("TreeView_DataBindings"), MergableProperty(false), Editor("System.Web.UI.Design.WebControls.TreeViewBindingsEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Data")]
        public TreeNodeBindingCollection DataBindings
        {
            get
            {
                if (this._bindings == null)
                {
                    this._bindings = new TreeNodeBindingCollection();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._bindings).TrackViewState();
                    }
                }
                return this._bindings;
            }
        }

        [Themeable(false), WebCategory("Behavior"), DefaultValue(true), WebSysDescription("TreeView_EnableClientScript")]
        public bool EnableClientScript
        {
            get
            {
                object obj2 = this.ViewState["EnableClientScript"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["EnableClientScript"] = value;
            }
        }

        internal bool EnableHover
        {
            get
            {
                return ((((this.Page != null) && ((this.Page.SupportsStyleSheets || this.Page.IsCallback) || ((this.Page.ScriptManager != null) && this.Page.ScriptManager.IsInAsyncPostBack))) && this.RenderClientScript) && (this._hoverNodeStyle != null));
            }
        }

        [DefaultValue(-1), TypeConverter(typeof(TreeViewExpandDepthConverter)), WebCategory("Behavior"), WebSysDescription("TreeView_ExpandDepth")]
        public int ExpandDepth
        {
            get
            {
                object obj2 = this.ViewState["ExpandDepth"];
                if (obj2 == null)
                {
                    return -1;
                }
                return (int) obj2;
            }
            set
            {
                this.ViewState["ExpandDepth"] = value;
            }
        }

        [WebSysDefaultValue("TreeView_ExpandImageToolTipDefaultValue"), WebCategory("Appearance"), WebSysDescription("TreeView_ExpandImageToolTip"), Localizable(true)]
        public string ExpandImageToolTip
        {
            get
            {
                string str = (string) this.ViewState["ExpandImageToolTip"];
                if (str == null)
                {
                    return System.Web.SR.GetString("TreeView_ExpandImageToolTipDefaultValue");
                }
                return str;
            }
            set
            {
                this.ViewState["ExpandImageToolTip"] = value;
            }
        }

        [WebSysDescription("TreeView_ExpandImageUrl"), WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
        public string ExpandImageUrl
        {
            get
            {
                string str = (string) this.ViewState["ExpandImageUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ExpandImageUrl"] = value;
            }
        }

        internal string ExpandImageUrlInternal
        {
            get
            {
                if (this._cachedExpandImageUrl == null)
                {
                    switch (this.ImageSet)
                    {
                        case TreeViewImageSet.Custom:
                            this._cachedExpandImageUrl = this.ExpandImageUrl;
                            goto Label_0124;

                        case TreeViewImageSet.XPFileExplorer:
                            this._cachedExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_Expand.gif");
                            goto Label_0124;

                        case TreeViewImageSet.Msdn:
                            this._cachedExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_MSDN_Expand.gif");
                            goto Label_0124;

                        case TreeViewImageSet.WindowsHelp:
                            this._cachedExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Windows_Help_Expand.gif");
                            goto Label_0124;

                        case TreeViewImageSet.Arrows:
                            this._cachedExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Arrows_Expand.gif");
                            goto Label_0124;

                        case TreeViewImageSet.Contacts:
                            this._cachedExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Contacts_Expand.gif");
                            goto Label_0124;
                    }
                    this._cachedExpandImageUrl = string.Empty;
                }
            Label_0124:
                return this._cachedExpandImageUrl;
            }
        }

        internal string ExpandStateID
        {
            get
            {
                if (this._cachedExpandStateID == null)
                {
                    this._cachedExpandStateID = this.ClientID + "_ExpandState";
                }
                return this._cachedExpandStateID;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("TreeView_HoverNodeStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), DefaultValue((string) null)]
        public Style HoverNodeStyle
        {
            get
            {
                if (this._hoverNodeStyle == null)
                {
                    this._hoverNodeStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._hoverNodeStyle).TrackViewState();
                    }
                }
                return this._hoverNodeStyle;
            }
        }

        internal string ImageArrayID
        {
            get
            {
                if (this._cachedImageArrayID == null)
                {
                    this._cachedImageArrayID = this.ClientID + "_ImageArray";
                }
                return this._cachedImageArrayID;
            }
        }

        [WebCategory("Appearance"), DefaultValue(0), WebSysDescription("TreeView_ImageSet")]
        public TreeViewImageSet ImageSet
        {
            get
            {
                object obj2 = this.ViewState["ImageSet"];
                if (obj2 == null)
                {
                    return TreeViewImageSet.Custom;
                }
                return (TreeViewImageSet) obj2;
            }
            set
            {
                if ((value < TreeViewImageSet.Custom) || (value > TreeViewImageSet.Faq))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ImageSet"] = value;
            }
        }

        private string[] ImageUrls
        {
            get
            {
                if (this._imageUrls == null)
                {
                    this._imageUrls = new string[0x13];
                }
                return this._imageUrls;
            }
        }

        internal bool IsNotIE
        {
            get
            {
                return this._isNotIE;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("TreeView_LeafNodeStyle"), DefaultValue((string) null)]
        public TreeNodeStyle LeafNodeStyle
        {
            get
            {
                if (this._leafNodeStyle == null)
                {
                    this._leafNodeStyle = new TreeNodeStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._leafNodeStyle).TrackViewState();
                    }
                }
                return this._leafNodeStyle;
            }
        }

        private string[] LevelImageUrls
        {
            get
            {
                if (this._levelImageUrls == null)
                {
                    this._levelImageUrls = new string[this.LevelStyles.Count];
                }
                return this._levelImageUrls;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("TreeView_LevelStyles"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.TreeNodeStyleCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public TreeNodeStyleCollection LevelStyles
        {
            get
            {
                if (this._levelStyles == null)
                {
                    this._levelStyles = new TreeNodeStyleCollection();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._levelStyles).TrackViewState();
                    }
                }
                return this._levelStyles;
            }
        }

        [WebSysDescription("TreeView_LineImagesFolderUrl"), DefaultValue(""), WebCategory("Appearance")]
        public string LineImagesFolder
        {
            get
            {
                string str = (string) this.ViewState["LineImagesFolder"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["LineImagesFolder"] = value;
            }
        }

        internal bool LoadingNodeState
        {
            get
            {
                return this._loadingNodeState;
            }
        }

        [DefaultValue(-1), WebSysDescription("TreeView_MaxDataBindDepth"), WebCategory("Behavior")]
        public int MaxDataBindDepth
        {
            get
            {
                object obj2 = this.ViewState["MaxDataBindDepth"];
                if (obj2 == null)
                {
                    return -1;
                }
                return (int) obj2;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["MaxDataBindDepth"] = value;
            }
        }

        [WebSysDescription("TreeView_NodeIndent"), WebCategory("Appearance"), DefaultValue(20)]
        public int NodeIndent
        {
            get
            {
                object obj2 = this.ViewState["NodeIndent"];
                if (obj2 == null)
                {
                    return 20;
                }
                return (int) obj2;
            }
            set
            {
                this.ViewState["NodeIndent"] = value;
            }
        }

        [WebSysDescription("TreeView_Nodes"), MergableProperty(false), Editor("System.Web.UI.Design.WebControls.TreeNodeCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public TreeNodeCollection Nodes
        {
            get
            {
                return this.RootNode.ChildNodes;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), WebSysDescription("TreeView_NodeStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), DefaultValue((string) null)]
        public TreeNodeStyle NodeStyle
        {
            get
            {
                if (this._nodeStyle == null)
                {
                    this._nodeStyle = new TreeNodeStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._nodeStyle).TrackViewState();
                    }
                }
                return this._nodeStyle;
            }
        }

        [WebSysDescription("TreeView_NodeWrap"), WebCategory("Appearance"), DefaultValue(false)]
        public bool NodeWrap
        {
            get
            {
                object obj2 = this.ViewState["NodeWrap"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["NodeWrap"] = value;
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("TreeView_NoExpandImageUrl"), DefaultValue(""), WebCategory("Appearance"), UrlProperty]
        public string NoExpandImageUrl
        {
            get
            {
                string str = (string) this.ViewState["NoExpandImageUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["NoExpandImageUrl"] = value;
            }
        }

        internal string NoExpandImageUrlInternal
        {
            get
            {
                if (this._cachedNoExpandImageUrl == null)
                {
                    switch (this.ImageSet)
                    {
                        case TreeViewImageSet.Custom:
                            this._cachedNoExpandImageUrl = this.NoExpandImageUrl;
                            goto Label_0187;

                        case TreeViewImageSet.XPFileExplorer:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_NoExpand.gif");
                            goto Label_0187;

                        case TreeViewImageSet.Msdn:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_MSDN_NoExpand.gif");
                            goto Label_0187;

                        case TreeViewImageSet.WindowsHelp:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Windows_Help_NoExpand.gif");
                            goto Label_0187;

                        case TreeViewImageSet.Simple:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Simple_NoExpand.gif");
                            goto Label_0187;

                        case TreeViewImageSet.Simple2:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Simple2_NoExpand.gif");
                            goto Label_0187;

                        case TreeViewImageSet.Arrows:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Arrows_NoExpand.gif");
                            goto Label_0187;

                        case TreeViewImageSet.Contacts:
                            this._cachedNoExpandImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Contacts_NoExpand.gif");
                            goto Label_0187;
                    }
                    this._cachedNoExpandImageUrl = string.Empty;
                }
            Label_0187:
                return this._cachedNoExpandImageUrl;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("TreeView_ParentNodeStyle"), DefaultValue((string) null), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TreeNodeStyle ParentNodeStyle
        {
            get
            {
                if (this._parentNodeStyle == null)
                {
                    this._parentNodeStyle = new TreeNodeStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._parentNodeStyle).TrackViewState();
                    }
                }
                return this._parentNodeStyle;
            }
        }

        [DefaultValue('/'), WebSysDescription("TreeView_PathSeparator")]
        public char PathSeparator
        {
            get
            {
                object obj2 = this.ViewState["PathSeparator"];
                if (obj2 == null)
                {
                    return '/';
                }
                return (char) obj2;
            }
            set
            {
                if (value == '\0')
                {
                    this.ViewState["PathSeparator"] = null;
                }
                else
                {
                    this.ViewState["PathSeparator"] = value;
                }
                foreach (TreeNode node in this.Nodes)
                {
                    node.ResetValuePathRecursive();
                }
            }
        }

        internal string PopulateLogID
        {
            get
            {
                if (this._cachedPopulateLogID == null)
                {
                    this._cachedPopulateLogID = this.ClientID + "_PopulateLog";
                }
                return this._cachedPopulateLogID;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("TreeView_PopulateNodesFromClient"), DefaultValue(true)]
        public bool PopulateNodesFromClient
        {
            get
            {
                if ((!base.DesignMode && (this.Page != null)) && !this.Page.Request.Browser.SupportsCallback)
                {
                    return false;
                }
                object obj2 = this.ViewState["PopulateNodesFromClient"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["PopulateNodesFromClient"] = value;
            }
        }

        internal bool RenderClientScript
        {
            get
            {
                return this._renderClientScript;
            }
        }

        internal TreeNode RootNode
        {
            get
            {
                if (this._rootNode == null)
                {
                    this._rootNode = new TreeNode(this, true);
                }
                return this._rootNode;
            }
        }

        [NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("TreeView_RootNodeStyle"), DefaultValue((string) null)]
        public TreeNodeStyle RootNodeStyle
        {
            get
            {
                if (this._rootNodeStyle == null)
                {
                    this._rootNodeStyle = new TreeNodeStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._rootNodeStyle).TrackViewState();
                    }
                }
                return this._rootNodeStyle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNode SelectedNode
        {
            get
            {
                return this._selectedNode;
            }
        }

        internal string SelectedNodeFieldID
        {
            get
            {
                if (this._cachedSelectedNodeFieldID == null)
                {
                    this._cachedSelectedNodeFieldID = this.ClientID + "_SelectedNode";
                }
                return this._cachedSelectedNodeFieldID;
            }
        }

        [WebSysDescription("TreeView_SelectedNodeStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TreeNodeStyle SelectedNodeStyle
        {
            get
            {
                if (this._selectedNodeStyle == null)
                {
                    this._selectedNodeStyle = new TreeNodeStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._selectedNodeStyle).TrackViewState();
                    }
                }
                return this._selectedNodeStyle;
            }
        }

        [DefaultValue(""), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedValue
        {
            get
            {
                if (this.SelectedNode != null)
                {
                    return this.SelectedNode.Value;
                }
                return string.Empty;
            }
        }

        [WebSysDescription("TreeView_ShowCheckBoxes"), DefaultValue(0), WebCategory("Behavior")]
        public TreeNodeTypes ShowCheckBoxes
        {
            get
            {
                object obj2 = this.ViewState["ShowCheckBoxes"];
                if (obj2 == null)
                {
                    return TreeNodeTypes.None;
                }
                return (TreeNodeTypes) obj2;
            }
            set
            {
                if ((value < TreeNodeTypes.None) || (value > TreeNodeTypes.All))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ShowCheckBoxes"] = value;
            }
        }

        [DefaultValue(true), WebSysDescription("TreeView_ShowExpandCollapse"), WebCategory("Appearance")]
        public bool ShowExpandCollapse
        {
            get
            {
                object obj2 = this.ViewState["ShowExpandCollapse"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowExpandCollapse"] = value;
            }
        }

        [WebSysDescription("TreeView_ShowLines"), DefaultValue(false), WebCategory("Appearance")]
        public bool ShowLines
        {
            get
            {
                object obj2 = this.ViewState["ShowLines"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["ShowLines"] = value;
            }
        }

        [WebCategory("Accessibility"), Localizable(true), WebSysDefaultValue("TreeView_Default_SkipLinkText"), WebSysDescription("TreeView_SkipLinkText")]
        public string SkipLinkText
        {
            get
            {
                string str = this.ViewState["SkipLinkText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("TreeView_Default_SkipLinkText");
            }
            set
            {
                this.ViewState["SkipLinkText"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (!base.DesignMode)
                {
                    return HtmlTextWriterTag.Div;
                }
                return HtmlTextWriterTag.Table;
            }
        }

        [DefaultValue(""), WebSysDescription("TreeNode_Target")]
        public string Target
        {
            get
            {
                string str = (string) this.ViewState["Target"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                if (((value && (this.Page != null)) && (this.Page.IsPostBack && (this.ViewState["NeverExpanded"] != null))) && ((bool) this.ViewState["NeverExpanded"]))
                {
                    this.ExpandToDepth(this.Nodes, this.ExpandDepth);
                }
                base.Visible = value;
            }
        }

        private class TreeViewExpandDepthConverter : Int32Converter
        {
            private static object[] expandDepthValues = new object[] { 
                -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 
                15, 0x10, 0x11, 0x12, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 30
             };
            private const string fullyExpandedString = "FullyExpand";

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(int)) || ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType)));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string a = value as string;
                if ((a != null) && string.Equals(a, "FullyExpand", StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    if ((value is int) && (((int) value) == -1))
                    {
                        return "FullyExpand";
                    }
                    string a = value as string;
                    if ((a != null) && string.Equals(a, "FullyExpand", StringComparison.OrdinalIgnoreCase))
                    {
                        return value;
                    }
                }
                else if (destinationType == typeof(int))
                {
                    string str2 = value as string;
                    if ((str2 != null) && string.Equals(str2, "FullyExpand", StringComparison.OrdinalIgnoreCase))
                    {
                        return -1;
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new TypeConverter.StandardValuesCollection(expandDepthValues);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}

