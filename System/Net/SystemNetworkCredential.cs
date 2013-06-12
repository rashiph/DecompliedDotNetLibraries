namespace System.Net
{
    using System;

    internal class SystemNetworkCredential : NetworkCredential
    {
        internal static readonly SystemNetworkCredential defaultCredential = new SystemNetworkCredential();

        private SystemNetworkCredential() : base(string.Empty, string.Empty, string.Empty)
        {
        }
    }
}

