namespace System.ComponentModel.Design.Serialization
{
    using System;

    public interface IDesignerSerializationProvider
    {
        object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType);
    }
}

