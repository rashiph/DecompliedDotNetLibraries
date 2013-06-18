namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Permissions;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ProjectData
    {
        internal Hashtable m_AssemblyData = new Hashtable();
        private Assembly m_CachedMSCoreLibAssembly = typeof(int).Assembly;
        internal byte[] m_DigitArray = new byte[30];
        internal ErrObject m_Err;
        internal byte[] m_numprsPtr = new byte[0x18];
        [ThreadStatic]
        private static ProjectData m_oProject;
        internal int m_rndSeed = 0x50000;

        private ProjectData()
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public static void ClearProjectError()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                Information.Err().Clear();
            }
        }

        public static Exception CreateProjectError(int hr)
        {
            ErrObject obj2 = Information.Err();
            obj2.Clear();
            int num = obj2.MapErrorNumber(hr);
            return obj2.CreateException(hr, Utils.GetResourceString((vbErrors) num));
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.SelfAffectingProcessMgmt)]
        public static void EndApp()
        {
            FileSystem.CloseAllFiles(Assembly.GetCallingAssembly());
            Environment.Exit(0);
        }

        internal AssemblyData GetAssemblyData(Assembly assem)
        {
            if ((assem == Utils.VBRuntimeAssembly) || (assem == this.m_CachedMSCoreLibAssembly))
            {
                throw new SecurityException(Utils.GetResourceString("Security_LateBoundCallsNotPermitted"));
            }
            AssemblyData data = (AssemblyData) this.m_AssemblyData[assem];
            if (data == null)
            {
                data = new AssemblyData();
                this.m_AssemblyData[assem] = data;
            }
            return data;
        }

        internal static ProjectData GetProjectData()
        {
            ProjectData oProject = m_oProject;
            if (oProject == null)
            {
                oProject = new ProjectData();
                m_oProject = oProject;
            }
            return oProject;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public static void SetProjectError(Exception ex)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                Information.Err().CaptureException(ex);
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void SetProjectError(Exception ex, int lErl)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                Information.Err().CaptureException(ex, lErl);
            }
        }
    }
}

