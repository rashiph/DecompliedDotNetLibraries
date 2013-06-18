namespace System.ServiceModel.Channels
{
    using System;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;

    internal static class PipeUri
    {
        public static string BuildSharedMemoryName(string hostName, string path, bool global)
        {
            byte[] buffer2;
            string str2;
            StringBuilder builder = new StringBuilder();
            builder.Append(Uri.UriSchemeNetPipe);
            builder.Append("://");
            builder.Append(hostName.ToUpperInvariant());
            builder.Append(path);
            string s = builder.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            if (bytes.Length >= 0x80)
            {
                using (HashAlgorithm algorithm = GetHashAlgorithm())
                {
                    buffer2 = algorithm.ComputeHash(bytes);
                }
                str2 = ":H";
            }
            else
            {
                buffer2 = bytes;
                str2 = ":E";
            }
            builder = new StringBuilder();
            if (global)
            {
                builder.Append(@"Global\");
            }
            else
            {
                builder.Append(@"Local\");
            }
            builder.Append(Uri.UriSchemeNetPipe);
            builder.Append(str2);
            builder.Append(Convert.ToBase64String(buffer2));
            return builder.ToString();
        }

        public static string BuildSharedMemoryName(Uri uri, HostNameComparisonMode hostNameComparisonMode, bool global)
        {
            string path = GetPath(uri);
            string hostName = null;
            switch (hostNameComparisonMode)
            {
                case HostNameComparisonMode.StrongWildcard:
                    hostName = "+";
                    break;

                case HostNameComparisonMode.Exact:
                    hostName = uri.Host;
                    break;

                case HostNameComparisonMode.WeakWildcard:
                    hostName = "*";
                    break;
            }
            return BuildSharedMemoryName(hostName, path, global);
        }

        private static HashAlgorithm GetHashAlgorithm()
        {
            if (System.ServiceModel.Security.SecurityUtils.RequiresFipsCompliance)
            {
                return new SHA1CryptoServiceProvider();
            }
            return new SHA1Managed();
        }

        public static string GetParentPath(string path)
        {
            if (path.EndsWith("/", StringComparison.Ordinal))
            {
                path = path.Substring(0, path.Length - 1);
            }
            if (path.Length == 0)
            {
                return path;
            }
            return path.Substring(0, path.LastIndexOf('/') + 1);
        }

        public static string GetPath(Uri uri)
        {
            string str = uri.LocalPath.ToUpperInvariant();
            if (!str.EndsWith("/", StringComparison.Ordinal))
            {
                str = str + "/";
            }
            return str;
        }

        public static void Validate(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeNetPipe)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("uri", System.ServiceModel.SR.GetString("PipeUriSchemeWrong"));
            }
        }
    }
}

