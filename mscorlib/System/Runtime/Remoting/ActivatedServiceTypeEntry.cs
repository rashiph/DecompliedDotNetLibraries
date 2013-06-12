namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Threading;

    [ComVisible(true)]
    public class ActivatedServiceTypeEntry : TypeEntry
    {
        private IContextAttribute[] _contextAttributes;

        [SecuritySafeCritical]
        public ActivatedServiceTypeEntry(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            RuntimeType type2 = type as RuntimeType;
            if (type2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
            }
            base.TypeName = type.FullName;
            base.AssemblyName = type2.GetRuntimeAssembly().GetSimpleName();
        }

        public ActivatedServiceTypeEntry(string typeName, string assemblyName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            base.TypeName = typeName;
            base.AssemblyName = assemblyName;
        }

        public override string ToString()
        {
            return ("type='" + base.TypeName + ", " + base.AssemblyName + "'");
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

