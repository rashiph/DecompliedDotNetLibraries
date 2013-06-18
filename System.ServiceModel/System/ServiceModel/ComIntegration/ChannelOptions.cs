namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel;

    internal class ChannelOptions : IChannelOptions, IDisposable
    {
        protected IProvideChannelBuilderSettings channelBuilderSettings;

        internal ChannelOptions(IProvideChannelBuilderSettings channelBuilderSettings)
        {
            this.channelBuilderSettings = channelBuilderSettings;
        }

        internal static ComProxy Create(IntPtr outer, IProvideChannelBuilderSettings channelBuilderSettings)
        {
            ComProxy proxy2;
            if (channelBuilderSettings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotCreateChannelOption")));
            }
            ChannelOptions options = null;
            ComProxy proxy = null;
            try
            {
                options = new ChannelOptions(channelBuilderSettings);
                proxy = ComProxy.Create(outer, options, options);
                proxy2 = proxy;
            }
            finally
            {
                if ((proxy == null) && (options != null))
                {
                    ((IDisposable) options).Dispose();
                }
            }
            return proxy2;
        }

        void IDisposable.Dispose()
        {
        }
    }
}

