namespace System.Workflow.Activities
{
    using System;
    using System.Threading;

    internal sealed class CorrelationMethodResolver
    {
        private ICorrelationProvider correlationProvider;
        private object corrProviderSync = new object();
        private Type interfaceType;

        internal CorrelationMethodResolver(Type interfaceType)
        {
            this.interfaceType = interfaceType;
        }

        internal ICorrelationProvider CorrelationProvider
        {
            get
            {
                if (this.correlationProvider == null)
                {
                    lock (this.corrProviderSync)
                    {
                        if (this.correlationProvider == null)
                        {
                            ICorrelationProvider provider = null;
                            object[] customAttributes = this.interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), true);
                            if (customAttributes.Length == 0)
                            {
                                customAttributes = this.interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), true);
                                object[] objArray2 = this.interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), true);
                                if ((customAttributes.Length != 0) && (objArray2.Length != 0))
                                {
                                    provider = new DefaultCorrelationProvider(this.interfaceType);
                                }
                                else
                                {
                                    provider = new NonCorrelatedProvider();
                                }
                            }
                            else
                            {
                                CorrelationProviderAttribute attribute = customAttributes[0] as CorrelationProviderAttribute;
                                provider = Activator.CreateInstance(attribute.CorrelationProviderType) as ICorrelationProvider;
                            }
                            Thread.MemoryBarrier();
                            this.correlationProvider = provider;
                        }
                    }
                }
                return this.correlationProvider;
            }
        }
    }
}

