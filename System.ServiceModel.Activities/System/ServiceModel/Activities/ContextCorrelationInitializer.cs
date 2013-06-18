namespace System.ServiceModel.Activities
{
    public sealed class ContextCorrelationInitializer : CorrelationInitializer
    {
        internal override CorrelationInitializer CloneCore()
        {
            return new ContextCorrelationInitializer();
        }
    }
}

