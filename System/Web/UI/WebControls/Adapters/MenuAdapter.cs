namespace System.Web.UI.WebControls.Adapters
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class MenuAdapter : WebControlAdapter, IPostBackEventHandler
    {
        private int _currentAccessKey;
        private Panel _menuPanel;
        private string _path;
        private MenuItem _titleItem;

        private string Escape(string path)
        {
            StringBuilder builder = null;
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < path.Length; i++)
            {
                switch (path[i])
                {
                    case '\\':
                    {
                        if (((i + 1) >= path.Length) || (path[i + 1] != '\\'))
                        {
                            break;
                        }
                        if (builder == null)
                        {
                            builder = new StringBuilder(path.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(path, startIndex, count);
                        }
                        builder.Append(@"\_\");
                        i++;
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    case '_':
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(path.Length + 5);
                        }
                        if (count > 0)
                        {
                            builder.Append(path, startIndex, count);
                        }
                        builder.Append("__");
                        startIndex = i + 1;
                        count = 0;
                        continue;
                    }
                    default:
                        goto Label_00BD;
                }
                count++;
                continue;
            Label_00BD:
                count++;
            }
            if (builder == null)
            {
                return path;
            }
            if (count > 0)
            {
                builder.Append(path, startIndex, count);
            }
            return builder.ToString();
        }

        protected internal override void LoadAdapterControlState(object state)
        {
            if (state != null)
            {
                Pair pair = state as Pair;
                if (pair != null)
                {
                    base.LoadAdapterViewState(pair.First);
                    this._path = (string) pair.Second;
                }
                else
                {
                    base.LoadAdapterViewState(null);
                    this._path = state as string;
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Control.Page.RegisterRequiresControlState(this.Control);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            this.Control.OnPreRender(e, false);
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            if (eventArgument.Length != 0)
            {
                char ch2 = eventArgument[0];
                switch (ch2)
                {
                    case 'b':
                        this.Control.InternalRaisePostBackEvent(this.UnEscape(HttpUtility.UrlDecode(eventArgument.Substring(1))));
                        return;

                    case 'o':
                    {
                        string str = this.UnEscape(HttpUtility.UrlDecode(eventArgument.Substring(1)));
                        int num = 0;
                        for (int i = 0; i < str.Length; i++)
                        {
                            if ((str[i] == '\\') && (++num >= this.Control.MaximumDepth))
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDepth"));
                            }
                        }
                        MenuItem item = this.Control.Items.FindItem(str.Split(new char[] { '\\' }), 0);
                        if (item != null)
                        {
                            if (item.ChildItems.Count > 0)
                            {
                                this._path = str;
                                return;
                            }
                            this.Control.InternalRaisePostBackEvent(str);
                        }
                        return;
                    }
                }
                if ((ch2 == 'u') && (this._path != null))
                {
                    MenuItem item2 = this.Control.Items.FindItem(this._path.Split(new char[] { '\\' }), 0);
                    if (item2 != null)
                    {
                        MenuItem parent = item2.Parent;
                        if ((parent != null) && ((item2.Depth + 1) > this.Control.StaticDisplayLevels))
                        {
                            this._path = parent.InternalValuePath;
                        }
                        else
                        {
                            this._path = null;
                        }
                    }
                }
            }
        }

        protected override void RenderBeginTag(HtmlTextWriter writer)
        {
            MenuItem rootItem;
            Menu control = this.Control;
            if (control.SkipLinkText.Length != 0)
            {
                new HyperLink { NavigateUrl = '#' + control.ClientID + "_SkipLink", ImageUrl = control.SpacerImageUrl, Text = control.SkipLinkText, Height = Unit.Pixel(1), Width = Unit.Pixel(1), Page = base.Page }.RenderControl(writer);
            }
            this._menuPanel = new Panel();
            this._menuPanel.ID = control.UniqueID;
            this._menuPanel.Page = base.Page;
            if (this._path != null)
            {
                rootItem = control.Items.FindItem(this._path.Split(new char[] { '\\' }), 0);
                this._titleItem = rootItem;
            }
            else
            {
                rootItem = control.RootItem;
            }
            SubMenuStyle subMenuStyle = control.GetSubMenuStyle(rootItem);
            if (!subMenuStyle.IsEmpty)
            {
                if ((base.Page != null) && base.Page.SupportsStyleSheets)
                {
                    string subMenuCssClassName = control.GetSubMenuCssClassName(rootItem);
                    if (subMenuCssClassName.Trim().Length > 0)
                    {
                        this._menuPanel.CssClass = subMenuCssClassName;
                    }
                }
                else
                {
                    this._menuPanel.ApplyStyle(subMenuStyle);
                }
            }
            this._menuPanel.Width = control.Width;
            this._menuPanel.Height = control.Height;
            this._menuPanel.Enabled = control.IsEnabled;
            this._menuPanel.RenderBeginTag(writer);
        }

        private void RenderBreak(HtmlTextWriter writer)
        {
            if (this.Control.Orientation == Orientation.Vertical)
            {
                writer.WriteBreak();
            }
            else
            {
                writer.Write(' ');
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            Menu control = this.Control;
            int num = 0;
            if (this._titleItem != null)
            {
                if ((this._titleItem.Depth + 1) >= control.MaximumDepth)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidDepth"));
                }
                if (!this._titleItem.IsEnabled)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Menu_InvalidNavigation"));
                }
                this.RenderItem(writer, this._titleItem, num++);
                foreach (MenuItem item in this._titleItem.ChildItems)
                {
                    this.RenderItem(writer, item, num++);
                }
                if (base.PageAdapter != null)
                {
                    base.PageAdapter.RenderPostBackEvent(writer, control.UniqueID, "u", System.Web.SR.GetString("MenuAdapter_Up"), System.Web.SR.GetString("MenuAdapter_UpOneLevel"));
                }
                else
                {
                    new HyperLink { NavigateUrl = base.Page.ClientScript.GetPostBackClientHyperlink(control, "u"), Text = System.Web.SR.GetString("MenuAdapter_UpOneLevel"), Page = base.Page }.RenderControl(writer);
                }
            }
            else
            {
                num = 1;
                this._path = null;
                foreach (MenuItem item2 in control.Items)
                {
                    this.RenderItem(writer, item2, num++);
                    if ((control.StaticDisplayLevels > 1) && (item2.ChildItems.Count > 0))
                    {
                        this.RenderContentsRecursive(writer, item2, 1, control.StaticDisplayLevels);
                    }
                }
            }
        }

        private void RenderContentsRecursive(HtmlTextWriter writer, MenuItem parentItem, int depth, int maxDepth)
        {
            int num = 1;
            foreach (MenuItem item in parentItem.ChildItems)
            {
                this.RenderItem(writer, item, num++);
                if (((depth + 1) < maxDepth) && (item.ChildItems.Count > 0))
                {
                    this.RenderContentsRecursive(writer, item, depth + 1, maxDepth);
                }
            }
        }

        protected override void RenderEndTag(HtmlTextWriter writer)
        {
            this._menuPanel.RenderEndTag(writer);
            if (this.Control.SkipLinkText.Length != 0)
            {
                new HtmlAnchor { Name = this.Control.ClientID + "_SkipLink", Page = base.Page }.RenderControl(writer);
            }
        }

        private void RenderExpand(HtmlTextWriter writer, MenuItem item, Menu owner)
        {
            string expandImageUrl = item.GetExpandImageUrl();
            if (expandImageUrl.Length > 0)
            {
                Image image = new Image {
                    ImageUrl = expandImageUrl,
                    GenerateEmptyAlternateText = true
                };
                if (item.Depth < owner.StaticDisplayLevels)
                {
                    image.AlternateText = string.Format(CultureInfo.CurrentCulture, owner.StaticPopOutImageTextFormatString, new object[] { item.Text });
                }
                else
                {
                    image.AlternateText = string.Format(CultureInfo.CurrentCulture, owner.DynamicPopOutImageTextFormatString, new object[] { item.Text });
                }
                image.ImageAlign = ImageAlign.AbsMiddle;
                image.Page = base.Page;
                image.RenderControl(writer);
            }
            else
            {
                writer.Write(' ');
                if ((item.Depth < owner.StaticDisplayLevels) && (owner.StaticPopOutImageTextFormatString.Length != 0))
                {
                    writer.Write(HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, owner.StaticPopOutImageTextFormatString, new object[] { item.Text })));
                }
                else if ((item.Depth >= owner.StaticDisplayLevels) && (owner.DynamicPopOutImageTextFormatString.Length != 0))
                {
                    writer.Write(HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, owner.DynamicPopOutImageTextFormatString, new object[] { item.Text })));
                }
                else
                {
                    writer.Write(HttpUtility.HtmlEncode(System.Web.SR.GetString("MenuAdapter_Expand", new object[] { item.Text })));
                }
            }
        }

        protected internal virtual void RenderItem(HtmlTextWriter writer, MenuItem item, int position)
        {
            bool flag4;
            string str6;
            Menu control = this.Control;
            MenuItemStyle menuItemStyle = control.GetMenuItemStyle(item);
            string imageUrl = item.ImageUrl;
            int depth = item.Depth;
            int num2 = depth + 1;
            string toolTip = item.ToolTip;
            string navigateUrl = item.NavigateUrl;
            string text = item.Text;
            bool isEnabled = item.IsEnabled;
            bool selectable = item.Selectable;
            MenuItemCollection childItems = item.ChildItems;
            string staticTopSeparatorImageUrl = null;
            if ((depth < control.StaticDisplayLevels) && (control.StaticTopSeparatorImageUrl.Length != 0))
            {
                staticTopSeparatorImageUrl = control.StaticTopSeparatorImageUrl;
            }
            else if ((depth >= control.StaticDisplayLevels) && (control.DynamicTopSeparatorImageUrl.Length != 0))
            {
                staticTopSeparatorImageUrl = control.DynamicTopSeparatorImageUrl;
            }
            if (staticTopSeparatorImageUrl != null)
            {
                new Image { ImageUrl = staticTopSeparatorImageUrl, GenerateEmptyAlternateText = true, Page = base.Page }.RenderControl(writer);
                this.RenderBreak(writer);
            }
            if (((menuItemStyle != null) && !menuItemStyle.ItemSpacing.IsEmpty) && ((this._titleItem != null) || (position != 0)))
            {
                this.RenderSpace(writer, menuItemStyle.ItemSpacing, control.Orientation);
            }
            Panel panel = new SpanPanel {
                Enabled = isEnabled,
                Page = base.Page
            };
            if ((base.Page != null) && base.Page.SupportsStyleSheets)
            {
                string cssClassName = control.GetCssClassName(item, false);
                if (cssClassName.Trim().Length > 0)
                {
                    panel.CssClass = cssClassName;
                }
            }
            else if (menuItemStyle != null)
            {
                panel.ApplyStyle(menuItemStyle);
            }
            if (item.ToolTip.Length != 0)
            {
                panel.ToolTip = item.ToolTip;
            }
            panel.RenderBeginTag(writer);
            bool flag3 = (((position != 0) && (childItems.Count != 0)) && (num2 >= control.StaticDisplayLevels)) && (num2 < control.MaximumDepth);
            if (((position != 0) && (depth > 0)) && ((control.StaticSubMenuIndent != Unit.Pixel(0)) && (depth < control.StaticDisplayLevels)))
            {
                Image image2 = new Image {
                    ImageUrl = control.SpacerImageUrl,
                    GenerateEmptyAlternateText = true
                };
                double num3 = control.StaticSubMenuIndent.Value * depth;
                if (num3 < 32767.0)
                {
                    image2.Width = new Unit(num3, control.StaticSubMenuIndent.Type);
                }
                else
                {
                    image2.Width = new Unit(32767.0, control.StaticSubMenuIndent.Type);
                }
                image2.Height = Unit.Pixel(1);
                image2.Page = base.Page;
                image2.RenderControl(writer);
            }
            if ((imageUrl.Length > 0) && item.NotTemplated())
            {
                Image image3 = new Image {
                    ImageUrl = imageUrl
                };
                if (toolTip.Length != 0)
                {
                    image3.AlternateText = toolTip;
                }
                else
                {
                    image3.GenerateEmptyAlternateText = true;
                }
                image3.Page = base.Page;
                image3.RenderControl(writer);
                writer.Write(' ');
            }
            if ((base.Page != null) && base.Page.SupportsStyleSheets)
            {
                str6 = control.GetCssClassName(item, true, out flag4);
            }
            else
            {
                str6 = string.Empty;
                flag4 = false;
            }
            if (isEnabled && (flag3 || selectable))
            {
                string accessKey = control.AccessKey;
                string str8 = (((position == 0) || ((position == 1) && (depth == 0))) && (accessKey.Length != 0)) ? accessKey : null;
                if ((navigateUrl.Length > 0) && !flag3)
                {
                    if (base.PageAdapter != null)
                    {
                        base.PageAdapter.RenderBeginHyperlink(writer, control.ResolveClientUrl(navigateUrl), true, System.Web.SR.GetString("Adapter_GoLabel"), (str8 != null) ? str8 : ((this._currentAccessKey < 10) ? this._currentAccessKey++.ToString(CultureInfo.InvariantCulture) : null));
                        writer.Write(HttpUtility.HtmlEncode(item.FormattedText));
                        base.PageAdapter.RenderEndHyperlink(writer);
                    }
                    else
                    {
                        HyperLink link = new HyperLink {
                            NavigateUrl = control.ResolveClientUrl(navigateUrl)
                        };
                        string target = item.Target;
                        if (string.IsNullOrEmpty(target))
                        {
                            target = control.Target;
                        }
                        if (!string.IsNullOrEmpty(target))
                        {
                            link.Target = target;
                        }
                        link.AccessKey = str8;
                        link.Page = base.Page;
                        if (writer is Html32TextWriter)
                        {
                            link.RenderBeginTag(writer);
                            SpanPanel panel2 = new SpanPanel {
                                Page = base.Page
                            };
                            this.RenderStyle(writer, panel2, str6, menuItemStyle, flag4);
                            panel2.RenderBeginTag(writer);
                            item.RenderText(writer);
                            panel2.RenderEndTag(writer);
                            link.RenderEndTag(writer);
                        }
                        else
                        {
                            this.RenderStyle(writer, link, str6, menuItemStyle, flag4);
                            link.RenderBeginTag(writer);
                            item.RenderText(writer);
                            link.RenderEndTag(writer);
                        }
                    }
                }
                else if (base.PageAdapter != null)
                {
                    base.PageAdapter.RenderPostBackEvent(writer, control.UniqueID, (flag3 ? 'o' : 'b') + this.Escape(item.InternalValuePath), System.Web.SR.GetString("Adapter_OKLabel"), item.FormattedText, null, (str8 != null) ? str8 : ((this._currentAccessKey < 10) ? this._currentAccessKey++.ToString(CultureInfo.InvariantCulture) : null));
                    if (flag3)
                    {
                        this.RenderExpand(writer, item, control);
                    }
                }
                else
                {
                    HyperLink link2 = new HyperLink {
                        NavigateUrl = base.Page.ClientScript.GetPostBackClientHyperlink(control, (flag3 ? 'o' : 'b') + this.Escape(item.InternalValuePath), true),
                        AccessKey = str8,
                        Page = base.Page
                    };
                    if (writer is Html32TextWriter)
                    {
                        link2.RenderBeginTag(writer);
                        SpanPanel panel3 = new SpanPanel {
                            Page = base.Page
                        };
                        this.RenderStyle(writer, panel3, str6, menuItemStyle, flag4);
                        panel3.RenderBeginTag(writer);
                        item.RenderText(writer);
                        if (flag3)
                        {
                            this.RenderExpand(writer, item, control);
                        }
                        panel3.RenderEndTag(writer);
                        link2.RenderEndTag(writer);
                    }
                    else
                    {
                        this.RenderStyle(writer, link2, str6, menuItemStyle, flag4);
                        link2.RenderBeginTag(writer);
                        item.RenderText(writer);
                        if (flag3)
                        {
                            this.RenderExpand(writer, item, control);
                        }
                        link2.RenderEndTag(writer);
                    }
                }
            }
            else
            {
                item.RenderText(writer);
            }
            panel.RenderEndTag(writer);
            this.RenderBreak(writer);
            if ((menuItemStyle != null) && !menuItemStyle.ItemSpacing.IsEmpty)
            {
                this.RenderSpace(writer, menuItemStyle.ItemSpacing, control.Orientation);
            }
            string separatorImageUrl = null;
            if (item.SeparatorImageUrl.Length != 0)
            {
                separatorImageUrl = item.SeparatorImageUrl;
            }
            else if ((depth < control.StaticDisplayLevels) && (control.StaticBottomSeparatorImageUrl.Length != 0))
            {
                separatorImageUrl = control.StaticBottomSeparatorImageUrl;
            }
            else if ((depth >= control.StaticDisplayLevels) && (control.DynamicBottomSeparatorImageUrl.Length != 0))
            {
                separatorImageUrl = control.DynamicBottomSeparatorImageUrl;
            }
            if (separatorImageUrl != null)
            {
                new Image { ImageUrl = separatorImageUrl, GenerateEmptyAlternateText = true, Page = base.Page }.RenderControl(writer);
                this.RenderBreak(writer);
            }
        }

        private void RenderSpace(HtmlTextWriter writer, Unit space, Orientation orientation)
        {
            Image image = new Image {
                ImageUrl = this.Control.SpacerImageUrl,
                GenerateEmptyAlternateText = true,
                Page = base.Page
            };
            if (orientation == Orientation.Vertical)
            {
                image.Height = space;
                image.Width = Unit.Pixel(1);
                image.RenderControl(writer);
                writer.WriteBreak();
            }
            else
            {
                image.Width = space;
                image.Height = Unit.Pixel(1);
                image.RenderControl(writer);
            }
        }

        private void RenderStyle(HtmlTextWriter writer, WebControl control, string className, MenuItemStyle style, bool applyInlineBorder)
        {
            if (!string.IsNullOrEmpty(className))
            {
                control.CssClass = className;
                if (applyInlineBorder)
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "1em");
                }
            }
            else if (style != null)
            {
                control.ApplyStyle(style);
            }
        }

        protected internal override object SaveAdapterControlState()
        {
            object x = base.SaveAdapterViewState();
            if (x != null)
            {
                return new Pair(x, this._path);
            }
            if (this._path != null)
            {
                return this._path;
            }
            return null;
        }

        internal void SetPath(string path)
        {
            this._path = path;
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        private string UnEscape(string path)
        {
            return path.Replace(@"\\", @"\").Replace(@"\_\", @"\\").Replace("__", "_");
        }

        protected Menu Control
        {
            get
            {
                return (Menu) base.Control;
            }
        }

        private class SpanPanel : Panel
        {
            protected override HtmlTextWriterTag TagKey
            {
                get
                {
                    return HtmlTextWriterTag.Span;
                }
            }
        }
    }
}

