namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;

    internal static class DataControlFieldHelper
    {
        internal static IDictionary<Type, DataControlFieldDesigner> GetCustomFieldDesigners(DesignerForm designerForm, DataBoundControl control)
        {
            Dictionary<Type, DataControlFieldDesigner> dictionary = new Dictionary<Type, DataControlFieldDesigner>();
            ITypeDiscoveryService service = (ITypeDiscoveryService) control.Site.GetService(typeof(ITypeDiscoveryService));
            if (service != null)
            {
                foreach (Type type in service.GetTypes(typeof(DataControlField), false))
                {
                    DesignerAttribute customAttribute = (DesignerAttribute) Attribute.GetCustomAttribute(type, typeof(DesignerAttribute));
                    if (customAttribute != null)
                    {
                        Type type2 = Type.GetType(customAttribute.DesignerTypeName, false, true);
                        if ((type2 != null) && type2.IsSubclassOf(typeof(DataControlFieldDesigner)))
                        {
                            try
                            {
                                DataControlFieldDesigner designer = (DataControlFieldDesigner) Activator.CreateInstance(type2);
                                if (designer.IsEnabled(control))
                                {
                                    designer.DesignerForm = designerForm;
                                    dictionary.Add(type, designer);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        internal static ITemplate GetTemplate(DataBoundControl control, string templateContent)
        {
            try
            {
                IDesignerHost service = (IDesignerHost) control.Site.GetService(typeof(IDesignerHost));
                if ((templateContent != null) && (templateContent.Length > 0))
                {
                    return ControlParser.ParseTemplate(service, templateContent, null);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static TemplateField GetTemplateField(DataControlField dataControlField, DataBoundControl dataBoundControl)
        {
            TemplateField field = new TemplateField {
                HeaderText = dataControlField.HeaderText,
                HeaderImageUrl = dataControlField.HeaderImageUrl,
                AccessibleHeaderText = dataControlField.AccessibleHeaderText,
                FooterText = dataControlField.FooterText,
                SortExpression = dataControlField.SortExpression,
                Visible = dataControlField.Visible,
                InsertVisible = dataControlField.InsertVisible,
                ShowHeader = dataControlField.ShowHeader
            };
            field.ControlStyle.CopyFrom(dataControlField.ControlStyle);
            field.FooterStyle.CopyFrom(dataControlField.FooterStyle);
            field.HeaderStyle.CopyFrom(dataControlField.HeaderStyle);
            field.ItemStyle.CopyFrom(dataControlField.ItemStyle);
            return field;
        }
    }
}

