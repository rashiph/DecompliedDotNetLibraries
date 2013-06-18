namespace System.Web.UI.Design
{
    using System;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class TextControlDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            string designTimeHtml;
            Control viewControl = base.ViewControl;
            PropertyInfo property = viewControl.GetType().GetProperty("Text");
            string str = (string) property.GetValue(viewControl, null);
            bool flag = (str == null) || (str.Length == 0);
            bool flag2 = viewControl.HasControls();
            Control[] array = null;
            if (flag)
            {
                if (flag2)
                {
                    array = new Control[viewControl.Controls.Count];
                    viewControl.Controls.CopyTo(array, 0);
                }
                property.SetValue(viewControl, "[" + viewControl.ID + "]", null);
            }
            try
            {
                designTimeHtml = base.GetDesignTimeHtml();
            }
            finally
            {
                if (flag)
                {
                    property.SetValue(viewControl, str, null);
                    if (flag2)
                    {
                        foreach (Control control2 in array)
                        {
                            viewControl.Controls.Add(control2);
                        }
                    }
                }
            }
            return designTimeHtml;
        }
    }
}

