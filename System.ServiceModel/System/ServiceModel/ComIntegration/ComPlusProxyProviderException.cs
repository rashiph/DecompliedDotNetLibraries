namespace System.ServiceModel.ComIntegration
{
    using System;

    [Serializable]
    internal class ComPlusProxyProviderException : Exception
    {
        public ComPlusProxyProviderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

