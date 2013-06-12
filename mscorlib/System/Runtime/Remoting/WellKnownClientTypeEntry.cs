namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [ComVisible(true)]
    public class WellKnownClientTypeEntry : TypeEntry
    {
        private string _appUrl;
        private string _objectUrl;

        [SecuritySafeCritical]
        public WellKnownClientTypeEntry(Type type, string objectUrl)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (objectUrl == null)
            {
                throw new ArgumentNullException("objectUrl");
            }
            RuntimeType type2 = type as RuntimeType;
            if (type2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            base.TypeName = type.FullName;
            base.AssemblyName = type2.GetRuntimeAssembly().GetSimpleName();
            this._objectUrl = objectUrl;
        }

        public WellKnownClientTypeEntry(string typeName, string assemblyName, string objectUrl)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if (objectUrl == null)
            {
                throw new ArgumentNullException("objectUrl");
            }
            base.TypeName = typeName;
            base.AssemblyName = assemblyName;
            this._objectUrl = objectUrl;
        }

        public override string ToString()
        {
            string str = "type='" + base.TypeName + ", " + base.AssemblyName + "'; url=" + this._objectUrl;
            if (this._appUrl != null)
            {
                str = str + "; appUrl=" + this._appUrl;
            }
            return str;
        }

        public string ApplicationUrl
        {
            get
            {
                return this._appUrl;
            }
            set
            {
                this._appUrl = value;
            }
        }

        public Type ObjectType
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
                return RuntimeTypeHandle.GetTypeByName(base.TypeName + ", " + base.AssemblyName, ref lookForMyCaller);
            }
        }

        public string ObjectUrl
        {
            get
            {
                return this._objectUrl;
            }
        }
    }
}

