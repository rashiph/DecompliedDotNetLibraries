namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal abstract class MsmqReceiveParameters
    {
        private MsmqUri.IAddressTranslator addressTranslator;
        private bool durable;
        private bool exactlyOnce;
        private int maxRetryCycles;
        private MsmqReceiveContextSettings receiveContextSettings;
        private System.ServiceModel.ReceiveErrorHandling receiveErrorHandling;
        private int receiveRetryCount;
        private TimeSpan retryCycleDelay;
        private MsmqTransportSecurity transportSecurity;
        private bool useMsmqTracing;
        private bool useSourceJournal;

        internal MsmqReceiveParameters(MsmqBindingElementBase bindingElement) : this(bindingElement, bindingElement.AddressTranslator)
        {
        }

        internal MsmqReceiveParameters(MsmqBindingElementBase bindingElement, MsmqUri.IAddressTranslator addressTranslator)
        {
            this.addressTranslator = addressTranslator;
            this.durable = bindingElement.Durable;
            this.exactlyOnce = bindingElement.ExactlyOnce;
            this.maxRetryCycles = bindingElement.MaxRetryCycles;
            this.receiveErrorHandling = bindingElement.ReceiveErrorHandling;
            this.receiveRetryCount = bindingElement.ReceiveRetryCount;
            this.retryCycleDelay = bindingElement.RetryCycleDelay;
            this.transportSecurity = new MsmqTransportSecurity(bindingElement.MsmqTransportSecurity);
            this.useMsmqTracing = bindingElement.UseMsmqTracing;
            this.useSourceJournal = bindingElement.UseSourceJournal;
            this.receiveContextSettings = new MsmqReceiveContextSettings(bindingElement.ReceiveContextSettings);
        }

        internal MsmqUri.IAddressTranslator AddressTranslator
        {
            get
            {
                return this.addressTranslator;
            }
        }

        internal bool Durable
        {
            get
            {
                return this.durable;
            }
        }

        internal bool ExactlyOnce
        {
            get
            {
                return this.exactlyOnce;
            }
        }

        internal int MaxRetryCycles
        {
            get
            {
                return this.maxRetryCycles;
            }
        }

        internal MsmqReceiveContextSettings ReceiveContextSettings
        {
            get
            {
                return this.receiveContextSettings;
            }
        }

        internal System.ServiceModel.ReceiveErrorHandling ReceiveErrorHandling
        {
            get
            {
                return this.receiveErrorHandling;
            }
        }

        internal int ReceiveRetryCount
        {
            get
            {
                return this.receiveRetryCount;
            }
        }

        internal TimeSpan RetryCycleDelay
        {
            get
            {
                return this.retryCycleDelay;
            }
        }

        internal MsmqTransportSecurity TransportSecurity
        {
            get
            {
                return this.transportSecurity;
            }
        }

        internal bool UseMsmqTracing
        {
            get
            {
                return this.useMsmqTracing;
            }
        }

        internal bool UseSourceJournal
        {
            get
            {
                return this.useSourceJournal;
            }
        }
    }
}

