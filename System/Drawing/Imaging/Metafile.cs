namespace System.Drawing.Imaging
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Internal;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, Editor("System.Drawing.Design.MetafileEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class Metafile : Image
    {
        private Metafile()
        {
        }

        public Metafile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "stream", "null" }));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateMetafileFromStream(new GPStream(stream), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(string filename)
        {
            System.Drawing.IntSecurity.DemandReadFileIO(filename);
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateMetafileFromFile(filename, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(IntPtr henhmetafile, bool deleteEmf)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateMetafileFromEmf(new HandleRef(null, henhmetafile), deleteEmf, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(IntPtr referenceHdc, EmfType emfType) : this(referenceHdc, emfType, null)
        {
        }

        public Metafile(IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader) : this(hmetafile, wmfHeader, false)
        {
        }

        public Metafile(IntPtr referenceHdc, Rectangle frameRect) : this(referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible)
        {
        }

        public Metafile(IntPtr referenceHdc, RectangleF frameRect) : this(referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc) : this(stream, referenceHdc, EmfType.EmfPlusDual, (string) null)
        {
        }

        private Metafile(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc) : this(fileName, referenceHdc, EmfType.EmfPlusDual, (string) null)
        {
        }

        public Metafile(IntPtr referenceHdc, EmfType emfType, string description)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipRecordMetafile(new HandleRef(null, referenceHdc), (int) emfType, System.Drawing.NativeMethods.NullHandleRef, 7, description, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader, bool deleteWmf)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateMetafileFromWmf(new HandleRef(null, hmetafile), wmfHeader, deleteWmf, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) : this(referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual)
        {
        }

        public Metafile(IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) : this(referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, EmfType type) : this(stream, referenceHdc, type, (string) null)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect) : this(stream, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect) : this(stream, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, EmfType type) : this(fileName, referenceHdc, type, (string) null)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect) : this(fileName, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect) : this(fileName, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible)
        {
        }

        public Metafile(IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) : this(referenceHdc, frameRect, frameUnit, type, null)
        {
        }

        public Metafile(IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) : this(referenceHdc, frameRect, frameUnit, type, null)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, EmfType type, string description)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipRecordMetafileStream(new GPStream(stream), new HandleRef(null, referenceHdc), (int) type, System.Drawing.NativeMethods.NullHandleRef, 7, description, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) : this(stream, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) : this(stream, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, EmfType type, string description)
        {
            System.Drawing.IntSecurity.DemandReadFileIO(fileName);
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipRecordMetafileFileName(fileName, new HandleRef(null, referenceHdc), (int) type, System.Drawing.NativeMethods.NullHandleRef, 7, description, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) : this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) : this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual)
        {
        }

        public Metafile(IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string desc)
        {
            int num;
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            if (frameRect.IsEmpty)
            {
                num = SafeNativeMethods.Gdip.GdipRecordMetafile(new HandleRef(null, referenceHdc), (int) type, System.Drawing.NativeMethods.NullHandleRef, 7, desc, out zero);
            }
            else
            {
                GPRECT gprect = new GPRECT(frameRect);
                num = SafeNativeMethods.Gdip.GdipRecordMetafileI(new HandleRef(null, referenceHdc), (int) type, ref gprect, (int) frameUnit, desc, out zero);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            GPRECTF gprectf = new GPRECTF(frameRect);
            int status = SafeNativeMethods.Gdip.GdipRecordMetafile(new HandleRef(null, referenceHdc), (int) type, ref gprectf, (int) frameUnit, description, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) : this(stream, referenceHdc, frameRect, frameUnit, type, null)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) : this(stream, referenceHdc, frameRect, frameUnit, type, null)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) : this(fileName, referenceHdc, frameRect, frameUnit, type, null)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, string description) : this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, description)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) : this(fileName, referenceHdc, frameRect, frameUnit, type, null)
        {
        }

        public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, string desc) : this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, desc)
        {
        }

        public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
        {
            int num;
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            if (frameRect.IsEmpty)
            {
                num = SafeNativeMethods.Gdip.GdipRecordMetafileStream(new GPStream(stream), new HandleRef(null, referenceHdc), (int) type, System.Drawing.NativeMethods.NullHandleRef, (int) frameUnit, description, out zero);
            }
            else
            {
                GPRECT gprect = new GPRECT(frameRect);
                num = SafeNativeMethods.Gdip.GdipRecordMetafileStreamI(new GPStream(stream), new HandleRef(null, referenceHdc), (int) type, ref gprect, (int) frameUnit, description, out zero);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            GPRECTF gprectf = new GPRECTF(frameRect);
            int status = SafeNativeMethods.Gdip.GdipRecordMetafileStream(new GPStream(stream), new HandleRef(null, referenceHdc), (int) type, ref gprectf, (int) frameUnit, description, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
        {
            int num;
            System.Drawing.IntSecurity.DemandReadFileIO(fileName);
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            if (frameRect.IsEmpty)
            {
                num = SafeNativeMethods.Gdip.GdipRecordMetafileFileName(fileName, new HandleRef(null, referenceHdc), (int) type, System.Drawing.NativeMethods.NullHandleRef, (int) frameUnit, description, out zero);
            }
            else
            {
                GPRECT gprect = new GPRECT(frameRect);
                num = SafeNativeMethods.Gdip.GdipRecordMetafileFileNameI(fileName, new HandleRef(null, referenceHdc), (int) type, ref gprect, (int) frameUnit, description, out zero);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            base.SetNativeImage(zero);
        }

        public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
        {
            System.Drawing.IntSecurity.DemandReadFileIO(fileName);
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            GPRECTF gprectf = new GPRECTF(frameRect);
            int status = SafeNativeMethods.Gdip.GdipRecordMetafileFileName(fileName, new HandleRef(null, referenceHdc), (int) type, ref gprectf, (int) frameUnit, description, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeImage(zero);
        }

        internal static Metafile FromGDIplus(IntPtr nativeImage)
        {
            Metafile metafile = new Metafile();
            metafile.SetNativeImage(nativeImage);
            return metafile;
        }

        public IntPtr GetHenhmetafile()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipGetHemfFromMetafile(new HandleRef(this, base.nativeImage), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return zero;
        }

        public MetafileHeader GetMetafileHeader()
        {
            MetafileHeader header;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeaderEmf)));
            try
            {
                int status = SafeNativeMethods.Gdip.GdipGetMetafileHeaderFromMetafile(new HandleRef(this, base.nativeImage), ptr);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                int[] destination = new int[1];
                Marshal.Copy(ptr, destination, 0, 1);
                MetafileType type = (MetafileType) destination[0];
                header = new MetafileHeader();
                switch (type)
                {
                    case MetafileType.Wmf:
                    case MetafileType.WmfPlaceable:
                        header.wmf = (MetafileHeaderWmf) UnsafeNativeMethods.PtrToStructure(ptr, typeof(MetafileHeaderWmf));
                        header.emf = null;
                        return header;
                }
                header.wmf = null;
                header.emf = (MetafileHeaderEmf) UnsafeNativeMethods.PtrToStructure(ptr, typeof(MetafileHeaderEmf));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return header;
        }

        public static MetafileHeader GetMetafileHeader(IntPtr henhmetafile)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            MetafileHeader header = new MetafileHeader {
                emf = new MetafileHeaderEmf()
            };
            int status = SafeNativeMethods.Gdip.GdipGetMetafileHeaderFromEmf(new HandleRef(null, henhmetafile), header.emf);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return header;
        }

        public static MetafileHeader GetMetafileHeader(Stream stream)
        {
            MetafileHeader header;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeaderEmf)));
            try
            {
                int status = SafeNativeMethods.Gdip.GdipGetMetafileHeaderFromStream(new GPStream(stream), ptr);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                int[] destination = new int[1];
                Marshal.Copy(ptr, destination, 0, 1);
                MetafileType type = (MetafileType) destination[0];
                header = new MetafileHeader();
                switch (type)
                {
                    case MetafileType.Wmf:
                    case MetafileType.WmfPlaceable:
                        header.wmf = (MetafileHeaderWmf) UnsafeNativeMethods.PtrToStructure(ptr, typeof(MetafileHeaderWmf));
                        header.emf = null;
                        return header;
                }
                header.wmf = null;
                header.emf = (MetafileHeaderEmf) UnsafeNativeMethods.PtrToStructure(ptr, typeof(MetafileHeaderEmf));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return header;
        }

        public static MetafileHeader GetMetafileHeader(string fileName)
        {
            System.Drawing.IntSecurity.DemandReadFileIO(fileName);
            MetafileHeader header = new MetafileHeader();
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeaderEmf)));
            try
            {
                int status = SafeNativeMethods.Gdip.GdipGetMetafileHeaderFromFile(fileName, ptr);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                int[] destination = new int[1];
                Marshal.Copy(ptr, destination, 0, 1);
                switch (((MetafileType) destination[0]))
                {
                    case MetafileType.Wmf:
                    case MetafileType.WmfPlaceable:
                        header.wmf = (MetafileHeaderWmf) UnsafeNativeMethods.PtrToStructure(ptr, typeof(MetafileHeaderWmf));
                        header.emf = null;
                        return header;
                }
                header.wmf = null;
                header.emf = (MetafileHeaderEmf) UnsafeNativeMethods.PtrToStructure(ptr, typeof(MetafileHeaderEmf));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return header;
        }

        public static MetafileHeader GetMetafileHeader(IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            MetafileHeader header = new MetafileHeader {
                wmf = new MetafileHeaderWmf()
            };
            int status = SafeNativeMethods.Gdip.GdipGetMetafileHeaderFromWmf(new HandleRef(null, hmetafile), wmfHeader, header.wmf);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return header;
        }

        public void PlayRecord(EmfPlusRecordType recordType, int flags, int dataSize, byte[] data)
        {
            int status = SafeNativeMethods.Gdip.GdipPlayMetafileRecord(new HandleRef(this, base.nativeImage), recordType, flags, dataSize, data);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }
    }
}

