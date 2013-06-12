namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [ParseChildren(true, "ChildItems")]
    public sealed class MenuItem : IStateManager, ICloneable
    {
        private MenuItemCollection _childItems;
        private MenuItemTemplateContainer _container;
        private object _dataItem;
        private int _depth;
        internal string _id;
        private int _index;
        private string _internalValuePath;
        private bool _isRoot;
        private bool _isTrackingViewState;
        private Menu _owner;
        private MenuItem _parent;
        private int _selectDesired;
        private string _valuePath;
        private StateBag _viewState;
        private static readonly Unit HorizontalDefaultSpacing = Unit.Pixel(3);

        public MenuItem()
        {
            this._id = string.Empty;
            this._depth = -2;
            this._selectDesired = 0;
        }

        public MenuItem(string text) : this(text, null, null, null, null)
        {
        }

        public MenuItem(string text, string value) : this(text, value, null, null, null)
        {
        }

        internal MenuItem(Menu owner, bool isRoot) : this()
        {
            this._owner = owner;
            this._isRoot = isRoot;
        }

        public MenuItem(string text, string value, string imageUrl) : this(text, value, imageUrl, null, null)
        {
        }

        public MenuItem(string text, string value, string imageUrl, string navigateUrl) : this(text, value, imageUrl, navigateUrl, null)
        {
        }

        public MenuItem(string text, string value, string imageUrl, string navigateUrl, string target) : this()
        {
            if (text != null)
            {
                this.Text = text;
            }
            if (value != null)
            {
                this.Value = value;
            }
            if (!string.IsNullOrEmpty(imageUrl))
            {
                this.ImageUrl = imageUrl;
            }
            if (!string.IsNullOrEmpty(navigateUrl))
            {
                this.NavigateUrl = navigateUrl;
            }
            if (!string.IsNullOrEmpty(target))
            {
                this.Target = target;
            }
        }

        internal string GetExpandImageUrl()
        {
            if (this.ChildItems.Count > 0)
            {
                if (this.PopOutImageUrl.Length != 0)
                {
                    return this._owner.ResolveClientUrl(this.PopOutImageUrl);
                }
                if (this.Depth < this._owner.StaticDisplayLevels)
                {
                    if (this._owner.StaticPopOutImageUrl.Length != 0)
                    {
                        return this._owner.ResolveClientUrl(this._owner.StaticPopOutImageUrl);
                    }
                    if (this._owner.StaticEnableDefaultPopOutImage)
                    {
                        return this._owner.GetImageUrl(2);
                    }
                }
                else
                {
                    if (this._owner.DynamicPopOutImageUrl.Length != 0)
                    {
                        return this._owner.ResolveClientUrl(this._owner.DynamicPopOutImageUrl);
                    }
                    if (this._owner.DynamicEnableDefaultPopOutImage)
                    {
                        return this._owner.GetImageUrl(2);
                    }
                }
            }
            return string.Empty;
        }

        private void NotifyOwnerSelected()
        {
            object obj2 = this.ViewState["Selected"];
            bool flag = (obj2 != null) && ((bool) obj2);
            if (this._owner == null)
            {
                this._selectDesired = flag ? 1 : -1;
            }
            else if (flag)
            {
                this._owner.SetSelectedItem(this);
            }
            else if (this == this._owner.SelectedItem)
            {
                this._owner.SetSelectedItem(null);
            }
        }

        internal bool NotTemplated()
        {
            if ((this._owner.StaticItemTemplate != null) && (this.Depth < this._owner.StaticDisplayLevels))
            {
                return false;
            }
            if (this._owner.DynamicItemTemplate != null)
            {
                return (this.Depth < this._owner.StaticDisplayLevels);
            }
            return true;
        }

        internal void Render(HtmlTextWriter writer, bool enabled, bool staticOnly)
        {
            this.Render(writer, enabled, staticOnly, true);
        }

        internal void Render(HtmlTextWriter writer, bool enabled, bool staticOnly, bool recursive)
        {
            enabled = enabled && this.Enabled;
            int num = this.Depth + 1;
            if ((this.ChildItems.Count > 0) && (num < this._owner.MaximumDepth))
            {
                SubMenuStyle subMenuStyle = this._owner.GetSubMenuStyle(this);
                string subMenuCssClassName = null;
                if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
                {
                    subMenuCssClassName = this._owner.GetSubMenuCssClassName(this);
                }
                if (num >= this._owner.StaticDisplayLevels)
                {
                    if ((!staticOnly && enabled) && (!this._owner.DesignMode || !recursive))
                    {
                        PopOutPanel panel = this._owner.Panel;
                        if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
                        {
                            panel.ScrollerClass = this._owner.GetCssClassName(this.ChildItems[0], false);
                            panel.ScrollerStyle = null;
                        }
                        else
                        {
                            panel.ScrollerClass = null;
                            panel.ScrollerStyle = this._owner.GetMenuItemStyle(this.ChildItems[0]);
                        }
                        if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
                        {
                            panel.CssClass = subMenuCssClassName;
                            panel.SetInternalStyle(null);
                        }
                        else if (!subMenuStyle.IsEmpty)
                        {
                            panel.CssClass = string.Empty;
                            panel.SetInternalStyle(subMenuStyle);
                        }
                        else
                        {
                            panel.CssClass = string.Empty;
                            panel.SetInternalStyle(null);
                            panel.BackColor = Color.Empty;
                        }
                        panel.ID = this.Id + "Items";
                        panel.RenderBeginTag(writer);
                        writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                        writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                        writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                        writer.RenderBeginTag(HtmlTextWriterTag.Table);
                        for (int i = 0; i < this.ChildItems.Count; i++)
                        {
                            this.ChildItems[i].RenderItem(writer, i, enabled, Orientation.Vertical);
                        }
                        writer.RenderEndTag();
                        panel.RenderEndTag(writer);
                        if (recursive)
                        {
                            for (int j = 0; j < this.ChildItems.Count; j++)
                            {
                                this.ChildItems[j].Render(writer, enabled, false);
                            }
                        }
                    }
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
                    {
                        if ((subMenuCssClassName != null) && (subMenuCssClassName.Length > 0))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, subMenuCssClassName);
                        }
                    }
                    else
                    {
                        subMenuStyle.AddAttributesToRender(writer);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    if (this._owner.Orientation == Orientation.Horizontal)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    }
                    bool flag = (num + 1) < this._owner.StaticDisplayLevels;
                    bool flag2 = (num + 1) < this._owner.MaximumDepth;
                    for (int k = 0; k < this.ChildItems.Count; k++)
                    {
                        if (((recursive && (this.ChildItems[k].ChildItems.Count != 0)) && ((enabled && this.ChildItems[k].Enabled) || flag)) && flag2)
                        {
                            if (flag)
                            {
                                this.ChildItems[k].RenderItem(writer, k, enabled, this._owner.Orientation);
                                if (this._owner.Orientation == Orientation.Vertical)
                                {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                    this.ChildItems[k].Render(writer, enabled, staticOnly);
                                    writer.RenderEndTag();
                                    writer.RenderEndTag();
                                }
                                else
                                {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                    this.ChildItems[k].Render(writer, enabled, staticOnly);
                                    writer.RenderEndTag();
                                }
                            }
                            else
                            {
                                this.ChildItems[k].RenderItem(writer, k, enabled, this._owner.Orientation, staticOnly);
                            }
                        }
                        else
                        {
                            this.ChildItems[k].RenderItem(writer, k, enabled, this._owner.Orientation);
                        }
                    }
                    if (this._owner.Orientation == Orientation.Horizontal)
                    {
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag();
                    if ((!flag && !staticOnly) && (recursive && flag2))
                    {
                        for (int m = 0; m < this.ChildItems.Count; m++)
                        {
                            if (((this.ChildItems[m].ChildItems.Count != 0) && enabled) && this.ChildItems[m].Enabled)
                            {
                                this.ChildItems[m].Render(writer, enabled, false, true);
                            }
                        }
                    }
                }
            }
        }

        internal void RenderItem(HtmlTextWriter writer, int position, bool enabled, Orientation orientation)
        {
            this.RenderItem(writer, position, enabled, orientation, false);
        }

        internal void RenderItem(HtmlTextWriter writer, int position, bool enabled, Orientation orientation, bool staticOnly)
        {
            enabled = enabled && this.Enabled;
            int depth = this.Depth;
            MenuItemStyle menuItemStyle = this._owner.GetMenuItemStyle(this);
            int num2 = this.Depth + 1;
            bool flag = (depth < this._owner.StaticDisplayLevels) && (this._owner.StaticTopSeparatorImageUrl.Length != 0);
            bool flag2 = (depth >= this._owner.StaticDisplayLevels) && (this._owner.DynamicTopSeparatorImageUrl.Length != 0);
            if (flag || flag2)
            {
                if (orientation == Orientation.Vertical)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (flag)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(this._owner.StaticTopSeparatorImageUrl));
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(this._owner.DynamicTopSeparatorImageUrl));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
                if (orientation == Orientation.Vertical)
                {
                    writer.RenderEndTag();
                }
            }
            if (((menuItemStyle != null) && !menuItemStyle.ItemSpacing.IsEmpty) && ((depth != 0) || (position != 0)))
            {
                this.RenderItemSpacing(writer, menuItemStyle.ItemSpacing, orientation);
            }
            if (!staticOnly && this._owner.Enabled)
            {
                if (num2 > this._owner.StaticDisplayLevels)
                {
                    if ((this.Selectable && this.Enabled) || (this.ChildItems.Count != 0))
                    {
                        writer.AddAttribute("onmouseover", "Menu_HoverDynamic(this)");
                        this.RenderItemEvents(writer);
                    }
                    else
                    {
                        writer.AddAttribute("onmouseover", "Menu_HoverDisabled(this)");
                        writer.AddAttribute("onmouseout", "Menu_Unhover(this)");
                    }
                }
                else if (num2 == this._owner.StaticDisplayLevels)
                {
                    if ((this.Selectable && this.Enabled) || (this.ChildItems.Count != 0))
                    {
                        writer.AddAttribute("onmouseover", "Menu_HoverStatic(this)");
                        this.RenderItemEvents(writer);
                    }
                }
                else if (this.Selectable && this.Enabled)
                {
                    writer.AddAttribute("onmouseover", "Menu_HoverRoot(this)");
                    this.RenderItemEvents(writer);
                }
            }
            if (this.ToolTip.Length != 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.ToolTip);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.Id);
            if (orientation == Orientation.Vertical)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
            {
                string cssClassName = this._owner.GetCssClassName(this, false);
                if (cssClassName.Trim().Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClassName);
                }
            }
            else if (menuItemStyle != null)
            {
                menuItemStyle.AddAttributesToRender(writer);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (!this._owner.ItemWrap)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            if (orientation == Orientation.Vertical)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if ((this._owner.Page != null) && this._owner.Page.SupportsStyleSheets)
            {
                bool flag3;
                string str2 = this._owner.GetCssClassName(this, true, out flag3);
                if (str2.Trim().Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, str2);
                    if (flag3)
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "1em");
                    }
                }
            }
            else if (menuItemStyle != null)
            {
                menuItemStyle.HyperLinkStyle.AddAttributesToRender(writer);
            }
            string accessKey = this._owner.AccessKey;
            if (enabled && this.Selectable)
            {
                if (this.NavigateUrl.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, this._owner.ResolveClientUrl(this.NavigateUrl));
                    string target = this.ViewState["Target"] as string;
                    if (target == null)
                    {
                        target = this._owner.Target;
                    }
                    if (target.Length > 0)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
                    }
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, this._owner.Page.ClientScript.GetPostBackClientHyperlink(this._owner, this.InternalValuePath, true, true));
                }
                if (!this._owner.AccessKeyRendered && (accessKey.Length != 0))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey, true);
                    this._owner.AccessKeyRendered = true;
                }
            }
            else if (!enabled)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "true");
            }
            else if ((this.ChildItems.Count != 0) && (num2 >= this._owner.StaticDisplayLevels))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "text");
                if (!this._owner.AccessKeyRendered && (accessKey.Length != 0))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey, true);
                    this._owner.AccessKeyRendered = true;
                }
            }
            if ((depth != 0) && (depth < this._owner.StaticDisplayLevels))
            {
                Unit staticSubMenuIndent = this._owner.StaticSubMenuIndent;
                if (staticSubMenuIndent.IsEmpty)
                {
                    staticSubMenuIndent = Unit.Pixel(0x10);
                }
                if (staticSubMenuIndent.Value != 0.0)
                {
                    double num3 = staticSubMenuIndent.Value * depth;
                    if (num3 < 32767.0)
                    {
                        staticSubMenuIndent = new Unit(num3, staticSubMenuIndent.Type);
                    }
                    else
                    {
                        staticSubMenuIndent = new Unit(32767.0, staticSubMenuIndent.Type);
                    }
                    writer.AddStyleAttribute("margin-left", staticSubMenuIndent.ToString(CultureInfo.InvariantCulture));
                }
            }
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            if ((this.ImageUrl.Length > 0) && this.NotTemplated())
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(this.ImageUrl));
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.ToolTip);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.AddStyleAttribute("vertical-align", "middle");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
            this.RenderText(writer);
            writer.RenderEndTag();
            bool flag4 = (num2 >= this._owner.StaticDisplayLevels) && (num2 < this._owner.MaximumDepth);
            string str5 = flag4 ? this.GetExpandImageUrl() : string.Empty;
            bool flag5 = false;
            if ((((orientation == Orientation.Horizontal) && (depth < this._owner.StaticDisplayLevels)) && (!flag4 || (str5.Length == 0))) && ((menuItemStyle == null) || menuItemStyle.ItemSpacing.IsEmpty))
            {
                if (((this.Depth + 1) < this._owner.StaticDisplayLevels) && (this.ChildItems.Count != 0))
                {
                    flag5 = true;
                }
                else
                {
                    for (MenuItem item = this; item != null; item = item.Parent)
                    {
                        if ((((item.Parent == null) && (this._owner.Items.Count != 0)) && (item != this._owner.Items[this._owner.Items.Count - 1])) || (((item.Parent != null) && (item.Parent.ChildItems.Count != 0)) && (item != item.Parent.ChildItems[item.Parent.ChildItems.Count - 1])))
                        {
                            flag5 = true;
                            break;
                        }
                    }
                }
            }
            writer.RenderEndTag();
            if (flag4 && (str5.Length > 0))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, str5);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.AddStyleAttribute(HtmlTextWriterStyle.VerticalAlign, "middle");
                if (depth < this._owner.StaticDisplayLevels)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Format(CultureInfo.CurrentCulture, this._owner.StaticPopOutImageTextFormatString, new object[] { this.Text }));
                }
                else if (depth >= this._owner.StaticDisplayLevels)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Format(CultureInfo.CurrentCulture, this._owner.DynamicPopOutImageTextFormatString, new object[] { this.Text }));
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            if (orientation == Orientation.Vertical)
            {
                writer.RenderEndTag();
            }
            if ((menuItemStyle != null) && !menuItemStyle.ItemSpacing.IsEmpty)
            {
                this.RenderItemSpacing(writer, menuItemStyle.ItemSpacing, orientation);
            }
            else if (flag5)
            {
                this.RenderItemSpacing(writer, HorizontalDefaultSpacing, orientation);
            }
            bool flag6 = this.SeparatorImageUrl.Length != 0;
            bool flag7 = (depth < this._owner.StaticDisplayLevels) && (this._owner.StaticBottomSeparatorImageUrl.Length != 0);
            bool flag8 = (depth >= this._owner.StaticDisplayLevels) && (this._owner.DynamicBottomSeparatorImageUrl.Length != 0);
            if ((flag6 || flag7) || flag8)
            {
                if (orientation == Orientation.Vertical)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (flag6)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(this.SeparatorImageUrl));
                }
                else if (flag7)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(this._owner.StaticBottomSeparatorImageUrl));
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, this._owner.ResolveClientUrl(this._owner.DynamicBottomSeparatorImageUrl));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
                if (orientation == Orientation.Vertical)
                {
                    writer.RenderEndTag();
                }
            }
        }

        private void RenderItemEvents(HtmlTextWriter writer)
        {
            writer.AddAttribute("onmouseout", "Menu_Unhover(this)");
            if (this._owner.IsNotIE)
            {
                writer.AddAttribute("onkeyup", "Menu_Key(event)");
            }
            else
            {
                writer.AddAttribute("onkeyup", "Menu_Key(this)");
            }
        }

        private void RenderItemSpacing(HtmlTextWriter writer, Unit spacing, Orientation orientation)
        {
            if (orientation == Orientation.Vertical)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, spacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, spacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
            }
        }

        internal void RenderText(HtmlTextWriter writer)
        {
            if ((this.Container != null) && (((this._owner.StaticItemTemplate != null) && (this.Depth < this._owner.StaticDisplayLevels)) || ((this._owner.DynamicItemTemplate != null) && (this.Depth >= this._owner.StaticDisplayLevels))))
            {
                this.Container.RenderControl(writer);
            }
            else
            {
                writer.Write(this.FormattedText);
            }
        }

        internal void ResetValuePathRecursive()
        {
            if (this._valuePath != null)
            {
                this._valuePath = null;
                foreach (MenuItem item in this.ChildItems)
                {
                    item.ResetValuePathRecursive();
                }
            }
        }

        internal void SetDataBound(bool dataBound)
        {
            this.ViewState["DataBound"] = dataBound;
        }

        internal void SetDataItem(object dataItem)
        {
            this._dataItem = dataItem;
        }

        internal void SetDataPath(string dataPath)
        {
            this.ViewState["DataPath"] = dataPath;
        }

        internal void SetDepth(int depth)
        {
            this._depth = depth;
        }

        internal void SetDirty()
        {
            this.ViewState.SetDirty(true);
            if (this.ChildItems.Count > 0)
            {
                this.ChildItems.SetDirty();
            }
        }

        internal void SetOwner(Menu owner)
        {
            this._owner = owner;
            if (this._selectDesired == 1)
            {
                this._selectDesired = 0;
                this.Selected = true;
            }
            else if (this._selectDesired == -1)
            {
                this._selectDesired = 0;
                this.Selected = false;
            }
            foreach (MenuItem item in this.ChildItems)
            {
                item.SetOwner(this._owner);
            }
        }

        internal void SetParent(MenuItem parent)
        {
            this._parent = parent;
            this.SetPath(null);
        }

        internal void SetPath(string newPath)
        {
            this._internalValuePath = newPath;
            this._depth = -2;
        }

        internal void SetSelected(bool value)
        {
            this.ViewState["Selected"] = value;
            if (this._owner == null)
            {
                this._selectDesired = value ? 1 : -1;
            }
        }

        object ICloneable.Clone()
        {
            return new MenuItem { Enabled = this.Enabled, ImageUrl = this.ImageUrl, NavigateUrl = this.NavigateUrl, PopOutImageUrl = this.PopOutImageUrl, Selectable = this.Selectable, Selected = this.Selected, SeparatorImageUrl = this.SeparatorImageUrl, Target = this.Target, Text = this.Text, ToolTip = this.ToolTip, Value = this.Value };
        }

        void IStateManager.LoadViewState(object state)
        {
            object[] objArray = (object[]) state;
            if (objArray != null)
            {
                if (objArray[0] != null)
                {
                    ((IStateManager) this.ViewState).LoadViewState(objArray[0]);
                }
                this.NotifyOwnerSelected();
                if (objArray[1] != null)
                {
                    ((IStateManager) this.ChildItems).LoadViewState(objArray[1]);
                }
            }
        }

        object IStateManager.SaveViewState()
        {
            object[] objArray = new object[2];
            if (this._viewState != null)
            {
                objArray[0] = ((IStateManager) this._viewState).SaveViewState();
            }
            if (this._childItems != null)
            {
                objArray[1] = ((IStateManager) this._childItems).SaveViewState();
            }
            if ((objArray[0] == null) && (objArray[1] == null))
            {
                return null;
            }
            return objArray;
        }

        void IStateManager.TrackViewState()
        {
            this._isTrackingViewState = true;
            if (this._viewState != null)
            {
                ((IStateManager) this._viewState).TrackViewState();
            }
            if (this._childItems != null)
            {
                ((IStateManager) this._childItems).TrackViewState();
            }
        }

        [MergableProperty(false), Browsable(false), PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public MenuItemCollection ChildItems
        {
            get
            {
                if (this._childItems == null)
                {
                    this._childItems = new MenuItemCollection(this);
                }
                return this._childItems;
            }
        }

        internal MenuItemTemplateContainer Container
        {
            get
            {
                return this._container;
            }
            set
            {
                this._container = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DefaultValue(false)]
        public bool DataBound
        {
            get
            {
                object obj2 = this.ViewState["DataBound"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
        }

        [Browsable(false), DefaultValue((string) null)]
        public object DataItem
        {
            get
            {
                return this._dataItem;
            }
        }

        [DefaultValue(""), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DataPath
        {
            get
            {
                object obj2 = this.ViewState["DataPath"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Depth
        {
            get
            {
                if (this._depth == -2)
                {
                    if (this._isRoot)
                    {
                        return -1;
                    }
                    if (this.Parent == null)
                    {
                        return 0;
                    }
                    this._depth = this.Parent.Depth + 1;
                }
                return this._depth;
            }
        }

        [DefaultValue(true), Browsable(true), WebSysDescription("MenuItem_Enabled")]
        public bool Enabled
        {
            get
            {
                object obj2 = this.ViewState["Enabled"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["Enabled"] = value;
            }
        }

        internal string FormattedText
        {
            get
            {
                if ((this._owner.StaticItemFormatString.Length > 0) && (this.Depth < this._owner.StaticDisplayLevels))
                {
                    return string.Format(CultureInfo.CurrentCulture, this._owner.StaticItemFormatString, new object[] { this.Text });
                }
                if ((this._owner.DynamicItemFormatString.Length > 0) && (this.Depth >= this._owner.StaticDisplayLevels))
                {
                    return string.Format(CultureInfo.CurrentCulture, this._owner.DynamicItemFormatString, new object[] { this.Text });
                }
                return this.Text;
            }
        }

        internal string Id
        {
            get
            {
                if (this._id.Length == 0)
                {
                    this.Index = this._owner.CreateItemIndex();
                    this._id = this._owner.ClientID + 'n' + this.Index;
                }
                return this._id;
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebSysDescription("MenuItem_ImageUrl"), UrlProperty]
        public string ImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ImageUrl"] = value;
            }
        }

        internal int Index
        {
            get
            {
                return this._index;
            }
            set
            {
                this._index = value;
            }
        }

        internal string InternalValuePath
        {
            get
            {
                if (this._internalValuePath == null)
                {
                    if (this._parent == null)
                    {
                        return string.Empty;
                    }
                    List<string> list = new List<string> {
                        TreeView.Escape(this.Value)
                    };
                    for (MenuItem item = this._parent; (item != null) && !item._isRoot; item = item._parent)
                    {
                        if (item._internalValuePath != null)
                        {
                            list.Add(item._internalValuePath);
                            break;
                        }
                        list.Add(TreeView.Escape(item.Value));
                    }
                    list.Reverse();
                    this._internalValuePath = string.Join('\\'.ToString(), list.ToArray());
                }
                return this._internalValuePath;
            }
        }

        internal bool IsEnabled
        {
            get
            {
                return (this.IsEnabledNoOwner && this.Owner.IsEnabled);
            }
        }

        internal bool IsEnabledNoOwner
        {
            get
            {
                for (MenuItem item = this; item != null; item = item.Parent)
                {
                    if (!item.Enabled)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [DefaultValue(""), WebSysDescription("MenuItem_NavigateUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
        public string NavigateUrl
        {
            get
            {
                object obj2 = this.ViewState["NavigateUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        internal Menu Owner
        {
            get
            {
                return this._owner;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public MenuItem Parent
        {
            get
            {
                if ((this._parent != null) && !this._parent._isRoot)
                {
                    return this._parent;
                }
                return null;
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), UrlProperty, WebSysDescription("MenuItem_PopOutImageUrl")]
        public string PopOutImageUrl
        {
            get
            {
                object obj2 = this.ViewState["PopOutImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["PopOutImageUrl"] = value;
            }
        }

        [WebSysDescription("MenuItem_Selectable"), DefaultValue(true), Browsable(true)]
        public bool Selectable
        {
            get
            {
                object obj2 = this.ViewState["Selectable"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["Selectable"] = value;
            }
        }

        [WebSysDescription("MenuItem_Selected"), DefaultValue(false), Browsable(true)]
        public bool Selected
        {
            get
            {
                object obj2 = this.ViewState["Selected"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.SetSelected(value);
                this.NotifyOwnerSelected();
            }
        }

        [DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebSysDescription("MenuItem_SeparatorImageUrl")]
        public string SeparatorImageUrl
        {
            get
            {
                object obj2 = this.ViewState["SeparatorImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["SeparatorImageUrl"] = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        [WebSysDescription("MenuItem_Target"), DefaultValue("")]
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

        [WebSysDescription("MenuItem_Text"), Localizable(true), DefaultValue("")]
        public string Text
        {
            get
            {
                object obj2 = this.ViewState["Text"];
                if (obj2 == null)
                {
                    obj2 = this.ViewState["Value"];
                    if (obj2 == null)
                    {
                        return string.Empty;
                    }
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }

        [DefaultValue(""), Localizable(true), WebSysDescription("MenuItem_ToolTip")]
        public string ToolTip
        {
            get
            {
                object obj2 = this.ViewState["ToolTip"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ToolTip"] = value;
            }
        }

        [Localizable(true), DefaultValue(""), WebSysDescription("MenuItem_Value")]
        public string Value
        {
            get
            {
                object obj2 = this.ViewState["Value"];
                if (obj2 == null)
                {
                    obj2 = this.ViewState["Text"];
                    if (obj2 == null)
                    {
                        return string.Empty;
                    }
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["Value"] = value;
                this.ResetValuePathRecursive();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValuePath
        {
            get
            {
                if (this._valuePath == null)
                {
                    if (this._parent == null)
                    {
                        return string.Empty;
                    }
                    string valuePath = this._parent.ValuePath;
                    this._valuePath = ((valuePath.Length == 0) && (this._parent.Depth == -1)) ? this.Value : (valuePath + this._owner.PathSeparator + this.Value);
                }
                return this._valuePath;
            }
        }

        private StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag();
                    if (this._isTrackingViewState)
                    {
                        ((IStateManager) this._viewState).TrackViewState();
                    }
                }
                return this._viewState;
            }
        }
    }
}

