namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class BaseValidatorDesigner : PreviewControlDesigner
    {
        protected override Control CreateViewControl()
        {
            BaseValidator validator = (BaseValidator) base.CreateViewControl();
            validator.ForeColor = ((BaseValidator) base.Component).ForeColor;
            return validator;
        }

        public override string GetDesignTimeHtml()
        {
            BaseValidator viewControl = (BaseValidator) base.ViewControl;
            viewControl.IsValid = false;
            string errorMessage = viewControl.ErrorMessage;
            ValidatorDisplay display = viewControl.Display;
            bool flag = (display == ValidatorDisplay.None) || ((errorMessage.Trim().Length == 0) && (viewControl.Text.Trim().Length == 0));
            if (flag)
            {
                viewControl.ErrorMessage = "[" + viewControl.ID + "]";
                viewControl.Display = ValidatorDisplay.Static;
            }
            string designTimeHtml = base.GetDesignTimeHtml();
            if (flag)
            {
                viewControl.ErrorMessage = errorMessage;
                viewControl.Display = display;
            }
            return designTimeHtml;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (((BaseValidator) base.Component).RenderingCompatibility >= new Version(4, 0))
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["ForeColor"];
                properties["ForeColor"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { new DefaultValueAttribute(typeof(Color), "") });
            }
        }
    }
}

