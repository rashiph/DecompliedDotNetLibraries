namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    internal interface IConfigurationContextProviderInternal
    {
        ContextInformation GetEvaluationContext();
        ContextInformation GetOriginalEvaluationContext();
    }
}

