namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationSetDeploymentMetadata
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.Interface)]
        public System.Deployment.Internal.Isolation.IDefinitionAppId Deployment;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr InstallerReference;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr cPropertiesToTest;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr PropertiesToTest;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr cPropertiesToSet;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr PropertiesToSet;
        public StoreOperationSetDeploymentMetadata(System.Deployment.Internal.Isolation.IDefinitionAppId Deployment, System.Deployment.Internal.Isolation.StoreApplicationReference Reference, System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] SetProperties) : this(Deployment, Reference, SetProperties, null)
        {
        }

        [SecuritySafeCritical]
        public StoreOperationSetDeploymentMetadata(System.Deployment.Internal.Isolation.IDefinitionAppId Deployment, System.Deployment.Internal.Isolation.StoreApplicationReference Reference, System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] SetProperties, System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] TestProperties)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationSetDeploymentMetadata));
            this.Flags = OpFlags.Nothing;
            this.Deployment = Deployment;
            if (SetProperties != null)
            {
                this.PropertiesToSet = MarshalProperties(SetProperties);
                this.cPropertiesToSet = new IntPtr(SetProperties.Length);
            }
            else
            {
                this.PropertiesToSet = IntPtr.Zero;
                this.cPropertiesToSet = IntPtr.Zero;
            }
            if (TestProperties != null)
            {
                this.PropertiesToTest = MarshalProperties(TestProperties);
                this.cPropertiesToTest = new IntPtr(TestProperties.Length);
            }
            else
            {
                this.PropertiesToTest = IntPtr.Zero;
                this.cPropertiesToTest = IntPtr.Zero;
            }
            this.InstallerReference = Reference.ToIntPtr();
        }

        [SecurityCritical]
        public void Destroy()
        {
            if (this.PropertiesToSet != IntPtr.Zero)
            {
                DestroyProperties(this.PropertiesToSet, (ulong) this.cPropertiesToSet.ToInt64());
                this.PropertiesToSet = IntPtr.Zero;
                this.cPropertiesToSet = IntPtr.Zero;
            }
            if (this.PropertiesToTest != IntPtr.Zero)
            {
                DestroyProperties(this.PropertiesToTest, (ulong) this.cPropertiesToTest.ToInt64());
                this.PropertiesToTest = IntPtr.Zero;
                this.cPropertiesToTest = IntPtr.Zero;
            }
            if (this.InstallerReference != IntPtr.Zero)
            {
                System.Deployment.Internal.Isolation.StoreApplicationReference.Destroy(this.InstallerReference);
                this.InstallerReference = IntPtr.Zero;
            }
        }

        [SecurityCritical]
        private static void DestroyProperties(IntPtr rgItems, ulong iItems)
        {
            if (rgItems != IntPtr.Zero)
            {
                ulong num = (ulong) Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationMetadataProperty));
                for (ulong i = 0L; i < iItems; i += (ulong) 1L)
                {
                    Marshal.DestroyStructure(new IntPtr(((long) (i * num)) + rgItems.ToInt64()), typeof(System.Deployment.Internal.Isolation.StoreOperationMetadataProperty));
                }
                Marshal.FreeCoTaskMem(rgItems);
            }
        }

        [SecurityCritical]
        private static IntPtr MarshalProperties(System.Deployment.Internal.Isolation.StoreOperationMetadataProperty[] Props)
        {
            if ((Props == null) || (Props.Length == 0))
            {
                return IntPtr.Zero;
            }
            int num = Marshal.SizeOf(typeof(System.Deployment.Internal.Isolation.StoreOperationMetadataProperty));
            IntPtr ptr = Marshal.AllocCoTaskMem(num * Props.Length);
            for (int i = 0; i != Props.Length; i++)
            {
                Marshal.StructureToPtr(Props[i], new IntPtr((i * num) + ptr.ToInt64()), false);
            }
            return ptr;
        }
        public enum Disposition
        {
            Failed = 0,
            Set = 2
        }

        [Flags]
        public enum OpFlags
        {
            Nothing
        }
    }
}

