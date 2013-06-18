namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;

    public sealed class EncoderParameters : IDisposable
    {
        private EncoderParameter[] param;

        public EncoderParameters()
        {
            this.param = new EncoderParameter[1];
        }

        public EncoderParameters(int count)
        {
            this.param = new EncoderParameter[count];
        }

        internal static EncoderParameters ConvertFromMemory(IntPtr memory)
        {
            if (memory == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            int count = Marshal.ReadIntPtr(memory).ToInt32();
            EncoderParameters parameters = new EncoderParameters(count);
            int num2 = Marshal.SizeOf(typeof(EncoderParameter));
            long num3 = ((long) memory) + Marshal.SizeOf(typeof(IntPtr));
            IntSecurity.UnmanagedCode.Assert();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    Guid guid = (Guid) UnsafeNativeMethods.PtrToStructure((IntPtr) ((i * num2) + num3), typeof(Guid));
                    int numberOfValues = Marshal.ReadInt32((IntPtr) (((i * num2) + num3) + 0x10L));
                    int type = Marshal.ReadInt32((IntPtr) (((i * num2) + num3) + 20L));
                    int num7 = Marshal.ReadInt32((IntPtr) (((i * num2) + num3) + 0x18L));
                    parameters.param[i] = new EncoderParameter(new Encoder(guid), numberOfValues, type, num7);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return parameters;
        }

        internal IntPtr ConvertToMemory()
        {
            int num = Marshal.SizeOf(typeof(EncoderParameter));
            int length = this.param.Length;
            IntPtr ptr = Marshal.AllocHGlobal((int) ((length * num) + Marshal.SizeOf(typeof(IntPtr))));
            if (ptr == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteIntPtr(ptr, (IntPtr) length);
            long num3 = ((long) ptr) + Marshal.SizeOf(typeof(IntPtr));
            for (int i = 0; i < length; i++)
            {
                Marshal.StructureToPtr(this.param[i], (IntPtr) (num3 + (i * num)), false);
            }
            return ptr;
        }

        public void Dispose()
        {
            foreach (EncoderParameter parameter in this.param)
            {
                if (parameter != null)
                {
                    parameter.Dispose();
                }
            }
            this.param = null;
        }

        public EncoderParameter[] Param
        {
            get
            {
                return this.param;
            }
            set
            {
                this.param = value;
            }
        }
    }
}

