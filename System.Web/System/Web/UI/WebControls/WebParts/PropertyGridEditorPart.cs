namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class PropertyGridEditorPart : EditorPart
    {
        private ArrayList _editorControls;
        private string[] _errorMessages;
        private static readonly WebPart designModeWebPart = new DesignModeWebPart();
        private static readonly Attribute[] FilterAttributes = new Attribute[] { WebBrowsableAttribute.Yes };
        private const int TextBoxColumns = 30;
        private static readonly UrlPropertyAttribute urlPropertyAttribute = new UrlPropertyAttribute();

        public override bool ApplyChanges()
        {
            object editableObject = this.GetEditableObject();
            if (editableObject == null)
            {
                return true;
            }
            this.EnsureChildControls();
            if (this.Controls.Count == 0)
            {
                return true;
            }
            PropertyDescriptorCollection editableProperties = this.GetEditableProperties(editableObject, true);
            for (int i = 0; i < editableProperties.Count; i++)
            {
                PropertyDescriptor pd = editableProperties[i];
                Control editorControl = (Control) this.EditorControls[i];
                try
                {
                    object editorControlValue = this.GetEditorControlValue(editorControl, pd);
                    if (pd.Attributes.Matches(urlPropertyAttribute) && CrossSiteScriptingValidation.IsDangerousUrl(editorControlValue.ToString()))
                    {
                        this._errorMessages[i] = System.Web.SR.GetString("EditorPart_ErrorBadUrl");
                    }
                    else
                    {
                        try
                        {
                            pd.SetValue(editableObject, editorControlValue);
                        }
                        catch (Exception exception)
                        {
                            this._errorMessages[i] = base.CreateErrorMessage(exception.Message);
                        }
                    }
                }
                catch
                {
                    if ((this.Context != null) && this.Context.IsCustomErrorEnabled)
                    {
                        this._errorMessages[i] = System.Web.SR.GetString("EditorPart_ErrorConvertingProperty");
                    }
                    else
                    {
                        this._errorMessages[i] = System.Web.SR.GetString("EditorPart_ErrorConvertingPropertyWithType", new object[] { pd.PropertyType.FullName });
                    }
                }
            }
            return !this.HasError;
        }

        private bool CanEditProperty(PropertyDescriptor property)
        {
            if (property.IsReadOnly)
            {
                return false;
            }
            if (((base.WebPartManager != null) && (base.WebPartManager.Personalization != null)) && ((base.WebPartManager.Personalization.Scope == PersonalizationScope.User) && property.Attributes.Contains(PersonalizableAttribute.SharedPersonalizable)))
            {
                return false;
            }
            return Util.CanConvertToFrom(property.Converter, typeof(string));
        }

        protected internal override void CreateChildControls()
        {
            ControlCollection controls = this.Controls;
            controls.Clear();
            this.EditorControls.Clear();
            object editableObject = this.GetEditableObject();
            if (editableObject != null)
            {
                foreach (PropertyDescriptor descriptor in this.GetEditableProperties(editableObject, true))
                {
                    Control control = this.CreateEditorControl(descriptor);
                    this.EditorControls.Add(control);
                    this.Controls.Add(control);
                }
                this._errorMessages = new string[this.EditorControls.Count];
            }
            foreach (Control control2 in controls)
            {
                control2.EnableViewState = false;
            }
        }

        private Control CreateEditorControl(PropertyDescriptor pd)
        {
            Type propertyType = pd.PropertyType;
            if (propertyType == typeof(bool))
            {
                return new CheckBox();
            }
            if (typeof(Enum).IsAssignableFrom(propertyType))
            {
                DropDownList list = new DropDownList();
                foreach (object obj2 in pd.Converter.GetStandardValues())
                {
                    string text = pd.Converter.ConvertToString(obj2);
                    list.Items.Add(new ListItem(text));
                }
                return list;
            }
            return new TextBox { Columns = 30 };
        }

        private string GetDescription(PropertyDescriptor pd)
        {
            WebDescriptionAttribute attribute = (WebDescriptionAttribute) pd.Attributes[typeof(WebDescriptionAttribute)];
            if (attribute != null)
            {
                return attribute.Description;
            }
            return null;
        }

        private string GetDisplayName(PropertyDescriptor pd)
        {
            WebDisplayNameAttribute attribute = (WebDisplayNameAttribute) pd.Attributes[typeof(WebDisplayNameAttribute)];
            if ((attribute != null) && !string.IsNullOrEmpty(attribute.DisplayName))
            {
                return attribute.DisplayName;
            }
            return pd.Name;
        }

        private object GetEditableObject()
        {
            if (base.DesignMode)
            {
                return designModeWebPart;
            }
            WebPart webPartToEdit = base.WebPartToEdit;
            IWebEditable editable = webPartToEdit;
            if (editable != null)
            {
                return editable.WebBrowsableObject;
            }
            return webPartToEdit;
        }

        private PropertyDescriptorCollection GetEditableProperties(object editableObject, bool sort)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(editableObject, FilterAttributes);
            if (sort)
            {
                properties = properties.Sort();
            }
            PropertyDescriptorCollection descriptors2 = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor descriptor in properties)
            {
                if (this.CanEditProperty(descriptor))
                {
                    descriptors2.Add(descriptor);
                }
            }
            return descriptors2;
        }

        private object GetEditorControlValue(Control editorControl, PropertyDescriptor pd)
        {
            CheckBox box = editorControl as CheckBox;
            if (box != null)
            {
                return box.Checked;
            }
            DropDownList list = editorControl as DropDownList;
            if (list != null)
            {
                string selectedValue = list.SelectedValue;
                return pd.Converter.ConvertFromString(selectedValue);
            }
            TextBox box2 = (TextBox) editorControl;
            return pd.Converter.ConvertFromString(box2.Text);
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
            string[] propertyDisplayNames = null;
            string[] propertyDescriptions = null;
            object editableObject = this.GetEditableObject();
            if (editableObject != null)
            {
                PropertyDescriptorCollection editableProperties = this.GetEditableProperties(editableObject, true);
                propertyDisplayNames = new string[editableProperties.Count];
                propertyDescriptions = new string[editableProperties.Count];
                for (int i = 0; i < editableProperties.Count; i++)
                {
                    propertyDisplayNames[i] = this.GetDisplayName(editableProperties[i]);
                    propertyDescriptions[i] = this.GetDescription(editableProperties[i]);
                }
            }
            if (propertyDisplayNames != null)
            {
                WebControl[] propertyEditors = (WebControl[]) this.EditorControls.ToArray(typeof(WebControl));
                base.RenderPropertyEditors(writer, propertyDisplayNames, propertyDescriptions, propertyEditors, this._errorMessages);
            }
        }

        public override void SyncChanges()
        {
            object editableObject = this.GetEditableObject();
            if (editableObject != null)
            {
                this.EnsureChildControls();
                int num = 0;
                foreach (PropertyDescriptor descriptor in this.GetEditableProperties(editableObject, true))
                {
                    if (this.CanEditProperty(descriptor))
                    {
                        Control control = (Control) this.EditorControls[num];
                        this.SyncChanges(control, descriptor, editableObject);
                        num++;
                    }
                }
            }
        }

        private void SyncChanges(Control control, PropertyDescriptor pd, object instance)
        {
            Type propertyType = pd.PropertyType;
            if (propertyType == typeof(bool))
            {
                CheckBox box = (CheckBox) control;
                box.Checked = (bool) pd.GetValue(instance);
            }
            else if (typeof(Enum).IsAssignableFrom(propertyType))
            {
                DropDownList list = (DropDownList) control;
                list.SelectedValue = pd.Converter.ConvertToString(pd.GetValue(instance));
            }
            else
            {
                TextBox box2 = (TextBox) control;
                box2.Text = pd.Converter.ConvertToString(pd.GetValue(instance));
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        public override bool Display
        {
            get
            {
                if (!base.Display)
                {
                    return false;
                }
                object editableObject = this.GetEditableObject();
                return ((editableObject != null) && (this.GetEditableProperties(editableObject, false).Count > 0));
            }
        }

        private ArrayList EditorControls
        {
            get
            {
                if (this._editorControls == null)
                {
                    this._editorControls = new ArrayList();
                }
                return this._editorControls;
            }
        }

        private bool HasError
        {
            get
            {
                string[] strArray = this._errorMessages;
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i] != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [WebSysDefaultValue("PropertyGridEditorPart_PartTitle")]
        public override string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return System.Web.SR.GetString("PropertyGridEditorPart_PartTitle");
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }

        private sealed class DesignModeWebPart : WebPart
        {
            [WebSysWebDisplayName("PropertyGridEditorPart_DesignModeWebPart_BoolProperty"), WebBrowsable]
            public bool BoolProperty
            {
                get
                {
                    return false;
                }
                set
                {
                }
            }

            [WebSysWebDisplayName("PropertyGridEditorPart_DesignModeWebPart_EnumProperty"), WebBrowsable]
            public SampleEnum EnumProperty
            {
                get
                {
                    return SampleEnum.EnumValue;
                }
                set
                {
                }
            }

            [WebBrowsable, WebSysWebDisplayName("PropertyGridEditorPart_DesignModeWebPart_StringProperty")]
            public string StringProperty
            {
                get
                {
                    return string.Empty;
                }
                set
                {
                }
            }

            public enum SampleEnum
            {
                EnumValue
            }

            private sealed class WebSysWebDisplayNameAttribute : WebDisplayNameAttribute
            {
                private bool replaced;

                internal WebSysWebDisplayNameAttribute(string DisplayName) : base(DisplayName)
                {
                }

                public override string DisplayName
                {
                    get
                    {
                        if (!this.replaced)
                        {
                            this.replaced = true;
                            base.DisplayNameValue = System.Web.SR.GetString(base.DisplayName);
                        }
                        return base.DisplayName;
                    }
                }

                public override object TypeId
                {
                    get
                    {
                        return typeof(WebDisplayNameAttribute);
                    }
                }
            }
        }
    }
}

