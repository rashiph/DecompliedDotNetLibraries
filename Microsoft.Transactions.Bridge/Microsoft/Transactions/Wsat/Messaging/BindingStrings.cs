namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;

    internal static class BindingStrings
    {
        public const string AddressPrefix = "WsatService";
        public const string DisabledSuffix = "Disabled/";
        public const string InteropBindingName = "Interop";
        private static readonly string InteropBindingQName10 = new XmlQualifiedName("Interop", "http://schemas.xmlsoap.org/ws/2004/10/wsat").ToString();
        private static readonly string InteropBindingQName11 = new XmlQualifiedName("Interop", "http://docs.oasis-open.org/ws-tx/wsat/2006/06").ToString();
        public const string NamedPipeBindingName = "NamedPipe";
        public static readonly string NamedPipeBindingQName = new XmlQualifiedName("NamedPipe", "http://schemas.microsoft.com/ws/2006/02/transactions").ToString();
        public const string RemoteProxySuffix = "Remote/";
        public const string WindowsBindingName = "Windows";
        public static readonly string WindowsBindingQName = new XmlQualifiedName("Windows", "http://schemas.microsoft.com/ws/2006/02/transactions").ToString();

        public static string ActivationCoordinatorSuffix(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "ActivationCoordinatorSuffix");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "Activation/Coordinator/";

                case ProtocolVersion.Version11:
                    return "Activation/Coordinator11/";
            }
            return null;
        }

        public static string CompletionCoordinatorSuffix(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "CompletionCoordinatorSuffix");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "Completion/Coordinator/";

                case ProtocolVersion.Version11:
                    return "Completion/Coordinator11/";
            }
            return null;
        }

        public static string CompletionParticipantSuffix(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "CompletionParticipantSuffix");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "Completion/Participant/";

                case ProtocolVersion.Version11:
                    return "Completion/Participant11/";
            }
            return null;
        }

        public static string InteropBindingQName(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "InteropBindingQName");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return InteropBindingQName10;

                case ProtocolVersion.Version11:
                    return InteropBindingQName11;
            }
            return null;
        }

        public static string RegistrationCoordinatorSuffix(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "RegistrationCoordinatorSuffix");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "Registration/Coordinator/";

                case ProtocolVersion.Version11:
                    return "Registration/Coordinator11/";
            }
            return null;
        }

        public static string TwoPhaseCommitCoordinatorSuffix(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "TwoPhaseCommitCoordinatorSuffix");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "TwoPhaseCommit/Coordinator/";

                case ProtocolVersion.Version11:
                    return "TwoPhaseCommit/Coordinator11/";
            }
            return null;
        }

        public static string TwoPhaseCommitParticipantSuffix(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(BindingStrings), "TwoPhaseCommitParticipantSuffix");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "TwoPhaseCommit/Participant/";

                case ProtocolVersion.Version11:
                    return "TwoPhaseCommit/Participant11/";
            }
            return null;
        }
    }
}

