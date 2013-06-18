namespace System.ServiceModel.Activities
{
    public sealed class CallbackCorrelationInitializer : CorrelationInitializer
    {
        internal override CorrelationInitializer CloneCore()
        {
            return new CallbackCorrelationInitializer();
        }
    }
}

