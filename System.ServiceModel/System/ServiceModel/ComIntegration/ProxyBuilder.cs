namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class ProxyBuilder
    {
        internal static void Build(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable, ref Guid riid, IntPtr ppv)
        {
            string str;
            if (IntPtr.Zero == ppv)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ppv");
            }
            Marshal.WriteIntPtr(ppv, IntPtr.Zero);
            IProxyCreator proxyCreator = null;
            if (propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Wsdl, out str))
            {
                proxyCreator = new WsdlServiceChannelBuilder(propertyTable);
            }
            else if (propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexAddress, out str))
            {
                proxyCreator = new MexServiceChannelBuilder(propertyTable);
            }
            else
            {
                proxyCreator = new TypedServiceChannelBuilder(propertyTable);
            }
            IProxyManager proxyManager = new ProxyManager(proxyCreator);
            Marshal.WriteIntPtr(ppv, OuterProxyWrapper.CreateOuterProxyInstance(proxyManager, ref riid));
        }
    }
}

