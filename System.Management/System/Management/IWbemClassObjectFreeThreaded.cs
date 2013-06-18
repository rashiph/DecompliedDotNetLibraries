namespace System.Management
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class IWbemClassObjectFreeThreaded : IDisposable, ISerializable
    {
        public static Guid IID_IWbemClassObject = new Guid("DC12A681-737F-11CF-884D-00AA004B2E24");
        private static readonly string name = typeof(IWbemClassObjectFreeThreaded).FullName;
        private IntPtr pWbemClassObject;
        private const string SerializationBlobName = "flatWbemClassObject";

        public IWbemClassObjectFreeThreaded(IntPtr pWbemClassObject)
        {
            this.pWbemClassObject = IntPtr.Zero;
            this.pWbemClassObject = pWbemClassObject;
        }

        public IWbemClassObjectFreeThreaded(SerializationInfo info, StreamingContext context)
        {
            this.pWbemClassObject = IntPtr.Zero;
            byte[] rg = info.GetValue("flatWbemClassObject", typeof(byte[])) as byte[];
            if (rg == null)
            {
                throw new SerializationException();
            }
            this.DeserializeFromBlob(rg);
        }

        public int BeginEnumeration_(int lEnumFlags)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.BeginEnumeration_f(8, this.pWbemClassObject, lEnumFlags);
            GC.KeepAlive(this);
            return num;
        }

        public int BeginMethodEnumeration_(int lEnumFlags)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.BeginMethodEnumeration_f(0x16, this.pWbemClassObject, lEnumFlags);
            GC.KeepAlive(this);
            return num;
        }

        public int Clone_(out IWbemClassObjectFreeThreaded ppCopy)
        {
            IntPtr ptr;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.Clone_f(12, this.pWbemClassObject, out ptr);
            if (num < 0)
            {
                ppCopy = null;
            }
            else
            {
                ppCopy = new IWbemClassObjectFreeThreaded(ptr);
            }
            GC.KeepAlive(this);
            return num;
        }

        [DllImport("ole32.dll", PreserveSig=false)]
        private static extern void CoMarshalInterface([In] IStream pStm, [In] ref Guid riid, [In] IntPtr Unk, [In] uint dwDestContext, [In] IntPtr pvDestContext, [In] uint mshlflags);
        public int CompareTo_(int lFlags, IWbemClassObjectFreeThreaded pCompareTo)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.CompareTo_f(0x10, this.pWbemClassObject, lFlags, pCompareTo.pWbemClassObject);
            GC.KeepAlive(this);
            return num;
        }

        [DllImport("ole32.dll", PreserveSig=false)]
        private static extern IntPtr CoUnmarshalInterface([In] IStream pStm, [In] ref Guid riid);
        [DllImport("ole32.dll", PreserveSig=false)]
        private static extern IStream CreateStreamOnHGlobal(IntPtr hGlobal, int fDeleteOnRelease);
        public int Delete_(string wszName)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.Delete_f(6, this.pWbemClassObject, wszName);
            GC.KeepAlive(this);
            return num;
        }

        public int DeleteMethod_(string wszName)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.DeleteMethod_f(0x15, this.pWbemClassObject, wszName);
            GC.KeepAlive(this);
            return num;
        }

        private void DeserializeFromBlob(byte[] rg)
        {
            IntPtr zero = IntPtr.Zero;
            IStream pStm = null;
            try
            {
                this.pWbemClassObject = IntPtr.Zero;
                zero = Marshal.AllocHGlobal(rg.Length);
                Marshal.Copy(rg, 0, zero, rg.Length);
                pStm = CreateStreamOnHGlobal(zero, 0);
                this.pWbemClassObject = CoUnmarshalInterface(pStm, ref IID_IWbemClassObject);
            }
            finally
            {
                if (pStm != null)
                {
                    Marshal.ReleaseComObject(pStm);
                }
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Dispose()
        {
            this.Dispose_(false);
        }

        private void Dispose_(bool finalization)
        {
            if (this.pWbemClassObject != IntPtr.Zero)
            {
                Marshal.Release(this.pWbemClassObject);
                this.pWbemClassObject = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        public int EndEnumeration_()
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.EndEnumeration_f(10, this.pWbemClassObject);
            GC.KeepAlive(this);
            return num;
        }

        public int EndMethodEnumeration_()
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.EndMethodEnumeration_f(0x18, this.pWbemClassObject);
            GC.KeepAlive(this);
            return num;
        }

        ~IWbemClassObjectFreeThreaded()
        {
            this.Dispose_(true);
        }

        public int Get_(string wszName, int lFlags, ref object pVal, ref int pType, ref int plFlavor)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.Get_f(4, this.pWbemClassObject, wszName, lFlags, ref pVal, ref pType, ref plFlavor);
            if ((num == -2147217393) && (string.Compare(wszName, "__path", StringComparison.OrdinalIgnoreCase) == 0))
            {
                num = 0;
                pType = 8;
                plFlavor = 0x40;
                pVal = DBNull.Value;
            }
            GC.KeepAlive(this);
            return num;
        }

        [DllImport("ole32.dll", PreserveSig=false)]
        private static extern IntPtr GetHGlobalFromStream([In] IStream pstm);
        public int GetMethod_(string wszName, int lFlags, out IWbemClassObjectFreeThreaded ppInSignature, out IWbemClassObjectFreeThreaded ppOutSignature)
        {
            IntPtr ptr;
            IntPtr ptr2;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetMethod_f(0x13, this.pWbemClassObject, wszName, lFlags, out ptr, out ptr2);
            ppInSignature = null;
            ppOutSignature = null;
            if (num >= 0)
            {
                if (ptr != IntPtr.Zero)
                {
                    ppInSignature = new IWbemClassObjectFreeThreaded(ptr);
                }
                if (ptr2 != IntPtr.Zero)
                {
                    ppOutSignature = new IWbemClassObjectFreeThreaded(ptr2);
                }
            }
            GC.KeepAlive(this);
            return num;
        }

        public int GetMethodOrigin_(string wszMethodName, out string pstrClassName)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetMethodOrigin_f(0x1a, this.pWbemClassObject, wszMethodName, out pstrClassName);
            GC.KeepAlive(this);
            return num;
        }

        public int GetMethodQualifierSet_(string wszMethod, out IWbemQualifierSetFreeThreaded ppQualSet)
        {
            IntPtr ptr;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetMethodQualifierSet_f(0x19, this.pWbemClassObject, wszMethod, out ptr);
            if (num < 0)
            {
                ppQualSet = null;
            }
            else
            {
                ppQualSet = new IWbemQualifierSetFreeThreaded(ptr);
            }
            GC.KeepAlive(this);
            return num;
        }

        public int GetNames_(string wszQualifierName, int lFlags, ref object pQualifierVal, out string[] pNames)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetNames_f(7, this.pWbemClassObject, wszQualifierName, lFlags, ref pQualifierVal, out pNames);
            GC.KeepAlive(this);
            return num;
        }

        public int GetObjectText_(int lFlags, out string pstrObjectText)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetObjectText_f(13, this.pWbemClassObject, lFlags, out pstrObjectText);
            GC.KeepAlive(this);
            return num;
        }

        public int GetPropertyOrigin_(string wszName, out string pstrClassName)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetPropertyOrigin_f(0x11, this.pWbemClassObject, wszName, out pstrClassName);
            GC.KeepAlive(this);
            return num;
        }

        public int GetPropertyQualifierSet_(string wszProperty, out IWbemQualifierSetFreeThreaded ppQualSet)
        {
            IntPtr ptr;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetPropertyQualifierSet_f(11, this.pWbemClassObject, wszProperty, out ptr);
            if (num < 0)
            {
                ppQualSet = null;
            }
            else
            {
                ppQualSet = new IWbemQualifierSetFreeThreaded(ptr);
            }
            GC.KeepAlive(this);
            return num;
        }

        public int GetQualifierSet_(out IWbemQualifierSetFreeThreaded ppQualSet)
        {
            IntPtr ptr;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.GetQualifierSet_f(3, this.pWbemClassObject, out ptr);
            if (num < 0)
            {
                ppQualSet = null;
            }
            else
            {
                ppQualSet = new IWbemQualifierSetFreeThreaded(ptr);
            }
            GC.KeepAlive(this);
            return num;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock([In] IntPtr hGlobal);
        [DllImport("kernel32.dll")]
        private static extern int GlobalUnlock([In] IntPtr pData);
        public int InheritsFrom_(string strAncestor)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.InheritsFrom_f(0x12, this.pWbemClassObject, strAncestor);
            GC.KeepAlive(this);
            return num;
        }

        public int Next_(int lFlags, ref string strName, ref object pVal, ref int pType, ref int plFlavor)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            pVal = null;
            strName = null;
            int num = WmiNetUtilsHelper.Next_f(9, this.pWbemClassObject, lFlags, ref strName, ref pVal, ref pType, ref plFlavor);
            GC.KeepAlive(this);
            return num;
        }

        public int NextMethod_(int lFlags, out string pstrName, out IWbemClassObjectFreeThreaded ppInSignature, out IWbemClassObjectFreeThreaded ppOutSignature)
        {
            IntPtr ptr;
            IntPtr ptr2;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.NextMethod_f(0x17, this.pWbemClassObject, lFlags, out pstrName, out ptr, out ptr2);
            ppInSignature = null;
            ppOutSignature = null;
            if (num >= 0)
            {
                if (ptr != IntPtr.Zero)
                {
                    ppInSignature = new IWbemClassObjectFreeThreaded(ptr);
                }
                if (ptr2 != IntPtr.Zero)
                {
                    ppOutSignature = new IWbemClassObjectFreeThreaded(ptr2);
                }
            }
            GC.KeepAlive(this);
            return num;
        }

        public static implicit operator IntPtr(IWbemClassObjectFreeThreaded wbemClassObject)
        {
            if (wbemClassObject == null)
            {
                return IntPtr.Zero;
            }
            return wbemClassObject.pWbemClassObject;
        }

        public int Put_(string wszName, int lFlags, ref object pVal, int Type)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.Put_f(5, this.pWbemClassObject, wszName, lFlags, ref pVal, Type);
            GC.KeepAlive(this);
            return num;
        }

        public int PutMethod_(string wszName, int lFlags, IWbemClassObjectFreeThreaded pInSignature, IWbemClassObjectFreeThreaded pOutSignature)
        {
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.PutMethod_f(20, this.pWbemClassObject, wszName, lFlags, (IntPtr) pInSignature, (IntPtr) pOutSignature);
            GC.KeepAlive(this);
            return num;
        }

        private byte[] SerializeToBlob()
        {
            byte[] destination = null;
            IStream pStm = null;
            IntPtr zero = IntPtr.Zero;
            try
            {
                System.Runtime.InteropServices.ComTypes.STATSTG statstg;
                pStm = CreateStreamOnHGlobal(IntPtr.Zero, 1);
                CoMarshalInterface(pStm, ref IID_IWbemClassObject, this.pWbemClassObject, 2, IntPtr.Zero, 2);
                pStm.Stat(out statstg, 0);
                destination = new byte[statstg.cbSize];
                zero = GlobalLock(GetHGlobalFromStream(pStm));
                Marshal.Copy(zero, destination, 0, (int) statstg.cbSize);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    GlobalUnlock(zero);
                }
                if (pStm != null)
                {
                    Marshal.ReleaseComObject(pStm);
                }
            }
            GC.KeepAlive(this);
            return destination;
        }

        public int SpawnDerivedClass_(int lFlags, out IWbemClassObjectFreeThreaded ppNewClass)
        {
            IntPtr ptr;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.SpawnDerivedClass_f(14, this.pWbemClassObject, lFlags, out ptr);
            if (num < 0)
            {
                ppNewClass = null;
            }
            else
            {
                ppNewClass = new IWbemClassObjectFreeThreaded(ptr);
            }
            GC.KeepAlive(this);
            return num;
        }

        public int SpawnInstance_(int lFlags, out IWbemClassObjectFreeThreaded ppNewInstance)
        {
            IntPtr ptr;
            if (this.pWbemClassObject == IntPtr.Zero)
            {
                throw new ObjectDisposedException(name);
            }
            int num = WmiNetUtilsHelper.SpawnInstance_f(15, this.pWbemClassObject, lFlags, out ptr);
            if (num < 0)
            {
                ppNewInstance = null;
            }
            else
            {
                ppNewInstance = new IWbemClassObjectFreeThreaded(ptr);
            }
            GC.KeepAlive(this);
            return num;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("flatWbemClassObject", this.SerializeToBlob());
        }

        private enum MSHCTX
        {
            MSHCTX_LOCAL,
            MSHCTX_NOSHAREDMEM,
            MSHCTX_DIFFERENTMACHINE,
            MSHCTX_INPROC
        }

        private enum MSHLFLAGS
        {
            MSHLFLAGS_NORMAL,
            MSHLFLAGS_TABLESTRONG,
            MSHLFLAGS_TABLEWEAK,
            MSHLFLAGS_NOPING
        }

        private enum STATFLAG
        {
            STATFLAG_DEFAULT,
            STATFLAG_NONAME
        }
    }
}

