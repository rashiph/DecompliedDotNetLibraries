namespace System.Threading
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable]
    internal sealed class DomainCompressedStack
    {
        private bool m_bHaltConstruction;
        private PermissionListSet m_pls;

        [SecurityCritical]
        private static DomainCompressedStack CreateManagedObject(IntPtr unmanagedDCS)
        {
            DomainCompressedStack stack;
            return new DomainCompressedStack { m_pls = PermissionListSet.CreateCompressedState(unmanagedDCS, out stack.m_bHaltConstruction) };
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetDescCount(IntPtr dcs);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool GetDescriptorInfo(IntPtr dcs, int index, out PermissionSet granted, out PermissionSet refused, out Assembly assembly, out FrameSecurityDescriptor fsd);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void GetDomainPermissionSets(IntPtr dcs, out PermissionSet granted, out PermissionSet refused);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IgnoreDomain(IntPtr dcs);

        internal bool ConstructionHalted
        {
            get
            {
                return this.m_bHaltConstruction;
            }
        }

        internal PermissionListSet PLS
        {
            get
            {
                return this.m_pls;
            }
        }
    }
}

