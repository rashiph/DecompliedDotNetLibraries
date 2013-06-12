namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    public class ValidatedControlConverter : ControlIDConverter
    {
        protected override bool FilterControl(Control control)
        {
            ValidationPropertyAttribute attribute = (ValidationPropertyAttribute) TypeDescriptor.GetAttributes(control)[typeof(ValidationPropertyAttribute)];
            return ((attribute != null) && (attribute.Name != null));
        }
    }
}

