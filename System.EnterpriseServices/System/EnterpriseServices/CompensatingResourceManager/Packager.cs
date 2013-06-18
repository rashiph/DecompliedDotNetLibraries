namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    internal class Packager
    {
        private static volatile bool _initialized = false;
        private static BinaryFormatter _ser;

        internal static object Deserialize(BlobPackage b)
        {
            Init();
            byte[] bits = b.GetBits();
            return _ser.Deserialize(new MemoryStream(bits, false));
        }

        private static void Init()
        {
            if (!_initialized)
            {
                lock (typeof(Packager))
                {
                    if (!_initialized)
                    {
                        StreamingContext context = new StreamingContext(StreamingContextStates.Persistence | StreamingContextStates.File);
                        _ser = new BinaryFormatter(null, context);
                        _initialized = true;
                    }
                }
            }
        }

        internal static byte[] Serialize(object o)
        {
            Init();
            MemoryStream serializationStream = new MemoryStream();
            _ser.Serialize(serializationStream, o);
            return serializationStream.GetBuffer();
        }
    }
}

