namespace System.Net
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;

    internal static class HttpDigest
    {
        private static int AcceptorLengthOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "cbAcceptorLength"));
        private static int AcceptorOffsetOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "dwAcceptorOffset"));
        private static int AcceptorTypeOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "dwAcceptorAddrType"));
        private static int ApplicationDataLengthOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "cbApplicationDataLength"));
        private static int ApplicationDataOffsetOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "dwApplicationDataOffset"));
        internal const string DA_algorithm = "algorithm";
        internal const string DA_channelbinding = "channel-binding";
        internal const string DA_charset = "charset";
        internal const string DA_cipher = "cipher";
        internal const string DA_cnonce = "cnonce";
        internal const string DA_domain = "domain";
        internal const string DA_hasheddirs = "hashed-dirs";
        internal const string DA_nc = "nc";
        internal const string DA_nonce = "nonce";
        internal const string DA_opaque = "opaque";
        internal const string DA_qop = "qop";
        internal const string DA_realm = "realm";
        internal const string DA_response = "response";
        internal const string DA_servicename = "service-name";
        internal const string DA_stale = "stale";
        internal const string DA_uri = "uri";
        internal const string DA_username = "username";
        internal const string HashedDirs = "service-name,channel-binding";
        private static int InitiatorLengthOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "cbInitiatorLength"));
        private static int InitiatorOffsetOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "dwInitiatorOffset"));
        private static int InitiatorTypeOffset = ((int) Marshal.OffsetOf(typeof(SecChannelBindings), "dwInitiatorAddrType"));
        private static int MinimumFormattedBindingLength = (5 * SizeOfInt);
        private static readonly RNGCryptoServiceProvider RandomGenerator = new RNGCryptoServiceProvider();
        private static int SizeOfInt = Marshal.SizeOf(typeof(int));
        internal const string SupportedQuality = "auth";
        private static bool suppressExtendedProtection;
        private const string suppressExtendedProtectionKey = @"System\CurrentControlSet\Control\Lsa";
        private const string suppressExtendedProtectionKeyPath = @"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa";
        private const string suppressExtendedProtectionValueName = "SuppressExtendedProtection";
        internal const string Upgraded = "+Upgraded+";
        internal const string UpgradedV1 = "+Upgraded+v1";
        internal const string ValidSeparator = ", \"'\t\r\n";
        internal const string ZeroChannelBindingHash = "00000000000000000000000000000000";

        static HttpDigest()
        {
            ReadSuppressExtendedProtectionRegistryValue();
        }

        internal static Authorization Authenticate(HttpDigestChallenge digestChallenge, NetworkCredential NC, string spn, ChannelBinding binding)
        {
            string userName = NC.InternalGetUserName();
            if (ValidationHelper.IsBlankString(userName))
            {
                return null;
            }
            string password = NC.InternalGetPassword();
            bool flag = IsUpgraded(digestChallenge.Nonce, binding);
            if (flag)
            {
                digestChallenge.ServiceName = spn;
                digestChallenge.ChannelBinding = hashChannelBinding(binding, digestChallenge.MD5provider);
            }
            if (digestChallenge.QopPresent)
            {
                if ((digestChallenge.ClientNonce == null) || digestChallenge.Stale)
                {
                    if (flag)
                    {
                        digestChallenge.ClientNonce = createUpgradedNonce(digestChallenge);
                    }
                    else
                    {
                        digestChallenge.ClientNonce = createNonce(0x20);
                    }
                    digestChallenge.NonceCount = 1;
                }
                else
                {
                    digestChallenge.NonceCount++;
                }
            }
            StringBuilder builder = new StringBuilder();
            Charset charset = DetectCharset(userName);
            if (!digestChallenge.UTF8Charset && (charset == Charset.UTF8))
            {
                return null;
            }
            Charset charset2 = DetectCharset(password);
            if (!digestChallenge.UTF8Charset && (charset2 == Charset.UTF8))
            {
                return null;
            }
            if (digestChallenge.UTF8Charset)
            {
                builder.Append(pair("charset", "utf-8", false));
                builder.Append(",");
                if (charset == Charset.UTF8)
                {
                    userName = CharsetEncode(userName, Charset.UTF8);
                    builder.Append(pair("username", userName, true));
                    builder.Append(",");
                }
                else
                {
                    builder.Append(pair("username", CharsetEncode(userName, Charset.UTF8), true));
                    builder.Append(",");
                    userName = CharsetEncode(userName, charset);
                }
            }
            else
            {
                userName = CharsetEncode(userName, charset);
                builder.Append(pair("username", userName, true));
                builder.Append(",");
            }
            password = CharsetEncode(password, charset2);
            builder.Append(pair("realm", digestChallenge.Realm, true));
            builder.Append(",");
            builder.Append(pair("nonce", digestChallenge.Nonce, true));
            builder.Append(",");
            builder.Append(pair("uri", digestChallenge.Uri, true));
            if (digestChallenge.QopPresent)
            {
                if (digestChallenge.Algorithm != null)
                {
                    builder.Append(",");
                    builder.Append(pair("algorithm", digestChallenge.Algorithm, true));
                }
                builder.Append(",");
                builder.Append(pair("cnonce", digestChallenge.ClientNonce, true));
                builder.Append(",");
                builder.Append(pair("nc", digestChallenge.NonceCount.ToString("x8", NumberFormatInfo.InvariantInfo), false));
                builder.Append(",");
                builder.Append(pair("qop", "auth", true));
                if (flag)
                {
                    builder.Append(",");
                    builder.Append(pair("hashed-dirs", "service-name,channel-binding", true));
                    builder.Append(",");
                    builder.Append(pair("service-name", digestChallenge.ServiceName, true));
                    builder.Append(",");
                    builder.Append(pair("channel-binding", digestChallenge.ChannelBinding, true));
                }
            }
            string str3 = responseValue(digestChallenge, userName, password);
            if (str3 == null)
            {
                return null;
            }
            builder.Append(",");
            builder.Append(pair("response", str3, true));
            if (digestChallenge.Opaque != null)
            {
                builder.Append(",");
                builder.Append(pair("opaque", digestChallenge.Opaque, true));
            }
            return new Authorization("Digest " + builder.ToString(), false);
        }

        private static string CharsetEncode(string rawString, Charset charset)
        {
            if ((charset == Charset.UTF8) || (charset == Charset.ANSI))
            {
                byte[] buffer = (charset == Charset.UTF8) ? Encoding.UTF8.GetBytes(rawString) : Encoding.Default.GetBytes(rawString);
                char[] array = new char[buffer.Length];
                buffer.CopyTo(array, 0);
                rawString = new string(array);
            }
            return rawString;
        }

        private static string computeSecret(HttpDigestChallenge challenge, string username, string password)
        {
            if ((challenge.Algorithm == null) || (string.Compare(challenge.Algorithm, "md5", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return (username + ":" + challenge.Realm + ":" + password);
            }
            if (string.Compare(challenge.Algorithm, "md5-sess", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return (hashString(username + ":" + challenge.Realm + ":" + password, challenge.MD5provider) + ":" + challenge.Nonce + ":" + challenge.ClientNonce);
            }
            if (Logging.On)
            {
                Logging.PrintError(Logging.Web, SR.GetString("net_log_digest_hash_algorithm_not_supported", new object[] { challenge.Algorithm }));
            }
            return null;
        }

        private static string createNonce(int length)
        {
            int num = length;
            byte[] data = new byte[num];
            char[] chArray = new char[length];
            RandomGenerator.GetBytes(data);
            for (int i = 0; i < length; i++)
            {
                chArray[i] = Uri.HexLowerChars[data[i] & 15];
            }
            return new string(chArray);
        }

        private static string createUpgradedNonce(HttpDigestChallenge digestChallenge)
        {
            string s = digestChallenge.ServiceName + ":" + digestChallenge.ChannelBinding;
            byte[] rawbytes = digestChallenge.MD5provider.ComputeHash(Encoding.ASCII.GetBytes(s));
            return ("+Upgraded+v1" + hexEncode(rawbytes) + createNonce(0x20));
        }

        private static Charset DetectCharset(string rawString)
        {
            Charset aSCII = Charset.ASCII;
            for (int i = 0; i < rawString.Length; i++)
            {
                if (rawString[i] > '\x007f')
                {
                    byte[] bytes = Encoding.Default.GetBytes(rawString);
                    string strB = Encoding.Default.GetString(bytes);
                    return ((string.Compare(rawString, strB, StringComparison.Ordinal) == 0) ? Charset.ANSI : Charset.UTF8);
                }
            }
            return aSCII;
        }

        private static byte[] formatChannelBindingForHash(ChannelBinding binding)
        {
            int num = Marshal.ReadInt32(binding.DangerousGetHandle(), InitiatorTypeOffset);
            int num2 = Marshal.ReadInt32(binding.DangerousGetHandle(), InitiatorLengthOffset);
            int num3 = Marshal.ReadInt32(binding.DangerousGetHandle(), AcceptorTypeOffset);
            int num4 = Marshal.ReadInt32(binding.DangerousGetHandle(), AcceptorLengthOffset);
            int num5 = Marshal.ReadInt32(binding.DangerousGetHandle(), ApplicationDataLengthOffset);
            byte[] array = new byte[((MinimumFormattedBindingLength + num2) + num4) + num5];
            BitConverter.GetBytes(num).CopyTo(array, 0);
            BitConverter.GetBytes(num2).CopyTo(array, SizeOfInt);
            int startIndex = 2 * SizeOfInt;
            if (num2 > 0)
            {
                int b = Marshal.ReadInt32(binding.DangerousGetHandle(), InitiatorOffsetOffset);
                Marshal.Copy(IntPtrHelper.Add(binding.DangerousGetHandle(), b), array, startIndex, num2);
                startIndex += num2;
            }
            BitConverter.GetBytes(num3).CopyTo(array, startIndex);
            BitConverter.GetBytes(num4).CopyTo(array, (int) (startIndex + SizeOfInt));
            startIndex += 2 * SizeOfInt;
            if (num4 > 0)
            {
                int num8 = Marshal.ReadInt32(binding.DangerousGetHandle(), AcceptorOffsetOffset);
                Marshal.Copy(IntPtrHelper.Add(binding.DangerousGetHandle(), num8), array, startIndex, num4);
                startIndex += num4;
            }
            BitConverter.GetBytes(num5).CopyTo(array, startIndex);
            startIndex += SizeOfInt;
            if (num5 > 0)
            {
                int num9 = Marshal.ReadInt32(binding.DangerousGetHandle(), ApplicationDataOffsetOffset);
                Marshal.Copy(IntPtrHelper.Add(binding.DangerousGetHandle(), num9), array, startIndex, num5);
            }
            return array;
        }

        private static string hashChannelBinding(ChannelBinding binding, MD5CryptoServiceProvider MD5provider)
        {
            if (binding == null)
            {
                return "00000000000000000000000000000000";
            }
            byte[] buffer = formatChannelBindingForHash(binding);
            return hexEncode(MD5provider.ComputeHash(buffer));
        }

        private static string hashString(string myString, MD5CryptoServiceProvider MD5provider)
        {
            byte[] buffer = new byte[myString.Length];
            for (int i = 0; i < myString.Length; i++)
            {
                buffer[i] = (byte) myString[i];
            }
            return hexEncode(MD5provider.ComputeHash(buffer));
        }

        private static string hexEncode(byte[] rawbytes)
        {
            int length = rawbytes.Length;
            char[] chArray = new char[2 * length];
            int index = 0;
            int num3 = 0;
            while (index < length)
            {
                chArray[num3++] = Uri.HexLowerChars[rawbytes[index] >> 4];
                chArray[num3++] = Uri.HexLowerChars[rawbytes[index] & 15];
                index++;
            }
            return new string(chArray);
        }

        internal static HttpDigestChallenge Interpret(string challenge, int startingPoint, HttpWebRequest httpWebRequest)
        {
            int num2;
            string str2;
            HttpDigestChallenge challenge2 = new HttpDigestChallenge();
            challenge2.SetFromRequest(httpWebRequest);
            startingPoint = (startingPoint == -1) ? 0 : (startingPoint + DigestClient.SignatureSize);
            int startIndex = startingPoint;
        Label_001F:
            num2 = startIndex;
            int num3 = AuthenticationManager.SplitNoQuotes(challenge, ref num2);
            if (num2 >= 0)
            {
                if (string.Compare(challenge.Substring(startIndex, num2 - startIndex), "charset", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (num3 < 0)
                    {
                        str2 = unquote(challenge.Substring(num2 + 1));
                    }
                    else
                    {
                        str2 = unquote(challenge.Substring(num2 + 1, (num3 - num2) - 1));
                    }
                    if (string.Compare(str2, "utf-8", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        challenge2.UTF8Charset = true;
                        goto Label_009E;
                    }
                }
                if (num3 >= 0)
                {
                    startIndex = ++num3;
                    goto Label_001F;
                }
            }
        Label_009E:
            startIndex = startingPoint;
        Label_00A0:
            num2 = startIndex;
            num3 = AuthenticationManager.SplitNoQuotes(challenge, ref num2);
            if (num2 >= 0)
            {
                string name = challenge.Substring(startIndex, num2 - startIndex);
                if (num3 < 0)
                {
                    str2 = unquote(challenge.Substring(num2 + 1));
                }
                else
                {
                    str2 = unquote(challenge.Substring(num2 + 1, (num3 - num2) - 1));
                }
                if (challenge2.UTF8Charset)
                {
                    bool flag2 = true;
                    for (int i = 0; i < str2.Length; i++)
                    {
                        if (str2[i] > '\x007f')
                        {
                            flag2 = false;
                            break;
                        }
                    }
                    if (!flag2)
                    {
                        byte[] bytes = new byte[str2.Length];
                        for (int j = 0; j < str2.Length; j++)
                        {
                            bytes[j] = (byte) str2[j];
                        }
                        str2 = Encoding.UTF8.GetString(bytes);
                    }
                }
                bool flag = challenge2.defineAttribute(name, str2);
                if ((num3 >= 0) && flag)
                {
                    startIndex = ++num3;
                    goto Label_00A0;
                }
            }
            if (challenge2.Nonce != null)
            {
                return challenge2;
            }
            if (Logging.On)
            {
                Logging.PrintError(Logging.Web, SR.GetString("net_log_digest_requires_nonce"));
            }
            return null;
        }

        private static bool IsUpgraded(string nonce, ChannelBinding binding)
        {
            if ((binding == null) && suppressExtendedProtection)
            {
                return false;
            }
            return (AuthenticationManager.SspSupportsExtendedProtection && nonce.StartsWith("+Upgraded+", StringComparison.Ordinal));
        }

        internal static string pair(string name, string value, bool quote)
        {
            if (quote)
            {
                return (name + "=\"" + value + "\"");
            }
            return (name + "=" + value);
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa")]
        private static void ReadSuppressExtendedProtectionRegistryValue()
        {
            suppressExtendedProtection = !ComNetOS.IsWin7;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Lsa"))
                {
                    try
                    {
                        if (key.GetValueKind("SuppressExtendedProtection") == RegistryValueKind.DWord)
                        {
                            suppressExtendedProtection = ((int) key.GetValue("SuppressExtendedProtection")) == 1;
                        }
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", exception.Message);
                        }
                    }
                    catch (IOException exception2)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", exception2.Message);
                        }
                    }
                }
            }
            catch (SecurityException exception3)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", exception3.Message);
                }
            }
            catch (ObjectDisposedException exception4)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", exception4.Message);
                }
            }
        }

        private static string responseValue(HttpDigestChallenge challenge, string username, string password)
        {
            string myString = computeSecret(challenge, username, password);
            if (myString == null)
            {
                return null;
            }
            string str2 = challenge.Method + ":" + challenge.Uri;
            if (str2 == null)
            {
                return null;
            }
            string str3 = hashString(myString, challenge.MD5provider);
            string str4 = hashString(str2, challenge.MD5provider);
            string str5 = challenge.Nonce + ":" + (challenge.QopPresent ? (challenge.NonceCount.ToString("x8", NumberFormatInfo.InvariantInfo) + ":" + challenge.ClientNonce + ":auth:" + str4) : str4);
            return hashString(str3 + ":" + str5, challenge.MD5provider);
        }

        internal static string unquote(string quotedString)
        {
            return quotedString.Trim().Trim("\"".ToCharArray());
        }

        private enum Charset
        {
            ASCII,
            ANSI,
            UTF8
        }
    }
}

