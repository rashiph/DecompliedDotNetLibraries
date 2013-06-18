namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;

    internal class BindMarkupExtensionSerializer : MarkupExtensionSerializer
    {
        protected override InstanceDescriptor GetInstanceDescriptor(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            ActivityBind bind = value as ActivityBind;
            if (bind == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(ActivityBind).FullName }), "value");
            }
            return new InstanceDescriptor(typeof(ActivityBind).GetConstructor(new Type[] { typeof(string) }), new object[] { bind.Name });
        }
    }
}

