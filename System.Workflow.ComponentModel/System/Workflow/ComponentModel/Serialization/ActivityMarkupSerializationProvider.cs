namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;

    internal sealed class ActivityMarkupSerializationProvider : WorkflowMarkupSerializationProvider
    {
        public override object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if ((serializerType != typeof(WorkflowMarkupSerializer)) || (currentSerializer != null))
            {
                return null;
            }
            if (typeof(CompositeActivity).IsAssignableFrom(objectType))
            {
                return new CompositeActivityMarkupSerializer();
            }
            if (typeof(ItemList<>).IsAssignableFrom(objectType))
            {
                return new CollectionMarkupSerializer();
            }
            IDesignerSerializationProvider provider = new WorkflowMarkupSerializationProvider();
            object obj2 = provider.GetSerializer(manager, currentSerializer, objectType, serializerType);
            if (obj2.GetType() != typeof(WorkflowMarkupSerializer))
            {
                return obj2;
            }
            return new ActivityMarkupSerializer();
        }
    }
}

