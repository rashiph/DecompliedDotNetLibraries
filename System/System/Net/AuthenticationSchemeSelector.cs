namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate AuthenticationSchemes AuthenticationSchemeSelector(HttpListenerRequest httpRequest);
}

