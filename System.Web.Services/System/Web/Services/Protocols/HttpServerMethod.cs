namespace System.Web.Services.Protocols
{
    using System;

    internal class HttpServerMethod
    {
        internal LogicalMethodInfo methodInfo;
        internal string name;
        internal object[] readerInitializers;
        internal Type[] readerTypes;
        internal object writerInitializer;
        internal Type writerType;
    }
}

