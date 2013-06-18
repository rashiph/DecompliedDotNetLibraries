namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime.InteropServices;

    internal class ComThreadingInfo
    {
        private APTTYPE apartmentType;
        private Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private Guid logicalThreadId;
        private THDTYPE threadType;

        private ComThreadingInfo()
        {
            IComThreadingInfo info = (IComThreadingInfo) CoGetObjectContext(ref this.IID_IUnknown);
            this.apartmentType = info.GetCurrentApartmentType();
            this.threadType = info.GetCurrentThreadType();
            this.logicalThreadId = info.GetCurrentLogicalThreadId();
        }

        [return: MarshalAs(UnmanagedType.IUnknown)]
        [DllImport("ole32.dll", PreserveSig=false)]
        private static extern object CoGetObjectContext([In] ref Guid riid);
        public override string ToString()
        {
            return string.Format("{{{0}}} - {1} - {2}", this.LogicalThreadId, this.ApartmentType, this.ThreadType);
        }

        public APTTYPE ApartmentType
        {
            get
            {
                return this.apartmentType;
            }
        }

        public static ComThreadingInfo Current
        {
            get
            {
                return new ComThreadingInfo();
            }
        }

        public Guid LogicalThreadId
        {
            get
            {
                return this.logicalThreadId;
            }
        }

        public THDTYPE ThreadType
        {
            get
            {
                return this.threadType;
            }
        }

        public enum APTTYPE
        {
            APTTYPE_CURRENT = -1,
            APTTYPE_MAINSTA = 3,
            APTTYPE_MTA = 1,
            APTTYPE_NA = 2,
            APTTYPE_STA = 0
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000001ce-0000-0000-C000-000000000046")]
        private interface IComThreadingInfo
        {
            ComThreadingInfo.APTTYPE GetCurrentApartmentType();
            ComThreadingInfo.THDTYPE GetCurrentThreadType();
            Guid GetCurrentLogicalThreadId();
            void SetCurrentLogicalThreadId([In] Guid rguid);
        }

        public enum THDTYPE
        {
            THDTYPE_BLOCKMESSAGES,
            THDTYPE_PROCESSMESSAGES
        }
    }
}

