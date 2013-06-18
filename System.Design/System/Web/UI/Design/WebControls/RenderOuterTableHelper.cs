namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal static class RenderOuterTableHelper
    {
        internal static readonly string[] fontStyleProperties = new string[] { "Bold", "Italic", "Name", "Names", "Overline", "Size", "Strikeout", "Underline" };
        internal static readonly string[] formViewStylePropertiesOnOuterTable = new string[] { "BackImageUrl", "CellPadding", "CellSpacing", "GridLines", "HorizontalAlign", "BackColor", "BorderColor", "BorderWidth", "BorderStyle", "CssClass", "Font", "ForeColor", "Height", "Width" };
        internal static readonly string[] loginStylePropertiesOnOuterTable = new string[] { "BorderPadding", "BackColor", "BorderColor", "BorderWidth", "BorderStyle", "CssClass", "ForeColor", "Height", "Width" };

        internal static bool IsAnyPropertyOnOuterTableChanged(IComponent component, bool isFormView)
        {
            if (!isFormView)
            {
                return IsAnyPropertyOnOuterTableChangedHelper(component, loginStylePropertiesOnOuterTable);
            }
            if (!IsAnyPropertyOnOuterTableChangedHelper(component, formViewStylePropertiesOnOuterTable))
            {
                return IsAnyPropertyOnOuterTableChangedHelper(((FormView) component).Font, fontStyleProperties);
            }
            return true;
        }

        private static bool IsAnyPropertyOnOuterTableChangedHelper(object component, string[] propertiesOnOuterTable)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
            foreach (string str in propertiesOnOuterTable)
            {
                PropertyDescriptor descriptor = properties[str];
                DefaultValueAttribute attribute = (DefaultValueAttribute) descriptor.Attributes[typeof(DefaultValueAttribute)];
                if ((attribute != null) && !object.Equals(attribute.Value, descriptor.GetValue(component)))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void SetRenderOuterTable(bool value, ControlDesigner designer, bool isFormView)
        {
            TransactedChangeCallback callback = null;
            IComponent component = designer.Component;
            IRenderOuterTableControl control = (IRenderOuterTableControl) component;
            if (value != control.RenderOuterTable)
            {
                if (!value && IsAnyPropertyOnOuterTableChanged(component, isFormView))
                {
                    if (UIServiceHelper.ShowMessage(component.Site, System.Design.SR.GetString("RenderOuterTable_RemoveOuterTableWarning"), System.Design.SR.GetString("RenderOuterTable_RemoveOuterTableCaption", new object[] { control.GetType().Name, control.ID }), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return;
                    }
                    if (callback == null)
                    {
                        callback = delegate (object context) {
                            try
                            {
                                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                                string[] strArray = isFormView ? formViewStylePropertiesOnOuterTable : loginStylePropertiesOnOuterTable;
                                if (isFormView)
                                {
                                    ((FormView) control).ControlStyle.Reset();
                                }
                                foreach (string str in strArray)
                                {
                                    properties[str].ResetValue(component);
                                }
                                return true;
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        };
                    }
                    ControlDesigner.InvokeTransactedChange(component, callback, null, System.Design.SR.GetString("RenderOuterTableHelper_ResetProperties"));
                }
                control.RenderOuterTable = value;
                TypeDescriptor.Refresh(component);
            }
        }

        internal static void SetupRenderOuterTable(IDictionary properties, IComponent component, bool useFormViewStyleProperties, System.Type designerType)
        {
            if (properties["RenderOuterTable"] != null)
            {
                if (!((IRenderOuterTableControl) component).RenderOuterTable)
                {
                    string[] formViewStylePropertiesOnOuterTable;
                    if (useFormViewStyleProperties)
                    {
                        formViewStylePropertiesOnOuterTable = RenderOuterTableHelper.formViewStylePropertiesOnOuterTable;
                    }
                    else
                    {
                        formViewStylePropertiesOnOuterTable = loginStylePropertiesOnOuterTable;
                    }
                    foreach (string str in formViewStylePropertiesOnOuterTable)
                    {
                        PropertyDescriptor descriptor = (PropertyDescriptor) properties[str];
                        properties[str] = TypeDescriptor.CreateProperty(descriptor.ComponentType, descriptor, new Attribute[] { BrowsableAttribute.No });
                    }
                }
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["RenderOuterTable"];
                properties["RenderOuterTable"] = TypeDescriptor.CreateProperty(designerType, oldPropertyDescriptor, new Attribute[] { RefreshPropertiesAttribute.All });
            }
        }
    }
}

