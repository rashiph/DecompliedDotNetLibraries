namespace System.ServiceModel.Activities
{
    public sealed class RequestReplyCorrelationInitializer : CorrelationInitializer
    {
        internal override CorrelationInitializer CloneCore()
        {
            return new RequestReplyCorrelationInitializer();
        }
    }
}

