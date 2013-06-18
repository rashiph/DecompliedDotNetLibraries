namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("ControlID")]
    public class ControlParameter : Parameter
    {
        public ControlParameter()
        {
        }

        protected ControlParameter(ControlParameter original) : base(original)
        {
            this.ControlID = original.ControlID;
            this.PropertyName = original.PropertyName;
        }

        public ControlParameter(string name, string controlID) : base(name)
        {
            this.ControlID = controlID;
        }

        public ControlParameter(string name, string controlID, string propertyName) : base(name)
        {
            this.ControlID = controlID;
            this.PropertyName = propertyName;
        }

        public ControlParameter(string name, DbType dbType, string controlID, string propertyName) : base(name, dbType)
        {
            this.ControlID = controlID;
            this.PropertyName = propertyName;
        }

        public ControlParameter(string name, TypeCode type, string controlID, string propertyName) : base(name, type)
        {
            this.ControlID = controlID;
            this.PropertyName = propertyName;
        }

        protected override Parameter Clone()
        {
            return new ControlParameter(this);
        }

        protected internal override object Evaluate(HttpContext context, Control control)
        {
            if (control == null)
            {
                return null;
            }
            string controlID = this.ControlID;
            string propertyName = this.PropertyName;
            if (controlID.Length == 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("ControlParameter_ControlIDNotSpecified", new object[] { base.Name }));
            }
            Control component = DataBoundControlHelper.FindControl(control, controlID);
            if (component == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ControlParameter_CouldNotFindControl", new object[] { controlID, base.Name }));
            }
            ControlValuePropertyAttribute attribute = (ControlValuePropertyAttribute) TypeDescriptor.GetAttributes(component)[typeof(ControlValuePropertyAttribute)];
            if (propertyName.Length == 0)
            {
                if ((attribute == null) || string.IsNullOrEmpty(attribute.Name))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ControlParameter_PropertyNameNotSpecified", new object[] { controlID, base.Name }));
                }
                propertyName = attribute.Name;
            }
            object obj2 = DataBinder.Eval(component, propertyName);
            if (((attribute != null) && string.Equals(attribute.Name, propertyName, StringComparison.OrdinalIgnoreCase)) && ((attribute.DefaultValue != null) && attribute.DefaultValue.Equals(obj2)))
            {
                return null;
            }
            return obj2;
        }

        [RefreshProperties(RefreshProperties.All), TypeConverter(typeof(ControlIDConverter)), DefaultValue(""), WebCategory("Control"), WebSysDescription("ControlParameter_ControlID"), IDReferenceProperty]
        public string ControlID
        {
            get
            {
                object obj2 = base.ViewState["ControlID"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.ControlID != value)
                {
                    base.ViewState["ControlID"] = value;
                    base.OnParameterChanged();
                }
            }
        }

        [WebSysDescription("ControlParameter_PropertyName"), WebCategory("Control"), DefaultValue(""), TypeConverter(typeof(ControlPropertyNameConverter))]
        public string PropertyName
        {
            get
            {
                object obj2 = base.ViewState["PropertyName"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.PropertyName != value)
                {
                    base.ViewState["PropertyName"] = value;
                    base.OnParameterChanged();
                }
            }
        }
    }
}

