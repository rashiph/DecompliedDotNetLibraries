namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;

    public class BindingContext
    {
        private CustomBinding binding;
        private BindingParameterCollection bindingParameters;
        private Uri listenUriBaseAddress;
        private System.ServiceModel.Description.ListenUriMode listenUriMode;
        private string listenUriRelativeAddress;
        private BindingElementCollection remainingBindingElements;

        public BindingContext(CustomBinding binding, BindingParameterCollection parameters) : this(binding, parameters, null, string.Empty, System.ServiceModel.Description.ListenUriMode.Explicit)
        {
        }

        public BindingContext(CustomBinding binding, BindingParameterCollection parameters, Uri listenUriBaseAddress, string listenUriRelativeAddress, System.ServiceModel.Description.ListenUriMode listenUriMode)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (listenUriRelativeAddress == null)
            {
                listenUriRelativeAddress = string.Empty;
            }
            if (!ListenUriModeHelper.IsDefined(listenUriMode))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("listenUriMode"));
            }
            this.Initialize(binding, binding.Elements, parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode);
        }

        private BindingContext(CustomBinding binding, BindingElementCollection remainingBindingElements, BindingParameterCollection parameters, Uri listenUriBaseAddress, string listenUriRelativeAddress, System.ServiceModel.Description.ListenUriMode listenUriMode)
        {
            this.Initialize(binding, remainingBindingElements, parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode);
        }

        public IChannelFactory<TChannel> BuildInnerChannelFactory<TChannel>()
        {
            return this.RemoveNextElement().BuildChannelFactory<TChannel>(this);
        }

        public IChannelListener<TChannel> BuildInnerChannelListener<TChannel>() where TChannel: class, IChannel
        {
            return this.RemoveNextElement().BuildChannelListener<TChannel>(this);
        }

        public bool CanBuildInnerChannelFactory<TChannel>()
        {
            BindingContext context = this.Clone();
            return context.RemoveNextElement().CanBuildChannelFactory<TChannel>(context);
        }

        public bool CanBuildInnerChannelListener<TChannel>() where TChannel: class, IChannel
        {
            BindingContext context = this.Clone();
            return context.RemoveNextElement().CanBuildChannelListener<TChannel>(context);
        }

        public BindingContext Clone()
        {
            return new BindingContext(this.binding, this.remainingBindingElements, this.bindingParameters, this.listenUriBaseAddress, this.listenUriRelativeAddress, this.listenUriMode);
        }

        public T GetInnerProperty<T>() where T: class
        {
            if (this.remainingBindingElements.Count == 0)
            {
                return default(T);
            }
            BindingContext context = this.Clone();
            return context.RemoveNextElement().GetProperty<T>(context);
        }

        private void Initialize(CustomBinding binding, BindingElementCollection remainingBindingElements, BindingParameterCollection parameters, Uri listenUriBaseAddress, string listenUriRelativeAddress, System.ServiceModel.Description.ListenUriMode listenUriMode)
        {
            this.binding = binding;
            this.remainingBindingElements = new BindingElementCollection(remainingBindingElements);
            this.bindingParameters = new BindingParameterCollection(parameters);
            this.listenUriBaseAddress = listenUriBaseAddress;
            this.listenUriRelativeAddress = listenUriRelativeAddress;
            this.listenUriMode = listenUriMode;
        }

        private BindingElement RemoveNextElement()
        {
            BindingElement element = this.remainingBindingElements.Remove<BindingElement>();
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoChannelBuilderAvailable", new object[] { this.binding.Name, this.binding.Namespace })));
            }
            return element;
        }

        internal void ValidateBindingElementsConsumed()
        {
            if (this.RemainingBindingElements.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (BindingElement element in this.RemainingBindingElements)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                        builder.Append(" ");
                    }
                    string str = element.GetType().ToString();
                    builder.Append(str.Substring(str.LastIndexOf('.') + 1));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NotAllBindingElementsBuilt", new object[] { builder.ToString() })));
            }
        }

        public CustomBinding Binding
        {
            get
            {
                return this.binding;
            }
        }

        public BindingParameterCollection BindingParameters
        {
            get
            {
                return this.bindingParameters;
            }
        }

        public Uri ListenUriBaseAddress
        {
            get
            {
                return this.listenUriBaseAddress;
            }
            set
            {
                this.listenUriBaseAddress = value;
            }
        }

        public System.ServiceModel.Description.ListenUriMode ListenUriMode
        {
            get
            {
                return this.listenUriMode;
            }
            set
            {
                this.listenUriMode = value;
            }
        }

        public string ListenUriRelativeAddress
        {
            get
            {
                return this.listenUriRelativeAddress;
            }
            set
            {
                this.listenUriRelativeAddress = value;
            }
        }

        public BindingElementCollection RemainingBindingElements
        {
            get
            {
                return this.remainingBindingElements;
            }
        }
    }
}

