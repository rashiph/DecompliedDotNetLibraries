namespace System.ServiceModel.Activities
{
    using System;

    internal static class Constants
    {
        public const string CorrelatesWith = "CorrelatesWith";
        public static readonly Type CorrelationHandleType = typeof(CorrelationHandle);
        public static readonly object[] EmptyArray = new object[0];
        public static readonly string[] EmptyStringArray = new string[0];
        public static readonly Type[] EmptyTypeArray = new Type[0];
        public const string EndpointAddress = "EndpointAddress";
        public const string Message = "Message";
        public static readonly Type MessageType = typeof(System.ServiceModel.Channels.Message);
        public const string NoPersistHandle = "noPersistHandle";
        public static readonly Type NoPersistHandleType = typeof(System.Activities.NoPersistHandle);
        public const string Parameter = "Parameter";
        public const string RequestMessage = "RequestMessage";
        public const string Result = "Result";
        public const string TransactionHandle = "TransactionHandle";
        public static readonly Type UriType = typeof(Uri);
    }
}

