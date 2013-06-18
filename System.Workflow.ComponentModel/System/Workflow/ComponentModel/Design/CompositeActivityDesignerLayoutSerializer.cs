namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel.Serialization;

    public class CompositeActivityDesignerLayoutSerializer : ActivityDesignerLayoutSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            return new List<PropertyInfo>(base.GetProperties(serializationManager, obj)) { typeof(CompositeActivityDesigner).GetProperty("Designers", 0x24) }.ToArray();
        }
    }
}

