namespace System.Xml
{
    using System;

    internal interface IRemovableWriter
    {
        OnRemoveWriter OnRemoveWriterEvent { get; set; }
    }
}

