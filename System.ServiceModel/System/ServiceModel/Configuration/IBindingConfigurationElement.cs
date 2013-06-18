namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;

    public interface IBindingConfigurationElement
    {
        void ApplyConfiguration(Binding binding);

        TimeSpan CloseTimeout { get; }

        string Name { get; }

        TimeSpan OpenTimeout { get; }

        TimeSpan ReceiveTimeout { get; }

        TimeSpan SendTimeout { get; }
    }
}

