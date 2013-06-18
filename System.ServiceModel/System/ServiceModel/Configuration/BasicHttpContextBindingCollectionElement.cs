namespace System.ServiceModel.Configuration
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class BasicHttpContextBindingCollectionElement : StandardBindingCollectionElement<BasicHttpContextBinding, BasicHttpContextBindingElement>
    {
        internal const string basicHttpContextBindingName = "basicHttpContextBinding";

        internal static BasicHttpContextBindingCollectionElement GetBindingCollectionElement()
        {
            return (BasicHttpContextBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("basicHttpContextBinding");
        }
    }
}

