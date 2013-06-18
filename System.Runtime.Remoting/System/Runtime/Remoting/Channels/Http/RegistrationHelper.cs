namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;

    internal static class RegistrationHelper
    {
        private static void RegisterSingleType(string machineAndAppName, Type type)
        {
            string name = type.Name;
            string xmlNamespace = "http://" + machineAndAppName + "/" + type.FullName;
            SoapServices.RegisterInteropXmlElement(name, xmlNamespace, type);
            SoapServices.RegisterInteropXmlType(name, xmlNamespace, type);
            if (typeof(MarshalByRefObject).IsAssignableFrom(type))
            {
                foreach (MethodInfo info in type.GetMethods())
                {
                    SoapServices.RegisterSoapActionForMethodBase(info, xmlNamespace + "#" + info.Name);
                }
            }
        }

        public static void RegisterType(string machineAndAppName, Type type, string uri)
        {
            RemotingConfiguration.RegisterWellKnownServiceType(type, uri, WellKnownObjectMode.SingleCall);
            foreach (Type type2 in type.Assembly.GetTypes())
            {
                RegisterSingleType(machineAndAppName, type2);
            }
        }
    }
}

