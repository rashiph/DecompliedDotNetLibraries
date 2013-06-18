namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel.Design.Serialization;

    internal class TypeExtensionSerializer : MarkupExtensionSerializer
    {
        protected override InstanceDescriptor GetInstanceDescriptor(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            TypeExtension extension = value as TypeExtension;
            if (extension == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(TypeExtension).FullName }), "value");
            }
            if (extension.Type != null)
            {
                return new InstanceDescriptor(typeof(TypeExtension).GetConstructor(new Type[] { typeof(Type) }), new object[] { extension.Type });
            }
            return new InstanceDescriptor(typeof(TypeExtension).GetConstructor(new Type[] { typeof(string) }), new object[] { extension.TypeName });
        }
    }
}

