namespace System.Web.Services.Protocols
{
    using System;

    internal class HttpClientMethod
    {
        internal LogicalMethodInfo methodInfo;
        internal object readerInitializer;
        internal Type readerType;
        internal object writerInitializer;
        internal Type writerType;
    }
}

