namespace System.Runtime.Serialization
{
    using System;

    public interface IExtensibleDataObject
    {
        ExtensionDataObject ExtensionData { get; set; }
    }
}

