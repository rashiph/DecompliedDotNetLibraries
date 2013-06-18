namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    public class CompositeActivityMarkupSerializer : ActivityMarkupSerializer
    {
        internal override void OnBeforeSerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            base.OnBeforeSerializeContents(serializationManager, obj);
            CompositeActivity activity = obj as CompositeActivity;
            XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
            if ((activity.Parent == null) && (writer != null))
            {
                Dictionary<string, Activity> dictionary = new Dictionary<string, Activity>();
                string prefix = string.Empty;
                XmlQualifiedName xmlQualifiedName = serializationManager.GetXmlQualifiedName(activity.GetType(), out prefix);
                dictionary.Add(xmlQualifiedName.Namespace, activity);
                foreach (Activity activity2 in Helpers.GetNestedActivities(activity))
                {
                    prefix = string.Empty;
                    xmlQualifiedName = serializationManager.GetXmlQualifiedName(activity2.GetType(), out prefix);
                    if (!dictionary.ContainsKey(xmlQualifiedName.Namespace))
                    {
                        writer.WriteAttributeString("xmlns", prefix, null, xmlQualifiedName.Namespace);
                        dictionary.Add(xmlQualifiedName.Namespace, activity2);
                    }
                }
            }
        }
    }
}

