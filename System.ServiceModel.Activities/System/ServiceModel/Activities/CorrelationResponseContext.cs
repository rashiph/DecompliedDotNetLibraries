namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;

    internal class CorrelationResponseContext
    {
        internal System.Exception Exception { get; set; }

        internal System.ServiceModel.Channels.MessageVersion MessageVersion { get; set; }

        internal System.ServiceModel.Activities.WorkflowOperationContext WorkflowOperationContext { get; set; }
    }
}

