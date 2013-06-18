namespace System.ServiceModel.Configuration
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class WSHttpContextBindingCollectionElement : StandardBindingCollectionElement<WSHttpContextBinding, WSHttpContextBindingElement>
    {
        internal const string wsHttpContextBindingName = "wsHttpContextBinding";

        internal static WSHttpContextBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSHttpContextBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("wsHttpContextBinding");
        }
    }
}

