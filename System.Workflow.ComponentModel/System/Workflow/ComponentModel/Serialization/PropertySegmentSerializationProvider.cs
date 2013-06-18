namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Design;

    internal sealed class PropertySegmentSerializationProvider : WorkflowMarkupSerializationProvider
    {
        public override object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (!serializerType.IsAssignableFrom(typeof(WorkflowMarkupSerializer)))
            {
                return base.GetSerializer(manager, currentSerializer, objectType, serializerType);
            }
            if (currentSerializer is PropertySegmentSerializer)
            {
                return currentSerializer;
            }
            if (objectType == typeof(PropertySegment))
            {
                return new PropertySegmentSerializer(null);
            }
            if (currentSerializer is WorkflowMarkupSerializer)
            {
                return new PropertySegmentSerializer(currentSerializer as WorkflowMarkupSerializer);
            }
            return null;
        }
    }
}

