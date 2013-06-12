namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_MethodRental)), ComVisible(true), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class MethodRental : _MethodRental
    {
        public const int JitImmediate = 1;
        public const int JitOnDemand = 0;

        private MethodRental()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public static void SwapMethodBody(Type cls, int methodtoken, IntPtr rgIL, int methodSize, int flags)
        {
            InternalModuleBuilder internalModule;
            RuntimeType runtimeType;
            if ((methodSize <= 0) || (methodSize >= 0x3f0000))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadSizeForData"), "methodSize");
            }
            if (cls == null)
            {
                throw new ArgumentNullException("cls");
            }
            Module module = cls.Module;
            ModuleBuilder builder2 = module as ModuleBuilder;
            if (builder2 != null)
            {
                internalModule = builder2.InternalModule;
            }
            else
            {
                internalModule = module as InternalModuleBuilder;
            }
            if (internalModule == null)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NotDynamicModule"));
            }
            if (cls is TypeBuilder)
            {
                TypeBuilder builder3 = (TypeBuilder) cls;
                if (!builder3.m_hasBeenCreated)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_NotAllTypesAreBaked", new object[] { builder3.Name }));
                }
                runtimeType = builder3.m_runtimeType;
            }
            else
            {
                runtimeType = cls as RuntimeType;
            }
            if (runtimeType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "cls");
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            RuntimeAssembly runtimeAssembly = internalModule.GetRuntimeAssembly();
            lock (runtimeAssembly.SyncRoot)
            {
                SwapMethodBody(runtimeType.GetTypeHandleInternal(), methodtoken, rgIL, methodSize, flags, JitHelpers.GetStackCrawlMarkHandle(ref lookForMyCaller));
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SwapMethodBody(RuntimeTypeHandle cls, int methodtoken, IntPtr rgIL, int methodSize, int flags, StackCrawlMarkHandle stackMark);
        void _MethodRental.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _MethodRental.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodRental.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodRental.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }
    }
}

