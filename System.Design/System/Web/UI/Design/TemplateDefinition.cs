namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class TemplateDefinition : DesignerObject
    {
        private bool _serverControlsOnly;
        private System.Web.UI.WebControls.Style _style;
        private bool _supportsDataBinding;
        private object _templatedObject;
        private PropertyDescriptor _templateProperty;
        private string _templatePropertyName;

        public TemplateDefinition(ControlDesigner designer, string name, object templatedObject, string templatePropertyName) : this(designer, name, templatedObject, templatePropertyName, false)
        {
        }

        public TemplateDefinition(ControlDesigner designer, string name, object templatedObject, string templatePropertyName, bool serverControlsOnly) : this(designer, name, templatedObject, templatePropertyName, null, serverControlsOnly)
        {
        }

        public TemplateDefinition(ControlDesigner designer, string name, object templatedObject, string templatePropertyName, System.Web.UI.WebControls.Style style) : this(designer, name, templatedObject, templatePropertyName, style, false)
        {
        }

        public TemplateDefinition(ControlDesigner designer, string name, object templatedObject, string templatePropertyName, System.Web.UI.WebControls.Style style, bool serverControlsOnly) : base(designer, name)
        {
            if ((templatePropertyName == null) || (templatePropertyName.Length == 0))
            {
                throw new ArgumentNullException("templatePropertyName");
            }
            if (templatedObject == null)
            {
                throw new ArgumentNullException("templatedObject");
            }
            this._serverControlsOnly = serverControlsOnly;
            this._style = style;
            this._templatedObject = templatedObject;
            this._templatePropertyName = templatePropertyName;
        }

        public virtual bool AllowEditing
        {
            get
            {
                return true;
            }
        }

        public virtual string Content
        {
            get
            {
                ITemplate template = (ITemplate) this.TemplateProperty.GetValue(this.TemplatedObject);
                IDesignerHost service = (IDesignerHost) base.GetService(typeof(IDesignerHost));
                return ControlPersister.PersistTemplate(template, service);
            }
            set
            {
                IDesignerHost service = (IDesignerHost) base.GetService(typeof(IDesignerHost));
                ITemplate template = ControlParser.ParseTemplate(service, value);
                this.TemplateProperty.SetValue(this.TemplatedObject, template);
            }
        }

        public bool ServerControlsOnly
        {
            get
            {
                return this._serverControlsOnly;
            }
        }

        public System.Web.UI.WebControls.Style Style
        {
            get
            {
                return this._style;
            }
        }

        public bool SupportsDataBinding
        {
            get
            {
                return this._supportsDataBinding;
            }
            set
            {
                this._supportsDataBinding = value;
            }
        }

        public object TemplatedObject
        {
            get
            {
                return this._templatedObject;
            }
        }

        private PropertyDescriptor TemplateProperty
        {
            get
            {
                if (this._templateProperty == null)
                {
                    this._templateProperty = TypeDescriptor.GetProperties(this.TemplatedObject)[this.TemplatePropertyName];
                    if ((this._templateProperty == null) || !typeof(ITemplate).IsAssignableFrom(this._templateProperty.PropertyType))
                    {
                        throw new InvalidOperationException(System.Design.SR.GetString("TemplateDefinition_InvalidTemplateProperty", new object[] { this.TemplatePropertyName }));
                    }
                }
                return this._templateProperty;
            }
        }

        public string TemplatePropertyName
        {
            get
            {
                return this._templatePropertyName;
            }
        }
    }
}

