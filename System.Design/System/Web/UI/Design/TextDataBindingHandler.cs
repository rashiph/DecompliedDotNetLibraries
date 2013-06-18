namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class TextDataBindingHandler : DataBindingHandler
    {
        public override void DataBindControl(IDesignerHost designerHost, Control control)
        {
            DataBinding runtimeDataBinding = ((IDataBindingsAccessor) control).DataBindings["Text"];
            if (runtimeDataBinding != null)
            {
                PropertyInfo property = control.GetType().GetProperty("Text");
                if ((property != null) && (property.PropertyType == typeof(string)))
                {
                    DesignTimeDataBinding binding2 = new DesignTimeDataBinding(runtimeDataBinding);
                    string str = string.Empty;
                    if (!binding2.IsCustom)
                    {
                        try
                        {
                            str = DataBinder.Eval(((IDataItemContainer) control.NamingContainer).DataItem, binding2.Field, binding2.Format);
                        }
                        catch
                        {
                        }
                    }
                    if ((str == null) || (str.Length == 0))
                    {
                        str = System.Design.SR.GetString("Sample_Databound_Text");
                    }
                    property.SetValue(control, str, null);
                }
            }
        }
    }
}

