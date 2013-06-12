namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class PropertyItemInternal : IDisposable
    {
        public int id;
        public int len;
        public short type;
        public IntPtr value = IntPtr.Zero;
        internal PropertyItemInternal()
        {
        }

        ~PropertyItemInternal()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this.value != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.value);
                this.value = IntPtr.Zero;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        internal static PropertyItemInternal ConvertFromPropertyItem(PropertyItem propItem)
        {
            PropertyItemInternal internal2 = new PropertyItemInternal {
                id = propItem.Id,
                len = 0,
                type = propItem.Type
            };
            byte[] source = propItem.Value;
            if (source != null)
            {
                int length = source.Length;
                internal2.len = length;
                internal2.value = Marshal.AllocHGlobal(length);
                Marshal.Copy(source, 0, internal2.value, length);
            }
            return internal2;
        }

        internal static PropertyItem[] ConvertFromMemory(IntPtr propdata, int count)
        {
            PropertyItem[] itemArray = new PropertyItem[count];
            for (int i = 0; i < count; i++)
            {
                using (PropertyItemInternal internal2 = null)
                {
                    internal2 = (PropertyItemInternal) UnsafeNativeMethods.PtrToStructure(propdata, typeof(PropertyItemInternal));
                    itemArray[i] = new PropertyItem();
                    itemArray[i].Id = internal2.id;
                    itemArray[i].Len = internal2.len;
                    itemArray[i].Type = internal2.type;
                    itemArray[i].Value = internal2.Value;
                    internal2.value = IntPtr.Zero;
                }
                propdata = (IntPtr) (((long) propdata) + Marshal.SizeOf(typeof(PropertyItemInternal)));
            }
            return itemArray;
        }

        public byte[] Value
        {
            get
            {
                if (this.len == 0)
                {
                    return null;
                }
                byte[] destination = new byte[this.len];
                Marshal.Copy(this.value, destination, 0, this.len);
                return destination;
            }
        }
    }
}

