namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;

    internal interface IObjectSerializer
    {
        Dictionary<XName, object> DeserializePropertyBag(byte[] bytes);
        object DeserializeValue(byte[] bytes);
        ArraySegment<byte> SerializePropertyBag(Dictionary<XName, object> value);
        ArraySegment<byte> SerializeValue(object value);
    }
}

