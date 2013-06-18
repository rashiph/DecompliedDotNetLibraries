namespace System.Runtime
{
    using System;
    using System.Xml.Linq;

    internal static class WorkflowServiceNamespace
    {
        private const string baseNamespace = "urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties";
        private static XName controlEndpoint;
        private static XName creationContext;
        private static readonly XNamespace endpointsNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties/endpoints");
        private static XName relativeApplicationPath;
        private static XName relativeServicePath;
        private static XName service;
        private static XName siteName;
        private static XName suspendException;
        private static XName suspendReason;
        private static readonly XNamespace workflowServiceNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties");

        public static XName ControlEndpoint
        {
            get
            {
                if (controlEndpoint == null)
                {
                    controlEndpoint = workflowServiceNamespace.GetName("ControlEndpoint");
                }
                return controlEndpoint;
            }
        }

        public static XName CreationContext
        {
            get
            {
                if (creationContext == null)
                {
                    creationContext = workflowServiceNamespace.GetName("CreationContext");
                }
                return creationContext;
            }
        }

        public static XNamespace EndpointsPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return endpointsNamespace;
            }
        }

        public static XName RelativeApplicationPath
        {
            get
            {
                if (relativeApplicationPath == null)
                {
                    relativeApplicationPath = workflowServiceNamespace.GetName("RelativeApplicationPath");
                }
                return relativeApplicationPath;
            }
        }

        public static XName RelativeServicePath
        {
            get
            {
                if (relativeServicePath == null)
                {
                    relativeServicePath = workflowServiceNamespace.GetName("RelativeServicePath");
                }
                return relativeServicePath;
            }
        }

        public static XName Service
        {
            get
            {
                if (service == null)
                {
                    service = workflowServiceNamespace.GetName("Service");
                }
                return service;
            }
        }

        public static XName SiteName
        {
            get
            {
                if (siteName == null)
                {
                    siteName = workflowServiceNamespace.GetName("SiteName");
                }
                return siteName;
            }
        }

        public static XName SuspendException
        {
            get
            {
                if (suspendException == null)
                {
                    suspendException = workflowServiceNamespace.GetName("SuspendException");
                }
                return suspendException;
            }
        }

        public static XName SuspendReason
        {
            get
            {
                if (suspendReason == null)
                {
                    suspendReason = workflowServiceNamespace.GetName("SuspendReason");
                }
                return suspendReason;
            }
        }
    }
}

