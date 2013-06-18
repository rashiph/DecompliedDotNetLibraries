namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Transactions;

    [DataContract]
    internal class TransactedReceiveData
    {
        private const string propertyName = "System.ServiceModel.Activities.TransactedReceiveDataExecutionPropertyName";

        public Transaction InitiatingTransaction { get; set; }

        public static string TransactedReceiveDataExecutionPropertyName
        {
            get
            {
                return "System.ServiceModel.Activities.TransactedReceiveDataExecutionPropertyName";
            }
        }
    }
}

