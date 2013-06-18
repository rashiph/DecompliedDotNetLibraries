namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    internal class StateDesignerLayoutSerializer : FreeformActivityDesignerLayoutSerializer
    {
        protected override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            StateDesigner designer = obj as StateDesigner;
            if (designer != null)
            {
                foreach (PropertyInfo info in base.GetProperties(serializationManager, obj))
                {
                    if (info.Name.Equals("Location", StringComparison.Ordinal) || info.Name.Equals("Size", StringComparison.Ordinal))
                    {
                        list.Add(new System.Workflow.Activities.ExtendedPropertyInfo(info, new System.Workflow.Activities.GetValueHandler(designer.OnGetPropertyValue)));
                    }
                    else
                    {
                        list.Add(info);
                    }
                }
            }
            else
            {
                list.AddRange(base.GetProperties(serializationManager, obj));
            }
            return list.ToArray();
        }
    }
}

