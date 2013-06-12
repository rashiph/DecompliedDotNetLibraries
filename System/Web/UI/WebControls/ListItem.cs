namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [TypeConverter(typeof(ExpandableObjectConverter)), ParseChildren(true, "Text"), ControlBuilder(typeof(ListItemControlBuilder))]
    public sealed class ListItem : IStateManager, IParserAccessor, IAttributeAccessor
    {
        private System.Web.UI.AttributeCollection _attributes;
        private bool enabled;
        private bool enabledisdirty;
        private bool marked;
        private bool selected;
        private string text;
        private bool textisdirty;
        private string value;
        private bool valueisdirty;

        public ListItem() : this(null, null)
        {
        }

        public ListItem(string text) : this(text, null)
        {
        }

        public ListItem(string text, string value) : this(text, value, true)
        {
        }

        public ListItem(string text, string value, bool enabled)
        {
            this.text = text;
            this.value = value;
            this.enabled = enabled;
        }

        public override bool Equals(object o)
        {
            ListItem item = o as ListItem;
            if (item == null)
            {
                return false;
            }
            return (this.Value.Equals(item.Value) && this.Text.Equals(item.Text));
        }

        public static ListItem FromString(string s)
        {
            return new ListItem(s);
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this.Value.GetHashCode(), this.Text.GetHashCode());
        }

        internal void LoadViewState(object state)
        {
            if (state != null)
            {
                if (state is Triplet)
                {
                    Triplet triplet = (Triplet) state;
                    if (triplet.First != null)
                    {
                        this.Text = (string) triplet.First;
                    }
                    if (triplet.Second != null)
                    {
                        this.Value = (string) triplet.Second;
                    }
                    if (triplet.Third != null)
                    {
                        try
                        {
                            this.Enabled = (bool) triplet.Third;
                        }
                        catch
                        {
                        }
                    }
                }
                else if (state is Pair)
                {
                    Pair pair = (Pair) state;
                    if (pair.First != null)
                    {
                        this.Text = (string) pair.First;
                    }
                    this.Value = (string) pair.Second;
                }
                else
                {
                    this.Text = (string) state;
                }
            }
        }

        internal void RenderAttributes(HtmlTextWriter writer)
        {
            if (this._attributes != null)
            {
                this._attributes.AddAttributes(writer);
            }
        }

        private void ResetText()
        {
            this.Text = null;
        }

        private void ResetValue()
        {
            this.Value = null;
        }

        internal object SaveViewState()
        {
            string x = null;
            string y = null;
            if (this.textisdirty)
            {
                x = this.Text;
            }
            if (this.valueisdirty)
            {
                y = this.Value;
            }
            if (this.enabledisdirty)
            {
                return new Triplet(x, y, this.Enabled);
            }
            if (this.valueisdirty)
            {
                return new Pair(x, y);
            }
            if (this.textisdirty)
            {
                return x;
            }
            return null;
        }

        private bool ShouldSerializeText()
        {
            return ((this.text != null) && (this.text.Length != 0));
        }

        private bool ShouldSerializeValue()
        {
            return ((this.value != null) && (this.value.Length != 0));
        }

        string IAttributeAccessor.GetAttribute(string name)
        {
            return this.Attributes[name];
        }

        void IAttributeAccessor.SetAttribute(string name, string value)
        {
            this.Attributes[name] = value;
        }

        void IParserAccessor.AddParsedSubObject(object obj)
        {
            if (obj is LiteralControl)
            {
                this.Text = ((LiteralControl) obj).Text;
            }
            else
            {
                if (obj is DataBoundLiteralControl)
                {
                    throw new HttpException(System.Web.SR.GetString("Control_Cannot_Databind", new object[] { "ListItem" }));
                }
                throw new HttpException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "ListItem", obj.GetType().Name.ToString(CultureInfo.InvariantCulture) }));
            }
        }

        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        public override string ToString()
        {
            return this.Text;
        }

        internal void TrackViewState()
        {
            this.marked = true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.AttributeCollection Attributes
        {
            get
            {
                if (this._attributes == null)
                {
                    this._attributes = new System.Web.UI.AttributeCollection(new StateBag(true));
                }
                return this._attributes;
            }
        }

        internal bool Dirty
        {
            get
            {
                if (!this.textisdirty && !this.valueisdirty)
                {
                    return this.enabledisdirty;
                }
                return true;
            }
            set
            {
                this.textisdirty = value;
                this.valueisdirty = value;
                this.enabledisdirty = value;
            }
        }

        [DefaultValue(true)]
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
                if (((IStateManager) this).IsTrackingViewState)
                {
                    this.enabledisdirty = true;
                }
            }
        }

        internal bool HasAttributes
        {
            get
            {
                return ((this._attributes != null) && (this._attributes.Count > 0));
            }
        }

        [DefaultValue(false), TypeConverter(typeof(MinimizableAttributeTypeConverter))]
        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.marked;
            }
        }

        [Localizable(true), DefaultValue(""), PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty)]
        public string Text
        {
            get
            {
                if (this.text != null)
                {
                    return this.text;
                }
                if (this.value != null)
                {
                    return this.value;
                }
                return string.Empty;
            }
            set
            {
                this.text = value;
                if (((IStateManager) this).IsTrackingViewState)
                {
                    this.textisdirty = true;
                }
            }
        }

        [DefaultValue(""), Localizable(true)]
        public string Value
        {
            get
            {
                if (this.value != null)
                {
                    return this.value;
                }
                if (this.text != null)
                {
                    return this.text;
                }
                return string.Empty;
            }
            set
            {
                this.value = value;
                if (((IStateManager) this).IsTrackingViewState)
                {
                    this.valueisdirty = true;
                }
            }
        }
    }
}

