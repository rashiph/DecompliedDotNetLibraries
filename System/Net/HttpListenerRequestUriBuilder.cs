namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal sealed class HttpListenerRequestUriBuilder
    {
        private static readonly Encoding ansiEncoding = Encoding.GetEncoding(0, new EncoderExceptionFallback(), new DecoderExceptionFallback());
        private readonly string cookedUriHost;
        private readonly string cookedUriPath;
        private readonly string cookedUriQuery;
        private readonly string cookedUriScheme;
        private List<byte> rawOctets;
        private string rawPath;
        private readonly string rawUri;
        private Uri requestUri;
        private StringBuilder requestUriString;
        private static readonly bool useCookedRequestUrl = SettingsSectionInternal.Section.HttpListenerUnescapeRequestUrl;
        private static readonly Encoding utf8Encoding = new UTF8Encoding(false, true);

        private HttpListenerRequestUriBuilder(string rawUri, string cookedUriScheme, string cookedUriHost, string cookedUriPath, string cookedUriQuery)
        {
            this.rawUri = rawUri;
            this.cookedUriScheme = cookedUriScheme;
            this.cookedUriHost = cookedUriHost;
            this.cookedUriPath = AddSlashToAsteriskOnlyPath(cookedUriPath);
            if (cookedUriQuery == null)
            {
                this.cookedUriQuery = string.Empty;
            }
            else
            {
                this.cookedUriQuery = cookedUriQuery;
            }
        }

        private bool AddPercentEncodedOctetToRawOctetsList(Encoding encoding, string escapedCharacter)
        {
            byte num;
            if (!byte.TryParse(escapedCharacter, NumberStyles.HexNumber, (IFormatProvider) null, out num))
            {
                this.LogWarning("AddPercentEncodedOctetToRawOctetsList", "net_log_listener_cant_convert_percent_value", new object[] { escapedCharacter });
                return false;
            }
            this.rawOctets.Add(num);
            return true;
        }

        private static string AddSlashToAsteriskOnlyPath(string path)
        {
            if ((path.Length == 1) && (path[0] == '*'))
            {
                return "/*";
            }
            return path;
        }

        private static void AppendOctetsPercentEncoded(StringBuilder target, IEnumerable<byte> octets)
        {
            foreach (byte num in octets)
            {
                target.Append('%');
                target.Append(num.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        private bool AppendUnicodeCodePointValuePercentEncoded(string codePoint)
        {
            int num;
            if (!int.TryParse(codePoint, NumberStyles.HexNumber, null, out num))
            {
                this.LogWarning("AppendUnicodeCodePointValuePercentEncoded", "net_log_listener_cant_convert_percent_value", new object[] { codePoint });
                return false;
            }
            string s = null;
            try
            {
                s = char.ConvertFromUtf32(num);
                AppendOctetsPercentEncoded(this.requestUriString, utf8Encoding.GetBytes(s));
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                this.LogWarning("AppendUnicodeCodePointValuePercentEncoded", "net_log_listener_cant_convert_percent_value", new object[] { codePoint });
            }
            catch (EncoderFallbackException exception)
            {
                this.LogWarning("AppendUnicodeCodePointValuePercentEncoded", "net_log_listener_cant_convert_to_utf8", new object[] { s, exception.Message });
            }
            return false;
        }

        private Uri Build()
        {
            if (useCookedRequestUrl)
            {
                this.BuildRequestUriUsingCookedPath();
                if (this.requestUri == null)
                {
                    this.BuildRequestUriUsingRawPath();
                }
            }
            else
            {
                this.BuildRequestUriUsingRawPath();
                if (this.requestUri == null)
                {
                    this.BuildRequestUriUsingCookedPath();
                }
            }
            return this.requestUri;
        }

        private void BuildRequestUriUsingCookedPath()
        {
            if (!Uri.TryCreate(this.cookedUriScheme + Uri.SchemeDelimiter + this.cookedUriHost + this.cookedUriPath + this.cookedUriQuery, UriKind.Absolute, out this.requestUri))
            {
                this.LogWarning("BuildRequestUriUsingCookedPath", "net_log_listener_cant_create_uri", new object[] { this.cookedUriScheme, this.cookedUriHost, this.cookedUriPath, this.cookedUriQuery });
            }
        }

        private void BuildRequestUriUsingRawPath()
        {
            bool flag = false;
            this.rawPath = GetPath(this.rawUri);
            if (!HttpSysSettings.EnableNonUtf8 || (this.rawPath == string.Empty))
            {
                string rawPath = this.rawPath;
                if (rawPath == string.Empty)
                {
                    rawPath = "/";
                }
                flag = Uri.TryCreate(this.cookedUriScheme + Uri.SchemeDelimiter + this.cookedUriHost + rawPath + this.cookedUriQuery, UriKind.Absolute, out this.requestUri);
            }
            else
            {
                ParsingResult result = this.BuildRequestUriUsingRawPath(GetEncoding(EncodingType.Primary));
                if (result == ParsingResult.EncodingError)
                {
                    Encoding encoding = GetEncoding(EncodingType.Secondary);
                    result = this.BuildRequestUriUsingRawPath(encoding);
                }
                flag = result == ParsingResult.Success;
            }
            if (!flag)
            {
                this.LogWarning("BuildRequestUriUsingRawPath", "net_log_listener_cant_create_uri", new object[] { this.cookedUriScheme, this.cookedUriHost, this.rawPath, this.cookedUriQuery });
            }
        }

        private ParsingResult BuildRequestUriUsingRawPath(Encoding encoding)
        {
            this.rawOctets = new List<byte>();
            this.requestUriString = new StringBuilder();
            this.requestUriString.Append(this.cookedUriScheme);
            this.requestUriString.Append(Uri.SchemeDelimiter);
            this.requestUriString.Append(this.cookedUriHost);
            ParsingResult invalidString = this.ParseRawPath(encoding);
            if (invalidString == ParsingResult.Success)
            {
                this.requestUriString.Append(this.cookedUriQuery);
                if (!Uri.TryCreate(this.requestUriString.ToString(), UriKind.Absolute, out this.requestUri))
                {
                    invalidString = ParsingResult.InvalidString;
                }
            }
            if (invalidString != ParsingResult.Success)
            {
                this.LogWarning("BuildRequestUriUsingRawPath", "net_log_listener_cant_convert_raw_path", new object[] { this.rawPath, encoding.EncodingName });
            }
            return invalidString;
        }

        private bool EmptyDecodeAndAppendRawOctetsList(Encoding encoding)
        {
            if (this.rawOctets.Count == 0)
            {
                return true;
            }
            string s = null;
            try
            {
                s = encoding.GetString(this.rawOctets.ToArray());
                if (encoding == utf8Encoding)
                {
                    AppendOctetsPercentEncoded(this.requestUriString, this.rawOctets.ToArray());
                }
                else
                {
                    AppendOctetsPercentEncoded(this.requestUriString, utf8Encoding.GetBytes(s));
                }
                this.rawOctets.Clear();
                return true;
            }
            catch (DecoderFallbackException exception)
            {
                this.LogWarning("EmptyDecodeAndAppendRawOctetsList", "net_log_listener_cant_convert_bytes", new object[] { GetOctetsAsString(this.rawOctets), exception.Message });
            }
            catch (EncoderFallbackException exception2)
            {
                this.LogWarning("EmptyDecodeAndAppendRawOctetsList", "net_log_listener_cant_convert_to_utf8", new object[] { s, exception2.Message });
            }
            return false;
        }

        private static Encoding GetEncoding(EncodingType type)
        {
            if (((type != EncodingType.Primary) || HttpSysSettings.FavorUtf8) && ((type != EncodingType.Secondary) || !HttpSysSettings.FavorUtf8))
            {
                return utf8Encoding;
            }
            return ansiEncoding;
        }

        private static string GetOctetsAsString(IEnumerable<byte> octets)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (byte num in octets)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append(" ");
                }
                builder.Append(num.ToString("X2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private static string GetPath(string uriString)
        {
            int startIndex = 0;
            if (uriString[0] != '/')
            {
                int num2 = 0;
                if (uriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    num2 = 7;
                }
                else if (uriString.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    num2 = 8;
                }
                if (num2 > 0)
                {
                    startIndex = uriString.IndexOf('/', num2);
                    if (startIndex == -1)
                    {
                        startIndex = uriString.Length;
                    }
                }
                else
                {
                    uriString = "/" + uriString;
                }
            }
            int index = uriString.IndexOf('?');
            if (index == -1)
            {
                index = uriString.Length;
            }
            return AddSlashToAsteriskOnlyPath(uriString.Substring(startIndex, index - startIndex));
        }

        public static Uri GetRequestUri(string rawUri, string cookedUriScheme, string cookedUriHost, string cookedUriPath, string cookedUriQuery)
        {
            HttpListenerRequestUriBuilder builder = new HttpListenerRequestUriBuilder(rawUri, cookedUriScheme, cookedUriHost, cookedUriPath, cookedUriQuery);
            return builder.Build();
        }

        private void LogWarning(string methodName, string message, params object[] args)
        {
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.HttpListener, this, methodName, SR.GetString(message, args));
            }
        }

        private ParsingResult ParseRawPath(Encoding encoding)
        {
            int startIndex = 0;
            char ch = '\0';
            while (startIndex < this.rawPath.Length)
            {
                ch = this.rawPath[startIndex];
                if (ch == '%')
                {
                    startIndex++;
                    ch = this.rawPath[startIndex];
                    switch (ch)
                    {
                        case 'u':
                        case 'U':
                        {
                            if (!this.EmptyDecodeAndAppendRawOctetsList(encoding))
                            {
                                return ParsingResult.EncodingError;
                            }
                            if (!this.AppendUnicodeCodePointValuePercentEncoded(this.rawPath.Substring(startIndex + 1, 4)))
                            {
                                return ParsingResult.InvalidString;
                            }
                            startIndex += 5;
                            continue;
                        }
                    }
                    if (!this.AddPercentEncodedOctetToRawOctetsList(encoding, this.rawPath.Substring(startIndex, 2)))
                    {
                        return ParsingResult.InvalidString;
                    }
                    startIndex += 2;
                }
                else
                {
                    if (!this.EmptyDecodeAndAppendRawOctetsList(encoding))
                    {
                        return ParsingResult.EncodingError;
                    }
                    this.requestUriString.Append(ch);
                    startIndex++;
                }
            }
            if (!this.EmptyDecodeAndAppendRawOctetsList(encoding))
            {
                return ParsingResult.EncodingError;
            }
            return ParsingResult.Success;
        }

        private enum EncodingType
        {
            Primary,
            Secondary
        }

        private enum ParsingResult
        {
            Success,
            InvalidString,
            EncodingError
        }
    }
}

