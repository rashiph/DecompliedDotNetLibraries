namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Workflow.ComponentModel.Serialization;

    internal sealed class ThemeSerializationProvider : WorkflowMarkupSerializationProvider
    {
        public override object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (serializerType.IsAssignableFrom(typeof(WorkflowMarkupSerializer)))
            {
                if (typeof(Color) == objectType)
                {
                    return new ColorMarkupSerializer();
                }
                if (typeof(Size) == objectType)
                {
                    return new SizeMarkupSerializer();
                }
            }
            return base.GetSerializer(manager, currentSerializer, objectType, serializerType);
        }
    }
}

