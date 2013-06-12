namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IDeserializationCallback
    {
        void OnDeserialization(object sender);
    }
}

