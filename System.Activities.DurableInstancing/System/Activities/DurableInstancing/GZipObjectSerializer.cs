namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    internal sealed class GZipObjectSerializer : DefaultObjectSerializer
    {
        protected override Dictionary<XName, object> DeserializePropertyBag(Stream stream)
        {
            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                return base.DeserializePropertyBag(stream2);
            }
        }

        protected override object DeserializeValue(Stream stream)
        {
            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                return base.DeserializeValue(stream2);
            }
        }

        protected override void SerializePropertyBag(Stream stream, Dictionary<XName, object> propertyBag)
        {
            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, true))
            {
                base.SerializePropertyBag(stream2, propertyBag);
            }
        }

        protected override void SerializeValue(Stream stream, object value)
        {
            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress, true))
            {
                base.SerializeValue(stream2, value);
            }
        }
    }
}

