namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public class CheckBoxField : BoundField
    {
        private bool _suppressPropertyThrows;

        protected override void CopyProperties(DataControlField newField)
        {
            ((CheckBoxField) newField).Text = this.Text;
            this._suppressPropertyThrows = true;
            ((CheckBoxField) newField)._suppressPropertyThrows = true;
            base.CopyProperties(newField);
            this._suppressPropertyThrows = false;
            ((CheckBoxField) newField)._suppressPropertyThrows = false;
        }

        protected override DataControlField CreateField()
        {
            return new CheckBoxField();
        }

        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
        {
            Control control = null;
            string dataField = this.DataField;
            object obj2 = null;
            if (cell.Controls.Count > 0)
            {
                control = cell.Controls[0];
                CheckBox box = control as CheckBox;
                if ((box != null) && (includeReadOnly || box.Enabled))
                {
                    obj2 = box.Checked;
                }
            }
            if (obj2 != null)
            {
                if (dictionary.Contains(dataField))
                {
                    dictionary[dataField] = obj2;
                }
                else
                {
                    dictionary.Add(dataField, obj2);
                }
            }
        }

        protected override object GetDesignTimeValue()
        {
            return true;
        }

        protected override void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
        {
            CheckBox child = null;
            CheckBox box2 = null;
            if ((((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && !this.ReadOnly) || ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal))
            {
                CheckBox box3 = new CheckBox {
                    ToolTip = this.HeaderText
                };
                child = box3;
                if ((this.DataField.Length != 0) && ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal))
                {
                    box2 = box3;
                }
            }
            else if (this.DataField.Length != 0)
            {
                CheckBox box4 = new CheckBox {
                    Text = this.Text,
                    Enabled = false
                };
                child = box4;
                box2 = box4;
            }
            if (child != null)
            {
                cell.Controls.Add(child);
            }
            if ((box2 != null) && base.Visible)
            {
                box2.DataBinding += new EventHandler(this.OnDataBindField);
            }
        }

        protected override void OnDataBindField(object sender, EventArgs e)
        {
            Control control = (Control) sender;
            Control namingContainer = control.NamingContainer;
            object obj2 = this.GetValue(namingContainer);
            if (!(control is CheckBox))
            {
                throw new HttpException(System.Web.SR.GetString("CheckBoxField_WrongControlType", new object[] { this.DataField }));
            }
            if (DataBinder.IsNull(obj2))
            {
                ((CheckBox) control).Checked = false;
            }
            else if (obj2 is bool)
            {
                ((CheckBox) control).Checked = (bool) obj2;
            }
            else
            {
                try
                {
                    ((CheckBox) control).Checked = bool.Parse(obj2.ToString());
                }
                catch (FormatException exception)
                {
                    throw new HttpException(System.Web.SR.GetString("CheckBoxField_CouldntParseAsBoolean", new object[] { this.DataField }), exception);
                }
            }
            ((CheckBox) control).Text = this.Text;
        }

        public override void ValidateSupportsCallback()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool ApplyFormatInEditMode
        {
            get
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "ApplyFormatInEditMode" }));
                }
                return false;
            }
            set
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "ApplyFormatInEditMode" }));
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool ConvertEmptyStringToNull
        {
            get
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "ConvertEmptyStringToNull" }));
                }
                return false;
            }
            set
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "ConvertEmptyStringToNull" }));
                }
            }
        }

        [TypeConverter("System.Web.UI.Design.DataSourceBooleanViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public override string DataField
        {
            get
            {
                return base.DataField;
            }
            set
            {
                base.DataField = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string DataFormatString
        {
            get
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "DataFormatString" }));
                }
                return string.Empty;
            }
            set
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "DataFormatString" }));
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool HtmlEncode
        {
            get
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "HtmlEncode" }));
                }
                return false;
            }
            set
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "HtmlEncode" }));
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool HtmlEncodeFormatString
        {
            get
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "HtmlEncodeFormatString" }));
                }
                return false;
            }
            set
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "HtmlEncodeFormatString" }));
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string NullDisplayText
        {
            get
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "NullDisplayText" }));
                }
                return string.Empty;
            }
            set
            {
                if (!this._suppressPropertyThrows)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("CheckBoxField_NotSupported", new object[] { "NullDisplayText" }));
                }
            }
        }

        protected override bool SupportsHtmlEncode
        {
            get
            {
                return false;
            }
        }

        [WebCategory("Appearance"), Localizable(true), WebSysDescription("CheckBoxField_Text"), DefaultValue("")]
        public virtual string Text
        {
            get
            {
                object obj2 = base.ViewState["Text"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, base.ViewState["Text"]))
                {
                    base.ViewState["Text"] = value;
                    this.OnFieldChanged();
                }
            }
        }
    }
}

