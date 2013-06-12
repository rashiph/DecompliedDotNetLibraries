namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;

    public interface IDesignerSerializationService
    {
        ICollection Deserialize(object serializationData);
        object Serialize(ICollection objects);
    }
}

