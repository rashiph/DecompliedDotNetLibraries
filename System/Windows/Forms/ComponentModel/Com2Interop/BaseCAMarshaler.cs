namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal abstract class BaseCAMarshaler
    {
        private IntPtr caArrayAddress;
        private static TraceSwitch CAMarshalSwitch = new TraceSwitch("CAMarshal", "BaseCAMarshaler: Debug CA* struct marshaling");
        private int count;
        private object[] itemArray;

        protected BaseCAMarshaler(System.Windows.Forms.NativeMethods.CA_STRUCT caStruct)
        {
            if (caStruct == null)
            {
                this.count = 0;
            }
            this.count = caStruct.cElems;
            this.caArrayAddress = caStruct.pElems;
        }

        protected abstract Array CreateArray();
        ~BaseCAMarshaler()
        {
            try
            {
                if ((this.itemArray == null) && (this.caArrayAddress != IntPtr.Zero))
                {
                    object[] items = this.Items;
                }
            }
            catch
            {
            }
        }

        private object[] Get_Items()
        {
            Array array = new object[this.Count];
            for (int i = 0; i < this.count; i++)
            {
                try
                {
                    IntPtr addr = Marshal.ReadIntPtr(this.caArrayAddress, i * IntPtr.Size);
                    object itemFromAddress = this.GetItemFromAddress(addr);
                    if ((itemFromAddress != null) && this.ItemType.IsInstanceOfType(itemFromAddress))
                    {
                        array.SetValue(itemFromAddress, i);
                    }
                }
                catch (Exception)
                {
                }
            }
            Marshal.FreeCoTaskMem(this.caArrayAddress);
            this.caArrayAddress = IntPtr.Zero;
            return (object[]) array;
        }

        protected abstract object GetItemFromAddress(IntPtr addr);

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public virtual object[] Items
        {
            get
            {
                try
                {
                    if (this.itemArray == null)
                    {
                        this.itemArray = this.Get_Items();
                    }
                }
                catch (Exception)
                {
                }
                return this.itemArray;
            }
        }

        public abstract System.Type ItemType { get; }
    }
}

