namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public class ChannelParameterCollection : Collection<object>
    {
        private IChannel channel;

        public ChannelParameterCollection()
        {
        }

        public ChannelParameterCollection(IChannel channel)
        {
            this.channel = channel;
        }

        protected override void ClearItems()
        {
            this.ThrowIfDisposedOrImmutable();
            base.ClearItems();
        }

        protected override void InsertItem(int index, object item)
        {
            this.ThrowIfDisposedOrImmutable();
            base.InsertItem(index, item);
        }

        public void PropagateChannelParameters(IChannel innerChannel)
        {
            if (innerChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerChannel");
            }
            this.ThrowIfMutable();
            ChannelParameterCollection property = innerChannel.GetProperty<ChannelParameterCollection>();
            if (property != null)
            {
                for (int i = 0; i < base.Count; i++)
                {
                    property.Add(base[i]);
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            this.ThrowIfDisposedOrImmutable();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, object item)
        {
            this.ThrowIfDisposedOrImmutable();
            base.SetItem(index, item);
        }

        private void ThrowIfDisposedOrImmutable()
        {
            IChannel channel = this.Channel;
            if (channel != null)
            {
                CommunicationState state = channel.State;
                string message = null;
                switch (state)
                {
                    case CommunicationState.Created:
                        break;

                    case CommunicationState.Opening:
                    case CommunicationState.Opened:
                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                    case CommunicationState.Faulted:
                        message = System.ServiceModel.SR.GetString("ChannelParametersCannotBeModified", new object[] { channel.GetType().ToString(), state.ToString() });
                        break;

                    default:
                        message = System.ServiceModel.SR.GetString("CommunicationObjectInInvalidState", new object[] { channel.GetType().ToString(), state.ToString() });
                        break;
                }
                if (message != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
                }
            }
        }

        private void ThrowIfMutable()
        {
            IChannel channel = this.Channel;
            if (channel != null)
            {
                CommunicationState state = channel.State;
                string message = null;
                switch (state)
                {
                    case CommunicationState.Created:
                        message = System.ServiceModel.SR.GetString("ChannelParametersCannotBePropagated", new object[] { channel.GetType().ToString(), state.ToString() });
                        break;

                    case CommunicationState.Opening:
                    case CommunicationState.Opened:
                    case CommunicationState.Closing:
                    case CommunicationState.Closed:
                    case CommunicationState.Faulted:
                        break;

                    default:
                        message = System.ServiceModel.SR.GetString("CommunicationObjectInInvalidState", new object[] { channel.GetType().ToString(), state.ToString() });
                        break;
                }
                if (message != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
                }
            }
        }

        protected virtual IChannel Channel
        {
            get
            {
                return this.channel;
            }
        }
    }
}

