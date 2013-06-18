namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;

    internal class WorkflowMarkupSerializationProvider : IDesignerSerializationProvider
    {
        public virtual object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if ((serializerType != typeof(WorkflowMarkupSerializer)) || (currentSerializer != null))
            {
                return null;
            }
            if (typeof(IDictionary).IsAssignableFrom(objectType))
            {
                return new DictionaryMarkupSerializer();
            }
            if (CollectionMarkupSerializer.IsValidCollectionType(objectType))
            {
                return new CollectionMarkupSerializer();
            }
            return new WorkflowMarkupSerializer();
        }
    }
}

