namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class AppearanceEditorPart : EditorPart
    {
        private DropDownList _chromeType;
        private string _chromeTypeErrorMessage;
        private DropDownList _direction;
        private string _directionErrorMessage;
        private UnitInput _height;
        private string _heightErrorMessage;
        private CheckBox _hidden;
        private string _hiddenErrorMessage;
        private TextBox _title;
        private string _titleErrorMessage;
        private UnitInput _width;
        private string _widthErrorMessage;
        private const int MaxUnitValue = 0x7fff;
        private const int MinUnitValue = 0;
        private const int TextBoxColumns = 30;

        public override bool ApplyChanges()
        {
            WebPart webPartToEdit = base.WebPartToEdit;
            if (webPartToEdit != null)
            {
                this.EnsureChildControls();
                bool allowLayoutChange = webPartToEdit.Zone.AllowLayoutChange;
                try
                {
                    webPartToEdit.Title = this._title.Text;
                }
                catch (Exception exception)
                {
                    this._titleErrorMessage = base.CreateErrorMessage(exception.Message);
                }
                if (allowLayoutChange)
                {
                    try
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(typeof(PartChromeType));
                        webPartToEdit.ChromeType = (PartChromeType) converter.ConvertFromString(this._chromeType.SelectedValue);
                    }
                    catch (Exception exception2)
                    {
                        this._chromeTypeErrorMessage = base.CreateErrorMessage(exception2.Message);
                    }
                }
                try
                {
                    TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(ContentDirection));
                    webPartToEdit.Direction = (ContentDirection) converter2.ConvertFromString(this._direction.SelectedValue);
                }
                catch (Exception exception3)
                {
                    this._directionErrorMessage = base.CreateErrorMessage(exception3.Message);
                }
                if (allowLayoutChange)
                {
                    Unit empty = Unit.Empty;
                    if (!string.IsNullOrEmpty(this._height.Value))
                    {
                        double num;
                        if (double.TryParse(this._height.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out num))
                        {
                            if (num < 0.0)
                            {
                                object[] args = new object[] { 0.ToString(CultureInfo.CurrentCulture) };
                                this._heightErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMinValue", args);
                            }
                            else if (num > 32767.0)
                            {
                                object[] objArray2 = new object[] { 0x7fff.ToString(CultureInfo.CurrentCulture) };
                                this._heightErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMaxValue", objArray2);
                            }
                            else
                            {
                                empty = new Unit(num, this._height.Type);
                            }
                        }
                        else
                        {
                            this._heightErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMustBeDecimal");
                        }
                    }
                    if (this._heightErrorMessage == null)
                    {
                        try
                        {
                            webPartToEdit.Height = empty;
                        }
                        catch (Exception exception4)
                        {
                            this._heightErrorMessage = base.CreateErrorMessage(exception4.Message);
                        }
                    }
                }
                if (allowLayoutChange)
                {
                    Unit unit2 = Unit.Empty;
                    if (!string.IsNullOrEmpty(this._width.Value))
                    {
                        double num2;
                        if (double.TryParse(this._width.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out num2))
                        {
                            if (num2 < 0.0)
                            {
                                object[] objArray3 = new object[] { 0.ToString(CultureInfo.CurrentCulture) };
                                this._widthErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMinValue", objArray3);
                            }
                            else if (num2 > 32767.0)
                            {
                                object[] objArray4 = new object[] { 0x7fff.ToString(CultureInfo.CurrentCulture) };
                                this._widthErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMaxValue", objArray4);
                            }
                            else
                            {
                                unit2 = new Unit(num2, this._width.Type);
                            }
                        }
                        else
                        {
                            this._widthErrorMessage = System.Web.SR.GetString("EditorPart_PropertyMustBeDecimal");
                        }
                    }
                    if (this._widthErrorMessage == null)
                    {
                        try
                        {
                            webPartToEdit.Width = unit2;
                        }
                        catch (Exception exception5)
                        {
                            this._widthErrorMessage = base.CreateErrorMessage(exception5.Message);
                        }
                    }
                }
                if (allowLayoutChange && webPartToEdit.AllowHide)
                {
                    try
                    {
                        webPartToEdit.Hidden = this._hidden.Checked;
                    }
                    catch (Exception exception6)
                    {
                        this._hiddenErrorMessage = base.CreateErrorMessage(exception6.Message);
                    }
                }
            }
            return !this.HasError;
        }

        protected internal override void CreateChildControls()
        {
            ControlCollection controls = this.Controls;
            controls.Clear();
            this._title = new TextBox();
            this._title.Columns = 30;
            controls.Add(this._title);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(PartChromeType));
            this._chromeType = new DropDownList();
            this._chromeType.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeType_Default"), converter.ConvertToString(PartChromeType.Default)));
            this._chromeType.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeType_TitleAndBorder"), converter.ConvertToString(PartChromeType.TitleAndBorder)));
            this._chromeType.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeType_TitleOnly"), converter.ConvertToString(PartChromeType.TitleOnly)));
            this._chromeType.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeType_BorderOnly"), converter.ConvertToString(PartChromeType.BorderOnly)));
            this._chromeType.Items.Add(new ListItem(System.Web.SR.GetString("PartChromeType_None"), converter.ConvertToString(PartChromeType.None)));
            controls.Add(this._chromeType);
            TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(ContentDirection));
            this._direction = new DropDownList();
            this._direction.Items.Add(new ListItem(System.Web.SR.GetString("ContentDirection_NotSet"), converter2.ConvertToString(ContentDirection.NotSet)));
            this._direction.Items.Add(new ListItem(System.Web.SR.GetString("ContentDirection_LeftToRight"), converter2.ConvertToString(ContentDirection.LeftToRight)));
            this._direction.Items.Add(new ListItem(System.Web.SR.GetString("ContentDirection_RightToLeft"), converter2.ConvertToString(ContentDirection.RightToLeft)));
            controls.Add(this._direction);
            this._height = new UnitInput();
            controls.Add(this._height);
            this._width = new UnitInput();
            controls.Add(this._width);
            this._hidden = new CheckBox();
            controls.Add(this._hidden);
            foreach (Control control in controls)
            {
                control.EnableViewState = false;
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Display && this.Visible) && !this.HasError)
            {
                this.SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.EnsureChildControls();
            string[] propertyDisplayNames = new string[] { System.Web.SR.GetString("AppearanceEditorPart_Title"), System.Web.SR.GetString("AppearanceEditorPart_ChromeType"), System.Web.SR.GetString("AppearanceEditorPart_Direction"), System.Web.SR.GetString("AppearanceEditorPart_Height"), System.Web.SR.GetString("AppearanceEditorPart_Width"), System.Web.SR.GetString("AppearanceEditorPart_Hidden") };
            WebControl[] propertyEditors = new WebControl[] { this._title, this._chromeType, this._direction, this._height, this._width, this._hidden };
            string[] errorMessages = new string[] { this._titleErrorMessage, this._chromeTypeErrorMessage, this._directionErrorMessage, this._heightErrorMessage, this._widthErrorMessage, this._hiddenErrorMessage };
            base.RenderPropertyEditors(writer, propertyDisplayNames, null, propertyEditors, errorMessages);
        }

        public override void SyncChanges()
        {
            WebPart webPartToEdit = base.WebPartToEdit;
            if (webPartToEdit != null)
            {
                bool allowLayoutChange = webPartToEdit.Zone.AllowLayoutChange;
                this.EnsureChildControls();
                this._title.Text = webPartToEdit.Title;
                this._chromeType.SelectedValue = TypeDescriptor.GetConverter(typeof(PartChromeType)).ConvertToString(webPartToEdit.ChromeType);
                this._chromeType.Enabled = allowLayoutChange;
                this._direction.SelectedValue = TypeDescriptor.GetConverter(typeof(ContentDirection)).ConvertToString(webPartToEdit.Direction);
                this._height.Unit = webPartToEdit.Height;
                this._height.Enabled = allowLayoutChange;
                this._width.Unit = webPartToEdit.Width;
                this._width.Enabled = allowLayoutChange;
                this._hidden.Checked = webPartToEdit.Hidden;
                this._hidden.Enabled = allowLayoutChange && webPartToEdit.AllowHide;
            }
        }

        [Browsable(false), Themeable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string DefaultButton
        {
            get
            {
                return base.DefaultButton;
            }
            set
            {
                base.DefaultButton = value;
            }
        }

        private bool HasError
        {
            get
            {
                if ((((this._titleErrorMessage == null) && (this._heightErrorMessage == null)) && ((this._widthErrorMessage == null) && (this._chromeTypeErrorMessage == null))) && (this._hiddenErrorMessage == null))
                {
                    return (this._directionErrorMessage != null);
                }
                return true;
            }
        }

        [WebSysDefaultValue("AppearanceEditorPart_PartTitle")]
        public override string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return System.Web.SR.GetString("AppearanceEditorPart_PartTitle");
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }

        private sealed class UnitInput : CompositeControl
        {
            private DropDownList _type;
            private TextBox _value;
            private const int TextBoxColumns = 2;

            protected internal override void CreateChildControls()
            {
                this.Controls.Clear();
                this._value = new TextBox();
                this._value.Columns = 2;
                this.Controls.Add(this._value);
                this._type = new DropDownList();
                int num = 1;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Pixels"), num.ToString(CultureInfo.InvariantCulture)));
                int num2 = 2;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Points"), num2.ToString(CultureInfo.InvariantCulture)));
                int num3 = 3;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Picas"), num3.ToString(CultureInfo.InvariantCulture)));
                int num4 = 4;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Inches"), num4.ToString(CultureInfo.InvariantCulture)));
                int num5 = 5;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Millimeters"), num5.ToString(CultureInfo.InvariantCulture)));
                int num6 = 6;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Centimeters"), num6.ToString(CultureInfo.InvariantCulture)));
                int num7 = 7;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Percent"), num7.ToString(CultureInfo.InvariantCulture)));
                int num8 = 8;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Em"), num8.ToString(CultureInfo.InvariantCulture)));
                int num9 = 9;
                this._type.Items.Add(new ListItem(System.Web.SR.GetString("AppearanceEditorPart_Ex"), num9.ToString(CultureInfo.InvariantCulture)));
                this.Controls.Add(this._type);
            }

            protected internal override void Render(HtmlTextWriter writer)
            {
                this.EnsureChildControls();
                this._value.ApplyStyle(base.ControlStyle);
                this._value.RenderControl(writer);
                writer.Write("&nbsp;");
                this._type.ApplyStyle(base.ControlStyle);
                this._type.RenderControl(writer);
            }

            public UnitType Type
            {
                get
                {
                    if (this._type == null)
                    {
                        return (UnitType) 0;
                    }
                    return (UnitType) int.Parse(this._type.SelectedValue, CultureInfo.InvariantCulture);
                }
            }

            public System.Web.UI.WebControls.Unit Unit
            {
                set
                {
                    this.EnsureChildControls();
                    if (value == System.Web.UI.WebControls.Unit.Empty)
                    {
                        this._value.Text = string.Empty;
                        this._type.SelectedIndex = 0;
                    }
                    else
                    {
                        this._value.Text = value.Value.ToString(CultureInfo.CurrentCulture);
                        this._type.SelectedValue = ((int) value.Type).ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            public string Value
            {
                get
                {
                    if (this._value == null)
                    {
                        return string.Empty;
                    }
                    return this._value.Text;
                }
            }
        }
    }
}

