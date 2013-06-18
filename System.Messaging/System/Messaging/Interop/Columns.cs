namespace System.Messaging.Interop
{
    using System;
    using System.Globalization;
    using System.Messaging;
    using System.Runtime.InteropServices;

    internal class Columns
    {
        private MQCOLUMNSET columnSet = new MQCOLUMNSET();
        private int maxCount;

        public Columns(int maxCount)
        {
            this.maxCount = maxCount;
            this.columnSet.columnIdentifiers = Marshal.AllocHGlobal((int) (maxCount * 4));
            this.columnSet.columnCount = 0;
        }

        public virtual void AddColumnId(int columnId)
        {
            lock (this)
            {
                if (this.columnSet.columnCount >= this.maxCount)
                {
                    throw new InvalidOperationException(Res.GetString("TooManyColumns", new object[] { this.maxCount.ToString(CultureInfo.CurrentCulture) }));
                }
                this.columnSet.columnCount++;
                this.columnSet.SetId(columnId, this.columnSet.columnCount - 1);
            }
        }

        public virtual MQCOLUMNSET GetColumnsRef()
        {
            return this.columnSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MQCOLUMNSET
        {
            public int columnCount;
            public IntPtr columnIdentifiers;
            ~MQCOLUMNSET()
            {
                if (this.columnIdentifiers != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.columnIdentifiers);
                    this.columnIdentifiers = IntPtr.Zero;
                }
            }

            public virtual void SetId(int columnId, int index)
            {
                Marshal.WriteInt32((IntPtr) (((long) this.columnIdentifiers) + (index * 4)), columnId);
            }
        }
    }
}

