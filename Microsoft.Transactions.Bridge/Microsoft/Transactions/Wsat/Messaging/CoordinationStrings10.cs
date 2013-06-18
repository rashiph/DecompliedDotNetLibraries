namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class CoordinationStrings10 : CoordinationStrings
    {
        private static CoordinationStrings instance = new CoordinationStrings10();

        public override string CreateCoordinationContextAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext";
            }
        }

        public override string CreateCoordinationContextResponseAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse";
            }
        }

        public override string FaultAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault";
            }
        }

        public static CoordinationStrings Instance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return instance;
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor";
            }
        }

        public override string RegisterAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register";
            }
        }

        public override string RegisterResponseAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse";
            }
        }
    }
}

