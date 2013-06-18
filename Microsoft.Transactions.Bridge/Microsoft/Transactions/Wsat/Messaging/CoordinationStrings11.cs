namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class CoordinationStrings11 : CoordinationStrings
    {
        private static CoordinationStrings instance = new CoordinationStrings11();

        public override string CreateCoordinationContextAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContext";
            }
        }

        public override string CreateCoordinationContextResponseAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContextResponse";
            }
        }

        public override string FaultAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault";
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
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06";
            }
        }

        public override string RegisterAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/Register";
            }
        }

        public override string RegisterResponseAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/RegisterResponse";
            }
        }
    }
}

