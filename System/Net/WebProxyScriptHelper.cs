namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class WebProxyScriptHelper : IReflect
    {
        private static int MAX_IPADDRESS_LIST_LENGTH = 0x400;

        private static DayOfWeek dayOfWeek(string weekDay)
        {
            if ((weekDay != null) && (weekDay.Length == 3))
            {
                if ((weekDay[0] == 'T') || (weekDay[0] == 't'))
                {
                    if (((weekDay[1] == 'U') || (weekDay[1] == 'u')) && ((weekDay[2] == 'E') || (weekDay[2] == 'e')))
                    {
                        return DayOfWeek.Tuesday;
                    }
                    if (((weekDay[1] == 'H') || (weekDay[1] == 'h')) && ((weekDay[2] == 'U') || (weekDay[2] == 'u')))
                    {
                        return DayOfWeek.Thursday;
                    }
                }
                if ((weekDay[0] == 'S') || (weekDay[0] == 's'))
                {
                    if (((weekDay[1] == 'U') || (weekDay[1] == 'u')) && ((weekDay[2] == 'N') || (weekDay[2] == 'n')))
                    {
                        return DayOfWeek.Sunday;
                    }
                    if (((weekDay[1] == 'A') || (weekDay[1] == 'a')) && ((weekDay[2] == 'T') || (weekDay[2] == 't')))
                    {
                        return DayOfWeek.Saturday;
                    }
                }
                if ((((weekDay[0] == 'M') || (weekDay[0] == 'm')) && ((weekDay[1] == 'O') || (weekDay[1] == 'o'))) && ((weekDay[2] == 'N') || (weekDay[2] == 'n')))
                {
                    return DayOfWeek.Monday;
                }
                if ((((weekDay[0] == 'W') || (weekDay[0] == 'w')) && ((weekDay[1] == 'E') || (weekDay[1] == 'e'))) && ((weekDay[2] == 'D') || (weekDay[2] == 'd')))
                {
                    return DayOfWeek.Wednesday;
                }
                if ((((weekDay[0] == 'F') || (weekDay[0] == 'f')) && ((weekDay[1] == 'R') || (weekDay[1] == 'r'))) && ((weekDay[2] == 'I') || (weekDay[2] == 'i')))
                {
                    return DayOfWeek.Friday;
                }
            }
            return ~DayOfWeek.Sunday;
        }

        public bool dnsDomainIs(string host, string domain)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.dnsDomainIs()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            if (domain == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.dnsDomainIs()", "domain" }));
                }
                throw new ArgumentNullException("domain");
            }
            int num = host.LastIndexOf(domain);
            return ((num != -1) && ((num + domain.Length) == host.Length));
        }

        public int dnsDomainLevels(string host)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.dnsDomainLevels()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            int startIndex = 0;
            int num2 = 0;
            while ((startIndex = host.IndexOf('.', startIndex)) != -1)
            {
                num2++;
                startIndex++;
            }
            return num2;
        }

        public string dnsResolve(string host)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.dnsResolve()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            IPHostEntry hostByName = null;
            try
            {
                hostByName = Dns.InternalGetHostByName(host);
            }
            catch
            {
            }
            if (hostByName != null)
            {
                for (int i = 0; i < hostByName.AddressList.Length; i++)
                {
                    if (hostByName.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return hostByName.AddressList[i].ToString();
                    }
                }
            }
            return string.Empty;
        }

        public string dnsResolveEx(string host)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.dnsResolve()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            IPHostEntry hostByName = null;
            try
            {
                hostByName = Dns.InternalGetHostByName(host);
            }
            catch
            {
            }
            if (hostByName == null)
            {
                return string.Empty;
            }
            IPAddress[] addressList = hostByName.AddressList;
            if (addressList.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < addressList.Length; i++)
            {
                builder.Append(addressList[i].ToString());
                if (i != (addressList.Length - 1))
                {
                    builder.Append(";");
                }
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            return builder.ToString();
        }

        public string getClientVersion()
        {
            return "1.0";
        }

        private static bool isGMT(string gmt)
        {
            return (string.Compare(gmt, "GMT", StringComparison.OrdinalIgnoreCase) == 0);
        }

        public bool isInNet(string host, string pattern, string mask)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isInNet()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            if (pattern == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isInNet()", "pattern" }));
                }
                throw new ArgumentNullException("pattern");
            }
            if (mask == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isInNet()", "mask" }));
                }
                throw new ArgumentNullException("mask");
            }
            try
            {
                IPAddress address = IPAddress.Parse(host);
                IPAddress address2 = IPAddress.Parse(pattern);
                byte[] addressBytes = IPAddress.Parse(mask).GetAddressBytes();
                byte[] buffer2 = address.GetAddressBytes();
                byte[] buffer3 = address2.GetAddressBytes();
                if ((addressBytes.Length != buffer2.Length) || (addressBytes.Length != buffer3.Length))
                {
                    return false;
                }
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    if ((buffer3[i] & addressBytes[i]) != (buffer2[i] & addressBytes[i]))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool isInNetEx(string ipAddress, string ipPrefix)
        {
            IPAddress address;
            IPAddress address2;
            if (ipAddress == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isResolvable()", "ipAddress" }));
                }
                throw new ArgumentNullException("ipAddress");
            }
            if (ipPrefix == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isResolvable()", "ipPrefix" }));
                }
                throw new ArgumentNullException("ipPrefix");
            }
            if (!IPAddress.TryParse(ipAddress, out address))
            {
                throw new FormatException(SR.GetString("dns_bad_ip_address"));
            }
            if (ipPrefix.IndexOf("/") < 0)
            {
                throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
            }
            string[] strArray = ipPrefix.Split(new char[] { '/' });
            if ((((strArray.Length != 2) || (strArray[0] == null)) || ((strArray[0].Length == 0) || (strArray[1] == null))) || ((strArray[1].Length == 0) || (strArray[1].Length > 2)))
            {
                throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
            }
            if (!IPAddress.TryParse(strArray[0], out address2))
            {
                throw new FormatException(SR.GetString("dns_bad_ip_address"));
            }
            int result = 0;
            if (!int.TryParse(strArray[1], out result))
            {
                throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
            }
            if (address.AddressFamily != address2.AddressFamily)
            {
                throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
            }
            if (((address.AddressFamily == AddressFamily.InterNetworkV6) && ((result < 1) || (result > 0x40))) || ((address.AddressFamily == AddressFamily.InterNetwork) && ((result < 1) || (result > 0x20))))
            {
                throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
            }
            byte[] addressBytes = address2.GetAddressBytes();
            byte index = (byte) (result / 8);
            byte num4 = (byte) (result % 8);
            byte num5 = index;
            if (num4 != 0)
            {
                if ((0xff & (addressBytes[index] << num4)) != 0)
                {
                    throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
                }
                num5 = (byte) (num5 + 1);
            }
            int num6 = (address2.AddressFamily == AddressFamily.InterNetworkV6) ? 0x10 : 4;
            while (num5 < num6)
            {
                num5 = (byte) (num5 + 1);
                if (addressBytes[num5] != 0)
                {
                    throw new FormatException(SR.GetString("net_bad_ip_address_prefix"));
                }
            }
            byte[] buffer2 = address.GetAddressBytes();
            for (num5 = 0; num5 < index; num5 = (byte) (num5 + 1))
            {
                if (buffer2[num5] != addressBytes[num5])
                {
                    return false;
                }
            }
            if (num4 > 0)
            {
                byte num7 = buffer2[index];
                byte num8 = addressBytes[index];
                num7 = (byte) (num7 >> (8 - num4));
                num7 = (byte) (num7 << (8 - num4));
                if (num7 != num8)
                {
                    return false;
                }
            }
            return true;
        }

        public bool isPlainHostName(string hostName)
        {
            if (hostName != null)
            {
                return (hostName.IndexOf('.') == -1);
            }
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isPlainHostName()", "hostName" }));
            }
            throw new ArgumentNullException("hostName");
        }

        public bool isResolvable(string host)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.isResolvable()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            IPHostEntry hostByName = null;
            try
            {
                hostByName = Dns.InternalGetHostByName(host);
            }
            catch
            {
            }
            if (hostByName != null)
            {
                for (int i = 0; i < hostByName.AddressList.Length; i++)
                {
                    if (hostByName.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool isResolvableEx(string host)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.dnsResolve()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            IPHostEntry hostByName = null;
            try
            {
                hostByName = Dns.InternalGetHostByName(host);
            }
            catch
            {
            }
            if (hostByName == null)
            {
                return false;
            }
            if (hostByName.AddressList.Length == 0)
            {
                return false;
            }
            return true;
        }

        public bool localHostOrDomainIs(string host, string hostDom)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.localHostOrDomainIs()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            if (hostDom == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.localHostOrDomainIs()", "hostDom" }));
                }
                throw new ArgumentNullException("hostDom");
            }
            if (this.isPlainHostName(host))
            {
                int index = hostDom.IndexOf('.');
                if (index > 0)
                {
                    hostDom = hostDom.Substring(0, index);
                }
            }
            return (string.Compare(host, hostDom, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public string myIpAddress()
        {
            IPAddress[] localAddresses = NclUtilities.LocalAddresses;
            for (int i = 0; i < localAddresses.Length; i++)
            {
                if (!IPAddress.IsLoopback(localAddresses[i]) && (localAddresses[i].AddressFamily == AddressFamily.InterNetwork))
                {
                    return localAddresses[i].ToString();
                }
            }
            return string.Empty;
        }

        public string myIpAddressEx()
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                IPAddress[] localAddresses = NclUtilities.LocalAddresses;
                for (int i = 0; i < localAddresses.Length; i++)
                {
                    if (!IPAddress.IsLoopback(localAddresses[i]))
                    {
                        builder.Append(localAddresses[i].ToString());
                        if (i != (localAddresses.Length - 1))
                        {
                            builder.Append(";");
                        }
                    }
                }
            }
            catch
            {
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            return builder.ToString();
        }

        public bool shExpMatch(string host, string pattern)
        {
            if (host == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.shExpMatch()", "host" }));
                }
                throw new ArgumentNullException("host");
            }
            if (pattern == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.shExpMatch()", "pattern" }));
                }
                throw new ArgumentNullException("pattern");
            }
            try
            {
                ShellExpression expression = new ShellExpression(pattern);
                return expression.IsMatch(host);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public unsafe string sortIpAddressList(string IPAddressList)
        {
            string str;
            if ((IPAddressList == null) || (IPAddressList.Length == 0))
            {
                return string.Empty;
            }
            string[] strArray = IPAddressList.Split(new char[] { ';' });
            if (strArray.Length > MAX_IPADDRESS_LIST_LENGTH)
            {
                throw new ArgumentException(string.Format(SR.GetString("net_max_ip_address_list_length_exceeded"), MAX_IPADDRESS_LIST_LENGTH), "IPAddressList");
            }
            if (strArray.Length == 1)
            {
                return IPAddressList;
            }
            SocketAddress[] addressArray = new SocketAddress[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = strArray[i].Trim();
                if (strArray[i].Length == 0)
                {
                    throw new ArgumentException(SR.GetString("dns_bad_ip_address"), "IPAddressList");
                }
                SocketAddress address = new SocketAddress(AddressFamily.InterNetworkV6, 0x1c);
                if (UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(strArray[i], AddressFamily.InterNetworkV6, IntPtr.Zero, address.m_Buffer, ref address.m_Size) != SocketError.Success)
                {
                    SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork, 0x10);
                    if (UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(strArray[i], AddressFamily.InterNetwork, IntPtr.Zero, socketAddress.m_Buffer, ref socketAddress.m_Size) != SocketError.Success)
                    {
                        throw new ArgumentException(SR.GetString("dns_bad_ip_address"), "IPAddressList");
                    }
                    IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
                    IPEndPoint point2 = (IPEndPoint) point.Create(socketAddress);
                    byte[] addressBytes = point2.Address.GetAddressBytes();
                    byte[] buffer2 = new byte[0x10];
                    for (int k = 0; k < 10; k++)
                    {
                        buffer2[k] = 0;
                    }
                    buffer2[10] = 0xff;
                    buffer2[11] = 0xff;
                    buffer2[12] = addressBytes[0];
                    buffer2[13] = addressBytes[1];
                    buffer2[14] = addressBytes[2];
                    buffer2[15] = addressBytes[3];
                    IPAddress address3 = new IPAddress(buffer2);
                    address = new IPEndPoint(address3, point2.Port).Serialize();
                }
                addressArray[i] = address;
            }
            int cb = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS_LIST)) + ((addressArray.Length - 1) * Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS)));
            Dictionary<IntPtr, KeyValuePair<SocketAddress, string>> dictionary = new Dictionary<IntPtr, KeyValuePair<SocketAddress, string>>();
            GCHandle[] handleArray = new GCHandle[addressArray.Length];
            for (int j = 0; j < addressArray.Length; j++)
            {
                handleArray[j] = GCHandle.Alloc(addressArray[j].m_Buffer, GCHandleType.Pinned);
            }
            IntPtr optionInValue = Marshal.AllocHGlobal(cb);
            try
            {
                UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS_LIST* socket_address_listPtr = (UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS_LIST*) optionInValue;
                socket_address_listPtr->iAddressCount = addressArray.Length;
                UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS* socket_addressPtr = &socket_address_listPtr->Addresses;
                for (int m = 0; m < socket_address_listPtr->iAddressCount; m++)
                {
                    socket_addressPtr[m].iSockaddrLength = 0x1c;
                    socket_addressPtr[m].lpSockAddr = handleArray[m].AddrOfPinnedObject();
                    dictionary[socket_addressPtr[m].lpSockAddr] = new KeyValuePair<SocketAddress, string>(addressArray[m], strArray[m]);
                }
                new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp).IOControl(IOControlCode.AddressListSort, optionInValue, cb, optionInValue, cb);
                StringBuilder builder = new StringBuilder();
                for (int n = 0; n < socket_address_listPtr->iAddressCount; n++)
                {
                    IntPtr lpSockAddr = socket_addressPtr[n].lpSockAddr;
                    KeyValuePair<SocketAddress, string> pair = dictionary[lpSockAddr];
                    builder.Append(pair.Value);
                    if (n != (socket_address_listPtr->iAddressCount - 1))
                    {
                        builder.Append(";");
                    }
                }
                str = builder.ToString();
            }
            finally
            {
                if (optionInValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(optionInValue);
                }
                for (int num7 = 0; num7 < handleArray.Length; num7++)
                {
                    if (handleArray[num7].IsAllocated)
                    {
                        handleArray[num7].Free();
                    }
                }
            }
            return str;
        }

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return new FieldInfo[0];
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return new MemberInfo[] { new MyMethodInfo(name) };
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return new MemberInfo[0];
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return new MethodInfo[0];
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return new PropertyInfo[0];
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        object IReflect.InvokeMember(string name, BindingFlags bindingAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return null;
        }

        public bool weekdayRange(string wd1, [Optional] object wd2, [Optional] object gmt)
        {
            if (wd1 == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.weekdayRange()", "wd1" }));
                }
                throw new ArgumentNullException("wd1");
            }
            string str = null;
            string str2 = null;
            if (((gmt != null) && (gmt != DBNull.Value)) && (gmt != Missing.Value))
            {
                str = gmt as string;
                if (str == null)
                {
                    throw new ArgumentException(SR.GetString("net_param_not_string", new object[] { gmt.GetType().FullName }), "gmt");
                }
            }
            if (((wd2 != null) && (wd2 != DBNull.Value)) && (gmt != Missing.Value))
            {
                str2 = wd2 as string;
                if (str2 == null)
                {
                    throw new ArgumentException(SR.GetString("net_param_not_string", new object[] { wd2.GetType().FullName }), "wd2");
                }
            }
            if (str == null)
            {
                if (str2 == null)
                {
                    return weekdayRangeInternal(DateTime.Now, dayOfWeek(wd1), dayOfWeek(wd1));
                }
                if (isGMT(str2))
                {
                    return weekdayRangeInternal(DateTime.UtcNow, dayOfWeek(wd1), dayOfWeek(wd1));
                }
                return weekdayRangeInternal(DateTime.Now, dayOfWeek(wd1), dayOfWeek(str2));
            }
            if (isGMT(str))
            {
                return weekdayRangeInternal(DateTime.UtcNow, dayOfWeek(wd1), dayOfWeek(str2));
            }
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_null_parameter", new object[] { "WebProxyScriptHelper.weekdayRange()", "gmt" }));
            }
            throw new ArgumentException(SR.GetString("net_proxy_not_gmt"), "gmt");
        }

        private static bool weekdayRangeInternal(DateTime now, DayOfWeek wd1, DayOfWeek wd2)
        {
            if ((wd1 < DayOfWeek.Sunday) || (wd2 < DayOfWeek.Sunday))
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Web, SR.GetString("net_log_proxy_called_with_invalid_parameter", new object[] { "WebProxyScriptHelper.weekdayRange()" }));
                }
                throw new ArgumentException(SR.GetString("net_proxy_invalid_dayofweek"), (wd1 < DayOfWeek.Sunday) ? "wd1" : "wd2");
            }
            if (wd1 <= wd2)
            {
                return ((wd1 <= now.DayOfWeek) && (now.DayOfWeek <= wd2));
            }
            if (wd2 < now.DayOfWeek)
            {
                return (now.DayOfWeek >= wd1);
            }
            return true;
        }

        Type IReflect.UnderlyingSystemType
        {
            get
            {
                return null;
            }
        }

        private class MyMethodInfo : MethodInfo
        {
            private string name;

            public MyMethodInfo(string name)
            {
                this.name = name;
            }

            public override MethodInfo GetBaseDefinition()
            {
                return null;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return null;
            }

            public override object[] GetCustomAttributes(Type type, bool inherit)
            {
                return null;
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return MethodImplAttributes.IL;
            }

            public override ParameterInfo[] GetParameters()
            {
                return typeof(WebProxyScriptHelper).GetMethod(this.name, ~BindingFlags.Default).GetParameters();
            }

            public override object Invoke(object target, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture)
            {
                return typeof(WebProxyScriptHelper).GetMethod(this.name, ~BindingFlags.Default).Invoke(target, ~BindingFlags.Default, binder, args, culture);
            }

            public override bool IsDefined(Type type, bool inherit)
            {
                return type.Equals(typeof(WebProxyScriptHelper));
            }

            public override MethodAttributes Attributes
            {
                get
                {
                    return MethodAttributes.Public;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return typeof(WebProxyScriptHelper.MyMethodInfo);
                }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get
                {
                    return new RuntimeMethodHandle();
                }
            }

            public override System.Reflection.Module Module
            {
                get
                {
                    return base.GetType().Module;
                }
            }

            public override string Name
            {
                get
                {
                    return this.name;
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return null;
                }
            }

            public override Type ReturnType
            {
                get
                {
                    Type type = null;
                    if (string.Compare(this.name, "isPlainHostName", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (string.Compare(this.name, "dnsDomainIs", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (string.Compare(this.name, "localHostOrDomainIs", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (string.Compare(this.name, "isResolvable", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (string.Compare(this.name, "dnsResolve", StringComparison.Ordinal) == 0)
                    {
                        return typeof(string);
                    }
                    if (string.Compare(this.name, "myIpAddress", StringComparison.Ordinal) == 0)
                    {
                        return typeof(string);
                    }
                    if (string.Compare(this.name, "dnsDomainLevels", StringComparison.Ordinal) == 0)
                    {
                        return typeof(int);
                    }
                    if (string.Compare(this.name, "isInNet", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (string.Compare(this.name, "shExpMatch", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (string.Compare(this.name, "weekdayRange", StringComparison.Ordinal) == 0)
                    {
                        return typeof(bool);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        if (string.Compare(this.name, "dnsResolveEx", StringComparison.Ordinal) == 0)
                        {
                            return typeof(string);
                        }
                        if (string.Compare(this.name, "isResolvableEx", StringComparison.Ordinal) == 0)
                        {
                            return typeof(bool);
                        }
                        if (string.Compare(this.name, "myIpAddressEx", StringComparison.Ordinal) == 0)
                        {
                            return typeof(string);
                        }
                        if (string.Compare(this.name, "isInNetEx", StringComparison.Ordinal) == 0)
                        {
                            return typeof(bool);
                        }
                        if (string.Compare(this.name, "sortIpAddressList", StringComparison.Ordinal) == 0)
                        {
                            return typeof(string);
                        }
                        if (string.Compare(this.name, "getClientVersion", StringComparison.Ordinal) == 0)
                        {
                            type = typeof(string);
                        }
                    }
                    return type;
                }
            }

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get
                {
                    return null;
                }
            }
        }
    }
}

