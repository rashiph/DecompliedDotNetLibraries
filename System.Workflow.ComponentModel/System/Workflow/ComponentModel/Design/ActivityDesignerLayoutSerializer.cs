namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    public class ActivityDesignerLayoutSerializer : WorkflowMarkupSerializer
    {
        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            Activity activity3;
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object obj2 = null;
            IDesignerHost service = serializationManager.GetService(typeof(IDesignerHost)) as IDesignerHost;
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if ((service == null) || (reader == null))
            {
                return obj2;
            }
            string str = string.Empty;
            while (reader.MoveToNextAttribute() && !reader.LocalName.Equals("Name", StringComparison.Ordinal))
            {
            }
            if (reader.LocalName.Equals("Name", StringComparison.Ordinal) && reader.ReadAttributeValue())
            {
                str = reader.Value;
            }
            reader.MoveToElement();
            if (string.IsNullOrEmpty(str))
            {
                serializationManager.ReportError(SR.GetString("Error_LayoutSerializationAssociatedActivityNotFound", new object[] { reader.LocalName, "Name" }));
                return obj2;
            }
            CompositeActivityDesigner designer = serializationManager.Context[typeof(CompositeActivityDesigner)] as CompositeActivityDesigner;
            if (designer != null)
            {
                CompositeActivity activity2 = designer.Activity as CompositeActivity;
                if (activity2 == null)
                {
                    goto Label_01D0;
                }
                activity3 = null;
                foreach (Activity activity4 in activity2.Activities)
                {
                    if (str.Equals(activity4.Name, StringComparison.Ordinal))
                    {
                        activity3 = activity4;
                        break;
                    }
                }
            }
            else
            {
                Activity rootComponent = service.RootComponent as Activity;
                if ((rootComponent != null) && !str.Equals(rootComponent.Name, StringComparison.Ordinal))
                {
                    foreach (IComponent component in service.Container.Components)
                    {
                        rootComponent = component as Activity;
                        if ((rootComponent != null) && str.Equals(rootComponent.Name, StringComparison.Ordinal))
                        {
                            break;
                        }
                    }
                }
                if (rootComponent != null)
                {
                    obj2 = service.GetDesigner(rootComponent);
                }
                goto Label_01D0;
            }
            if (activity3 != null)
            {
                obj2 = service.GetDesigner(activity3);
            }
        Label_01D0:
            if (obj2 == null)
            {
                serializationManager.ReportError(SR.GetString("Error_LayoutSerializationActivityNotFound", new object[] { reader.LocalName, str, "Name" }));
            }
            return obj2;
        }

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
            List<PropertyInfo> list = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));
            ActivityDesigner designer = obj as ActivityDesigner;
            if (designer != null)
            {
                PropertyInfo property = designer.GetType().GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null)
                {
                    list.Insert(0, property);
                }
            }
            return list.ToArray();
        }

        protected override void OnBeforeSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeSerialize(serializationManager, obj);
            ActivityDesigner designer = obj as ActivityDesigner;
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if (((designer.Activity != null) && (designer.Activity.Parent == null)) && (writer != null))
            {
                string prefix = string.Empty;
                XmlQualifiedName xmlQualifiedName = serializationManager.GetXmlQualifiedName(typeof(Point), out prefix);
                writer.WriteAttributeString("xmlns", prefix, null, xmlQualifiedName.Namespace);
            }
        }
    }
}

