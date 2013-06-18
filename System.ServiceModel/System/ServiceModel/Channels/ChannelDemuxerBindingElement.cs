namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ChannelDemuxerBindingElement : BindingElement
    {
        private bool cacheContextState;
        private CachedBindingContextState cachedContextState;
        private ChannelDemuxer demuxer;

        public ChannelDemuxerBindingElement(bool cacheContextState)
        {
            this.cacheContextState = cacheContextState;
            if (cacheContextState)
            {
                this.cachedContextState = new CachedBindingContextState();
            }
            this.demuxer = new ChannelDemuxer();
        }

        public ChannelDemuxerBindingElement(ChannelDemuxerBindingElement element)
        {
            this.demuxer = element.demuxer;
            this.cacheContextState = element.cacheContextState;
            this.cachedContextState = element.cachedContextState;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.SubstituteCachedBindingContextParametersIfNeeded(context);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            ChannelDemuxerFilter filter = context.BindingParameters.Remove<ChannelDemuxerFilter>();
            this.SubstituteCachedBindingContextParametersIfNeeded(context);
            if (filter == null)
            {
                return this.demuxer.BuildChannelListener<TChannel>(context);
            }
            return this.demuxer.BuildChannelListener<TChannel>(context, filter);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new ChannelDemuxerBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (this.cacheContextState && this.cachedContextState.IsStateCached)
            {
                for (int i = 0; i < this.cachedContextState.CachedBindingParameters.Count; i++)
                {
                    if (!context.BindingParameters.Contains(this.cachedContextState.CachedBindingParameters[i].GetType()))
                    {
                        context.BindingParameters.Add(this.cachedContextState.CachedBindingParameters[i]);
                    }
                }
            }
            return context.GetInnerProperty<T>();
        }

        private void SubstituteCachedBindingContextParametersIfNeeded(BindingContext context)
        {
            if (this.cacheContextState)
            {
                if (!this.cachedContextState.IsStateCached)
                {
                    foreach (object obj2 in context.BindingParameters)
                    {
                        this.cachedContextState.CachedBindingParameters.Add(obj2);
                    }
                    this.cachedContextState.IsStateCached = true;
                }
                else
                {
                    context.BindingParameters.Clear();
                    foreach (object obj3 in this.cachedContextState.CachedBindingParameters)
                    {
                        context.BindingParameters.Add(obj3);
                    }
                }
            }
        }

        public int MaxPendingSessions
        {
            get
            {
                return this.demuxer.MaxPendingSessions;
            }
            set
            {
                if (value < 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
                }
                this.demuxer.MaxPendingSessions = value;
            }
        }

        public TimeSpan PeekTimeout
        {
            get
            {
                return this.demuxer.PeekTimeout;
            }
            set
            {
                if ((value < TimeSpan.Zero) && (value != ChannelDemuxer.UseDefaultReceiveTimeout))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.demuxer.PeekTimeout = value;
            }
        }

        private class CachedBindingContextState
        {
            public BindingParameterCollection CachedBindingParameters = new BindingParameterCollection();
            public bool IsStateCached;
        }
    }
}

