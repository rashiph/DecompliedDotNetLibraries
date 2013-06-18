namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    internal sealed class ActivityChangeActionMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> list = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));
            foreach (PropertyInfo info in obj.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
            {
                if (((Helpers.GetSerializationVisibility(info) != DesignerSerializationVisibility.Hidden) && (info.GetSetMethod() == null)) && (info.GetSetMethod(true) != null))
                {
                    list.Add(info);
                }
            }
            return list.ToArray();
        }
    }
}

