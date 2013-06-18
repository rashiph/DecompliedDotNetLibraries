namespace System.Activities.DurableInstancing
{
    using System;

    internal static class ObjectSerializerFactory
    {
        public static IObjectSerializer GetDefaultObjectSerializer()
        {
            return new DefaultObjectSerializer();
        }

        public static IObjectSerializer GetObjectSerializer(InstanceEncodingOption instanceEncodingOption)
        {
            switch (instanceEncodingOption)
            {
                case InstanceEncodingOption.None:
                    return new DefaultObjectSerializer();

                case InstanceEncodingOption.GZip:
                    return new GZipObjectSerializer();
            }
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.UnknownCompressionOption(instanceEncodingOption)));
        }
    }
}

