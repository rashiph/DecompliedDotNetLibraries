namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Threading;

    [ComVisible(true)]
    public class ActivatedClientTypeEntry : TypeEntry
    {
        private string _appUrl;
        private IContextAttribute[] _contextAttributes;

        [SecuritySafeCritical]
        public ActivatedClientTypeEntry(Type type, string appUrl)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (appUrl == null)
            {
                throw new ArgumentNullException("appUrl");
            }
            RuntimeType type2 = type as RuntimeType;
            if (type2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
            }
            base.TypeName = type.FullName;
            base.AssemblyName = type2.GetRuntimeAssembly().GetSimpleName();
            this._appUrl = appUrl;
        }

        public ActivatedClientTypeEntry(string typeName, string assemblyName, string appUrl)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if (appUrl == null)
            {
                throw new ArgumentNullException("appUrl");
            }
            base.TypeName = typeName;
            base.AssemblyName = assemblyName;
            this._appUrl = appUrl;
        }

        public override string ToString()
        {
            return ("type='" + base.TypeName + ", " + base.AssemblyName + "'; appUrl=" + this._appUrl);
        }

        public string ApplicationUrl
        {
            get
            {
                return this._appUrl;
            }
        }

        public IContextAttribute[] ContextAttributes
        {
            get
            {
                return this._contextAttributes;
            }
            set
            {
                this._contextAttributes = value;
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
    }
}

