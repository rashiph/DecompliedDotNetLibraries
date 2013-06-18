namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Runtime;

    public abstract class SoapExtension
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SoapExtension()
        {
        }

        public virtual Stream ChainStream(Stream stream)
        {
            return stream;
        }

        public abstract object GetInitializer(Type serviceType);
        public abstract object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute);
        public abstract void Initialize(object initializer);
        public abstract void ProcessMessage(SoapMessage message);
    }
}

