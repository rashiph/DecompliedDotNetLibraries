namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public static class MetadataExchangeBindings
    {
        private static Binding httpBinding;
        private static Binding httpGetBinding;
        private static Binding httpsBinding;
        private static Binding httpsGetBinding;
        private static Binding pipeBinding;
        private static Binding tcpBinding;

        private static CustomBinding CreateGetBinding(HttpTransportBindingElement httpTransport)
        {
            TextMessageEncodingBindingElement element = new TextMessageEncodingBindingElement {
                MessageVersion = MessageVersion.None
            };
            httpTransport.Method = "GET";
            httpTransport.InheritBaseAddressSettings = true;
            return new CustomBinding(new BindingElement[] { element, httpTransport });
        }

        private static WSHttpBinding CreateHttpBinding()
        {
            return new WSHttpBinding(SecurityMode.None, false) { Name = "MetadataExchangeHttpBinding", Namespace = "http://schemas.microsoft.com/ws/2005/02/mex/bindings" };
        }

        private static CustomBinding CreateHttpGetBinding()
        {
            return CreateGetBinding(new HttpTransportBindingElement());
        }

        private static WSHttpBinding CreateHttpsBinding()
        {
            return new WSHttpBinding(new WSHttpSecurity(SecurityMode.Transport, new HttpTransportSecurity(), new NonDualMessageSecurityOverHttp()), false) { Name = "MetadataExchangeHttpsBinding", Namespace = "http://schemas.microsoft.com/ws/2005/02/mex/bindings" };
        }

        private static CustomBinding CreateHttpsGetBinding()
        {
            return CreateGetBinding(new HttpsTransportBindingElement());
        }

        public static Binding CreateMexHttpBinding()
        {
            return CreateHttpBinding();
        }

        public static Binding CreateMexHttpsBinding()
        {
            return CreateHttpsBinding();
        }

        public static Binding CreateMexNamedPipeBinding()
        {
            return CreateNamedPipeBinding();
        }

        public static Binding CreateMexTcpBinding()
        {
            return CreateTcpBinding();
        }

        private static CustomBinding CreateNamedPipeBinding()
        {
            CustomBinding binding = new CustomBinding("MetadataExchangeNamedPipeBinding", "http://schemas.microsoft.com/ws/2005/02/mex/bindings", new BindingElement[0]);
            NamedPipeTransportBindingElement item = new NamedPipeTransportBindingElement();
            binding.Elements.Add(item);
            return binding;
        }

        private static CustomBinding CreateTcpBinding()
        {
            CustomBinding binding = new CustomBinding("MetadataExchangeTcpBinding", "http://schemas.microsoft.com/ws/2005/02/mex/bindings", new BindingElement[0]);
            TcpTransportBindingElement item = new TcpTransportBindingElement();
            binding.Elements.Add(item);
            return binding;
        }

        internal static Binding GetBindingForScheme(string scheme)
        {
            Binding binding = null;
            TryGetBindingForScheme(scheme, out binding);
            return binding;
        }

        internal static bool IsSchemeSupported(string scheme)
        {
            Binding binding;
            return TryGetBindingForScheme(scheme, out binding);
        }

        internal static bool TryGetBindingForScheme(string scheme, out Binding binding)
        {
            if (string.Compare(scheme, "http", StringComparison.OrdinalIgnoreCase) == 0)
            {
                binding = Http;
            }
            else if (string.Compare(scheme, "https", StringComparison.OrdinalIgnoreCase) == 0)
            {
                binding = Https;
            }
            else if (string.Compare(scheme, "net.tcp", StringComparison.OrdinalIgnoreCase) == 0)
            {
                binding = Tcp;
            }
            else if (string.Compare(scheme, "net.pipe", StringComparison.OrdinalIgnoreCase) == 0)
            {
                binding = NamedPipe;
            }
            else
            {
                binding = null;
            }
            return (binding != null);
        }

        internal static Binding Http
        {
            get
            {
                if (httpBinding == null)
                {
                    httpBinding = CreateHttpBinding();
                }
                return httpBinding;
            }
        }

        internal static Binding HttpGet
        {
            get
            {
                if (httpGetBinding == null)
                {
                    httpGetBinding = CreateHttpGetBinding();
                }
                return httpGetBinding;
            }
        }

        internal static Binding Https
        {
            get
            {
                if (httpsBinding == null)
                {
                    httpsBinding = CreateHttpsBinding();
                }
                return httpsBinding;
            }
        }

        internal static Binding HttpsGet
        {
            get
            {
                if (httpsGetBinding == null)
                {
                    httpsGetBinding = CreateHttpsGetBinding();
                }
                return httpsGetBinding;
            }
        }

        internal static Binding NamedPipe
        {
            get
            {
                if (pipeBinding == null)
                {
                    pipeBinding = CreateNamedPipeBinding();
                }
                return pipeBinding;
            }
        }

        internal static Binding Tcp
        {
            get
            {
                if (tcpBinding == null)
                {
                    tcpBinding = CreateTcpBinding();
                }
                return tcpBinding;
            }
        }
    }
}

