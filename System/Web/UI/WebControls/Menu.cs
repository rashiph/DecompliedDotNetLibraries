namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls.Adapters;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.MenuDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlValueProperty("SelectedValue"), DefaultEvent("MenuItemClick"), SupportsEventValidation]
    public class Menu : HierarchicalDataBoundControl, IPostBackEventHandler, INamingContainer
    {
        private bool _accessKeyRendered;
        private MenuItemBindingCollection _bindings;
        private Collection<int> _cachedLevelsContainingCssClass;
        private List<string> _cachedMenuItemClassNames;
        private List<string> _cachedMenuItemHyperLinkClassNames;
        private List<MenuItemStyle> _cachedMenuItemStyles;
        private string _cachedPopOutImageUrl;
        private string _cachedScrollDownImageUrl;
        private string _cachedScrollUpImageUrl;
        private List<string> _cachedSubMenuClassNames;
        private List<SubMenuStyle> _cachedSubMenuStyles;
        private string _currentSiteMapNodeUrl;
        private bool _dataBound;
        private Type _designTimeTextWriterType;
        private HyperLinkStyle _dynamicHoverHyperLinkStyle;
        private Style _dynamicHoverStyle;
        private MenuItemStyle _dynamicItemStyle;
        private SubMenuStyle _dynamicMenuStyle;
        private MenuItemStyle _dynamicSelectedStyle;
        private ITemplate _dynamicTemplate;
        private const string _getDesignTimeDynamicHtml = "GetDesignTimeDynamicHtml";
        private const string _getDesignTimeStaticHtml = "GetDesignTimeStaticHtml";
        private string[] _imageUrls;
        private bool _isNotIE;
        private MenuItemStyleCollection _levelMenuItemStyles;
        private MenuItemStyleCollection _levelSelectedStyles;
        private SubMenuStyleCollection _levelStyles;
        private int _maximumDepth = 0;
        private static readonly object _menuItemClickedEvent = new object();
        private static readonly object _menuItemDataBoundEvent = new object();
        private int _nodeIndex = 0;
        private PopOutPanel _panel;
        private Style _panelStyle;
        private MenuRenderer _renderer;
        private MenuRenderingMode _renderingMode;
        private MenuItem _rootItem;
        private Style _rootMenuItemStyle;
        private MenuItem _selectedItem;
        private HyperLinkStyle _staticHoverHyperLinkStyle;
        private Style _staticHoverStyle;
        private MenuItemStyle _staticItemStyle;
        private SubMenuStyle _staticMenuStyle;
        private MenuItemStyle _staticSelectedStyle;
        private ITemplate _staticTemplate;
        private bool _subControlsDataBound;
        internal const int ImageUrlsCount = 3;
        public static readonly string MenuItemClickCommandName = "Click";
        internal const int PopOutImageIndex = 2;
        internal const int ScrollDownImageIndex = 1;
        internal const int ScrollUpImageIndex = 0;

        [WebSysDescription("Menu_MenuItemClick"), WebCategory("Behavior")]
        public event MenuEventHandler MenuItemClick
        {
            add
            {
                base.Events.AddHandler(_menuItemClickedEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(_menuItemClickedEvent, value);
            }
        }

        [WebSysDescription("Menu_MenuItemDataBound"), WebCategory("Behavior")]
        public event MenuEventHandler MenuItemDataBound
        {
            add
            {
                base.Events.AddHandler(_menuItemDataBoundEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(_menuItemDataBoundEvent, value);
            }
        }

        public Menu()
        {
            this.IncludeStyleBlock = true;
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            this.VerifyRenderingInServerForm();
            string accessKey = this.AccessKey;
            try
            {
                this.AccessKey = string.Empty;
                base.AddAttributesToRender(writer);
            }
            finally
            {
                this.AccessKey = accessKey;
            }
        }

        private static bool AppendCssClassName(StringBuilder builder, MenuItemStyle style, bool hyperlink)
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

        private static void AppendMenuCssClassName(StringBuilder builder, SubMenuStyle style)
        {
            if (style != null)
            {
                if (style.CssClass.Length != 0)
                {
                    builder.Append(style.CssClass);
                    builder.Append(' ');
                }
                string registeredCssClass = style.RegisteredCssClass;
                if (registeredCssClass.Length > 0)
                {
                    builder.Append(registeredCssClass);
                    builder.Append(' ');
                }
            }
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

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            if ((this.StaticItemTemplate != null) || (this.DynamicItemTemplate != null))
            {
                if (base.RequiresDataBinding && (!string.IsNullOrEmpty(this.DataSourceID) || (this.DataSource != null)))
                {
                    this.EnsureDataBound();
                }
                else
                {
                    this.CreateChildControlsFromItems(false);
                    base.ClearChildViewState();
                }
            }
        }

        private void CreateChildControlsFromItems(bool dataBinding)
        {
            if ((this.StaticItemTemplate != null) || (this.DynamicItemTemplate != null))
            {
                int num = 0;
                foreach (MenuItem item in this.Items)
                {
                    this.CreateTemplatedControls(this.StaticItemTemplate, item, num++, 0, dataBinding);
                }
            }
        }

        internal int CreateItemIndex()
        {
            return this._nodeIndex++;
        }

        private void CreateTemplatedControls(ITemplate template, MenuItem item, int position, int depth, bool dataBinding)
        {
            if (template != null)
            {
                MenuItemTemplateContainer container = new MenuItemTemplateContainer(position, item);
                item.Container = container;
                template.InstantiateIn(container);
                this.Controls.Add(container);
                if (dataBinding)
                {
                    container.DataBind();
                }
            }
            int num = 0;
            foreach (MenuItem item2 in item.ChildItems)
            {
                int num2 = depth + 1;
                if (template == this.DynamicItemTemplate)
                {
                    this.CreateTemplatedControls(this.DynamicItemTemplate, item2, num++, num2, dataBinding);
                }
                else if (num2 < this.StaticDisplayLevels)
                {
                    this.CreateTemplatedControls(template, item2, num++, num2, dataBinding);
                }
                else if (this.DynamicItemTemplate != null)
                {
                    this.CreateTemplatedControls(this.DynamicItemTemplate, item2, num++, num2, dataBinding);
                }
            }
        }

        public sealed override void DataBind()
        {
            base.DataBind();
        }

        private void DataBindItem(MenuItem item)
        {
            HierarchicalDataSourceView data = this.GetData(item.DataPath);
            if (base.IsBoundUsingDataSourceID || (this.DataSource != null))
            {
                if (data == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Menu_DataSourceReturnedNullView", new object[] { this.ID }));
                }
                IHierarchicalEnumerable enumerable = data.Select();
                item.ChildItems.Clear();
                if (enumerable != null)
                {
                    if (base.IsBoundUsingDataSourceID)
                    {
                        SiteMapDataSource dataSource = this.GetDataSource() as SiteMapDataSource;
                        if (dataSource != null)
                        {
                            SiteMapNode currentNode = dataSource.Provider.CurrentNode;
                            if (currentNode != null)
                            {
                                this._currentSiteMapNodeUrl = currentNode.Url;
                            }
                        }
                    }
                    try
                    {
                        this.DataBindRecursive(item, enumerable);
                    }
                    finally
                    {
                        this._currentSiteMapNodeUrl = null;
                    }
                }
            }
        }

        private void DataBindRecursive(MenuItem node, IHierarchicalEnumerable enumerable)
        {
            int depth = node.Depth + 1;
            if ((this.MaximumDynamicDisplayLevels == -1) || (depth < this.MaximumDepth))
            {
                foreach (object obj2 in enumerable)
                {
                    IHierarchyData hierarchyData = enumerable.GetHierarchyData(obj2);
                    string text = null;
                    string str2 = null;
                    string navigateUrl = string.Empty;
                    string imageUrl = string.Empty;
                    string popOutImageUrl = string.Empty;
                    string separatorImageUrl = string.Empty;
                    string target = string.Empty;
                    bool result = true;
                    bool flag2 = false;
                    bool selectable = true;
                    bool flag4 = false;
                    string toolTip = string.Empty;
                    string dataMember = string.Empty;
                    dataMember = hierarchyData.Type;
                    MenuItemBinding binding = this.DataBindings.GetBinding(dataMember, depth);
                    if (binding != null)
                    {
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj2);
                        string textField = binding.TextField;
                        if (textField.Length > 0)
                        {
                            PropertyDescriptor descriptor = properties.Find(textField, true);
                            if (descriptor == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { textField, "TextField" }));
                            }
                            object obj3 = descriptor.GetValue(obj2);
                            if (obj3 != null)
                            {
                                if (binding.FormatString.Length > 0)
                                {
                                    text = string.Format(CultureInfo.CurrentCulture, binding.FormatString, new object[] { obj3 });
                                }
                                else
                                {
                                    text = obj3.ToString();
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(binding.Text))
                        {
                            text = binding.Text;
                        }
                        string valueField = binding.ValueField;
                        if (valueField.Length > 0)
                        {
                            PropertyDescriptor descriptor2 = properties.Find(valueField, true);
                            if (descriptor2 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { valueField, "ValueField" }));
                            }
                            object obj4 = descriptor2.GetValue(obj2);
                            if (obj4 != null)
                            {
                                str2 = obj4.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(binding.Value))
                        {
                            str2 = binding.Value;
                        }
                        string targetField = binding.TargetField;
                        if (targetField.Length > 0)
                        {
                            PropertyDescriptor descriptor3 = properties.Find(targetField, true);
                            if (descriptor3 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { targetField, "TargetField" }));
                            }
                            object obj5 = descriptor3.GetValue(obj2);
                            if (obj5 != null)
                            {
                                target = obj5.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(target))
                        {
                            target = binding.Target;
                        }
                        string imageUrlField = binding.ImageUrlField;
                        if (imageUrlField.Length > 0)
                        {
                            PropertyDescriptor descriptor4 = properties.Find(imageUrlField, true);
                            if (descriptor4 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { imageUrlField, "ImageUrlField" }));
                            }
                            object obj6 = descriptor4.GetValue(obj2);
                            if (obj6 != null)
                            {
                                imageUrl = obj6.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            imageUrl = binding.ImageUrl;
                        }
                        string navigateUrlField = binding.NavigateUrlField;
                        if (navigateUrlField.Length > 0)
                        {
                            PropertyDescriptor descriptor5 = properties.Find(navigateUrlField, true);
                            if (descriptor5 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { navigateUrlField, "NavigateUrlField" }));
                            }
                            object obj7 = descriptor5.GetValue(obj2);
                            if (obj7 != null)
                            {
                                navigateUrl = obj7.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(navigateUrl))
                        {
                            navigateUrl = binding.NavigateUrl;
                        }
                        string popOutImageUrlField = binding.PopOutImageUrlField;
                        if (popOutImageUrlField.Length > 0)
                        {
                            PropertyDescriptor descriptor6 = properties.Find(popOutImageUrlField, true);
                            if (descriptor6 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { popOutImageUrlField, "PopOutImageUrlField" }));
                            }
                            object obj8 = descriptor6.GetValue(obj2);
                            if (obj8 != null)
                            {
                                popOutImageUrl = obj8.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(popOutImageUrl))
                        {
                            popOutImageUrl = binding.PopOutImageUrl;
                        }
                        string separatorImageUrlField = binding.SeparatorImageUrlField;
                        if (separatorImageUrlField.Length > 0)
                        {
                            PropertyDescriptor descriptor7 = properties.Find(separatorImageUrlField, true);
                            if (descriptor7 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { separatorImageUrlField, "SeparatorImageUrlField" }));
                            }
                            object obj9 = descriptor7.GetValue(obj2);
                            if (obj9 != null)
                            {
                                separatorImageUrl = obj9.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(separatorImageUrl))
                        {
                            separatorImageUrl = binding.SeparatorImageUrl;
                        }
                        string toolTipField = binding.ToolTipField;
                        if (toolTipField.Length > 0)
                        {
                            PropertyDescriptor descriptor8 = properties.Find(toolTipField, true);
                            if (descriptor8 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { toolTipField, "ToolTipField" }));
                            }
                            object obj10 = descriptor8.GetValue(obj2);
                            if (obj10 != null)
                            {
                                toolTip = obj10.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(toolTip))
                        {
                            toolTip = binding.ToolTip;
                        }
                        string enabledField = binding.EnabledField;
                        if (enabledField.Length > 0)
                        {
                            PropertyDescriptor descriptor9 = properties.Find(enabledField, true);
                            if (descriptor9 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { enabledField, "EnabledField" }));
                            }
                            object obj11 = descriptor9.GetValue(obj2);
                            if (obj11 != null)
                            {
                                if (obj11 is bool)
                                {
                                    result = (bool) obj11;
                                    flag2 = true;
                                }
                                else if (bool.TryParse(obj11.ToString(), out result))
                                {
                                    flag2 = true;
                                }
                            }
                        }
                        if (!flag2)
                        {
                            result = binding.Enabled;
                        }
                        string selectableField = binding.SelectableField;
                        if (selectableField.Length > 0)
                        {
                            PropertyDescriptor descriptor10 = properties.Find(selectableField, true);
                            if (descriptor10 == null)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDataBinding", new object[] { selectableField, "SelectableField" }));
                            }
                            object obj12 = descriptor10.GetValue(obj2);
                            if (obj12 != null)
                            {
                                if (obj12 is bool)
                                {
                                    selectable = (bool) obj12;
                                    flag4 = true;
                                }
                                else if (bool.TryParse(obj12.ToString(), out selectable))
                                {
                                    flag4 = true;
                                }
                            }
                        }
                        if (!flag4)
                        {
                            selectable = binding.Selectable;
                        }
                    }
                    else if (obj2 is INavigateUIData)
                    {
                        INavigateUIData data2 = (INavigateUIData) obj2;
                        text = data2.Name;
                        str2 = data2.Value;
                        navigateUrl = data2.NavigateUrl;
                        if (string.IsNullOrEmpty(navigateUrl))
                        {
                            selectable = false;
                        }
                        toolTip = data2.Description;
                    }
                    if (text == null)
                    {
                        text = obj2.ToString();
                    }
                    MenuItem child = null;
                    if ((text != null) || (str2 != null))
                    {
                        child = new MenuItem(text, str2, imageUrl, navigateUrl, target);
                    }
                    if (child != null)
                    {
                        if (toolTip.Length > 0)
                        {
                            child.ToolTip = toolTip;
                        }
                        if (popOutImageUrl.Length > 0)
                        {
                            child.PopOutImageUrl = popOutImageUrl;
                        }
                        if (separatorImageUrl.Length > 0)
                        {
                            child.SeparatorImageUrl = separatorImageUrl;
                        }
                        child.Enabled = result;
                        child.Selectable = selectable;
                        child.SetDataPath(hierarchyData.Path);
                        child.SetDataBound(true);
                        node.ChildItems.Add(child);
                        if (string.Equals(hierarchyData.Path, this._currentSiteMapNodeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            child.Selected = true;
                        }
                        child.SetDataItem(hierarchyData.Item);
                        this.OnMenuItemDataBound(new MenuEventArgs(child));
                        child.SetDataItem(null);
                        if (hierarchyData.HasChildren && (depth < this.MaximumDepth))
                        {
                            IHierarchicalEnumerable children = hierarchyData.GetChildren();
                            if (children != null)
                            {
                                this.DataBindRecursive(child, children);
                            }
                        }
                    }
                }
            }
        }

        protected override void EnsureDataBound()
        {
            base.EnsureDataBound();
            if (!this._subControlsDataBound)
            {
                foreach (Control control in this.Controls)
                {
                    control.DataBind();
                }
                this._subControlsDataBound = true;
            }
        }

        internal void EnsureRootMenuStyle()
        {
            if (this._rootMenuItemStyle == null)
            {
                this._rootMenuItemStyle = new Style();
                this._rootMenuItemStyle.Font.CopyFrom(this.Font);
                if (!this.ForeColor.IsEmpty)
                {
                    this._rootMenuItemStyle.ForeColor = this.ForeColor;
                }
                if (!base.ControlStyle.IsSet(0x2000))
                {
                    this._rootMenuItemStyle.Font.Underline = false;
                }
            }
        }

        public MenuItem FindItem(string valuePath)
        {
            if (valuePath == null)
            {
                return null;
            }
            return this.Items.FindItem(valuePath.Split(new char[] { this.PathSeparator }), 0);
        }

        internal string GetCssClassName(MenuItem item, bool hyperLink)
        {
            bool flag;
            return this.GetCssClassName(item, hyperLink, out flag);
        }

        internal string GetCssClassName(MenuItem item, bool hyperlink, out bool containsClassName)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            containsClassName = false;
            int depth = item.Depth;
            string str = CacheGetItem<string>(hyperlink ? this.CachedMenuItemHyperLinkClassNames : this.CachedMenuItemClassNames, depth);
            if (this.CachedLevelsContainingCssClass.Contains(depth))
            {
                containsClassName = true;
            }
            if (!item.Selected && (str != null))
            {
                return str;
            }
            StringBuilder builder = new StringBuilder();
            if (str != null)
            {
                if (!item.Selected)
                {
                    return str;
                }
                builder.Append(str);
                builder.Append(' ');
            }
            else
            {
                if (hyperlink)
                {
                    builder.Append(this.RootMenuItemStyle.RegisteredCssClass);
                    builder.Append(' ');
                }
                if (depth < this.StaticDisplayLevels)
                {
                    containsClassName |= AppendCssClassName(builder, this._staticItemStyle, hyperlink);
                }
                else
                {
                    containsClassName |= AppendCssClassName(builder, this._dynamicItemStyle, hyperlink);
                }
                if ((depth < this.LevelMenuItemStyles.Count) && (this.LevelMenuItemStyles[depth] != null))
                {
                    containsClassName |= AppendCssClassName(builder, this.LevelMenuItemStyles[depth], hyperlink);
                }
                str = builder.ToString().Trim();
                CacheSetItem<string>(hyperlink ? this.CachedMenuItemHyperLinkClassNames : this.CachedMenuItemClassNames, depth, str);
                if (containsClassName && !this.CachedLevelsContainingCssClass.Contains(depth))
                {
                    this.CachedLevelsContainingCssClass.Add(depth);
                }
            }
            if (!item.Selected)
            {
                return str;
            }
            if (depth < this.StaticDisplayLevels)
            {
                containsClassName |= AppendCssClassName(builder, this._staticSelectedStyle, hyperlink);
            }
            else
            {
                containsClassName |= AppendCssClassName(builder, this._dynamicSelectedStyle, hyperlink);
            }
            if ((depth < this.LevelSelectedStyles.Count) && (this.LevelSelectedStyles[depth] != null))
            {
                MenuItemStyle style = this.LevelSelectedStyles[depth];
                containsClassName |= AppendCssClassName(builder, style, hyperlink);
            }
            return builder.ToString().Trim();
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override IDictionary GetDesignModeState()
        {
            IDictionary designModeState = base.GetDesignModeState();
            this.CreateChildControls();
            foreach (Control control in this.Controls)
            {
                control.DataBind();
            }
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                using (HtmlTextWriter writer2 = this.GetDesignTimeWriter(writer))
                {
                    this.Renderer.RenderBeginTag(writer2, true);
                    this.Renderer.RenderContents(writer2, true);
                    this.Renderer.RenderEndTag(writer2, true);
                    designModeState["GetDesignTimeStaticHtml"] = writer.ToString();
                }
            }
            int staticDisplayLevels = this.StaticDisplayLevels;
            try
            {
                MenuItem oneDynamicItem = this.GetOneDynamicItem(this.RootItem);
                if (oneDynamicItem == null)
                {
                    this._dataBound = false;
                    this.StaticDisplayLevels = 1;
                    oneDynamicItem = new MenuItem();
                    oneDynamicItem.SetDepth(0);
                    oneDynamicItem.SetOwner(this);
                    string text = System.Web.SR.GetString("Menu_DesignTimeDummyItemText");
                    for (int i = 0; i < 5; i++)
                    {
                        MenuItem dataItem = new MenuItem(text);
                        if (this.DynamicItemTemplate != null)
                        {
                            MenuItemTemplateContainer container = new MenuItemTemplateContainer(i, dataItem);
                            dataItem.Container = container;
                            this.DynamicItemTemplate.InstantiateIn(container);
                            container.Site = base.Site;
                            container.DataBind();
                        }
                        oneDynamicItem.ChildItems.Add(dataItem);
                    }
                    oneDynamicItem.ChildItems[1].ChildItems.Add(new MenuItem());
                    this._cachedLevelsContainingCssClass = null;
                    this._cachedMenuItemStyles = null;
                    this._cachedSubMenuStyles = null;
                    this._cachedMenuItemClassNames = null;
                    this._cachedMenuItemHyperLinkClassNames = null;
                    this._cachedSubMenuClassNames = null;
                }
                else
                {
                    oneDynamicItem = oneDynamicItem.Parent;
                }
                using (StringWriter writer3 = new StringWriter(CultureInfo.CurrentCulture))
                {
                    using (HtmlTextWriter writer4 = this.GetDesignTimeWriter(writer3))
                    {
                        base.Attributes.AddAttributes(writer4);
                        writer4.RenderBeginTag(HtmlTextWriterTag.Table);
                        writer4.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer4.RenderBeginTag(HtmlTextWriterTag.Td);
                        oneDynamicItem.Render(writer4, true, false, false);
                        writer4.RenderEndTag();
                        writer4.RenderEndTag();
                        writer4.RenderEndTag();
                        designModeState["GetDesignTimeDynamicHtml"] = writer3.ToString();
                    }
                    return designModeState;
                }
            }
            finally
            {
                if (this.StaticDisplayLevels != staticDisplayLevels)
                {
                    this.StaticDisplayLevels = staticDisplayLevels;
                }
            }
            return designModeState;
        }

        private HtmlTextWriter GetDesignTimeWriter(StringWriter stringWriter)
        {
            if (this._designTimeTextWriterType == null)
            {
                return new HtmlTextWriter(stringWriter);
            }
            ConstructorInfo constructor = this._designTimeTextWriterType.GetConstructor(new Type[] { typeof(TextWriter) });
            if (constructor == null)
            {
                return new HtmlTextWriter(stringWriter);
            }
            return (HtmlTextWriter) constructor.Invoke(new object[] { stringWriter });
        }

        internal string GetImageUrl(int index)
        {
            if (this.ImageUrls[index] == null)
            {
                switch (index)
                {
                    case 0:
                        this.ImageUrls[index] = this.ScrollUpImageUrlInternal;
                        break;

                    case 1:
                        this.ImageUrls[index] = this.ScrollDownImageUrlInternal;
                        break;

                    case 2:
                        this.ImageUrls[index] = this.PopoutImageUrlInternal;
                        break;
                }
                this.ImageUrls[index] = base.ResolveClientUrl(this.ImageUrls[index]);
            }
            return this.ImageUrls[index];
        }

        internal MenuItemStyle GetMenuItemStyle(MenuItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            int depth = item.Depth;
            MenuItemStyle style = CacheGetItem<MenuItemStyle>(this.CachedMenuItemStyles, depth);
            if (!item.Selected && (style != null))
            {
                return style;
            }
            if (style == null)
            {
                style = new MenuItemStyle();
                style.CopyFrom(this.RootMenuItemStyle);
                if (depth < this.StaticDisplayLevels)
                {
                    if (this._staticItemStyle != null)
                    {
                        TreeView.GetMergedStyle(style, this._staticItemStyle);
                    }
                }
                else if ((depth >= this.StaticDisplayLevels) && (this._dynamicItemStyle != null))
                {
                    TreeView.GetMergedStyle(style, this._dynamicItemStyle);
                }
                if ((depth < this.LevelMenuItemStyles.Count) && (this.LevelMenuItemStyles[depth] != null))
                {
                    TreeView.GetMergedStyle(style, this.LevelMenuItemStyles[depth]);
                }
                CacheSetItem<MenuItemStyle>(this.CachedMenuItemStyles, depth, style);
            }
            if (!item.Selected)
            {
                return style;
            }
            MenuItemStyle style2 = new MenuItemStyle();
            style2.CopyFrom(style);
            if (depth < this.StaticDisplayLevels)
            {
                if (this._staticSelectedStyle != null)
                {
                    TreeView.GetMergedStyle(style2, this._staticSelectedStyle);
                }
            }
            else if ((depth >= this.StaticDisplayLevels) && (this._dynamicSelectedStyle != null))
            {
                TreeView.GetMergedStyle(style2, this._dynamicSelectedStyle);
            }
            if ((depth < this.LevelSelectedStyles.Count) && (this.LevelSelectedStyles[depth] != null))
            {
                TreeView.GetMergedStyle(style2, this.LevelSelectedStyles[depth]);
            }
            return style2;
        }

        private MenuItem GetOneDynamicItem(MenuItem item)
        {
            if (item.Depth >= this.StaticDisplayLevels)
            {
                return item;
            }
            for (int i = 0; i < item.ChildItems.Count; i++)
            {
                MenuItem oneDynamicItem = this.GetOneDynamicItem(item.ChildItems[i]);
                if (oneDynamicItem != null)
                {
                    return oneDynamicItem;
                }
            }
            return null;
        }

        internal string GetSubMenuCssClassName(MenuItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            int index = item.Depth + 1;
            string str = CacheGetItem<string>(this.CachedSubMenuClassNames, index);
            if (str == null)
            {
                StringBuilder builder = new StringBuilder();
                if (index < this.StaticDisplayLevels)
                {
                    AppendMenuCssClassName(builder, this._staticMenuStyle);
                }
                else
                {
                    SubMenuStyle style = this._panelStyle as SubMenuStyle;
                    if (style != null)
                    {
                        AppendMenuCssClassName(builder, style);
                    }
                    AppendMenuCssClassName(builder, this._dynamicMenuStyle);
                }
                if ((index < this.LevelSubMenuStyles.Count) && (this.LevelSubMenuStyles[index] != null))
                {
                    SubMenuStyle style2 = this.LevelSubMenuStyles[index];
                    AppendMenuCssClassName(builder, style2);
                }
                str = builder.ToString().Trim();
                CacheSetItem<string>(this.CachedSubMenuClassNames, index, str);
            }
            return str;
        }

        internal SubMenuStyle GetSubMenuStyle(MenuItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            int index = item.Depth + 1;
            SubMenuStyle style = CacheGetItem<SubMenuStyle>(this.CachedSubMenuStyles, index);
            if (style == null)
            {
                int staticDisplayLevels = this.StaticDisplayLevels;
                if ((index >= staticDisplayLevels) && !base.DesignMode)
                {
                    style = new PopOutPanel.PopOutPanelStyle(this.Panel);
                }
                else
                {
                    style = new SubMenuStyle();
                }
                if (index < staticDisplayLevels)
                {
                    if (this._staticMenuStyle != null)
                    {
                        style.CopyFrom(this._staticMenuStyle);
                    }
                }
                else if ((index >= staticDisplayLevels) && (this._dynamicMenuStyle != null))
                {
                    style.CopyFrom(this._dynamicMenuStyle);
                }
                if (((this._levelStyles != null) && (this._levelStyles.Count > index)) && (this._levelStyles[index] != null))
                {
                    TreeView.GetMergedStyle(style, this._levelStyles[index]);
                }
                CacheSetItem<SubMenuStyle>(this.CachedSubMenuStyles, index, style);
            }
            return style;
        }

        internal void InternalRaisePostBackEvent(string eventArgument)
        {
            if (eventArgument.Length != 0)
            {
                string str = HttpUtility.HtmlDecode(eventArgument);
                int num = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    if ((str[i] == '\\') && (++num >= this.MaximumDepth))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDepth"));
                    }
                }
                MenuItem item = this.Items.FindItem(str.Split(new char[] { '\\' }), 0);
                if (item != null)
                {
                    this.OnMenuItemClick(new MenuEventArgs(item));
                }
            }
        }

        protected internal override void LoadControlState(object savedState)
        {
            Pair pair = savedState as Pair;
            if (pair == null)
            {
                base.LoadControlState(savedState);
            }
            else
            {
                base.LoadControlState(pair.First);
                this._selectedItem = null;
                if (pair.Second != null)
                {
                    string second = pair.Second as string;
                    if (second != null)
                    {
                        this._selectedItem = this.Items.FindItem(second.Split(new char[] { '\\' }), 0);
                    }
                }
            }
        }

        protected override void LoadViewState(object state)
        {
            if (state != null)
            {
                object[] objArray = (object[]) state;
                if (objArray[1] != null)
                {
                    ((IStateManager) this.StaticMenuItemStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.StaticSelectedStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.StaticHoverStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.StaticMenuStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.DynamicMenuItemStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.DynamicSelectedStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.DynamicHoverStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.DynamicMenuStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.LevelMenuItemStyles).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.LevelSelectedStyles).LoadViewState(objArray[10]);
                }
                if (objArray[11] != null)
                {
                    ((IStateManager) this.LevelSubMenuStyles).LoadViewState(objArray[11]);
                }
                if (objArray[12] != null)
                {
                    ((IStateManager) this.Items).LoadViewState(objArray[12]);
                    if (!string.IsNullOrEmpty(this.DataSourceID) || (this.DataSource != null))
                    {
                        this._dataBound = true;
                    }
                }
                if (objArray[0] != null)
                {
                    base.LoadViewState(objArray[0]);
                }
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            MenuEventArgs args = e as MenuEventArgs;
            if ((args != null) && StringUtil.EqualsIgnoreCase(args.CommandName, MenuItemClickCommandName))
            {
                if (base.IsEnabled)
                {
                    this.OnMenuItemClick(args);
                    if (base.AdapterInternal != null)
                    {
                        MenuAdapter adapterInternal = base.AdapterInternal as MenuAdapter;
                        if (adapterInternal != null)
                        {
                            MenuItem item = args.Item;
                            if (((item != null) && (item.ChildItems.Count > 0)) && ((item.Depth + 1) >= this.StaticDisplayLevels))
                            {
                                adapterInternal.SetPath(args.Item.InternalValuePath);
                            }
                        }
                    }
                    base.RaiseBubbleEvent(this, e);
                }
                return true;
            }
            if (e is CommandEventArgs)
            {
                base.RaiseBubbleEvent(this, e);
                return true;
            }
            return false;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            this.EnsureChildControls();
            base.OnDataBinding(e);
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
        }

        protected virtual void OnMenuItemClick(MenuEventArgs e)
        {
            this.SetSelectedItem(e.Item);
            MenuEventHandler handler = (MenuEventHandler) base.Events[_menuItemClickedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMenuItemDataBound(MenuEventArgs e)
        {
            MenuEventHandler handler = (MenuEventHandler) base.Events[_menuItemDataBoundEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Items.Count > 0)
            {
                this.Renderer.PreRender(base.IsEnabled);
            }
        }

        internal void OnPreRender(EventArgs e, bool registerScript)
        {
            base.OnPreRender(e);
            if (this.Items.Count > 0)
            {
                this.Renderer.PreRender(registerScript);
            }
        }

        protected internal override void PerformDataBinding()
        {
            base.PerformDataBinding();
            this.DataBindItem(this.RootItem);
            if ((!base.DesignMode && this._dataBound) && (string.IsNullOrEmpty(this.DataSourceID) && (this.DataSource == null)))
            {
                this.Items.Clear();
                this.Controls.Clear();
                base.ClearChildViewState();
                this.TrackViewState();
                base.ChildControlsCreated = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(this.DataSourceID) || (this.DataSource != null))
                {
                    this.Controls.Clear();
                    base.ClearChildState();
                    this.TrackViewState();
                    this.CreateChildControlsFromItems(true);
                    base.ChildControlsCreated = true;
                    this._dataBound = true;
                }
                else if (!this._subControlsDataBound)
                {
                    foreach (Control control in this.Controls)
                    {
                        control.DataBind();
                    }
                }
                this._subControlsDataBound = true;
            }
        }

        protected internal virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if (base.IsEnabled)
            {
                this.EnsureChildControls();
                if (base.AdapterInternal != null)
                {
                    IPostBackEventHandler adapterInternal = base.AdapterInternal as IPostBackEventHandler;
                    if (adapterInternal != null)
                    {
                        adapterInternal.RaisePostBackEvent(eventArgument);
                    }
                }
                else
                {
                    this.InternalRaisePostBackEvent(eventArgument);
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.VerifyRenderingInServerForm();
            if (this.Items.Count > 0)
            {
                this.Renderer.RenderBeginTag(writer, false);
                this.Renderer.RenderContents(writer, false);
                this.Renderer.RenderEndTag(writer, false);
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            this.Renderer.RenderBeginTag(writer, false);
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            this.Renderer.RenderContents(writer, false);
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            this.Renderer.RenderEndTag(writer, false);
        }

        internal void ResetCachedStyles()
        {
            if (this._dynamicItemStyle != null)
            {
                this._dynamicItemStyle.ResetCachedStyles();
            }
            if (this._staticItemStyle != null)
            {
                this._staticItemStyle.ResetCachedStyles();
            }
            if (this._dynamicSelectedStyle != null)
            {
                this._dynamicSelectedStyle.ResetCachedStyles();
            }
            if (this._staticSelectedStyle != null)
            {
                this._staticSelectedStyle.ResetCachedStyles();
            }
            if (this._staticHoverStyle != null)
            {
                this._staticHoverHyperLinkStyle = new HyperLinkStyle(this._staticHoverStyle);
            }
            if (this._dynamicHoverStyle != null)
            {
                this._dynamicHoverHyperLinkStyle = new HyperLinkStyle(this._dynamicHoverStyle);
            }
            foreach (MenuItemStyle style in this.LevelMenuItemStyles)
            {
                style.ResetCachedStyles();
            }
            foreach (MenuItemStyle style2 in this.LevelSelectedStyles)
            {
                style2.ResetCachedStyles();
            }
            if (this._imageUrls != null)
            {
                for (int i = 0; i < this._imageUrls.Length; i++)
                {
                    this._imageUrls[i] = null;
                }
            }
            this._cachedPopOutImageUrl = null;
            this._cachedScrollDownImageUrl = null;
            this._cachedScrollUpImageUrl = null;
            this._cachedLevelsContainingCssClass = null;
            this._cachedMenuItemClassNames = null;
            this._cachedMenuItemHyperLinkClassNames = null;
            this._cachedMenuItemStyles = null;
            this._cachedSubMenuClassNames = null;
            this._cachedSubMenuStyles = null;
        }

        protected internal override object SaveControlState()
        {
            object x = base.SaveControlState();
            if (this._selectedItem != null)
            {
                return new Pair(x, this._selectedItem.InternalValuePath);
            }
            return x;
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[13];
            objArray[0] = base.SaveViewState();
            bool flag = objArray[0] != null;
            if (this._staticItemStyle != null)
            {
                objArray[1] = ((IStateManager) this._staticItemStyle).SaveViewState();
                flag |= objArray[1] != null;
            }
            if (this._staticSelectedStyle != null)
            {
                objArray[2] = ((IStateManager) this._staticSelectedStyle).SaveViewState();
                flag |= objArray[2] != null;
            }
            if (this._staticHoverStyle != null)
            {
                objArray[3] = ((IStateManager) this._staticHoverStyle).SaveViewState();
                flag |= objArray[3] != null;
            }
            if (this._staticMenuStyle != null)
            {
                objArray[4] = ((IStateManager) this._staticMenuStyle).SaveViewState();
                flag |= objArray[4] != null;
            }
            if (this._dynamicItemStyle != null)
            {
                objArray[5] = ((IStateManager) this._dynamicItemStyle).SaveViewState();
                flag |= objArray[5] != null;
            }
            if (this._dynamicSelectedStyle != null)
            {
                objArray[6] = ((IStateManager) this._dynamicSelectedStyle).SaveViewState();
                flag |= objArray[6] != null;
            }
            if (this._dynamicHoverStyle != null)
            {
                objArray[7] = ((IStateManager) this._dynamicHoverStyle).SaveViewState();
                flag |= objArray[7] != null;
            }
            if (this._dynamicMenuStyle != null)
            {
                objArray[8] = ((IStateManager) this._dynamicMenuStyle).SaveViewState();
                flag |= objArray[8] != null;
            }
            if (this._levelMenuItemStyles != null)
            {
                objArray[9] = ((IStateManager) this._levelMenuItemStyles).SaveViewState();
                flag |= objArray[9] != null;
            }
            if (this._levelSelectedStyles != null)
            {
                objArray[10] = ((IStateManager) this._levelSelectedStyles).SaveViewState();
                flag |= objArray[10] != null;
            }
            if (this._levelStyles != null)
            {
                objArray[11] = ((IStateManager) this._levelStyles).SaveViewState();
                flag |= objArray[11] != null;
            }
            objArray[12] = ((IStateManager) this.Items).SaveViewState();
            if (flag | (objArray[12] != null))
            {
                return objArray;
            }
            return null;
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override void SetDesignModeState(IDictionary data)
        {
            if (data.Contains("DesignTimeTextWriterType"))
            {
                Type type = data["DesignTimeTextWriterType"] as Type;
                if ((type != null) && type.IsSubclassOf(typeof(HtmlTextWriter)))
                {
                    this._designTimeTextWriterType = type;
                }
            }
            base.SetDesignModeState(data);
        }

        protected void SetItemDataBound(MenuItem node, bool dataBound)
        {
            node.SetDataBound(dataBound);
        }

        protected void SetItemDataItem(MenuItem node, object dataItem)
        {
            node.SetDataItem(dataItem);
        }

        protected void SetItemDataPath(MenuItem node, string dataPath)
        {
            node.SetDataPath(dataPath);
        }

        internal void SetSelectedItem(MenuItem node)
        {
            if (this._selectedItem != node)
            {
                if (node != null)
                {
                    if (node.Depth >= this.MaximumDepth)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDepth"));
                    }
                    if (!node.IsEnabledNoOwner || !node.Selectable)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidSelection"));
                    }
                }
                if ((this._selectedItem != null) && this._selectedItem.Selected)
                {
                    this._selectedItem.SetSelected(false);
                }
                this._selectedItem = node;
                if ((this._selectedItem != null) && !this._selectedItem.Selected)
                {
                    this._selectedItem.SetSelected(true);
                }
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._staticItemStyle != null)
            {
                ((IStateManager) this._staticItemStyle).TrackViewState();
            }
            if (this._staticSelectedStyle != null)
            {
                ((IStateManager) this._staticSelectedStyle).TrackViewState();
            }
            if (this._staticHoverStyle != null)
            {
                ((IStateManager) this._staticHoverStyle).TrackViewState();
            }
            if (this._staticMenuStyle != null)
            {
                ((IStateManager) this._staticMenuStyle).TrackViewState();
            }
            if (this._dynamicItemStyle != null)
            {
                ((IStateManager) this._dynamicItemStyle).TrackViewState();
            }
            if (this._dynamicSelectedStyle != null)
            {
                ((IStateManager) this._dynamicSelectedStyle).TrackViewState();
            }
            if (this._dynamicHoverStyle != null)
            {
                ((IStateManager) this._dynamicHoverStyle).TrackViewState();
            }
            if (this._dynamicMenuStyle != null)
            {
                ((IStateManager) this._dynamicMenuStyle).TrackViewState();
            }
            if (this._levelMenuItemStyles != null)
            {
                ((IStateManager) this._levelMenuItemStyles).TrackViewState();
            }
            if (this._levelSelectedStyles != null)
            {
                ((IStateManager) this._levelSelectedStyles).TrackViewState();
            }
            if (this._levelStyles != null)
            {
                ((IStateManager) this._levelStyles).TrackViewState();
            }
            if (this._bindings != null)
            {
                ((IStateManager) this._bindings).TrackViewState();
            }
            ((IStateManager) this.Items).TrackViewState();
        }

        internal void VerifyRenderingInServerForm()
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
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

        private List<string> CachedMenuItemClassNames
        {
            get
            {
                if (this._cachedMenuItemClassNames == null)
                {
                    this._cachedMenuItemClassNames = new List<string>();
                }
                return this._cachedMenuItemClassNames;
            }
        }

        private List<string> CachedMenuItemHyperLinkClassNames
        {
            get
            {
                if (this._cachedMenuItemHyperLinkClassNames == null)
                {
                    this._cachedMenuItemHyperLinkClassNames = new List<string>();
                }
                return this._cachedMenuItemHyperLinkClassNames;
            }
        }

        private List<MenuItemStyle> CachedMenuItemStyles
        {
            get
            {
                if (this._cachedMenuItemStyles == null)
                {
                    this._cachedMenuItemStyles = new List<MenuItemStyle>();
                }
                return this._cachedMenuItemStyles;
            }
        }

        private List<string> CachedSubMenuClassNames
        {
            get
            {
                if (this._cachedSubMenuClassNames == null)
                {
                    this._cachedSubMenuClassNames = new List<string>();
                }
                return this._cachedSubMenuClassNames;
            }
        }

        private List<SubMenuStyle> CachedSubMenuStyles
        {
            get
            {
                if (this._cachedSubMenuStyles == null)
                {
                    this._cachedSubMenuStyles = new List<SubMenuStyle>();
                }
                return this._cachedSubMenuStyles;
            }
        }

        internal string ClientDataObjectID
        {
            get
            {
                return (this.ClientID + "_Data");
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [Editor("System.Web.UI.Design.WebControls.MenuBindingsEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null), MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Data"), WebSysDescription("Menu_Bindings")]
        public MenuItemBindingCollection DataBindings
        {
            get
            {
                if (this._bindings == null)
                {
                    this._bindings = new MenuItemBindingCollection(this);
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._bindings).TrackViewState();
                    }
                }
                return this._bindings;
            }
        }

        [WebCategory("Behavior"), Themeable(false), DefaultValue(500), WebSysDescription("Menu_DisappearAfter")]
        public int DisappearAfter
        {
            get
            {
                object obj2 = this.ViewState["DisappearAfter"];
                if (obj2 == null)
                {
                    return 500;
                }
                return (int) obj2;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["DisappearAfter"] = value;
            }
        }

        [WebSysDescription("Menu_DynamicBottomSeparatorImageUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Themeable(true), UrlProperty, WebCategory("Appearance")]
        public string DynamicBottomSeparatorImageUrl
        {
            get
            {
                object obj2 = this.ViewState["DynamicBottomSeparatorImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DynamicBottomSeparatorImageUrl"] = value;
            }
        }

        [WebCategory("Appearance"), DefaultValue(true), WebSysDescription("Menu_DynamicDisplayPopOutImage")]
        public bool DynamicEnableDefaultPopOutImage
        {
            get
            {
                object obj2 = this.ViewState["DynamicEnableDefaultPopOutImage"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["DynamicEnableDefaultPopOutImage"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Appearance"), WebSysDescription("Menu_DynamicHorizontalOffset")]
        public int DynamicHorizontalOffset
        {
            get
            {
                object obj2 = this.ViewState["DynamicHorizontalOffset"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                this.ViewState["DynamicHorizontalOffset"] = value;
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("Menu_DynamicHoverStyle"), WebCategory("Styles")]
        public Style DynamicHoverStyle
        {
            get
            {
                if (this._dynamicHoverStyle == null)
                {
                    this._dynamicHoverStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._dynamicHoverStyle).TrackViewState();
                    }
                }
                return this._dynamicHoverStyle;
            }
        }

        [WebSysDescription("Menu_DynamicItemFormatString"), DefaultValue(""), WebCategory("Appearance")]
        public string DynamicItemFormatString
        {
            get
            {
                object obj2 = this.ViewState["DynamicItemFormatString"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DynamicItemFormatString"] = value;
            }
        }

        [WebSysDescription("Menu_DynamicTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(MenuItemTemplateContainer))]
        public ITemplate DynamicItemTemplate
        {
            get
            {
                return this._dynamicTemplate;
            }
            set
            {
                this._dynamicTemplate = value;
            }
        }

        [DefaultValue((string) null), WebSysDescription("Menu_DynamicMenuItemStyle"), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public MenuItemStyle DynamicMenuItemStyle
        {
            get
            {
                if (this._dynamicItemStyle == null)
                {
                    this._dynamicItemStyle = new MenuItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._dynamicItemStyle).TrackViewState();
                    }
                }
                return this._dynamicItemStyle;
            }
        }

        [WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Menu_DynamicMenuStyle")]
        public SubMenuStyle DynamicMenuStyle
        {
            get
            {
                if (this._dynamicMenuStyle == null)
                {
                    this._dynamicMenuStyle = new SubMenuStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._dynamicMenuStyle).TrackViewState();
                    }
                }
                return this._dynamicMenuStyle;
            }
        }

        [WebSysDescription("Menu_DynamicPopoutImageText"), WebSysDefaultValue("MenuAdapter_Expand"), WebCategory("Appearance")]
        public string DynamicPopOutImageTextFormatString
        {
            get
            {
                object obj2 = this.ViewState["DynamicPopOutImageTextFormatString"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("MenuAdapter_Expand");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DynamicPopOutImageTextFormatString"] = value;
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Appearance"), WebSysDescription("Menu_DynamicPopoutImageUrl"), UrlProperty, DefaultValue("")]
        public string DynamicPopOutImageUrl
        {
            get
            {
                object obj2 = this.ViewState["DynamicPopOutImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DynamicPopOutImageUrl"] = value;
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Menu_DynamicSelectedStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public MenuItemStyle DynamicSelectedStyle
        {
            get
            {
                if (this._dynamicSelectedStyle == null)
                {
                    this._dynamicSelectedStyle = new MenuItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._dynamicSelectedStyle).TrackViewState();
                    }
                }
                return this._dynamicSelectedStyle;
            }
        }

        [WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("Menu_DynamicTopSeparatorImageUrl")]
        public string DynamicTopSeparatorImageUrl
        {
            get
            {
                object obj2 = this.ViewState["DynamicTopSeparatorImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DynamicTopSeparatorImageUrl"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Appearance"), WebSysDescription("Menu_DynamicVerticalOffset")]
        public int DynamicVerticalOffset
        {
            get
            {
                object obj2 = this.ViewState["DynamicVerticalOffset"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                this.ViewState["DynamicVerticalOffset"] = value;
            }
        }

        private string[] ImageUrls
        {
            get
            {
                if (this._imageUrls == null)
                {
                    this._imageUrls = new string[3];
                }
                return this._imageUrls;
            }
        }

        [DefaultValue(true), WebCategory("Appearance"), Description("Determines whether or not to render the inline style block (only used in standards compliance mode)")]
        public bool IncludeStyleBlock { get; set; }

        internal bool IsNotIE
        {
            get
            {
                return this._isNotIE;
            }
        }

        [MergableProperty(false), Editor("System.Web.UI.Design.WebControls.MenuItemCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebSysDescription("Menu_Items")]
        public MenuItemCollection Items
        {
            get
            {
                return this.RootItem.ChildItems;
            }
        }

        [DefaultValue(false), WebCategory("Appearance"), WebSysDescription("Menu_ItemWrap")]
        public bool ItemWrap
        {
            get
            {
                object obj2 = this.ViewState["ItemWrap"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["ItemWrap"] = value;
            }
        }

        [Editor("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("Menu_LevelMenuItemStyles")]
        public MenuItemStyleCollection LevelMenuItemStyles
        {
            get
            {
                if (this._levelMenuItemStyles == null)
                {
                    this._levelMenuItemStyles = new MenuItemStyleCollection();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._levelMenuItemStyles).TrackViewState();
                    }
                }
                return this._levelMenuItemStyles;
            }
        }

        [DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("Menu_LevelSelectedStyles")]
        public MenuItemStyleCollection LevelSelectedStyles
        {
            get
            {
                if (this._levelSelectedStyles == null)
                {
                    this._levelSelectedStyles = new MenuItemStyleCollection();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._levelSelectedStyles).TrackViewState();
                    }
                }
                return this._levelSelectedStyles;
            }
        }

        [WebCategory("Styles"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.SubMenuStyleCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Menu_LevelSubMenuStyles")]
        public SubMenuStyleCollection LevelSubMenuStyles
        {
            get
            {
                if (this._levelStyles == null)
                {
                    this._levelStyles = new SubMenuStyleCollection();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._levelStyles).TrackViewState();
                    }
                }
                return this._levelStyles;
            }
        }

        internal int MaximumDepth
        {
            get
            {
                if (this._maximumDepth <= 0)
                {
                    this._maximumDepth = this.MaximumDynamicDisplayLevels + this.StaticDisplayLevels;
                    if ((this._maximumDepth < this.MaximumDynamicDisplayLevels) || (this._maximumDepth < this.StaticDisplayLevels))
                    {
                        this._maximumDepth = 0x7fffffff;
                    }
                }
                return this._maximumDepth;
            }
        }

        [WebSysDescription("Menu_MaximumDynamicDisplayLevels"), DefaultValue(3), Themeable(true), WebCategory("Behavior")]
        public int MaximumDynamicDisplayLevels
        {
            get
            {
                object obj2 = this.ViewState["MaximumDynamicDisplayLevels"];
                if (obj2 == null)
                {
                    return 3;
                }
                return (int) obj2;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MaximumDynamicDisplayLevels", System.Web.SR.GetString("Menu_MaximumDynamicDisplayLevelsInvalid"));
                }
                this.ViewState["MaximumDynamicDisplayLevels"] = value;
                this._maximumDepth = 0;
                if (this._dataBound)
                {
                    this._dataBound = false;
                    this.PerformDataBinding();
                }
            }
        }

        [WebSysDescription("Menu_Orientation"), DefaultValue(1), WebCategory("Layout")]
        public System.Web.UI.WebControls.Orientation Orientation
        {
            get
            {
                object obj2 = this.ViewState["Orientation"];
                if (obj2 == null)
                {
                    return System.Web.UI.WebControls.Orientation.Vertical;
                }
                return (System.Web.UI.WebControls.Orientation) obj2;
            }
            set
            {
                this.ViewState["Orientation"] = value;
            }
        }

        internal PopOutPanel Panel
        {
            get
            {
                if (this._panel == null)
                {
                    this._panel = new PopOutPanel(this, this._panelStyle);
                    if (!base.DesignMode)
                    {
                        this._panel.Page = this.Page;
                    }
                }
                return this._panel;
            }
        }

        [WebSysDescription("Menu_PathSeparator"), DefaultValue('/')]
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
                foreach (MenuItem item in this.Items)
                {
                    item.ResetValuePathRecursive();
                }
            }
        }

        internal string PopoutImageUrlInternal
        {
            get
            {
                if (this._cachedPopOutImageUrl == null)
                {
                    this._cachedPopOutImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(Menu), "Menu_Popout.gif");
                }
                return this._cachedPopOutImageUrl;
            }
        }

        private MenuRenderer Renderer
        {
            get
            {
                if (this._renderer == null)
                {
                    switch (this.RenderingMode)
                    {
                        case MenuRenderingMode.Default:
                            if (this.RenderingCompatibility >= VersionUtil.Framework40)
                            {
                                this._renderer = new MenuRendererStandards(this);
                                break;
                            }
                            this._renderer = new MenuRendererClassic(this);
                            break;

                        case MenuRenderingMode.Table:
                            this._renderer = new MenuRendererClassic(this);
                            break;

                        case MenuRenderingMode.List:
                            this._renderer = new MenuRendererStandards(this);
                            break;
                    }
                }
                return this._renderer;
            }
        }

        [WebCategory("Layout"), DefaultValue(0), WebSysDescription("Menu_RenderingMode")]
        public MenuRenderingMode RenderingMode
        {
            get
            {
                return this._renderingMode;
            }
            set
            {
                if ((value < MenuRenderingMode.Default) || (value > MenuRenderingMode.List))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this._renderer != null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Menu_CannotChangeRenderingMode"));
                }
                this._renderingMode = value;
            }
        }

        internal MenuItem RootItem
        {
            get
            {
                if (this._rootItem == null)
                {
                    this._rootItem = new MenuItem(this, true);
                }
                return this._rootItem;
            }
        }

        internal Style RootMenuItemStyle
        {
            get
            {
                this.EnsureRootMenuStyle();
                return this._rootMenuItemStyle;
            }
        }

        [DefaultValue(""), WebCategory("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("Menu_ScrollDownImageUrl")]
        public string ScrollDownImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ScrollDownImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ScrollDownImageUrl"] = value;
            }
        }

        internal string ScrollDownImageUrlInternal
        {
            get
            {
                if (this._cachedScrollDownImageUrl == null)
                {
                    this._cachedScrollDownImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(Menu), "Menu_ScrollDown.gif");
                }
                return this._cachedScrollDownImageUrl;
            }
        }

        [WebSysDescription("Menu_ScrollDownText"), WebSysDefaultValue("Menu_ScrollDown"), Localizable(true), WebCategory("Appearance")]
        public string ScrollDownText
        {
            get
            {
                object obj2 = this.ViewState["ScrollDownText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("Menu_ScrollDown");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ScrollDownText"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("Menu_ScrollUpImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance")]
        public string ScrollUpImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ScrollUpImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ScrollUpImageUrl"] = value;
            }
        }

        internal string ScrollUpImageUrlInternal
        {
            get
            {
                if (this._cachedScrollUpImageUrl == null)
                {
                    this._cachedScrollUpImageUrl = this.Page.ClientScript.GetWebResourceUrl(typeof(Menu), "Menu_ScrollUp.gif");
                }
                return this._cachedScrollUpImageUrl;
            }
        }

        [WebSysDefaultValue("Menu_ScrollUp"), WebCategory("Appearance"), WebSysDescription("Menu_ScrollUpText"), Localizable(true)]
        public string ScrollUpText
        {
            get
            {
                object obj2 = this.ViewState["ScrollUpText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("Menu_ScrollUp");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ScrollUpText"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public MenuItem SelectedItem
        {
            get
            {
                return this._selectedItem;
            }
        }

        [DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string SelectedValue
        {
            get
            {
                if (this.SelectedItem != null)
                {
                    return this.SelectedItem.Value;
                }
                return string.Empty;
            }
        }

        [WebCategory("Accessibility"), WebSysDefaultValue("Menu_SkipLinkTextDefault"), Localizable(true), WebSysDescription("WebControl_SkipLinkText")]
        public string SkipLinkText
        {
            get
            {
                object obj2 = this.ViewState["SkipLinkText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("Menu_SkipLinkTextDefault");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["SkipLinkText"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("Menu_StaticBottomSeparatorImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance")]
        public string StaticBottomSeparatorImageUrl
        {
            get
            {
                object obj2 = this.ViewState["StaticBottomSeparatorImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["StaticBottomSeparatorImageUrl"] = value;
            }
        }

        [DefaultValue(1), WebCategory("Behavior"), Themeable(true), WebSysDescription("Menu_StaticDisplayLevels")]
        public int StaticDisplayLevels
        {
            get
            {
                object obj2 = this.ViewState["StaticDisplayLevels"];
                if (obj2 == null)
                {
                    return 1;
                }
                return (int) obj2;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["StaticDisplayLevels"] = value;
                this._maximumDepth = 0;
                if (this._dataBound && !base.DesignMode)
                {
                    this._dataBound = false;
                    this.PerformDataBinding();
                }
            }
        }

        [DefaultValue(true), WebCategory("Appearance"), WebSysDescription("Menu_StaticDisplayPopOutImage")]
        public bool StaticEnableDefaultPopOutImage
        {
            get
            {
                object obj2 = this.ViewState["StaticEnableDefaultPopOutImage"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["StaticEnableDefaultPopOutImage"] = value;
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("Menu_StaticHoverStyle"), WebCategory("Styles")]
        public Style StaticHoverStyle
        {
            get
            {
                if (this._staticHoverStyle == null)
                {
                    this._staticHoverStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._staticHoverStyle).TrackViewState();
                    }
                }
                return this._staticHoverStyle;
            }
        }

        [WebSysDescription("Menu_StaticItemFormatString"), DefaultValue(""), WebCategory("Appearance")]
        public string StaticItemFormatString
        {
            get
            {
                object obj2 = this.ViewState["StaticItemFormatString"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["StaticItemFormatString"] = value;
            }
        }

        [Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(MenuItemTemplateContainer)), WebSysDescription("Menu_StaticTemplate")]
        public ITemplate StaticItemTemplate
        {
            get
            {
                return this._staticTemplate;
            }
            set
            {
                this._staticTemplate = value;
            }
        }

        [NotifyParentProperty(true), WebCategory("Styles"), WebSysDescription("Menu_StaticMenuItemStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public MenuItemStyle StaticMenuItemStyle
        {
            get
            {
                if (this._staticItemStyle == null)
                {
                    this._staticItemStyle = new MenuItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._staticItemStyle).TrackViewState();
                    }
                }
                return this._staticItemStyle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Menu_StaticMenuStyle")]
        public SubMenuStyle StaticMenuStyle
        {
            get
            {
                if (this._staticMenuStyle == null)
                {
                    this._staticMenuStyle = new SubMenuStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._staticMenuStyle).TrackViewState();
                    }
                }
                return this._staticMenuStyle;
            }
        }

        [WebSysDefaultValue("MenuAdapter_Expand"), WebCategory("Appearance"), WebSysDescription("Menu_StaticPopoutImageText")]
        public string StaticPopOutImageTextFormatString
        {
            get
            {
                object obj2 = this.ViewState["StaticPopOutImageTextFormatString"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("MenuAdapter_Expand");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["StaticPopOutImageTextFormatString"] = value;
            }
        }

        [DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance"), WebSysDescription("Menu_StaticPopoutImageUrl")]
        public string StaticPopOutImageUrl
        {
            get
            {
                object obj2 = this.ViewState["StaticPopOutImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["StaticPopOutImageUrl"] = value;
            }
        }

        [DefaultValue((string) null), WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Menu_StaticSelectedStyle")]
        public MenuItemStyle StaticSelectedStyle
        {
            get
            {
                if (this._staticSelectedStyle == null)
                {
                    this._staticSelectedStyle = new MenuItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._staticSelectedStyle).TrackViewState();
                    }
                }
                return this._staticSelectedStyle;
            }
        }

        [WebCategory("Appearance"), DefaultValue(typeof(Unit), ""), Themeable(true), WebSysDescription("Menu_StaticSubMenuIndent")]
        public Unit StaticSubMenuIndent
        {
            get
            {
                object obj2 = this.ViewState["StaticSubMenuIndent"];
                if (obj2 == null)
                {
                    return Unit.Empty;
                }
                return (Unit) obj2;
            }
            set
            {
                if (value.Value < 0.0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["StaticSubMenuIndent"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("Menu_StaticTopSeparatorImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Appearance")]
        public string StaticTopSeparatorImageUrl
        {
            get
            {
                object obj2 = this.ViewState["StaticTopSeparatorImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["StaticTopSeparatorImageUrl"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        [DefaultValue(""), WebSysDescription("MenuItem_Target")]
        public string Target
        {
            get
            {
                object obj2 = this.ViewState["Target"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        internal abstract class MenuRenderer
        {
            protected MenuRenderer(System.Web.UI.WebControls.Menu menu)
            {
                this.Menu = menu;
            }

            public abstract void PreRender(bool registerScript);
            public virtual void Render(HtmlTextWriter writer, bool staticOnly)
            {
                this.RenderBeginTag(writer, staticOnly);
                this.RenderContents(writer, staticOnly);
                this.RenderEndTag(writer, staticOnly);
            }

            public abstract void RenderBeginTag(HtmlTextWriter writer, bool staticOnly);
            public abstract void RenderContents(HtmlTextWriter writer, bool staticOnly);
            public abstract void RenderEndTag(HtmlTextWriter writer, bool staticOnly);

            protected System.Web.UI.WebControls.Menu Menu { get; private set; }
        }

        private class MenuRendererClassic : Menu.MenuRenderer
        {
            private int _cssStyleIndex;

            public MenuRendererClassic(Menu menu) : base(menu)
            {
            }

            internal void EnsureRenderSettings()
            {
                if (base.Menu.Page != null)
                {
                    if (base.Menu.Page.Header == null)
                    {
                        if (base.Menu._staticHoverStyle != null)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("NeedHeader", new object[] { "Menu.StaticHoverStyle" }));
                        }
                        if (base.Menu._dynamicHoverStyle != null)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("NeedHeader", new object[] { "Menu.DynamicHoverStyle" }));
                        }
                    }
                    else
                    {
                        base.Menu._isNotIE = base.Menu.Page.Request.Browser.MSDomVersion.Major < 4;
                        if (base.Menu.Page.SupportsStyleSheets || ((base.Menu.Page.ScriptManager != null) && base.Menu.Page.ScriptManager.IsInAsyncPostBack))
                        {
                            base.Menu._panelStyle = base.Menu.Panel.GetEmptyPopOutPanelStyle();
                            this.RegisterStyle(base.Menu._panelStyle);
                            this.RegisterStyle(base.Menu.RootMenuItemStyle);
                            this.RegisterStyle(base.Menu.ControlStyle);
                            if (base.Menu._staticItemStyle != null)
                            {
                                base.Menu._staticItemStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(base.Menu._staticItemStyle.HyperLinkStyle);
                                this.RegisterStyle(base.Menu._staticItemStyle);
                            }
                            if (base.Menu._staticMenuStyle != null)
                            {
                                this.RegisterStyle(base.Menu._staticMenuStyle);
                            }
                            if (base.Menu._dynamicItemStyle != null)
                            {
                                base.Menu._dynamicItemStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(base.Menu._dynamicItemStyle.HyperLinkStyle);
                                this.RegisterStyle(base.Menu._dynamicItemStyle);
                            }
                            if (base.Menu._dynamicMenuStyle != null)
                            {
                                this.RegisterStyle(base.Menu._dynamicMenuStyle);
                            }
                            foreach (MenuItemStyle style in base.Menu.LevelMenuItemStyles)
                            {
                                style.HyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(style.HyperLinkStyle);
                                this.RegisterStyle(style);
                            }
                            foreach (SubMenuStyle style2 in base.Menu.LevelSubMenuStyles)
                            {
                                this.RegisterStyle(style2);
                            }
                            if (base.Menu._staticSelectedStyle != null)
                            {
                                base.Menu._staticSelectedStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(base.Menu._staticSelectedStyle.HyperLinkStyle);
                                this.RegisterStyle(base.Menu._staticSelectedStyle);
                            }
                            if (base.Menu._dynamicSelectedStyle != null)
                            {
                                base.Menu._dynamicSelectedStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(base.Menu._dynamicSelectedStyle.HyperLinkStyle);
                                this.RegisterStyle(base.Menu._dynamicSelectedStyle);
                            }
                            foreach (MenuItemStyle style3 in base.Menu.LevelSelectedStyles)
                            {
                                style3.HyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(style3.HyperLinkStyle);
                                this.RegisterStyle(style3);
                            }
                            if (base.Menu._staticHoverStyle != null)
                            {
                                base.Menu._staticHoverHyperLinkStyle = new HyperLinkStyle(base.Menu._staticHoverStyle);
                                base.Menu._staticHoverHyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(base.Menu._staticHoverHyperLinkStyle);
                                this.RegisterStyle(base.Menu._staticHoverStyle);
                            }
                            if (base.Menu._dynamicHoverStyle != null)
                            {
                                base.Menu._dynamicHoverHyperLinkStyle = new HyperLinkStyle(base.Menu._dynamicHoverStyle);
                                base.Menu._dynamicHoverHyperLinkStyle.DoNotRenderDefaults = true;
                                this.RegisterStyle(base.Menu._dynamicHoverHyperLinkStyle);
                                this.RegisterStyle(base.Menu._dynamicHoverStyle);
                            }
                        }
                    }
                }
            }

            public override void PreRender(bool registerScript)
            {
                this.EnsureRenderSettings();
                if ((base.Menu.Page != null) && registerScript)
                {
                    base.Menu.Page.RegisterWebFormsScript();
                    base.Menu.Page.ClientScript.RegisterClientScriptResource(base.Menu, typeof(Menu), "Menu.js");
                    string clientDataObjectID = base.Menu.ClientDataObjectID;
                    StringBuilder builder = new StringBuilder("var ");
                    builder.Append(clientDataObjectID);
                    builder.Append(" = new Object();\r\n");
                    builder.Append(clientDataObjectID);
                    builder.Append(".disappearAfter = ");
                    builder.Append(base.Menu.DisappearAfter);
                    builder.Append(";\r\n");
                    builder.Append(clientDataObjectID);
                    builder.Append(".horizontalOffset = ");
                    builder.Append(base.Menu.DynamicHorizontalOffset);
                    builder.Append(";\r\n");
                    builder.Append(clientDataObjectID);
                    builder.Append(".verticalOffset = ");
                    builder.Append(base.Menu.DynamicVerticalOffset);
                    builder.Append(";\r\n");
                    if (base.Menu._dynamicHoverStyle != null)
                    {
                        builder.Append(clientDataObjectID);
                        builder.Append(".hoverClass = '");
                        builder.Append(base.Menu._dynamicHoverStyle.RegisteredCssClass);
                        if (!string.IsNullOrEmpty(base.Menu._dynamicHoverStyle.CssClass))
                        {
                            if (!string.IsNullOrEmpty(base.Menu._dynamicHoverStyle.RegisteredCssClass))
                            {
                                builder.Append(' ');
                            }
                            builder.Append(base.Menu._dynamicHoverStyle.CssClass);
                        }
                        builder.Append("';\r\n");
                        if (base.Menu._dynamicHoverHyperLinkStyle != null)
                        {
                            builder.Append(clientDataObjectID);
                            builder.Append(".hoverHyperLinkClass = '");
                            builder.Append(base.Menu._dynamicHoverHyperLinkStyle.RegisteredCssClass);
                            if (!string.IsNullOrEmpty(base.Menu._dynamicHoverStyle.CssClass))
                            {
                                if (!string.IsNullOrEmpty(base.Menu._dynamicHoverHyperLinkStyle.RegisteredCssClass))
                                {
                                    builder.Append(' ');
                                }
                                builder.Append(base.Menu._dynamicHoverStyle.CssClass);
                            }
                            builder.Append("';\r\n");
                        }
                    }
                    if ((base.Menu._staticHoverStyle != null) && (base.Menu._staticHoverHyperLinkStyle != null))
                    {
                        builder.Append(clientDataObjectID);
                        builder.Append(".staticHoverClass = '");
                        builder.Append(base.Menu._staticHoverStyle.RegisteredCssClass);
                        if (!string.IsNullOrEmpty(base.Menu._staticHoverStyle.CssClass))
                        {
                            if (!string.IsNullOrEmpty(base.Menu._staticHoverStyle.RegisteredCssClass))
                            {
                                builder.Append(' ');
                            }
                            builder.Append(base.Menu._staticHoverStyle.CssClass);
                        }
                        builder.Append("';\r\n");
                        if (base.Menu._staticHoverHyperLinkStyle != null)
                        {
                            builder.Append(clientDataObjectID);
                            builder.Append(".staticHoverHyperLinkClass = '");
                            builder.Append(base.Menu._staticHoverHyperLinkStyle.RegisteredCssClass);
                            if (!string.IsNullOrEmpty(base.Menu._staticHoverStyle.CssClass))
                            {
                                if (!string.IsNullOrEmpty(base.Menu._staticHoverHyperLinkStyle.RegisteredCssClass))
                                {
                                    builder.Append(' ');
                                }
                                builder.Append(base.Menu._staticHoverStyle.CssClass);
                            }
                            builder.Append("';\r\n");
                        }
                    }
                    if ((base.Menu.Page.RequestInternal != null) && string.Equals(base.Menu.Page.Request.Url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append(clientDataObjectID);
                        builder.Append(".iframeUrl = '");
                        builder.Append(Util.QuoteJScriptString(base.Menu.Page.ClientScript.GetWebResourceUrl(typeof(Menu), "SmartNav.htm"), false));
                        builder.Append("';\r\n");
                    }
                    base.Menu.Page.ClientScript.RegisterStartupScript(base.Menu, base.GetType(), base.Menu.ClientID + "_CreateDataObject", builder.ToString(), true);
                }
            }

            private void RegisterStyle(Style style)
            {
                if ((base.Menu.Page != null) && base.Menu.Page.SupportsStyleSheets)
                {
                    string cssClass = base.Menu.ClientID + "_" + this._cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                    base.Menu.Page.Header.StyleSheet.CreateStyleRule(style, base.Menu, "." + cssClass);
                    style.SetRegisteredCssClass(cssClass);
                }
            }

            public override void RenderBeginTag(HtmlTextWriter writer, bool staticOnly)
            {
                if ((base.Menu.SkipLinkText.Length != 0) && !base.Menu.DesignMode)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, '#' + base.Menu.ClientID + "_SkipLink");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, base.Menu.SkipLinkText);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, base.Menu.SpacerImageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                base.Menu.EnsureRootMenuStyle();
                if (base.Menu.Font != null)
                {
                    base.Menu.Font.Reset();
                }
                base.Menu.ForeColor = Color.Empty;
                SubMenuStyle subMenuStyle = base.Menu.GetSubMenuStyle(base.Menu.RootItem);
                if ((base.Menu.Page != null) && base.Menu.Page.SupportsStyleSheets)
                {
                    string subMenuCssClassName = base.Menu.GetSubMenuCssClassName(base.Menu.RootItem);
                    if (subMenuCssClassName.Length > 0)
                    {
                        if (base.Menu.CssClass.Length == 0)
                        {
                            base.Menu.CssClass = subMenuCssClassName;
                        }
                        else
                        {
                            Menu menu = base.Menu;
                            menu.CssClass = menu.CssClass + ' ' + subMenuCssClassName;
                        }
                    }
                }
                else if ((subMenuStyle != null) && !subMenuStyle.IsEmpty)
                {
                    subMenuStyle.Font.Reset();
                    subMenuStyle.ForeColor = Color.Empty;
                    base.Menu.ControlStyle.CopyFrom(subMenuStyle);
                }
                base.Menu.AddAttributesToRender(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
            }

            public override void RenderContents(HtmlTextWriter writer, bool staticOnly)
            {
                if (base.Menu.Orientation == Orientation.Horizontal)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                bool isEnabled = base.Menu.IsEnabled;
                if (base.Menu.StaticDisplayLevels > 1)
                {
                    if (base.Menu.Orientation == Orientation.Vertical)
                    {
                        for (int i = 0; i < base.Menu.Items.Count; i++)
                        {
                            base.Menu.Items[i].RenderItem(writer, i, isEnabled, base.Menu.Orientation, staticOnly);
                            if (base.Menu.Items[i].ChildItems.Count != 0)
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                base.Menu.Items[i].Render(writer, isEnabled, staticOnly);
                                writer.RenderEndTag();
                                writer.RenderEndTag();
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < base.Menu.Items.Count; j++)
                        {
                            base.Menu.Items[j].RenderItem(writer, j, isEnabled, base.Menu.Orientation, staticOnly);
                            if (base.Menu.Items[j].ChildItems.Count != 0)
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                base.Menu.Items[j].Render(writer, isEnabled, staticOnly);
                                writer.RenderEndTag();
                            }
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < base.Menu.Items.Count; k++)
                    {
                        base.Menu.Items[k].RenderItem(writer, k, isEnabled, base.Menu.Orientation, staticOnly);
                    }
                }
                if (base.Menu.Orientation == Orientation.Horizontal)
                {
                    writer.RenderEndTag();
                }
                if (base.Menu.DesignMode)
                {
                    base.Menu.ResetCachedStyles();
                }
            }

            public override void RenderEndTag(HtmlTextWriter writer, bool staticOnly)
            {
                writer.RenderEndTag();
                if ((base.Menu.StaticDisplayLevels <= 1) && !staticOnly)
                {
                    bool isEnabled = base.Menu.IsEnabled;
                    for (int i = 0; i < base.Menu.Items.Count; i++)
                    {
                        base.Menu.Items[i].Render(writer, isEnabled, staticOnly);
                    }
                }
                if ((base.Menu.SkipLinkText.Length != 0) && !base.Menu.DesignMode)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, base.Menu.ClientID + "_SkipLink");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.RenderEndTag();
                }
            }
        }

        internal class MenuRendererStandards : Menu.MenuRenderer
        {
            private string _dynamicPopOutUrl;
            private string _staticPopOutUrl;

            public MenuRendererStandards(Menu menu) : base(menu)
            {
            }

            private void AddScriptReference()
            {
                string key = "_registerMenu_" + base.Menu.ClientID;
                object[] args = new object[5];
                args[0] = base.Menu.ClientID;
                args[1] = base.Menu.DisappearAfter;
                args[2] = base.Menu.Orientation.ToString().ToLowerInvariant();
                args[3] = base.Menu.TabIndex;
                bool flag = !base.Menu.IsEnabled;
                args[4] = flag.ToString().ToLowerInvariant();
                string script = string.Format(CultureInfo.InvariantCulture, "<script type='text/javascript'>new Sys.WebForms.Menu({{ element: '{0}', disappearAfter: {1}, orientation: '{2}', tabIndex: {3}, disabled: {4} }});</script>", args);
                if (base.Menu.Page.ScriptManager != null)
                {
                    base.Menu.Page.ScriptManager.RegisterClientScriptResource(base.Menu.Page, typeof(Menu), "MenuStandards.js");
                    base.Menu.Page.ScriptManager.RegisterStartupScript(base.Menu, typeof(Menu.MenuRendererStandards), key, script, false);
                }
                else
                {
                    base.Menu.Page.ClientScript.RegisterClientScriptResource(base.Menu.Page, typeof(Menu), "MenuStandards.js");
                    base.Menu.Page.ClientScript.RegisterStartupScript(typeof(Menu.MenuRendererStandards), key, script);
                }
            }

            private void AddStyleBlock()
            {
                if (base.Menu.IncludeStyleBlock)
                {
                    base.Menu.Page.Header.Controls.Add(this.CreateStyleBlock());
                }
            }

            private StyleBlock CreateStyleBlock()
            {
                StyleBlock block = new StyleBlock();
                Style rootMenuItemStyle = base.Menu.RootMenuItemStyle;
                Style style = null;
                if (!base.Menu.ControlStyle.IsEmpty)
                {
                    style = new Style();
                    style.CopyFrom(base.Menu.ControlStyle);
                    style.Font.Reset();
                    style.ForeColor = Color.Empty;
                }
                block.AddStyleDefinition("#{0}", new object[] { base.Menu.ClientID }).AddStyles(style);
                block.AddStyleDefinition("#{0} img.icon", new object[] { base.Menu.ClientID }).AddStyle(HtmlTextWriterStyle.BorderStyle, "none").AddStyle(HtmlTextWriterStyle.VerticalAlign, "middle");
                block.AddStyleDefinition("#{0} img.separator", new object[] { base.Menu.ClientID }).AddStyle(HtmlTextWriterStyle.BorderStyle, "none").AddStyle(HtmlTextWriterStyle.Display, "block");
                if (base.Menu.Orientation == Orientation.Horizontal)
                {
                    block.AddStyleDefinition("#{0} img.horizontal-separator", new object[] { base.Menu.ClientID }).AddStyle(HtmlTextWriterStyle.BorderStyle, "none").AddStyle(HtmlTextWriterStyle.VerticalAlign, "middle");
                }
                block.AddStyleDefinition("#{0} ul", new object[] { base.Menu.ClientID }).AddStyle("list-style", "none").AddStyle(HtmlTextWriterStyle.Margin, "0").AddStyle(HtmlTextWriterStyle.Padding, "0").AddStyle(HtmlTextWriterStyle.Width, "auto");
                block.AddStyleDefinition("#{0} ul.static", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._staticMenuStyle);
                StyleBlockStyles styles = block.AddStyleDefinition("#{0} ul.dynamic", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._dynamicMenuStyle).AddStyle(HtmlTextWriterStyle.ZIndex, "1");
                if (base.Menu.DynamicHorizontalOffset != 0)
                {
                    styles.AddStyle(HtmlTextWriterStyle.MarginLeft, base.Menu.DynamicHorizontalOffset.ToString(CultureInfo.InvariantCulture) + "px");
                }
                if (base.Menu.DynamicVerticalOffset != 0)
                {
                    styles.AddStyle(HtmlTextWriterStyle.MarginTop, base.Menu.DynamicVerticalOffset.ToString(CultureInfo.InvariantCulture) + "px");
                }
                if (base.Menu._levelStyles != null)
                {
                    int num = 1;
                    foreach (MenuItemStyle style3 in base.Menu._levelStyles)
                    {
                        block.AddStyleDefinition("#{0} ul.level{1}", new object[] { base.Menu.ClientID, num++ }).AddStyles(style3);
                    }
                }
                block.AddStyleDefinition("#{0} a", new object[] { base.Menu.ClientID }).AddStyle(HtmlTextWriterStyle.WhiteSpace, "nowrap").AddStyle(HtmlTextWriterStyle.Display, "block").AddStyles(rootMenuItemStyle);
                StyleBlockStyles styles2 = block.AddStyleDefinition("#{0} a.static", new object[] { base.Menu.ClientID });
                if ((base.Menu.Orientation == Orientation.Horizontal) && ((base.Menu._staticItemStyle == null) || base.Menu._staticItemStyle.HorizontalPadding.IsEmpty))
                {
                    styles2.AddStyle(HtmlTextWriterStyle.PaddingLeft, "0.15em").AddStyle(HtmlTextWriterStyle.PaddingRight, "0.15em");
                }
                styles2.AddStyles(base.Menu._staticItemStyle);
                if (base.Menu._staticItemStyle != null)
                {
                    styles2.AddStyles(base.Menu._staticItemStyle.HyperLinkStyle);
                }
                if (!string.IsNullOrEmpty(this.StaticPopOutUrl))
                {
                    block.AddStyleDefinition("#{0} a.popout", new object[] { base.Menu.ClientID }).AddStyle("background-image", "url(\"" + base.Menu.ResolveClientUrl(this.StaticPopOutUrl).Replace("\"", "\\\"") + "\")").AddStyle("background-repeat", "no-repeat").AddStyle("background-position", "right center").AddStyle(HtmlTextWriterStyle.PaddingRight, "14px");
                }
                if (!string.IsNullOrEmpty(this.DynamicPopOutUrl) && (this.DynamicPopOutUrl != this.StaticPopOutUrl))
                {
                    block.AddStyleDefinition("#{0} a.popout-dynamic", new object[] { base.Menu.ClientID }).AddStyle("background", "url(\"" + base.Menu.ResolveClientUrl(this.DynamicPopOutUrl).Replace("\"", "\\\"") + "\") no-repeat right center").AddStyle(HtmlTextWriterStyle.PaddingRight, "14px");
                }
                StyleBlockStyles styles3 = block.AddStyleDefinition("#{0} a.dynamic", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._dynamicItemStyle);
                if (base.Menu._dynamicItemStyle != null)
                {
                    styles3.AddStyles(base.Menu._dynamicItemStyle.HyperLinkStyle);
                }
                if ((base.Menu._levelMenuItemStyles != null) || (base.Menu.StaticDisplayLevels > 1))
                {
                    int staticDisplayLevels = base.Menu.StaticDisplayLevels;
                    if (base.Menu._levelMenuItemStyles != null)
                    {
                        staticDisplayLevels = Math.Max(staticDisplayLevels, base.Menu._levelMenuItemStyles.Count);
                    }
                    for (int i = 0; i < staticDisplayLevels; i++)
                    {
                        StyleBlockStyles styles4 = block.AddStyleDefinition("#{0} a.level{1}", new object[] { base.Menu.ClientID, i + 1 });
                        if ((i > 0) && (i < base.Menu.StaticDisplayLevels))
                        {
                            Unit staticSubMenuIndent = base.Menu.StaticSubMenuIndent;
                            if (staticSubMenuIndent.IsEmpty && (base.Menu.Orientation == Orientation.Vertical))
                            {
                                staticSubMenuIndent = new Unit(1.0, UnitType.Em);
                            }
                            if (!staticSubMenuIndent.IsEmpty && (staticSubMenuIndent.Value != 0.0))
                            {
                                double num4 = staticSubMenuIndent.Value * i;
                                if (num4 < 32767.0)
                                {
                                    staticSubMenuIndent = new Unit(num4, staticSubMenuIndent.Type);
                                }
                                else
                                {
                                    staticSubMenuIndent = new Unit(32767.0, staticSubMenuIndent.Type);
                                }
                                styles4.AddStyle(HtmlTextWriterStyle.PaddingLeft, staticSubMenuIndent.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                        if ((base.Menu._levelMenuItemStyles != null) && (i < base.Menu._levelMenuItemStyles.Count))
                        {
                            MenuItemStyle style4 = base.Menu._levelMenuItemStyles[i];
                            styles4.AddStyles(style4).AddStyles(style4.HyperLinkStyle);
                        }
                    }
                }
                styles3 = block.AddStyleDefinition("#{0} a.static.selected", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._staticSelectedStyle);
                if (base.Menu._staticSelectedStyle != null)
                {
                    styles3.AddStyles(base.Menu._staticSelectedStyle.HyperLinkStyle);
                }
                styles3 = block.AddStyleDefinition("#{0} a.dynamic.selected", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._dynamicSelectedStyle);
                if (base.Menu._dynamicSelectedStyle != null)
                {
                    styles3.AddStyles(base.Menu._dynamicSelectedStyle.HyperLinkStyle);
                }
                block.AddStyleDefinition("#{0} a.static.highlighted", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._staticHoverStyle);
                block.AddStyleDefinition("#{0} a.dynamic.highlighted", new object[] { base.Menu.ClientID }).AddStyles(base.Menu._dynamicHoverStyle);
                if (base.Menu._levelSelectedStyles != null)
                {
                    int num5 = 1;
                    foreach (MenuItemStyle style5 in base.Menu._levelSelectedStyles)
                    {
                        block.AddStyleDefinition("#{0} a.selected.level{1}", new object[] { base.Menu.ClientID, num5++ }).AddStyles(style5).AddStyles(style5.HyperLinkStyle);
                    }
                }
                return block;
            }

            private string GetCssClass(int level, Style staticStyle, Style dynamicStyle, IList levelStyles)
            {
                Style style;
                string str = "level" + level;
                if (level > base.Menu.StaticDisplayLevels)
                {
                    style = dynamicStyle;
                }
                else
                {
                    if (base.Menu.DesignMode)
                    {
                        str = str + " static";
                        if (base.Menu.Orientation == Orientation.Horizontal)
                        {
                            str = str + " horizontal";
                        }
                    }
                    style = staticStyle;
                }
                if ((style != null) && !string.IsNullOrEmpty(style.CssClass))
                {
                    str = str + " " + style.CssClass;
                }
                if ((levelStyles != null) && (levelStyles.Count >= level))
                {
                    Style style2 = (Style) levelStyles[level - 1];
                    if ((style2 != null) && !string.IsNullOrEmpty(style2.CssClass))
                    {
                        str = str + " " + style2.CssClass;
                    }
                }
                return str;
            }

            protected virtual string GetDynamicPopOutImageUrl()
            {
                string dynamicPopOutImageUrl = base.Menu.DynamicPopOutImageUrl;
                if (string.IsNullOrEmpty(dynamicPopOutImageUrl) && base.Menu.DynamicEnableDefaultPopOutImage)
                {
                    dynamicPopOutImageUrl = base.Menu.GetImageUrl(2);
                }
                return dynamicPopOutImageUrl;
            }

            private string GetMenuCssClass(int level)
            {
                return this.GetCssClass(level, base.Menu.StaticMenuStyle, base.Menu.DynamicMenuStyle, base.Menu._levelStyles);
            }

            private string GetMenuItemCssClass(MenuItem item, int level)
            {
                string str = null;
                if (this.ShouldHavePopOutImage(item))
                {
                    if (level > base.Menu.StaticDisplayLevels)
                    {
                        if (!string.IsNullOrEmpty(this.DynamicPopOutUrl))
                        {
                            str = (this.DynamicPopOutUrl == this.StaticPopOutUrl) ? "popout" : "popout-dynamic";
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.StaticPopOutUrl))
                    {
                        str = "popout";
                    }
                }
                string str2 = this.GetCssClass(level, base.Menu.StaticMenuItemStyle, base.Menu.DynamicMenuItemStyle, base.Menu._levelMenuItemStyles);
                if (!string.IsNullOrEmpty(str))
                {
                    return (str + " " + str2);
                }
                return str2;
            }

            protected virtual string GetPostBackEventReference(MenuItem item)
            {
                return base.Menu.Page.ClientScript.GetPostBackEventReference(base.Menu, item.InternalValuePath, true);
            }

            protected virtual string GetStaticPopOutImageUrl()
            {
                string staticPopOutImageUrl = base.Menu.StaticPopOutImageUrl;
                if (string.IsNullOrEmpty(staticPopOutImageUrl) && base.Menu.StaticEnableDefaultPopOutImage)
                {
                    staticPopOutImageUrl = base.Menu.GetImageUrl(2);
                }
                return staticPopOutImageUrl;
            }

            private bool IsChildDepthDynamic(MenuItem item)
            {
                return ((item.Depth + 1) >= base.Menu.StaticDisplayLevels);
            }

            private bool IsChildPastMaximumDepth(MenuItem item)
            {
                return ((item.Depth + 1) >= base.Menu.MaximumDepth);
            }

            private bool IsDepthDynamic(MenuItem item)
            {
                return (item.Depth >= base.Menu.StaticDisplayLevels);
            }

            private bool IsDepthStatic(MenuItem item)
            {
                return !this.IsDepthDynamic(item);
            }

            public override void PreRender(bool registerScript)
            {
                if (!base.Menu.DesignMode && (base.Menu.Page != null))
                {
                    if (base.Menu.IncludeStyleBlock && (base.Menu.Page.Header == null))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("NeedHeader", new object[] { "Menu.IncludeStyleBlock" }));
                    }
                    this.AddScriptReference();
                    this.AddStyleBlock();
                }
            }

            public override void RenderBeginTag(HtmlTextWriter writer, bool staticOnly)
            {
                if (!string.IsNullOrEmpty(base.Menu.SkipLinkText) && !base.Menu.DesignMode)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, '#' + base.Menu.ClientID + "_SkipLink");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, base.Menu.SkipLinkText);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this.SpacerImageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                if (base.Menu.DesignMode && base.Menu.IncludeStyleBlock)
                {
                    this.CreateStyleBlock().Render(writer);
                }
                if (base.Menu.HasAttributes)
                {
                    foreach (string str in base.Menu.Attributes.Keys)
                    {
                        writer.AddAttribute(str, base.Menu.Attributes[str]);
                    }
                }
                string str2 = base.Menu.CssClass ?? "";
                if (!base.Menu.Enabled)
                {
                    str2 = (str2 + " " + WebControl.DisabledCssClass).Trim();
                }
                if (!string.IsNullOrEmpty(str2))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, str2);
                }
                if (base.Menu.DesignMode)
                {
                    writer.AddStyleAttribute("float", "left");
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Id, base.Menu.ClientID);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            public override void RenderContents(HtmlTextWriter writer, bool staticOnly)
            {
                this.RenderItems(writer, (staticOnly || base.Menu.DesignMode) || !base.Menu.Enabled, base.Menu.Items, 1, !string.IsNullOrEmpty(base.Menu.AccessKey));
            }

            public override void RenderEndTag(HtmlTextWriter writer, bool staticOnly)
            {
                writer.RenderEndTag();
                if (base.Menu.DesignMode)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "clear: left");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.RenderEndTag();
                }
                else if (!string.IsNullOrEmpty(base.Menu.SkipLinkText))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, base.Menu.ClientID + "_SkipLink");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.RenderEndTag();
                }
            }

            private bool RenderItem(HtmlTextWriter writer, MenuItem item, int level, string cssClass, bool needsAccessKey)
            {
                this.RenderItemPreSeparator(writer, item);
                if (base.Menu.DesignMode && (base.Menu.Orientation == Orientation.Horizontal))
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                }
                needsAccessKey = this.RenderItemLinkAttributes(writer, item, level, cssClass, needsAccessKey);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                this.RenderItemIcon(writer, item);
                item.RenderText(writer);
                writer.RenderEndTag();
                this.RenderItemPostSeparator(writer, item);
                return needsAccessKey;
            }

            private void RenderItemIcon(HtmlTextWriter writer, MenuItem item)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl) && item.NotTemplated())
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, base.Menu.ResolveClientUrl(item.ImageUrl));
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, item.ToolTip);
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "icon");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }
            }

            private bool RenderItemLinkAttributes(HtmlTextWriter writer, MenuItem item, int level, string cssClass, bool needsAccessKey)
            {
                if (!string.IsNullOrEmpty(item.ToolTip))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
                }
                if (!item.Enabled || !base.Menu.Enabled)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass + " " + WebControl.DisabledCssClass);
                    return needsAccessKey;
                }
                if (!item.Selectable)
                {
                    return needsAccessKey;
                }
                if (item.Selected)
                {
                    cssClass = cssClass + " selected";
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
                if (needsAccessKey)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, base.Menu.AccessKey);
                }
                if (string.IsNullOrEmpty(item.NavigateUrl))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, this.GetPostBackEventReference(item));
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, base.Menu.ResolveClientUrl(item.NavigateUrl));
                    string target = item.Target;
                    if (string.IsNullOrEmpty(target))
                    {
                        target = base.Menu.Target;
                    }
                    if (!string.IsNullOrEmpty(target))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
                    }
                }
                return false;
            }

            private void RenderItemPostSeparator(HtmlTextWriter writer, MenuItem item)
            {
                string separatorImageUrl = item.SeparatorImageUrl;
                if (string.IsNullOrEmpty(separatorImageUrl))
                {
                    separatorImageUrl = this.IsDepthStatic(item) ? base.Menu.StaticBottomSeparatorImageUrl : base.Menu.DynamicBottomSeparatorImageUrl;
                }
                if (!string.IsNullOrEmpty(separatorImageUrl))
                {
                    this.RenderItemSeparatorImage(writer, item, separatorImageUrl);
                }
            }

            private void RenderItemPreSeparator(HtmlTextWriter writer, MenuItem item)
            {
                string str = this.IsDepthStatic(item) ? base.Menu.StaticTopSeparatorImageUrl : base.Menu.DynamicTopSeparatorImageUrl;
                if (!string.IsNullOrEmpty(str))
                {
                    this.RenderItemSeparatorImage(writer, item, str);
                }
            }

            private void RenderItems(HtmlTextWriter writer, bool staticOnly, MenuItemCollection items, int level, bool needsAccessKey)
            {
                if ((level == 1) || (level > base.Menu.StaticDisplayLevels))
                {
                    if (base.Menu.DesignMode && (base.Menu.Orientation == Orientation.Horizontal))
                    {
                        writer.AddStyleAttribute("float", "left");
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.GetMenuCssClass(level));
                    writer.RenderBeginTag(HtmlTextWriterTag.Ul);
                }
                foreach (MenuItem item in items)
                {
                    if (base.Menu.DesignMode && (base.Menu.Orientation == Orientation.Horizontal))
                    {
                        writer.AddStyleAttribute("float", "left");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    needsAccessKey = this.RenderItem(writer, item, level, this.GetMenuItemCssClass(item, level), needsAccessKey);
                    if (level < base.Menu.StaticDisplayLevels)
                    {
                        writer.RenderEndTag();
                    }
                    if ((((item.ChildItems.Count > 0) && !this.IsChildPastMaximumDepth(item)) && item.Enabled) && ((level < base.Menu.StaticDisplayLevels) || !staticOnly))
                    {
                        this.RenderItems(writer, staticOnly, item.ChildItems, level + 1, needsAccessKey);
                    }
                    if (level >= base.Menu.StaticDisplayLevels)
                    {
                        writer.RenderEndTag();
                    }
                }
                if ((level == 1) || (level > base.Menu.StaticDisplayLevels))
                {
                    writer.RenderEndTag();
                }
            }

            private void RenderItemSeparatorImage(HtmlTextWriter writer, MenuItem item, string separatorImageUrl)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, separatorImageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, (this.IsDepthStatic(item) && (base.Menu.Orientation == Orientation.Horizontal)) ? "horizontal-separator" : "separator");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            private bool ShouldHavePopOutImage(MenuItem item)
            {
                return (((item.ChildItems.Count > 0) && this.IsChildDepthDynamic(item)) && !this.IsChildPastMaximumDepth(item));
            }

            private string DynamicPopOutUrl
            {
                get
                {
                    if (this._dynamicPopOutUrl == null)
                    {
                        this._dynamicPopOutUrl = this.GetDynamicPopOutImageUrl();
                    }
                    return this._dynamicPopOutUrl;
                }
            }

            protected virtual string SpacerImageUrl
            {
                get
                {
                    return base.Menu.SpacerImageUrl;
                }
            }

            private string StaticPopOutUrl
            {
                get
                {
                    if (this._staticPopOutUrl == null)
                    {
                        this._staticPopOutUrl = this.GetStaticPopOutImageUrl();
                    }
                    return this._staticPopOutUrl;
                }
            }
        }
    }
}

