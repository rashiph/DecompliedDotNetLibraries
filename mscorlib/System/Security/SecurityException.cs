namespace System.Security
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public class SecurityException : SystemException
    {
        private const string ActionName = "Action";
        private const string Assembly_Name = "Assembly";
        private const string DemandedName = "Demanded";
        private const string DeniedName = "Denied";
        private const string FirstPermissionThatFailedName = "FirstPermissionThatFailed";
        private const string GrantedSetName = "GrantedSet";
        private SecurityAction m_action;
        private AssemblyName m_assemblyName;
        private string m_debugString;
        private string m_demanded;
        private string m_denied;
        private string m_granted;
        private string m_permissionThatFailed;
        private string m_permitOnly;
        private string m_refused;
        private byte[] m_serializedMethodInfo;
        private string m_strMethodInfo;
        [NonSerialized]
        private Type m_typeOfPermissionThatFailed;
        private string m_url;
        private SecurityZone m_zone;
        private const string MethodName_Serialized = "Method";
        private const string MethodName_String = "Method_String";
        private const string PermitOnlyName = "PermitOnly";
        private const string RefusedSetName = "RefusedSet";
        private const string UrlName = "Url";
        private const string ZoneName = "Zone";

        public SecurityException() : base(GetResString("Arg_SecurityException"))
        {
            base.SetErrorCode(-2146233078);
        }

        public SecurityException(string message) : base(message)
        {
            base.SetErrorCode(-2146233078);
        }

        [SecuritySafeCritical]
        protected SecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            try
            {
                this.m_action = (SecurityAction) info.GetValue("Action", typeof(SecurityAction));
                this.m_permissionThatFailed = (string) info.GetValueNoThrow("FirstPermissionThatFailed", typeof(string));
                this.m_demanded = (string) info.GetValueNoThrow("Demanded", typeof(string));
                this.m_granted = (string) info.GetValueNoThrow("GrantedSet", typeof(string));
                this.m_refused = (string) info.GetValueNoThrow("RefusedSet", typeof(string));
                this.m_denied = (string) info.GetValueNoThrow("Denied", typeof(string));
                this.m_permitOnly = (string) info.GetValueNoThrow("PermitOnly", typeof(string));
                this.m_assemblyName = (AssemblyName) info.GetValueNoThrow("Assembly", typeof(AssemblyName));
                this.m_serializedMethodInfo = (byte[]) info.GetValueNoThrow("Method", typeof(byte[]));
                this.m_strMethodInfo = (string) info.GetValueNoThrow("Method_String", typeof(string));
                this.m_zone = (SecurityZone) info.GetValue("Zone", typeof(SecurityZone));
                this.m_url = (string) info.GetValueNoThrow("Url", typeof(string));
            }
            catch
            {
                this.m_action = (SecurityAction) 0;
                this.m_permissionThatFailed = "";
                this.m_demanded = "";
                this.m_granted = "";
                this.m_refused = "";
                this.m_denied = "";
                this.m_permitOnly = "";
                this.m_assemblyName = null;
                this.m_serializedMethodInfo = null;
                this.m_strMethodInfo = null;
                this.m_zone = SecurityZone.NoZone;
                this.m_url = "";
            }
        }

        [SecurityCritical]
        internal SecurityException(PermissionSet grantedSetObj, PermissionSet refusedSetObj) : base(GetResString("Arg_SecurityException"))
        {
            PermissionSet.s_fullTrust.Assert();
            base.SetErrorCode(-2146233078);
            if (grantedSetObj != null)
            {
                this.m_granted = grantedSetObj.ToXml().ToString();
            }
            if (refusedSetObj != null)
            {
                this.m_refused = refusedSetObj.ToXml().ToString();
            }
        }

        public SecurityException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233078);
        }

        [SecuritySafeCritical]
        public SecurityException(string message, Type type) : base(message)
        {
            PermissionSet.s_fullTrust.Assert();
            base.SetErrorCode(-2146233078);
            this.m_typeOfPermissionThatFailed = type;
        }

        [SecurityCritical]
        internal SecurityException(string message, PermissionSet grantedSetObj, PermissionSet refusedSetObj) : base(message)
        {
            PermissionSet.s_fullTrust.Assert();
            base.SetErrorCode(-2146233078);
            if (grantedSetObj != null)
            {
                this.m_granted = grantedSetObj.ToXml().ToString();
            }
            if (refusedSetObj != null)
            {
                this.m_refused = refusedSetObj.ToXml().ToString();
            }
        }

        [SecuritySafeCritical]
        public SecurityException(string message, Type type, string state) : base(message)
        {
            PermissionSet.s_fullTrust.Assert();
            base.SetErrorCode(-2146233078);
            this.m_typeOfPermissionThatFailed = type;
            this.m_demanded = state;
        }

        [SecuritySafeCritical]
        public SecurityException(string message, object deny, object permitOnly, MethodInfo method, object demanded, IPermission permThatFailed) : base(message)
        {
            PermissionSet.s_fullTrust.Assert();
            base.SetErrorCode(-2146233078);
            this.Action = SecurityAction.Demand;
            if (permThatFailed != null)
            {
                this.m_typeOfPermissionThatFailed = permThatFailed.GetType();
            }
            this.FirstPermissionThatFailed = permThatFailed;
            this.Demanded = demanded;
            this.m_granted = "";
            this.m_refused = "";
            this.DenySetInstance = deny;
            this.PermitOnlySetInstance = permitOnly;
            this.m_assemblyName = null;
            this.Method = method;
            this.m_zone = SecurityZone.NoZone;
            this.m_url = "";
            this.m_debugString = this.ToString(true, false);
        }

        [SecuritySafeCritical]
        public SecurityException(string message, AssemblyName assemblyName, PermissionSet grant, PermissionSet refused, MethodInfo method, SecurityAction action, object demanded, IPermission permThatFailed, Evidence evidence) : base(message)
        {
            PermissionSet.s_fullTrust.Assert();
            base.SetErrorCode(-2146233078);
            this.Action = action;
            if (permThatFailed != null)
            {
                this.m_typeOfPermissionThatFailed = permThatFailed.GetType();
            }
            this.FirstPermissionThatFailed = permThatFailed;
            this.Demanded = demanded;
            this.m_granted = (grant == null) ? "" : grant.ToXml().ToString();
            this.m_refused = (refused == null) ? "" : refused.ToXml().ToString();
            this.m_denied = "";
            this.m_permitOnly = "";
            this.m_assemblyName = assemblyName;
            this.Method = method;
            this.m_url = "";
            this.m_zone = SecurityZone.NoZone;
            if (evidence != null)
            {
                System.Security.Policy.Url hostEvidence = evidence.GetHostEvidence<System.Security.Policy.Url>();
                if (hostEvidence != null)
                {
                    this.m_url = hostEvidence.GetURLString().ToString();
                }
                System.Security.Policy.Zone zone = evidence.GetHostEvidence<System.Security.Policy.Zone>();
                if (zone != null)
                {
                    this.m_zone = zone.SecurityZone;
                }
            }
            this.m_debugString = this.ToString(true, false);
        }

        private static object ByteArrayToObject(byte[] array)
        {
            if ((array == null) || (array.Length == 0))
            {
                return null;
            }
            MemoryStream serializationStream = new MemoryStream(array);
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(serializationStream);
        }

        [SecurityCritical]
        private bool CanAccessSensitiveInfo()
        {
            bool flag = false;
            try
            {
                new SecurityPermission(SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence).Demand();
                flag = true;
            }
            catch (SecurityException)
            {
            }
            return flag;
        }

        private MethodInfo getMethod()
        {
            return (MethodInfo) ByteArrayToObject(this.m_serializedMethodInfo);
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("Action", this.m_action, typeof(SecurityAction));
            info.AddValue("FirstPermissionThatFailed", this.m_permissionThatFailed, typeof(string));
            info.AddValue("Demanded", this.m_demanded, typeof(string));
            info.AddValue("GrantedSet", this.m_granted, typeof(string));
            info.AddValue("RefusedSet", this.m_refused, typeof(string));
            info.AddValue("Denied", this.m_denied, typeof(string));
            info.AddValue("PermitOnly", this.m_permitOnly, typeof(string));
            info.AddValue("Assembly", this.m_assemblyName, typeof(AssemblyName));
            info.AddValue("Method", this.m_serializedMethodInfo, typeof(byte[]));
            info.AddValue("Method_String", this.m_strMethodInfo, typeof(string));
            info.AddValue("Zone", this.m_zone, typeof(SecurityZone));
            info.AddValue("Url", this.m_url, typeof(string));
        }

        [SecuritySafeCritical]
        internal static string GetResString(string sResourceName)
        {
            PermissionSet.s_fullTrust.Assert();
            return Environment.GetResourceString(sResourceName);
        }

        [SecurityCritical]
        internal static Exception MakeSecurityException(AssemblyName asmName, Evidence asmEvidence, PermissionSet granted, PermissionSet refused, RuntimeMethodHandleInternal rmh, SecurityAction action, object demand, IPermission permThatFailed)
        {
            HostProtectionPermission permission = permThatFailed as HostProtectionPermission;
            if (permission != null)
            {
                return new HostProtectionException(GetResString("HostProtection_HostProtection"), HostProtectionPermission.protectedResources, permission.Resources);
            }
            string message = "";
            MethodInfo method = null;
            try
            {
                if (((granted == null) && (refused == null)) && (demand == null))
                {
                    message = GetResString("Security_NoAPTCA");
                }
                else if ((demand != null) && (demand is IPermission))
                {
                    message = string.Format(CultureInfo.InvariantCulture, GetResString("Security_Generic"), new object[] { demand.GetType().AssemblyQualifiedName });
                }
                else if (permThatFailed != null)
                {
                    message = string.Format(CultureInfo.InvariantCulture, GetResString("Security_Generic"), new object[] { permThatFailed.GetType().AssemblyQualifiedName });
                }
                else
                {
                    message = GetResString("Security_GenericNoType");
                }
                method = SecurityRuntime.GetMethodInfo(rmh);
            }
            catch (Exception exception)
            {
                if (exception is ThreadAbortException)
                {
                    throw;
                }
            }
            return new SecurityException(message, asmName, granted, refused, method, action, demand, permThatFailed, asmEvidence);
        }

        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            MemoryStream serializationStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(serializationStream, obj);
                return serializationStream.ToArray();
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return this.ToString(this.CanAccessSensitiveInfo(), true);
        }

        [SecurityCritical]
        private string ToString(bool includeSensitiveInfo, bool includeBaseInfo)
        {
            PermissionSet.s_fullTrust.Assert();
            StringBuilder sb = new StringBuilder();
            if (includeBaseInfo)
            {
                sb.Append(base.ToString());
            }
            if (this.Action > ((SecurityAction) 0))
            {
                this.ToStringHelper(sb, "Security_Action", this.Action);
            }
            this.ToStringHelper(sb, "Security_TypeFirstPermThatFailed", this.PermissionType);
            if (includeSensitiveInfo)
            {
                this.ToStringHelper(sb, "Security_FirstPermThatFailed", this.m_permissionThatFailed);
                this.ToStringHelper(sb, "Security_Demanded", this.m_demanded);
                this.ToStringHelper(sb, "Security_GrantedSet", this.m_granted);
                this.ToStringHelper(sb, "Security_RefusedSet", this.m_refused);
                this.ToStringHelper(sb, "Security_Denied", this.m_denied);
                this.ToStringHelper(sb, "Security_PermitOnly", this.m_permitOnly);
                this.ToStringHelper(sb, "Security_Assembly", this.m_assemblyName);
                this.ToStringHelper(sb, "Security_Method", this.m_strMethodInfo);
            }
            if (this.m_zone != SecurityZone.NoZone)
            {
                this.ToStringHelper(sb, "Security_Zone", this.m_zone);
            }
            if (includeSensitiveInfo)
            {
                this.ToStringHelper(sb, "Security_Url", this.m_url);
            }
            return sb.ToString();
        }

        private void ToStringHelper(StringBuilder sb, string resourceString, object attr)
        {
            if (attr != null)
            {
                string str = attr as string;
                if (str == null)
                {
                    str = attr.ToString();
                }
                if (str.Length != 0)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(GetResString(resourceString));
                    sb.Append(Environment.NewLine);
                    sb.Append(str);
                }
            }
        }

        [ComVisible(false)]
        public SecurityAction Action
        {
            get
            {
                return this.m_action;
            }
            set
            {
                this.m_action = value;
            }
        }

        [ComVisible(false)]
        public object Demanded
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return XMLUtil.XmlStringToSecurityObject(this.m_demanded);
            }
            [SecuritySafeCritical]
            set
            {
                this.m_demanded = XMLUtil.SecurityObjectToXmlString(value);
            }
        }

        [ComVisible(false)]
        public object DenySetInstance
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return XMLUtil.XmlStringToSecurityObject(this.m_denied);
            }
            [SecuritySafeCritical]
            set
            {
                this.m_denied = XMLUtil.SecurityObjectToXmlString(value);
            }
        }

        [ComVisible(false)]
        public AssemblyName FailedAssemblyInfo
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this.m_assemblyName;
            }
            set
            {
                this.m_assemblyName = value;
            }
        }

        public IPermission FirstPermissionThatFailed
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return (IPermission) XMLUtil.XmlStringToSecurityObject(this.m_permissionThatFailed);
            }
            [SecuritySafeCritical]
            set
            {
                this.m_permissionThatFailed = XMLUtil.SecurityObjectToXmlString(value);
            }
        }

        public string GrantedSet
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this.m_granted;
            }
            set
            {
                this.m_granted = value;
            }
        }

        [ComVisible(false)]
        public MethodInfo Method
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this.getMethod();
            }
            [SecuritySafeCritical]
            set
            {
                RuntimeMethodInfo info = value as RuntimeMethodInfo;
                this.m_serializedMethodInfo = ObjectToByteArray(info);
                if (info != null)
                {
                    this.m_strMethodInfo = info.ToString();
                }
            }
        }

        public string PermissionState
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this.m_demanded;
            }
            set
            {
                this.m_demanded = value;
            }
        }

        public Type PermissionType
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_typeOfPermissionThatFailed == null)
                {
                    object obj2 = XMLUtil.XmlStringToSecurityObject(this.m_permissionThatFailed);
                    if (obj2 == null)
                    {
                        obj2 = XMLUtil.XmlStringToSecurityObject(this.m_demanded);
                    }
                    if (obj2 != null)
                    {
                        this.m_typeOfPermissionThatFailed = obj2.GetType();
                    }
                }
                return this.m_typeOfPermissionThatFailed;
            }
            set
            {
                this.m_typeOfPermissionThatFailed = value;
            }
        }

        [ComVisible(false)]
        public object PermitOnlySetInstance
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return XMLUtil.XmlStringToSecurityObject(this.m_permitOnly);
            }
            [SecuritySafeCritical]
            set
            {
                this.m_permitOnly = XMLUtil.SecurityObjectToXmlString(value);
            }
        }

        public string RefusedSet
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this.m_refused;
            }
            set
            {
                this.m_refused = value;
            }
        }

        public string Url
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
            get
            {
                return this.m_url;
            }
            set
            {
                this.m_url = value;
            }
        }

        public SecurityZone Zone
        {
            get
            {
                return this.m_zone;
            }
            set
            {
                this.m_zone = value;
            }
        }
    }
}

