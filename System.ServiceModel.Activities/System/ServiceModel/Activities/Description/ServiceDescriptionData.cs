namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ServiceDescriptionData
    {
        public bool IsFirstReceiveOfTransactedReceiveScopeTree { get; set; }

        public bool IsInsideTransactedReceiveScope { get; set; }
    }
}

