namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    public class FreeformActivityDesignerLayoutSerializer : CompositeActivityDesignerLayoutSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            PropertyInfo[] properties = base.GetProperties(serializationManager, obj);
            FreeformActivityDesigner designer = obj as FreeformActivityDesigner;
            if (designer == null)
            {
                return properties;
            }
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (PropertyInfo info in properties)
            {
                if (((writer == null) || !info.Name.Equals("AutoSizeMargin", StringComparison.Ordinal)) || (designer.AutoSizeMargin != FreeformActivityDesigner.DefaultAutoSizeMargin))
                {
                    list.Add(info);
                }
            }
            list.Add(typeof(FreeformActivityDesigner).GetProperty("DesignerConnectors", BindingFlags.NonPublic | BindingFlags.Instance));
            return list.ToArray();
        }
    }
}

