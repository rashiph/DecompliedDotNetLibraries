namespace System.Net
{
    using System;
    using System.Collections;

    internal class WebProxyData
    {
        internal bool automaticallyDetectSettings;
        internal ArrayList bypassList;
        internal bool bypassOnLocal;
        internal Uri proxyAddress;
        internal Hashtable proxyHostAddresses;
        internal Uri scriptLocation;
    }
}

