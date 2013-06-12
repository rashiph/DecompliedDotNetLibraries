namespace System.Drawing
{
    using System;
    using System.Internal;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity]
    internal class UnsafeNativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern void CopyMemory(HandleRef destData, HandleRef srcData, int size);
        public static IntPtr CreateCompatibleDC(HandleRef hDC)
        {
            return System.Internal.HandleCollector.Add(IntCreateCompatibleDC(hDC), SafeNativeMethods.CommonHandles.GDI);
        }

        public static bool DeleteDC(HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, SafeNativeMethods.CommonHandles.GDI);
            return IntDeleteDC(hDC);
        }

        public static IntPtr GetDC(HandleRef hWnd)
        {
            return System.Internal.HandleCollector.Add(IntGetDC(hWnd), SafeNativeMethods.CommonHandles.HDC);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetObjectType(HandleRef hObject);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr GetStockObject(int nIndex);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetSystemDefaultLCID();
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetSystemMetrics(int nIndex);
        [DllImport("gdi32.dll", EntryPoint="CreateCompatibleDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateCompatibleDC(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="DeleteDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern bool IntDeleteDC(HandleRef hDC);
        [DllImport("user32.dll", EntryPoint="GetDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntGetDC(HandleRef hWnd);
        [DllImport("user32.dll", EntryPoint="ReleaseDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        public static void PtrToStructure(IntPtr lparam, object data)
        {
            Marshal.PtrToStructure(lparam, data);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        public static object PtrToStructure(IntPtr lparam, Type cls)
        {
            return Marshal.PtrToStructure(lparam, cls);
        }

        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, SafeNativeMethods.CommonHandles.HDC);
            return IntReleaseDC(hWnd, hDC);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, [In, Out] System.Drawing.NativeMethods.NONCLIENTMETRICS pvParam, int fWinIni);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, [In, Out] SafeNativeMethods.LOGFONT pvParam, int fWinIni);

        internal class ComStreamFromDataStream : System.Drawing.UnsafeNativeMethods.IStream
        {
            protected Stream dataStream;
            private long virtualPosition = -1L;

            internal ComStreamFromDataStream(Stream dataStream)
            {
                if (dataStream == null)
                {
                    throw new ArgumentNullException("dataStream");
                }
                this.dataStream = dataStream;
            }

            private void ActualizeVirtualPosition()
            {
                if (this.virtualPosition != -1L)
                {
                    if (this.virtualPosition > this.dataStream.Length)
                    {
                        this.dataStream.SetLength(this.virtualPosition);
                    }
                    this.dataStream.Position = this.virtualPosition;
                    this.virtualPosition = -1L;
                }
            }

            public virtual System.Drawing.UnsafeNativeMethods.IStream Clone()
            {
                NotImplemented();
                return null;
            }

            public virtual void Commit(int grfCommitFlags)
            {
                this.dataStream.Flush();
                this.ActualizeVirtualPosition();
            }

            public virtual long CopyTo(System.Drawing.UnsafeNativeMethods.IStream pstm, long cb, long[] pcbRead)
            {
                int num = 0x1000;
                IntPtr buf = Marshal.AllocHGlobal(num);
                if (buf == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                long num2 = 0L;
                try
                {
                    while (num2 < cb)
                    {
                        int length = num;
                        if ((num2 + length) > cb)
                        {
                            length = (int) (cb - num2);
                        }
                        int len = this.Read(buf, length);
                        if (len == 0)
                        {
                            goto Label_006C;
                        }
                        if (pstm.Write(buf, len) != len)
                        {
                            throw EFail("Wrote an incorrect number of bytes");
                        }
                        num2 += len;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            Label_006C:
                if ((pcbRead != null) && (pcbRead.Length > 0))
                {
                    pcbRead[0] = num2;
                }
                return num2;
            }

            protected static ExternalException EFail(string msg)
            {
                throw new ExternalException(msg, -2147467259);
            }

            public virtual Stream GetDataStream()
            {
                return this.dataStream;
            }

            public virtual void LockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            protected static void NotImplemented()
            {
                throw new ExternalException(System.Drawing.SR.GetString("NotImplemented"), -2147467263);
            }

            public virtual int Read(IntPtr buf, int length)
            {
                byte[] buffer = new byte[length];
                int num = this.Read(buffer, length);
                Marshal.Copy(buffer, 0, buf, length);
                return num;
            }

            public virtual int Read(byte[] buffer, int length)
            {
                this.ActualizeVirtualPosition();
                return this.dataStream.Read(buffer, 0, length);
            }

            public virtual void Revert()
            {
                NotImplemented();
            }

            public virtual long Seek(long offset, int origin)
            {
                long virtualPosition = this.virtualPosition;
                if (this.virtualPosition == -1L)
                {
                    virtualPosition = this.dataStream.Position;
                }
                long length = this.dataStream.Length;
                switch (origin)
                {
                    case 0:
                        if (offset > length)
                        {
                            this.virtualPosition = offset;
                            break;
                        }
                        this.dataStream.Position = offset;
                        this.virtualPosition = -1L;
                        break;

                    case 1:
                        if ((offset + virtualPosition) > length)
                        {
                            this.virtualPosition = offset + virtualPosition;
                            break;
                        }
                        this.dataStream.Position = virtualPosition + offset;
                        this.virtualPosition = -1L;
                        break;

                    case 2:
                        if (offset > 0L)
                        {
                            this.virtualPosition = length + offset;
                            break;
                        }
                        this.dataStream.Position = length + offset;
                        this.virtualPosition = -1L;
                        break;
                }
                if (this.virtualPosition != -1L)
                {
                    return this.virtualPosition;
                }
                return this.dataStream.Position;
            }

            public virtual void SetSize(long value)
            {
                this.dataStream.SetLength(value);
            }

            public virtual void Stat(IntPtr pstatstg, int grfStatFlag)
            {
                NotImplemented();
            }

            public virtual void UnlockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public virtual int Write(IntPtr buf, int length)
            {
                byte[] destination = new byte[length];
                Marshal.Copy(buf, destination, 0, length);
                return this.Write(destination, length);
            }

            public virtual int Write(byte[] buffer, int length)
            {
                this.ActualizeVirtualPosition();
                this.dataStream.Write(buffer, 0, length);
                return length;
            }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000000C-0000-0000-C000-000000000046")]
        public interface IStream
        {
            int Read([In] IntPtr buf, [In] int len);
            int Write([In] IntPtr buf, [In] int len);
            [return: MarshalAs(UnmanagedType.I8)]
            long Seek([In, MarshalAs(UnmanagedType.I8)] long dlibMove, [In] int dwOrigin);
            void SetSize([In, MarshalAs(UnmanagedType.I8)] long libNewSize);
            [return: MarshalAs(UnmanagedType.I8)]
            long CopyTo([In, MarshalAs(UnmanagedType.Interface)] System.Drawing.UnsafeNativeMethods.IStream pstm, [In, MarshalAs(UnmanagedType.I8)] long cb, [Out, MarshalAs(UnmanagedType.LPArray)] long[] pcbRead);
            void Commit([In] int grfCommitFlags);
            void Revert();
            void LockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);
            void UnlockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);
            void Stat([In] IntPtr pStatstg, [In] int grfStatFlag);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Drawing.UnsafeNativeMethods.IStream Clone();
        }
    }
}

