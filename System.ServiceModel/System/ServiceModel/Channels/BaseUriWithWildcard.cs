namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract]
    internal sealed class BaseUriWithWildcard
    {
        [DataMember]
        private Uri baseAddress;
        private Comparand comparand;
        private int hashCode;
        [DataMember]
        private System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private const int HttpsUriDefaultPort = 0x1bb;
        private const int HttpUriDefaultPort = 80;
        private const string plus = "+";
        private const char segmentDelimiter = '/';
        private const string star = "*";

        public BaseUriWithWildcard(Uri baseAddress, System.ServiceModel.HostNameComparisonMode hostNameComparisonMode)
        {
            this.baseAddress = baseAddress;
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.SetComparisonAddressAndHashCode();
        }

        private BaseUriWithWildcard(string protocol, int defaultPort, string binding, int segmentCount, string path, string sampleBinding)
        {
            string[] strArray = SplitBinding(binding);
            if (strArray.Length != segmentCount)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(System.ServiceModel.SR.GetString("Hosting_MisformattedBinding", new object[] { binding, protocol, sampleBinding })));
            }
            int index = segmentCount - 1;
            string host = this.ParseHostAndHostNameComparisonMode(strArray[index]);
            int result = -1;
            if (--index >= 0)
            {
                string str2 = strArray[index].Trim();
                if (!string.IsNullOrEmpty(str2) && !int.TryParse(str2, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(System.ServiceModel.SR.GetString("Hosting_MisformattedPort", new object[] { protocol, binding, str2 })));
                }
                if (result == defaultPort)
                {
                    result = -1;
                }
            }
            try
            {
                this.baseAddress = new UriBuilder(protocol, host, result, path).Uri;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceError)
                {
                    System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(System.ServiceModel.SR.GetString("Hosting_MisformattedBindingData", new object[] { binding, protocol })));
            }
            this.SetComparisonAddressAndHashCode();
        }

        internal static BaseUriWithWildcard CreateHostedPipeUri(string binding, string path)
        {
            return new BaseUriWithWildcard(Uri.UriSchemeNetPipe, -1, binding, 1, path, "*");
        }

        internal static BaseUriWithWildcard CreateHostedUri(string protocol, string binding, string path)
        {
            if (binding == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (path == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("path");
            }
            if (protocol.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseUriWithWildcard(Uri.UriSchemeHttp, 80, binding, 3, path, ":80:");
            }
            if (protocol.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseUriWithWildcard(Uri.UriSchemeHttps, 0x1bb, binding, 3, path, ":443:");
            }
            if (protocol.Equals(Uri.UriSchemeNetTcp, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseUriWithWildcard(Uri.UriSchemeNetTcp, 0x328, binding, 2, path, "808:*");
            }
            if (protocol.Equals(Uri.UriSchemeNetPipe, StringComparison.OrdinalIgnoreCase))
            {
                return CreateHostedPipeUri(binding, path);
            }
            if (protocol.Equals(MsmqUri.NetMsmqAddressTranslator.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseUriWithWildcard(MsmqUri.NetMsmqAddressTranslator.Scheme, -1, binding, 1, path, "*");
            }
            if (!protocol.Equals(MsmqUri.FormatNameAddressTranslator.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(System.ServiceModel.SR.GetString("Hosting_NotSupportedProtocol", new object[] { binding })));
            }
            return new BaseUriWithWildcard(MsmqUri.FormatNameAddressTranslator.Scheme, -1, binding, 1, path, "*");
        }

        public override bool Equals(object o)
        {
            BaseUriWithWildcard wildcard = o as BaseUriWithWildcard;
            if (((wildcard == null) || (wildcard.hashCode != this.hashCode)) || ((wildcard.hostNameComparisonMode != this.hostNameComparisonMode) || (wildcard.comparand.Port != this.comparand.Port)))
            {
                return false;
            }
            if (!object.ReferenceEquals(wildcard.comparand.Scheme, this.comparand.Scheme))
            {
                return false;
            }
            return this.comparand.Address.Equals(wildcard.comparand.Address);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        internal bool IsBaseOf(Uri fullAddress)
        {
            if (this.baseAddress.Scheme != fullAddress.Scheme)
            {
                return false;
            }
            if (this.baseAddress.Port != fullAddress.Port)
            {
                return false;
            }
            if ((this.HostNameComparisonMode == System.ServiceModel.HostNameComparisonMode.Exact) && (string.Compare(this.baseAddress.Host, fullAddress.Host, StringComparison.OrdinalIgnoreCase) != 0))
            {
                return false;
            }
            string components = this.baseAddress.GetComponents(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.Unescaped);
            string strA = fullAddress.GetComponents(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.Unescaped);
            if (components.Length > strA.Length)
            {
                return false;
            }
            if (((components.Length < strA.Length) && (components[components.Length - 1] != '/')) && (strA[components.Length] != '/'))
            {
                return false;
            }
            return (string.Compare(strA, 0, components, 0, components.Length, StringComparison.OrdinalIgnoreCase) == 0);
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            UriSchemeKeyedCollection.ValidateBaseAddress(this.baseAddress, "context");
            if (!HostNameComparisonModeHelper.IsDefined(this.HostNameComparisonMode))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("context", System.ServiceModel.SR.GetString("Hosting_BaseUriDeserializedNotValid"));
            }
            this.SetComparisonAddressAndHashCode();
        }

        private string ParseHostAndHostNameComparisonMode(string host)
        {
            if (string.IsNullOrEmpty(host) || host.Equals("*"))
            {
                this.hostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.WeakWildcard;
                host = DnsCache.MachineName;
                return host;
            }
            if (host.Equals("+"))
            {
                this.hostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                host = DnsCache.MachineName;
                return host;
            }
            this.hostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.Exact;
            return host;
        }

        private void SetComparisonAddressAndHashCode()
        {
            if (this.HostNameComparisonMode == System.ServiceModel.HostNameComparisonMode.Exact)
            {
                this.comparand.Address = this.baseAddress.ToString();
            }
            else
            {
                this.comparand.Address = this.baseAddress.GetComponents(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.UriEscaped);
            }
            this.comparand.Port = this.baseAddress.Port;
            this.comparand.Scheme = this.baseAddress.Scheme;
            if ((this.comparand.Port == -1) && (this.comparand.Scheme == Uri.UriSchemeNetTcp))
            {
                this.comparand.Port = 0x328;
            }
            int hashCode = this.comparand.Address.GetHashCode();
            this.hashCode = (hashCode ^ this.comparand.Port) ^ this.HostNameComparisonMode;
        }

        private static string[] SplitBinding(string binding)
        {
            bool flag = false;
            string[] strArray = null;
            List<int> list = null;
            for (int i = 0; i < binding.Length; i++)
            {
                if (flag && (binding[i] == ']'))
                {
                    flag = false;
                }
                else if (binding[i] == '[')
                {
                    flag = true;
                }
                else if (!flag && (binding[i] == ':'))
                {
                    if (list == null)
                    {
                        list = new List<int>();
                    }
                    list.Add(i);
                }
            }
            if (list == null)
            {
                return new string[] { binding };
            }
            strArray = new string[list.Count + 1];
            int startIndex = 0;
            for (int j = 0; j < strArray.Length; j++)
            {
                if (j < list.Count)
                {
                    int num4 = list[j];
                    strArray[j] = binding.Substring(startIndex, num4 - startIndex);
                    startIndex = num4 + 1;
                }
                else if (startIndex < binding.Length)
                {
                    strArray[j] = binding.Substring(startIndex, binding.Length - startIndex);
                }
                else
                {
                    strArray[j] = string.Empty;
                }
            }
            return strArray;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { this.HostNameComparisonMode, this.BaseAddress });
        }

        internal Uri BaseAddress
        {
            get
            {
                return this.baseAddress;
            }
        }

        internal System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Comparand
        {
            public string Address;
            public int Port;
            public string Scheme;
        }
    }
}

