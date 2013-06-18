namespace System.Management
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal sealed class IWbemQualifierSetFreeThreaded : IDisposable
    {
        public static Guid IID_IWbemClassObject = new Guid("DC12A681-737F-11CF-884D-00AA004B2E24");
        private static readonly string name = typeof(IWbemQualifierSetFreeThreaded).FullName;
        private IntPtr pWbemQualifierSet = IntPtr.Zero;
        private const string SerializationBlobName = "flatWbemClassObject";

        public IWbemQualifierSetFreeThreaded(IntPtr pWbemQualifierSet)
        {
            this.pWbemQualifierSet = pWbemQualifierSet;
        }

        public int BeginEnumeration_(int lFlags)
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierBeginEnumeration_f(7, this.pWbemQualifierSet, lFlags);
            GC.KeepAlive(this);
            return num;
        }

        public int Delete_(string wszName)
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierDelete_f(5, this.pWbemQualifierSet, wszName);
            GC.KeepAlive(this);
            return num;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Dispose()
        {
            this.Dispose_(false);
        }

        private void Dispose_(bool finalization)
        {
            if (this.pWbemQualifierSet != IntPtr.Zero)
            {
                Marshal.Release(this.pWbemQualifierSet);
                this.pWbemQualifierSet = IntPtr.Zero;
            }
            if (!finalization)
            {
                GC.KeepAlive(this);
            }
            GC.SuppressFinalize(this);
        }

        public int EndEnumeration_()
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierEndEnumeration_f(9, this.pWbemQualifierSet);
            GC.KeepAlive(this);
            return num;
        }

        ~IWbemQualifierSetFreeThreaded()
        {
            this.Dispose_(true);
        }

        public int Get_(string wszName, int lFlags, ref object pVal, ref int plFlavor)
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierGet_f(3, this.pWbemQualifierSet, wszName, lFlags, ref pVal, ref plFlavor);
            GC.KeepAlive(this);
            return num;
        }

        public int GetNames_(int lFlags, out string[] pNames)
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierGetNames_f(6, this.pWbemQualifierSet, lFlags, out pNames);
            GC.KeepAlive(this);
            return num;
        }

        public int Next_(int lFlags, out string pstrName, out object pVal, out int plFlavor)
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierNext_f(8, this.pWbemQualifierSet, lFlags, out pstrName, out pVal, out plFlavor);
            GC.KeepAlive(this);
            return num;
        }

        public int Put_(string wszName, ref object pVal, int lFlavor)
        {
            if (this.pWbemQualifierSet == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.QualifierPut_f(4, this.pWbemQualifierSet, wszName, ref pVal, lFlavor);
            GC.KeepAlive(this);
            return num;
        }
    }
}

