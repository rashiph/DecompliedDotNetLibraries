namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime.Serialization;

    internal sealed class SimpleTypesSurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj.GetType() == typeof(Guid))
            {
                Guid guid = (Guid) obj;
                info.AddValue("typeID", TypeID.Guid);
                info.AddValue("bits", guid.ToByteArray());
            }
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            TypeID eid = (TypeID) info.GetValue("typeID", typeof(TypeID));
            if (eid == TypeID.Guid)
            {
                return new Guid(info.GetValue("bits", typeof(byte[])) as byte[]);
            }
            return null;
        }

        private enum TypeID : byte
        {
            Guid = 1,
            Null = 2
        }
    }
}

