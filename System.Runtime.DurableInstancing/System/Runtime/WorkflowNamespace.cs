namespace System.Runtime
{
    using System;
    using System.Xml.Linq;

    internal static class WorkflowNamespace
    {
        private const string baseNamespace = "urn:schemas-microsoft-com:System.Activities/4.0/properties";
        private static XName bookmarks;
        private static XName exception;
        private static XName keyProvider;
        private static XName lastUpdate;
        private static readonly XNamespace outputNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties/output");
        private static XName status;
        private static readonly XNamespace variablesNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties/variables");
        private static XName workflow;
        private static XName workflowHostType;
        private static readonly XNamespace workflowNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties");

        public static XName Bookmarks
        {
            get
            {
                if (bookmarks == null)
                {
                    bookmarks = workflowNamespace.GetName("Bookmarks");
                }
                return bookmarks;
            }
        }

        public static XName Exception
        {
            get
            {
                if (exception == null)
                {
                    exception = workflowNamespace.GetName("Exception");
                }
                return exception;
            }
        }

        public static XName KeyProvider
        {
            get
            {
                if (keyProvider == null)
                {
                    keyProvider = workflowNamespace.GetName("KeyProvider");
                }
                return keyProvider;
            }
        }

        public static XName LastUpdate
        {
            get
            {
                if (lastUpdate == null)
                {
                    lastUpdate = workflowNamespace.GetName("LastUpdate");
                }
                return lastUpdate;
            }
        }

        public static XNamespace OutputPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return outputNamespace;
            }
        }

        public static XName Status
        {
            get
            {
                if (status == null)
                {
                    status = workflowNamespace.GetName("Status");
                }
                return status;
            }
        }

        public static XNamespace VariablesPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return variablesNamespace;
            }
        }

        public static XName Workflow
        {
            get
            {
                if (workflow == null)
                {
                    workflow = workflowNamespace.GetName("Workflow");
                }
                return workflow;
            }
        }

        public static XName WorkflowHostType
        {
            get
            {
                if (workflowHostType == null)
                {
                    workflowHostType = workflowNamespace.GetName("WorkflowHostType");
                }
                return workflowHostType;
            }
        }
    }
}

