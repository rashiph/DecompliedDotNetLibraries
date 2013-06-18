namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.BulletedListDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("BulletStyle"), SupportsEventValidation, DefaultEvent("Click")]
    public class BulletedList : ListControl, IPostBackEventHandler
    {
        private bool _cachedIsEnabled;
        private int _firstItem = 0;
        private int _itemCount = -1;
        private static readonly object EventClick = new object();

        [WebSysDescription("BulletedList_OnClick"), WebCategory("Action")]
        public event BulletedListEventHandler Click
        {
            add
            {
                base.Events.AddHandler(EventClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClick, value);
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            bool flag = false;
            switch (this.BulletStyle)
            {
                case System.Web.UI.WebControls.BulletStyle.Numbered:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "decimal");
                    flag = true;
                    break;

                case System.Web.UI.WebControls.BulletStyle.LowerAlpha:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "lower-alpha");
                    flag = true;
                    break;

                case System.Web.UI.WebControls.BulletStyle.UpperAlpha:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "upper-alpha");
                    flag = true;
                    break;

                case System.Web.UI.WebControls.BulletStyle.LowerRoman:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "lower-roman");
                    flag = true;
                    break;

                case System.Web.UI.WebControls.BulletStyle.UpperRoman:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "upper-roman");
                    flag = true;
                    break;

                case System.Web.UI.WebControls.BulletStyle.Disc:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "disc");
                    break;

                case System.Web.UI.WebControls.BulletStyle.Circle:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "circle");
                    break;

                case System.Web.UI.WebControls.BulletStyle.Square:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "square");
                    break;

                case System.Web.UI.WebControls.BulletStyle.CustomImage:
                {
                    string str = base.ResolveClientUrl(this.BulletImageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleImage, "url(" + HttpUtility.UrlPathEncode(str) + ")");
                    break;
                }
            }
            int firstBulletNumber = this.FirstBulletNumber;
            if (flag && (firstBulletNumber != 1))
            {
                writer.AddAttribute("start", firstBulletNumber.ToString(CultureInfo.InvariantCulture));
            }
            base.AddAttributesToRender(writer);
        }

        private string GetPostBackEventReference(string eventArgument)
        {
            if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
            {
                return ("javascript:" + Util.GetClientValidatedPostback(this, this.ValidationGroup, eventArgument));
            }
            return this.Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true);
        }

        protected virtual void OnClick(BulletedListEventArgs e)
        {
            BulletedListEventHandler handler = (BulletedListEventHandler) base.Events[EventClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            base.ValidateEvent(this.UniqueID, eventArgument);
            if (this.CausesValidation)
            {
                this.Page.Validate(this.ValidationGroup);
            }
            this.OnClick(new BulletedListEventArgs(int.Parse(eventArgument, CultureInfo.InvariantCulture)));
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Items.Count != 0)
            {
                base.Render(writer);
            }
        }

        internal void RenderAccessKey(HtmlTextWriter writer, string AccessKey)
        {
            string str = AccessKey;
            if (str.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, str);
            }
        }

        protected virtual void RenderBulletText(ListItem item, int index, HtmlTextWriter writer)
        {
            switch (this.DisplayMode)
            {
                case BulletedListDisplayMode.Text:
                    if (!item.Enabled)
                    {
                        this.RenderDisabledAttributeHelper(writer, false);
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    }
                    HttpUtility.HtmlEncode(item.Text, writer);
                    if (!item.Enabled)
                    {
                        writer.RenderEndTag();
                    }
                    return;

                case BulletedListDisplayMode.HyperLink:
                    if (!this._cachedIsEnabled || !item.Enabled)
                    {
                        this.RenderDisabledAttributeHelper(writer, item.Enabled);
                        break;
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, base.ResolveClientUrl(item.Value));
                    if (!string.IsNullOrEmpty(this.Target))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, this.Target);
                    }
                    break;

                case BulletedListDisplayMode.LinkButton:
                    if (!this._cachedIsEnabled || !item.Enabled)
                    {
                        this.RenderDisabledAttributeHelper(writer, item.Enabled);
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, this.GetPostBackEventReference(index.ToString(CultureInfo.InvariantCulture)));
                    }
                    this.RenderAccessKey(writer, this.AccessKey);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    HttpUtility.HtmlEncode(item.Text, writer);
                    writer.RenderEndTag();
                    return;

                default:
                    return;
            }
            this.RenderAccessKey(writer, this.AccessKey);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            HttpUtility.HtmlEncode(item.Text, writer);
            writer.RenderEndTag();
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            this._cachedIsEnabled = base.IsEnabled;
            if (this._itemCount == -1)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    this.Items[i].RenderAttributes(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    this.RenderBulletText(this.Items[i], i, writer);
                    writer.RenderEndTag();
                }
            }
            else
            {
                for (int j = this._firstItem; j < (this._firstItem + this._itemCount); j++)
                {
                    this.Items[j].RenderAttributes(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    this.RenderBulletText(this.Items[j], j, writer);
                    writer.RenderEndTag();
                }
            }
        }

        private void RenderDisabledAttributeHelper(HtmlTextWriter writer, bool isItemEnabled)
        {
            if (this.SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            else if (!isItemEnabled && !string.IsNullOrEmpty(WebControl.DisabledCssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, WebControl.DisabledCssClass);
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoPostBack
        {
            get
            {
                return base.AutoPostBack;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("Property_Set_Not_Supported", new object[] { "AutoPostBack", base.GetType().ToString() }));
            }
        }

        [WebCategory("Appearance"), UrlProperty, WebSysDescription("BulletedList_BulletImageUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string BulletImageUrl
        {
            get
            {
                object obj2 = this.ViewState["BulletImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["BulletImageUrl"] = value;
            }
        }

        [WebSysDescription("BulletedList_BulletStyle"), WebCategory("Appearance"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.BulletStyle BulletStyle
        {
            get
            {
                object obj2 = this.ViewState["BulletStyle"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.BulletStyle) obj2;
                }
                return System.Web.UI.WebControls.BulletStyle.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.BulletStyle.NotSet) || (value > System.Web.UI.WebControls.BulletStyle.CustomImage))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["BulletStyle"] = value;
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                return new EmptyControlCollection(this);
            }
        }

        [WebSysDescription("BulletedList_BulletedListDisplayMode"), WebCategory("Behavior"), DefaultValue(0)]
        public virtual BulletedListDisplayMode DisplayMode
        {
            get
            {
                object obj2 = this.ViewState["DisplayMode"];
                if (obj2 != null)
                {
                    return (BulletedListDisplayMode) obj2;
                }
                return BulletedListDisplayMode.Text;
            }
            set
            {
                if ((value < BulletedListDisplayMode.Text) || (value > BulletedListDisplayMode.LinkButton))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["DisplayMode"] = value;
            }
        }

        [DefaultValue(1), WebSysDescription("BulletedList_FirstBulletNumber"), WebCategory("Appearance")]
        public virtual int FirstBulletNumber
        {
            get
            {
                object obj2 = this.ViewState["FirstBulletNumber"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 1;
            }
            set
            {
                this.ViewState["FirstBulletNumber"] = value;
            }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override int SelectedIndex
        {
            get
            {
                return base.SelectedIndex;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("BulletedList_SelectionNotSupported"));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override ListItem SelectedItem
        {
            get
            {
                return base.SelectedItem;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override string SelectedValue
        {
            get
            {
                return base.SelectedValue;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("BulletedList_SelectionNotSupported"));
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return this.TagKeyInternal;
            }
        }

        internal HtmlTextWriterTag TagKeyInternal
        {
            get
            {
                switch (this.BulletStyle)
                {
                    case System.Web.UI.WebControls.BulletStyle.NotSet:
                        return HtmlTextWriterTag.Ul;

                    case System.Web.UI.WebControls.BulletStyle.Numbered:
                    case System.Web.UI.WebControls.BulletStyle.LowerAlpha:
                    case System.Web.UI.WebControls.BulletStyle.UpperAlpha:
                    case System.Web.UI.WebControls.BulletStyle.LowerRoman:
                    case System.Web.UI.WebControls.BulletStyle.UpperRoman:
                        return HtmlTextWriterTag.Ol;

                    case System.Web.UI.WebControls.BulletStyle.Disc:
                    case System.Web.UI.WebControls.BulletStyle.Circle:
                    case System.Web.UI.WebControls.BulletStyle.Square:
                        return HtmlTextWriterTag.Ul;

                    case System.Web.UI.WebControls.BulletStyle.CustomImage:
                        return HtmlTextWriterTag.Ul;
                }
                return HtmlTextWriterTag.Ol;
            }
        }

        [TypeConverter(typeof(TargetConverter)), WebSysDescription("BulletedList_Target"), DefaultValue(""), WebCategory("Behavior")]
        public virtual string Target
        {
            get
            {
                object obj2 = this.ViewState["Target"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("BulletedList_TextNotSupported"));
            }
        }
    }
}

