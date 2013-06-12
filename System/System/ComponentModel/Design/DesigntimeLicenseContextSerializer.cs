namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class DesigntimeLicenseContextSerializer
    {
        private DesigntimeLicenseContextSerializer()
        {
        }

        internal static void Deserialize(Stream o, string cryptoKey, RuntimeLicenseContext context)
        {
            IFormatter formatter = new BinaryFormatter();
            object obj2 = formatter.Deserialize(o);
            if (obj2 is object[])
            {
                object[] objArray = (object[]) obj2;
                if ((objArray[0] is string) && (((string) objArray[0]) == cryptoKey))
                {
                    context.savedLicenseKeys = (Hashtable) objArray[1];
                }
            }
        }

        public static void Serialize(Stream o, string cryptoKey, DesigntimeLicenseContext context)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(o, new object[] { cryptoKey, context.savedLicenseKeys });
        }
    }
}

