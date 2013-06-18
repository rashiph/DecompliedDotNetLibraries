namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class NetDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        internal NetDataContractSerializerOperationBehavior(OperationDescription operation) : base(operation)
        {
        }

        internal static NetDataContractSerializerOperationBehavior ApplyTo(OperationDescription operation)
        {
            NetDataContractSerializerOperationBehavior item = null;
            DataContractSerializerOperationBehavior behavior2 = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (behavior2 != null)
            {
                item = new NetDataContractSerializerOperationBehavior(operation);
                operation.Behaviors.Remove(behavior2);
                operation.Behaviors.Add(item);
                return item;
            }
            return null;
        }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }
    }
}

