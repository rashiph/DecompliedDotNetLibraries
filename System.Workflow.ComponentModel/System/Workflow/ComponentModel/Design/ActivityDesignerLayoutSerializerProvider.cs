namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;

    internal sealed class ActivityDesignerLayoutSerializerProvider : IDesignerSerializationProvider
    {
        object IDesignerSerializationProvider.GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (typeof(Color) == objectType)
            {
                currentSerializer = new ColorMarkupSerializer();
                return currentSerializer;
            }
            if (typeof(Size) == objectType)
            {
                currentSerializer = new SizeMarkupSerializer();
                return currentSerializer;
            }
            if (typeof(Point) == objectType)
            {
                currentSerializer = new PointMarkupSerializer();
            }
            return currentSerializer;
        }
    }
}

