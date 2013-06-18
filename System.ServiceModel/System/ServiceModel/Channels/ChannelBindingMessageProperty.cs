namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;

    internal sealed class ChannelBindingMessageProperty : IDisposable, IMessageProperty
    {
        private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding;
        private bool ownsCleanup;
        private const string propertyName = "ChannelBindingMessageProperty";
        private int refCount;
        private object thisLock;

        public ChannelBindingMessageProperty(System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding, bool ownsCleanup)
        {
            if (channelBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelBinding");
            }
            this.refCount = 1;
            this.thisLock = new object();
            this.channelBinding = channelBinding;
            this.ownsCleanup = ownsCleanup;
        }

        public void AddTo(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.AddTo(message.Properties);
        }

        public void AddTo(MessageProperties properties)
        {
            this.ThrowIfDisposed();
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            properties.Add(Name, this);
        }

        public IMessageProperty CreateCopy()
        {
            lock (this.thisLock)
            {
                this.ThrowIfDisposed();
                this.refCount++;
                return this;
            }
        }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                lock (this.thisLock)
                {
                    if ((!this.IsDisposed && (--this.refCount == 0)) && this.ownsCleanup)
                    {
                        this.channelBinding.Dispose();
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public static bool TryGet(Message message, out ChannelBindingMessageProperty property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out ChannelBindingMessageProperty property)
        {
            object obj2;
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            property = null;
            if (properties.TryGetValue(Name, out obj2))
            {
                property = obj2 as ChannelBindingMessageProperty;
                return (property != null);
            }
            return false;
        }

        public System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                this.ThrowIfDisposed();
                return this.channelBinding;
            }
        }

        private bool IsDisposed
        {
            get
            {
                return (this.refCount <= 0);
            }
        }

        public static string Name
        {
            get
            {
                return "ChannelBindingMessageProperty";
            }
        }
    }
}

