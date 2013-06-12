namespace System.Security.Policy
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class EvidenceBase
    {
        protected EvidenceBase()
        {
            if (!base.GetType().IsSerializable)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Policy_EvidenceMustBeSerializable"));
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Assert, SerializationFormatter=true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual EvidenceBase Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Position = 0L;
                return (formatter.Deserialize(stream) as EvidenceBase);
            }
        }
    }
}

