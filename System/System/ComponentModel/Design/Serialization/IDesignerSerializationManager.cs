namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public interface IDesignerSerializationManager : IServiceProvider
    {
        event ResolveNameEventHandler ResolveName;

        event EventHandler SerializationComplete;

        void AddSerializationProvider(IDesignerSerializationProvider provider);
        object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer);
        object GetInstance(string name);
        string GetName(object value);
        object GetSerializer(Type objectType, Type serializerType);
        Type GetType(string typeName);
        void RemoveSerializationProvider(IDesignerSerializationProvider provider);
        void ReportError(object errorInformation);
        void SetName(object instance, string name);

        ContextStack Context { get; }

        PropertyDescriptorCollection Properties { get; }
    }
}

