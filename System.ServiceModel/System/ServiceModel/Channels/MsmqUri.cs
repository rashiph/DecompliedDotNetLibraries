namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.ServiceModel;
    using System.Text;

    internal static class MsmqUri
    {
        private static IAddressTranslator activeDirectoryAddressTranslator;
        private static IAddressTranslator deadLetterQueueAddressTranslator;
        private static IAddressTranslator formatnameAddressTranslator;
        private static IAddressTranslator netMsmqAddressTranslator;
        private static IAddressTranslator srmpAddressTranslator;
        private static IAddressTranslator srmpsAddressTranslator;

        private static void AppendQueueName(StringBuilder builder, string relativePath, string slash)
        {
            if (relativePath.StartsWith("/private$", StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqWrongPrivateQueueSyntax")));
            }
            if (relativePath.StartsWith("/private", StringComparison.OrdinalIgnoreCase))
            {
                if ("/private".Length == relativePath.Length)
                {
                    builder.Append("private$");
                    builder.Append(slash);
                    relativePath = "/";
                }
                else if ('/' == relativePath["/private".Length])
                {
                    builder.Append("private$");
                    builder.Append(slash);
                    relativePath = relativePath.Substring("/private".Length);
                }
            }
            builder.Append(relativePath.Substring(1));
        }

        public static string UriToFormatNameByScheme(Uri uri)
        {
            if (uri.Scheme == NetMsmqAddressTranslator.Scheme)
            {
                return NetMsmqAddressTranslator.UriToFormatName(uri);
            }
            if (uri.Scheme != FormatNameAddressTranslator.Scheme)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("uri");
            }
            return FormatNameAddressTranslator.UriToFormatName(uri);
        }

        public static IAddressTranslator ActiveDirectoryAddressTranslator
        {
            get
            {
                if (activeDirectoryAddressTranslator == null)
                {
                    activeDirectoryAddressTranslator = new ActiveDirectory();
                }
                return activeDirectoryAddressTranslator;
            }
        }

        public static IAddressTranslator DeadLetterQueueAddressTranslator
        {
            get
            {
                if (deadLetterQueueAddressTranslator == null)
                {
                    deadLetterQueueAddressTranslator = new Dlq();
                }
                return deadLetterQueueAddressTranslator;
            }
        }

        public static IAddressTranslator FormatNameAddressTranslator
        {
            get
            {
                if (formatnameAddressTranslator == null)
                {
                    formatnameAddressTranslator = new FormatName();
                }
                return formatnameAddressTranslator;
            }
        }

        public static IAddressTranslator NetMsmqAddressTranslator
        {
            get
            {
                if (netMsmqAddressTranslator == null)
                {
                    netMsmqAddressTranslator = new NetMsmq();
                }
                return netMsmqAddressTranslator;
            }
        }

        public static IAddressTranslator SrmpAddressTranslator
        {
            get
            {
                if (srmpAddressTranslator == null)
                {
                    srmpAddressTranslator = new Srmp();
                }
                return srmpAddressTranslator;
            }
        }

        public static IAddressTranslator SrmpsAddressTranslator
        {
            get
            {
                if (srmpsAddressTranslator == null)
                {
                    srmpsAddressTranslator = new SrmpSecure();
                }
                return srmpsAddressTranslator;
            }
        }

        private class ActiveDirectory : MsmqUri.PathName
        {
            public override string UriToFormatName(Uri uri)
            {
                return MsmqFormatName.FromQueuePath(base.UriToFormatName(uri));
            }
        }

        private class Dlq : MsmqUri.PathName
        {
            protected override Uri PostVerify(Uri uri)
            {
                if (string.Compare(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return uri;
                }
                try
                {
                    if (string.Compare(DnsCache.MachineName, DnsCache.Resolve(uri.Host).HostName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return new UriBuilder(base.Scheme, "localhost", -1, uri.PathAndQuery).Uri;
                    }
                }
                catch (EndpointNotFoundException exception)
                {
                    MsmqDiagnostics.ExpectedException(exception);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqDLQNotLocal"), "uri"));
            }
        }

        private class FormatName : MsmqUri.IAddressTranslator
        {
            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string str;
                if (isPrivate)
                {
                    str = @"PRIVATE$\" + name;
                }
                else
                {
                    str = name;
                }
                str = "DIRECT=OS:" + host + @"\" + str;
                return new Uri(this.Scheme + ":" + str);
            }

            public string UriToFormatName(Uri uri)
            {
                if (null == uri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                }
                if (uri.Scheme != this.Scheme)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidScheme"), "uri"));
                }
                return Uri.UnescapeDataString(uri.AbsoluteUri.Substring(this.Scheme.Length + 1));
            }

            public string Scheme
            {
                get
                {
                    return "msmq.formatname";
                }
            }
        }

        internal interface IAddressTranslator
        {
            Uri CreateUri(string host, string name, bool isPrivate);
            string UriToFormatName(Uri uri);

            string Scheme { get; }
        }

        private class NetMsmq : MsmqUri.IAddressTranslator
        {
            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string pathValue = "/" + name;
                if (isPrivate)
                {
                    pathValue = "/private" + pathValue;
                }
                return new UriBuilder(this.Scheme, host, -1, pathValue).Uri;
            }

            public string UriToFormatName(Uri uri)
            {
                if (null == uri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                }
                if (uri.Scheme != this.Scheme)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidScheme"), "uri"));
                }
                if (string.IsNullOrEmpty(uri.Host))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("MsmqWrongUri"));
                }
                if (-1 != uri.Port)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("MsmqUnexpectedPort"));
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("DIRECT=");
                if (string.Compare(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    builder.Append("OS:.");
                }
                else
                {
                    IPAddress address = null;
                    if (IPAddress.TryParse(uri.Host, out address))
                    {
                        builder.Append("TCP:");
                    }
                    else
                    {
                        builder.Append("OS:");
                    }
                    builder.Append(uri.Host);
                }
                builder.Append(@"\");
                MsmqUri.AppendQueueName(builder, Uri.UnescapeDataString(uri.PathAndQuery), @"\");
                return builder.ToString();
            }

            public string Scheme
            {
                get
                {
                    return "net.msmq";
                }
            }
        }

        private class PathName : MsmqUri.IAddressTranslator
        {
            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string pathValue = "/" + name;
                if (isPrivate)
                {
                    pathValue = "/private" + pathValue;
                }
                return new UriBuilder(this.Scheme, host, -1, pathValue).Uri;
            }

            protected virtual Uri PostVerify(Uri uri)
            {
                return uri;
            }

            public virtual string UriToFormatName(Uri uri)
            {
                if (null == uri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                }
                if (uri.Scheme != this.Scheme)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidScheme"), "uri"));
                }
                if (string.IsNullOrEmpty(uri.Host))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("MsmqWrongUri"));
                }
                if (-1 != uri.Port)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("MsmqUnexpectedPort"));
                }
                uri = this.PostVerify(uri);
                StringBuilder builder = new StringBuilder();
                if (string.Compare(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    builder.Append(".");
                }
                else
                {
                    builder.Append(uri.Host);
                }
                builder.Append(@"\");
                MsmqUri.AppendQueueName(builder, Uri.UnescapeDataString(uri.PathAndQuery), @"\");
                return builder.ToString();
            }

            public string Scheme
            {
                get
                {
                    return "net.msmq";
                }
            }
        }

        private class Srmp : MsmqUri.SrmpBase
        {
            protected override string DirectScheme
            {
                get
                {
                    return "http://";
                }
            }
        }

        private abstract class SrmpBase : MsmqUri.IAddressTranslator
        {
            private const string msmqPart = "/msmq/";

            protected SrmpBase()
            {
            }

            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string pathValue = "/" + name;
                if (isPrivate)
                {
                    pathValue = "/private" + pathValue;
                }
                return new UriBuilder(this.Scheme, host, -1, pathValue).Uri;
            }

            public string UriToFormatName(Uri uri)
            {
                if (null == uri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                }
                if (uri.Scheme != this.Scheme)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MsmqInvalidScheme"), "uri"));
                }
                if (string.IsNullOrEmpty(uri.Host))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("MsmqWrongUri"));
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("DIRECT=");
                builder.Append(this.DirectScheme);
                builder.Append(uri.Host);
                if (-1 != uri.Port)
                {
                    builder.Append(":");
                    builder.Append(uri.Port.ToString(CultureInfo.InvariantCulture));
                }
                string relativePath = Uri.UnescapeDataString(uri.PathAndQuery);
                builder.Append("/msmq/");
                MsmqUri.AppendQueueName(builder, relativePath, "/");
                return builder.ToString();
            }

            protected abstract string DirectScheme { get; }

            public string Scheme
            {
                get
                {
                    return "net.msmq";
                }
            }
        }

        private class SrmpSecure : MsmqUri.SrmpBase
        {
            protected override string DirectScheme
            {
                get
                {
                    return "https://";
                }
            }
        }
    }
}

