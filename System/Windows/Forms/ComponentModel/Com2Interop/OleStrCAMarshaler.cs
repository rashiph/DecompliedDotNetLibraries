namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class OleStrCAMarshaler : BaseCAMarshaler
    {
        public OleStrCAMarshaler(System.Windows.Forms.NativeMethods.CA_STRUCT caAddr) : base(caAddr)
        {
        }

        protected override Array CreateArray()
        {
            return new string[base.Count];
        }

        protected override object GetItemFromAddress(IntPtr addr)
        {
            string str = Marshal.PtrToStringUni(addr);
            Marshal.FreeCoTaskMem(addr);
            return str;
        }

        public override System.Type ItemType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}

