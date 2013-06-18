namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class ColorPalette
    {
        private Color[] entries;
        private int flags;

        internal ColorPalette()
        {
            this.entries = new Color[1];
        }

        internal ColorPalette(int count)
        {
            this.entries = new Color[count];
        }

        internal void ConvertFromMemory(IntPtr memory)
        {
            this.flags = Marshal.ReadInt32(memory);
            int num = Marshal.ReadInt32((IntPtr) (((long) memory) + 4L));
            this.entries = new Color[num];
            for (int i = 0; i < num; i++)
            {
                int argb = Marshal.ReadInt32((IntPtr) ((((long) memory) + 8L) + (i * 4)));
                this.entries[i] = Color.FromArgb(argb);
            }
        }

        internal IntPtr ConvertToMemory()
        {
            int length = this.entries.Length;
            IntPtr ptr = Marshal.AllocHGlobal((int) (4 * (2 + length)));
            Marshal.WriteInt32(ptr, 0, this.flags);
            Marshal.WriteInt32((IntPtr) (((long) ptr) + 4L), 0, length);
            for (int i = 0; i < length; i++)
            {
                Marshal.WriteInt32((IntPtr) (((long) ptr) + (4 * (i + 2))), 0, this.entries[i].ToArgb());
            }
            return ptr;
        }

        public Color[] Entries
        {
            get
            {
                return this.entries;
            }
        }

        public int Flags
        {
            get
            {
                return this.flags;
            }
        }
    }
}

