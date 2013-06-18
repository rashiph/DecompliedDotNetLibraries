namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Security.Principal;
    using System.Text;
    using System.Web;

    internal static class CoreChannel
    {
        private static IByteBufferPool _bufferPool = new ByteBufferPool(10, 0x1000);
        private static System.Runtime.Remoting.Channels.RequestQueue _requestQueue = new System.Runtime.Remoting.Channels.RequestQueue(8, 4, 250);
        internal const string BinaryMimeType = "application/octet-stream";
        internal const int CLIENT_END_CALL = 0x13;
        internal const int CLIENT_MSG_GEN = 1;
        internal const int CLIENT_MSG_SEND = 4;
        internal const int CLIENT_MSG_SER = 3;
        internal const int CLIENT_MSG_SINK_CHAIN = 2;
        internal const int CLIENT_RET_DESER = 0x10;
        internal const int CLIENT_RET_PROPAGATION = 0x12;
        internal const int CLIENT_RET_RECEIVE = 15;
        internal const int CLIENT_RET_SINK_CHAIN = 0x11;
        internal const int MaxStringLen = 0x200;
        private static string s_hostName = null;
        private static bool s_isClientSKUInstallation = false;
        private static bool s_isClientSKUInstallationInitialized = false;
        private static string s_MachineIp = null;
        private static IPAddress s_MachineIpAddress = null;
        private static string s_MachineName = null;
        internal const int SERVER_DISPATCH = 9;
        internal const int SERVER_MSG_DESER = 6;
        internal const int SERVER_MSG_RECEIVE = 5;
        internal const int SERVER_MSG_SINK_CHAIN = 7;
        internal const int SERVER_MSG_STACK_BUILD = 8;
        internal const int SERVER_RET_END = 14;
        internal const int SERVER_RET_SEND = 13;
        internal const int SERVER_RET_SER = 12;
        internal const int SERVER_RET_SINK_CHAIN = 11;
        internal const int SERVER_RET_STACK_BUILD = 10;
        internal const string SOAPContentType = "text/xml; charset=\"utf-8\"";
        internal const string SOAPMimeType = "text/xml";
        internal static ResourceManager SystemResMgr;
        internal const int TIMING_DATA_EOF = 0x63;

        internal static void AppendProviderToClientProviderChain(IClientChannelSinkProvider providerChain, IClientChannelSinkProvider provider)
        {
            if (providerChain == null)
            {
                throw new ArgumentNullException("providerChain");
            }
            while (providerChain.Next != null)
            {
                providerChain = providerChain.Next;
            }
            providerChain.Next = provider;
        }

        internal static void CleanupUrlBashingForIisSslIfNecessary(bool bBashedUrl)
        {
            if (bBashedUrl)
            {
                CallContext.FreeNamedDataSlot("__bashChannelUrl");
            }
        }

        internal static void CollectChannelDataFromServerSinkProviders(ChannelDataStore channelData, IServerChannelSinkProvider provider)
        {
            while (provider != null)
            {
                provider.GetChannelData(channelData);
                provider = provider.Next;
            }
        }

        internal static BinaryFormatter CreateBinaryFormatter(bool serialize, bool includeVersionsOrStrictBinding)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (serialize)
            {
                RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
                formatter.SurrogateSelector = selector;
            }
            else
            {
                formatter.SurrogateSelector = null;
            }
            formatter.Context = new StreamingContext(StreamingContextStates.Other);
            formatter.AssemblyFormat = includeVersionsOrStrictBinding ? FormatterAssemblyStyle.Full : FormatterAssemblyStyle.Simple;
            return formatter;
        }

        internal static SoapFormatter CreateSoapFormatter(bool serialize, bool includeVersions)
        {
            SoapFormatter formatter = new SoapFormatter();
            if (serialize)
            {
                RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
                formatter.SurrogateSelector = selector;
                selector.UseSoapFormat();
            }
            else
            {
                formatter.SurrogateSelector = null;
            }
            formatter.Context = new StreamingContext(StreamingContextStates.Other);
            formatter.AssemblyFormat = includeVersions ? FormatterAssemblyStyle.Full : FormatterAssemblyStyle.Simple;
            return formatter;
        }

        [Conditional("_DEBUG")]
        internal static void DebugException(string name, Exception e)
        {
        }

        [Conditional("_DEBUG")]
        internal static void DebugMessage(IMessage msg)
        {
        }

        [Conditional("_DEBUG")]
        internal static void DebugOut(string s)
        {
        }

        [Conditional("_DEBUG")]
        internal static void DebugOutXMLStream(Stream stm, string tag)
        {
        }

        [Conditional("_DEBUG")]
        internal static void DebugStream(Stream stm)
        {
        }

        internal static string DecodeMachineName(string machineName)
        {
            if (machineName.Equals("$hostName"))
            {
                return GetHostName();
            }
            return machineName;
        }

        internal static IMessage DeserializeBinaryRequestMessage(string objectUri, Stream inputStream, bool bStrictBinding, TypeFilterLevel securityLevel)
        {
            BinaryFormatter formatter = CreateBinaryFormatter(false, bStrictBinding);
            formatter.FilterLevel = securityLevel;
            UriHeaderHandler handler = new UriHeaderHandler(objectUri);
            return (IMessage) formatter.UnsafeDeserialize(inputStream, new HeaderHandler(handler.HeaderHandler));
        }

        internal static IMessage DeserializeBinaryResponseMessage(Stream inputStream, IMethodCallMessage reqMsg, bool bStrictBinding)
        {
            return (IMessage) CreateBinaryFormatter(false, bStrictBinding).UnsafeDeserializeMethodResponse(inputStream, null, reqMsg);
        }

        internal static IMessage DeserializeMessage(string mimeType, Stream xstm, bool methodRequest, IMessage msg)
        {
            return DeserializeMessage(mimeType, xstm, methodRequest, msg, null);
        }

        internal static IMessage DeserializeMessage(string mimeType, Stream xstm, bool methodRequest, IMessage msg, Header[] h)
        {
            Stream serializationStream = null;
            object obj2;
            bool flag = false;
            bool flag2 = true;
            if (string.Compare(mimeType, "application/octet-stream", StringComparison.Ordinal) == 0)
            {
                flag2 = true;
            }
            if (string.Compare(mimeType, "text/xml", StringComparison.Ordinal) == 0)
            {
                flag2 = false;
            }
            if (!flag)
            {
                serializationStream = xstm;
            }
            else
            {
                long position = xstm.Position;
                byte[] bytes = ((MemoryStream) xstm).ToArray();
                xstm.Position = position;
                MemoryStream stream3 = new MemoryStream(Convert.FromBase64String(Encoding.ASCII.GetString(bytes, 0, bytes.Length)));
                serializationStream = stream3;
            }
            IRemotingFormatter formatter = MimeTypeToFormatter(mimeType, false);
            if (flag2)
            {
                obj2 = ((BinaryFormatter) formatter).UnsafeDeserializeMethodResponse(serializationStream, null, (IMethodCallMessage) msg);
            }
            else if (methodRequest)
            {
                MethodCall call = new MethodCall(h);
                formatter.Deserialize(serializationStream, new HeaderHandler(call.HeaderHandler));
                obj2 = call;
            }
            else
            {
                IMethodCallMessage mcm = (IMethodCallMessage) msg;
                MethodResponse response = new MethodResponse(h, mcm);
                formatter.Deserialize(serializationStream, new HeaderHandler(response.HeaderHandler));
                obj2 = response;
            }
            return (IMessage) obj2;
        }

        internal static IMessage DeserializeSoapRequestMessage(Stream inputStream, Header[] h, bool bStrictBinding, TypeFilterLevel securityLevel)
        {
            SoapFormatter formatter = CreateSoapFormatter(false, bStrictBinding);
            formatter.FilterLevel = securityLevel;
            MethodCall call = new MethodCall(h);
            formatter.Deserialize(inputStream, new HeaderHandler(call.HeaderHandler));
            return call;
        }

        internal static IMessage DeserializeSoapResponseMessage(Stream inputStream, IMessage requestMsg, Header[] h, bool bStrictBinding)
        {
            SoapFormatter formatter = CreateSoapFormatter(false, bStrictBinding);
            IMethodCallMessage mcm = (IMethodCallMessage) requestMsg;
            MethodResponse response = new MethodResponse(h, mcm);
            formatter.Deserialize(inputStream, new HeaderHandler(response.HeaderHandler));
            return response;
        }

        internal static SinkChannelProtocol DetermineChannelProtocol(IChannel channel)
        {
            string str;
            if (channel.Parse("http://foo.com/foo", out str) != null)
            {
                return SinkChannelProtocol.Http;
            }
            return SinkChannelProtocol.Other;
        }

        internal static string GetCurrentSidString()
        {
            return WindowsIdentity.GetCurrent().User.ToString();
        }

        internal static string GetHostName()
        {
            if (s_hostName == null)
            {
                s_hostName = Dns.GetHostName();
                if (s_hostName == null)
                {
                    throw new ArgumentNullException("hostName");
                }
            }
            return s_hostName;
        }

        internal static IPAddress GetMachineAddress(IPHostEntry host, AddressFamily addressFamily)
        {
            if (host != null)
            {
                IPAddress[] addressList = host.AddressList;
                for (int i = 0; i < addressList.Length; i++)
                {
                    if (addressList[i].AddressFamily == addressFamily)
                    {
                        return addressList[i];
                    }
                }
            }
            return null;
        }

        internal static string GetMachineIp()
        {
            if (s_MachineIp == null)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(GetMachineName());
                AddressFamily addressFamily = Socket.OSSupportsIPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
                IPAddress machineAddress = GetMachineAddress(hostEntry, addressFamily);
                if (machineAddress != null)
                {
                    s_MachineIp = machineAddress.ToString();
                }
                if (s_MachineIp == null)
                {
                    throw new ArgumentNullException("ip");
                }
            }
            return s_MachineIp;
        }

        internal static string GetMachineName()
        {
            if (s_MachineName == null)
            {
                string hostName = GetHostName();
                if (hostName != null)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                    if (hostEntry != null)
                    {
                        s_MachineName = hostEntry.HostName;
                    }
                }
                if (s_MachineName == null)
                {
                    throw new ArgumentNullException("machine");
                }
            }
            return s_MachineName;
        }

        internal static Header[] GetMessagePropertiesAsSoapHeader(IMessage reqMsg)
        {
            IDictionary properties = reqMsg.Properties;
            if (properties == null)
            {
                return null;
            }
            int count = properties.Count;
            if (count == 0)
            {
                return null;
            }
            IDictionaryEnumerator enumerator = properties.GetEnumerator();
            bool[] flagArray = new bool[count];
            int num2 = 0;
            int index = 0;
            IMethodMessage msg = (IMethodMessage) reqMsg;
            while (enumerator.MoveNext())
            {
                string key = (string) enumerator.Key;
                if (((key.Length >= 2) && (string.CompareOrdinal(key, 0, "__", 0, 2) == 0)) && (((((key.Equals("__Args") || key.Equals("__OutArgs")) || (key.Equals("__Return") || key.Equals("__Uri"))) || (key.Equals("__MethodName") || ((key.Equals("__MethodSignature") && !RemotingServices.IsMethodOverloaded(msg)) && !msg.HasVarArgs))) || (key.Equals("__TypeName") || key.Equals("__Fault"))) || (key.Equals("__CallContext") && ((enumerator.Value != null) ? !((LogicalCallContext) enumerator.Value).HasInfo : true))))
                {
                    index++;
                }
                else
                {
                    flagArray[index] = true;
                    index++;
                    num2++;
                }
            }
            if (num2 == 0)
            {
                return null;
            }
            Header[] sourceArray = new Header[num2];
            enumerator.Reset();
            int num4 = 0;
            index = 0;
            while (enumerator.MoveNext())
            {
                object obj2 = enumerator.Key;
                if (!flagArray[num4])
                {
                    num4++;
                }
                else
                {
                    Header header = enumerator.Value as Header;
                    if (header == null)
                    {
                        header = new Header((string) obj2, enumerator.Value, false, "http://schemas.microsoft.com/clr/soap/messageProperties");
                    }
                    if (index == sourceArray.Length)
                    {
                        Header[] destinationArray = new Header[index + 1];
                        Array.Copy(sourceArray, destinationArray, index);
                        sourceArray = destinationArray;
                    }
                    sourceArray[index] = header;
                    index++;
                    num4++;
                }
            }
            return sourceArray;
        }

        internal static string GetResourceString(string key)
        {
            if (SystemResMgr == null)
            {
                InitResourceManager();
            }
            return SystemResMgr.GetString(key, null);
        }

        internal static Header[] GetSoapHeaders(IMessage reqMsg)
        {
            return GetMessagePropertiesAsSoapHeader(reqMsg);
        }

        private static ResourceManager InitResourceManager()
        {
            if (SystemResMgr == null)
            {
                SystemResMgr = new ResourceManager("System.Runtime.Remoting", typeof(CoreChannel).Module.Assembly);
            }
            return SystemResMgr;
        }

        internal static bool IsLocalIpAddress(IPAddress remoteAddress)
        {
            if (s_MachineIpAddress == null)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(GetMachineName());
                if ((hostEntry == null) || (hostEntry.AddressList.Length != 1))
                {
                    return IsLocalIpAddress(hostEntry, remoteAddress.AddressFamily, remoteAddress);
                }
                if (Socket.OSSupportsIPv4)
                {
                    s_MachineIpAddress = GetMachineAddress(hostEntry, AddressFamily.InterNetwork);
                }
                else
                {
                    s_MachineIpAddress = GetMachineAddress(hostEntry, AddressFamily.InterNetworkV6);
                }
            }
            return s_MachineIpAddress.Equals(remoteAddress);
        }

        internal static bool IsLocalIpAddress(IPHostEntry host, AddressFamily addressFamily, IPAddress remoteAddress)
        {
            if (host != null)
            {
                IPAddress[] addressList = host.AddressList;
                for (int i = 0; i < addressList.Length; i++)
                {
                    if ((addressList[i].AddressFamily == addressFamily) && addressList[i].Equals(remoteAddress))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static IRemotingFormatter MimeTypeToFormatter(string mimeType, bool serialize)
        {
            if (string.Compare(mimeType, "text/xml", StringComparison.Ordinal) == 0)
            {
                return CreateSoapFormatter(serialize, true);
            }
            if (string.Compare(mimeType, "application/octet-stream", StringComparison.Ordinal) == 0)
            {
                return CreateBinaryFormatter(serialize, true);
            }
            return null;
        }

        internal static string RemoveApplicationNameFromUri(string uri)
        {
            if (uri == null)
            {
                return null;
            }
            string applicationName = RemotingConfiguration.ApplicationName;
            if (((applicationName != null) && (applicationName.Length != 0)) && ((uri.Length >= (applicationName.Length + 2)) && ((string.Compare(applicationName, 0, uri, 0, applicationName.Length, StringComparison.OrdinalIgnoreCase) == 0) && (uri[applicationName.Length] == '/'))))
            {
                uri = uri.Substring(applicationName.Length + 1);
            }
            return uri;
        }

        internal static void ReportUnknownProviderConfigProperty(string providerTypeName, string propertyName)
        {
            throw new RemotingException(string.Format(CultureInfo.CurrentCulture, GetResourceString("Remoting_Providers_Config_UnknownProperty"), new object[] { providerTypeName, propertyName }));
        }

        internal static Stream SerializeBinaryMessage(IMessage msg, bool includeVersions)
        {
            MemoryStream outputStream = new MemoryStream();
            SerializeBinaryMessage(msg, outputStream, includeVersions);
            outputStream.Position = 0L;
            return outputStream;
        }

        internal static void SerializeBinaryMessage(IMessage msg, Stream outputStream, bool includeVersions)
        {
            CreateBinaryFormatter(true, includeVersions).Serialize(outputStream, msg, null);
        }

        internal static Stream SerializeMessage(string mimeType, IMessage msg, bool includeVersions)
        {
            Stream outputStream = new MemoryStream();
            SerializeMessage(mimeType, msg, outputStream, includeVersions);
            outputStream.Position = 0L;
            return outputStream;
        }

        internal static void SerializeMessage(string mimeType, IMessage msg, Stream outputStream, bool includeVersions)
        {
            if (string.Compare(mimeType, "text/xml", StringComparison.Ordinal) == 0)
            {
                SerializeSoapMessage(msg, outputStream, includeVersions);
            }
            else if (string.Compare(mimeType, "application/octet-stream", StringComparison.Ordinal) == 0)
            {
                SerializeBinaryMessage(msg, outputStream, includeVersions);
            }
        }

        internal static Stream SerializeSoapMessage(IMessage msg, bool includeVersions)
        {
            MemoryStream outputStream = new MemoryStream();
            SerializeSoapMessage(msg, outputStream, includeVersions);
            outputStream.Position = 0L;
            return outputStream;
        }

        internal static void SerializeSoapMessage(IMessage msg, Stream outputStream, bool includeVersions)
        {
            SoapFormatter formatter = CreateSoapFormatter(true, includeVersions);
            IMethodMessage message = msg as IMethodMessage;
            if ((message != null) && (message.MethodBase != null))
            {
                SoapTypeAttribute cachedSoapAttribute = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute(message.MethodBase.DeclaringType);
                if ((cachedSoapAttribute.SoapOptions & SoapOption.AlwaysIncludeTypes) == SoapOption.AlwaysIncludeTypes)
                {
                    formatter.TypeFormat |= FormatterTypeStyle.TypesAlways;
                }
                if ((cachedSoapAttribute.SoapOptions & SoapOption.XsdString) == SoapOption.XsdString)
                {
                    formatter.TypeFormat |= FormatterTypeStyle.XsdString;
                }
            }
            Header[] soapHeaders = GetSoapHeaders(msg);
            ((RemotingSurrogateSelector) formatter.SurrogateSelector).SetRootObject(msg);
            formatter.Serialize(outputStream, msg, soapHeaders);
        }

        internal static bool SetupUrlBashingForIisSslIfNecessary()
        {
            if (IsClientSKUInstallation)
            {
                return false;
            }
            return SetupUrlBashingForIisSslIfNecessaryWorker();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool SetupUrlBashingForIisSslIfNecessaryWorker()
        {
            HttpContext current = HttpContext.Current;
            bool flag = false;
            if ((current != null) && current.Request.IsSecureConnection)
            {
                Uri url = current.Request.Url;
                StringBuilder builder = new StringBuilder(100);
                builder.Append("https://");
                builder.Append(url.Host);
                builder.Append(":");
                builder.Append(url.Port);
                builder.Append("/");
                builder.Append(RemotingConfiguration.ApplicationName);
                string[] data = new string[] { IisHelper.ApplicationUrl, builder.ToString() };
                CallContext.SetData("__bashChannelUrl", data);
                flag = true;
            }
            return flag;
        }

        internal static string SidToString(IntPtr sidPointer)
        {
            if (!System.Runtime.Remoting.Channels.NativeMethods.IsValidSid(sidPointer))
            {
                throw new RemotingException(GetResourceString("Remoting_InvalidSid"));
            }
            StringBuilder builder = new StringBuilder();
            IntPtr sidIdentifierAuthority = System.Runtime.Remoting.Channels.NativeMethods.GetSidIdentifierAuthority(sidPointer);
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
            byte[] destination = new byte[6];
            Marshal.Copy(sidIdentifierAuthority, destination, 0, 6);
            IntPtr sidSubAuthorityCount = System.Runtime.Remoting.Channels.NativeMethods.GetSidSubAuthorityCount(sidPointer);
            error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                throw new Win32Exception(error);
            }
            uint num2 = Marshal.ReadByte(sidSubAuthorityCount);
            if ((destination[0] != 0) && (destination[1] != 0))
            {
                builder.Append(string.Format(CultureInfo.CurrentCulture, "{0:x2}{1:x2}{2:x2}{3:x2}{4:x2}{5:x2}", new object[] { destination[0], destination[1], destination[2], destination[3], destination[4], destination[5] }));
            }
            else
            {
                uint num3 = (uint) (((destination[5] + (destination[4] << 8)) + (destination[3] << 0x10)) + (destination[2] << 0x18));
                builder.Append(string.Format(CultureInfo.CurrentCulture, "{0:x12}", new object[] { num3 }));
            }
            for (int i = 0; i < num2; i++)
            {
                IntPtr sidSubAuthority = System.Runtime.Remoting.Channels.NativeMethods.GetSidSubAuthority(sidPointer, i);
                error = Marshal.GetLastWin32Error();
                if (error != 0)
                {
                    throw new Win32Exception(error);
                }
                uint num5 = (uint) Marshal.ReadInt32(sidSubAuthority);
                builder.Append(string.Format(CultureInfo.CurrentCulture, "-{0:x12}", new object[] { num5 }));
            }
            return builder.ToString();
        }

        internal static void VerifyNoProviderData(string providerTypeName, ICollection providerData)
        {
            if ((providerData != null) && (providerData.Count > 0))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, GetResourceString("Remoting_Providers_Config_NotExpectingProviderData"), new object[] { providerTypeName }));
            }
        }

        internal static IByteBufferPool BufferPool
        {
            get
            {
                return _bufferPool;
            }
        }

        internal static bool IsClientSKUInstallation
        {
            get
            {
                if (!s_isClientSKUInstallationInitialized)
                {
                    s_isClientSKUInstallation = Type.GetType("System.Web.HttpContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false) == null;
                    s_isClientSKUInstallationInitialized = true;
                }
                return s_isClientSKUInstallation;
            }
        }

        internal static System.Runtime.Remoting.Channels.RequestQueue RequestQueue
        {
            get
            {
                return _requestQueue;
            }
        }

        private class UriHeaderHandler
        {
            private string _uri;

            internal UriHeaderHandler(string uri)
            {
                this._uri = uri;
            }

            public object HeaderHandler(Header[] Headers)
            {
                return this._uri;
            }
        }
    }
}

