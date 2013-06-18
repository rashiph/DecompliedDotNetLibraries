namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel.Description;

    internal static class ComPlusTypeValidator
    {
        private static Guid IID_IDisposable = new Guid("{805D7A98-D4AF-3F0F-967F-E5CF45312D2C}");
        private static Guid IID_IManagedObject = new Guid("{C3FCC19E-A970-11D2-8B5A-00A0C9B7C9C4}");
        private static Guid IID_IProcessInitializer = new Guid("{1113F52D-DC7F-4943-AED6-88D04027E32A}");
        private static Guid IID_IRemoteDispatch = new Guid("{6619A740-8154-43BE-A186-0319578E02DB}");
        private static Guid IID_IServicedComponentInfo = new Guid("{8165B19E-8D3A-4D0B-80C8-97DE310DB583}");
        private static Guid IID_Object = new Guid("{65074F7F-63C0-304E-AF0A-D51741CB4A8D}");

        public static bool IsValidInterface(Guid iid)
        {
            return (((!(iid == IID_Object) && !(iid == IID_IDisposable)) && (!(iid == IID_IManagedObject) && !(iid == IID_IProcessInitializer))) && ((!(iid == IID_IRemoteDispatch) && !(iid == IID_IServicedComponentInfo)) && !iid.ToString("D").EndsWith("C000-000000000046", StringComparison.OrdinalIgnoreCase)));
        }

        public static bool IsValidParameter(Type type, ICustomAttributeProvider attributeProvider, bool allowReferences)
        {
            foreach (MarshalAsAttribute attribute in ServiceReflector.GetCustomAttributes(attributeProvider, typeof(MarshalAsAttribute), true))
            {
                switch (attribute.Value)
                {
                    case UnmanagedType.IDispatch:
                    case UnmanagedType.Interface:
                    case UnmanagedType.IUnknown:
                        return allowReferences;
                }
            }
            XsdDataContractExporter exporter = new XsdDataContractExporter();
            if (!exporter.CanExport(type))
            {
                return false;
            }
            return true;
        }
    }
}

