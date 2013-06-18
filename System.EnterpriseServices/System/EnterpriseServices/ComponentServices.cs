namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Services;

    internal sealed class ComponentServices
    {
        private ComponentServices()
        {
        }

        internal static IMessage ConvertToMessage(string s, object tp)
        {
            ComponentSerializer serializer = ComponentSerializer.Get();
            byte[] bytes = GetBytes(s);
            IMessage message = (IMessage) serializer.UnmarshalFromBuffer(bytes, tp);
            serializer.Release();
            return message;
        }

        internal static IMessage ConvertToReturnMessage(string s, IMessage mcMsg)
        {
            ComponentSerializer serializer = ComponentSerializer.Get();
            byte[] bytes = GetBytes(s);
            IMessage message = (IMessage) serializer.UnmarshalReturnMessageFromBuffer(bytes, (IMethodCallMessage) mcMsg);
            serializer.Release();
            return message;
        }

        internal static string ConvertToString(IMessage reqMsg)
        {
            long num;
            ComponentSerializer serializer = ComponentSerializer.Get();
            string str = GetString(serializer.MarshalToBuffer(reqMsg, out num), (int) num);
            serializer.Release();
            return str;
        }

        public static void DeactivateObject(object otp, bool disposing)
        {
            ServicedComponentProxy realProxy = RemotingServices.GetRealProxy(otp) as ServicedComponentProxy;
            if (!realProxy.IsProxyDeactivated)
            {
                if (realProxy.IsObjectPooled)
                {
                    ReconnectForPooling(realProxy);
                }
                realProxy.DeactivateProxy(disposing);
            }
        }

        internal static unsafe byte[] GetBytes(string s)
        {
            int length = s.Length * 2;
            fixed (char* chRef = s.ToCharArray())
            {
                byte[] destination = new byte[length];
                Marshal.Copy((IntPtr) chRef, destination, 0, length);
                return destination;
            }
        }

        public static byte[] GetDCOMBuffer(object o)
        {
            int marshalSize = Proxy.GetMarshalSize(o);
            if (marshalSize == -1)
            {
                throw new RemotingException(Resource.FormatString("Remoting_InteropError"));
            }
            byte[] b = new byte[marshalSize];
            if (!Proxy.MarshalObject(o, b, marshalSize))
            {
                throw new RemotingException(Resource.FormatString("Remoting_InteropError"));
            }
            return b;
        }

        internal static unsafe string GetString(byte[] bytes, int count)
        {
            fixed (byte* numRef = bytes)
            {
                return Marshal.PtrToStringUni((IntPtr) numRef, count / 2);
            }
        }

        internal static void InitializeRemotingChannels()
        {
        }

        private static void ReconnectForPooling(ServicedComponentProxy scp)
        {
            Type proxiedType = scp.GetProxiedType();
            bool isJitActivated = scp.IsJitActivated;
            bool isObjectPooled = scp.IsObjectPooled;
            bool areMethodsSecure = scp.AreMethodsSecure;
            ProxyTearoff proxyTearoff = null;
            ServicedComponent server = scp.DisconnectForPooling(ref proxyTearoff);
            ServicedComponentProxy newcp = new ServicedComponentProxy(proxiedType, isJitActivated, isObjectPooled, areMethodsSecure, false);
            newcp.ConnectForPooling(scp, server, proxyTearoff, false);
            EnterpriseServicesHelper.SwitchWrappers(scp, newcp);
            if (proxyTearoff != null)
            {
                Marshal.ChangeWrapperHandleStrength(proxyTearoff, false);
            }
            Marshal.ChangeWrapperHandleStrength(newcp.GetTransparentProxy(), false);
        }
    }
}

