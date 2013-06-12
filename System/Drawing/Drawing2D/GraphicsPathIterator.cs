namespace System.Drawing.Drawing2D
{
    using System;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    public sealed class GraphicsPathIterator : MarshalByRefObject, IDisposable
    {
        internal IntPtr nativeIter;

        public GraphicsPathIterator(GraphicsPath path)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreatePathIter(out zero, new HandleRef(path, (path == null) ? IntPtr.Zero : path.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.nativeIter = zero;
        }

        public int CopyData(ref PointF[] points, ref byte[] types, int startIndex, int endIndex)
        {
            if ((points.Length != types.Length) || (((endIndex - startIndex) + 1) > points.Length))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            int resultCount = 0;
            int num2 = Marshal.SizeOf(typeof(GPPOINTF));
            int length = points.Length;
            byte[] buffer = new byte[length];
            IntPtr memoryPts = Marshal.AllocHGlobal((int) (length * num2));
            try
            {
                int status = SafeNativeMethods.Gdip.GdipPathIterCopyData(new HandleRef(this, this.nativeIter), out resultCount, memoryPts, buffer, startIndex, endIndex);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                if (resultCount < length)
                {
                    SafeNativeMethods.ZeroMemory((IntPtr) (((long) memoryPts) + (resultCount * num2)), (UIntPtr) ((length - resultCount) * num2));
                }
                points = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(memoryPts, length);
                buffer.CopyTo(types, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(memoryPts);
            }
            return resultCount;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.nativeIter != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeletePathIter(new HandleRef(this, this.nativeIter));
                }
                catch (Exception exception)
                {
                    if (System.Drawing.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.nativeIter = IntPtr.Zero;
                }
            }
        }

        public int Enumerate(ref PointF[] points, ref byte[] types)
        {
            if (points.Length != types.Length)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            int resultCount = 0;
            int num2 = Marshal.SizeOf(typeof(GPPOINTF));
            int length = points.Length;
            byte[] buffer = new byte[length];
            IntPtr memoryPts = Marshal.AllocHGlobal((int) (length * num2));
            try
            {
                int status = SafeNativeMethods.Gdip.GdipPathIterEnumerate(new HandleRef(this, this.nativeIter), out resultCount, memoryPts, buffer, length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                if (resultCount < length)
                {
                    SafeNativeMethods.ZeroMemory((IntPtr) (((long) memoryPts) + (resultCount * num2)), (UIntPtr) ((length - resultCount) * num2));
                }
                points = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(memoryPts, length);
                buffer.CopyTo(types, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(memoryPts);
            }
            return resultCount;
        }

        ~GraphicsPathIterator()
        {
            this.Dispose(false);
        }

        public bool HasCurve()
        {
            bool hasCurve = false;
            int status = SafeNativeMethods.Gdip.GdipPathIterHasCurve(new HandleRef(this, this.nativeIter), out hasCurve);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return hasCurve;
        }

        public int NextMarker(GraphicsPath path)
        {
            int resultCount = 0;
            int status = SafeNativeMethods.Gdip.GdipPathIterNextMarkerPath(new HandleRef(this, this.nativeIter), out resultCount, new HandleRef(path, (path == null) ? IntPtr.Zero : path.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return resultCount;
        }

        public int NextMarker(out int startIndex, out int endIndex)
        {
            int resultCount = 0;
            int status = SafeNativeMethods.Gdip.GdipPathIterNextMarker(new HandleRef(this, this.nativeIter), out resultCount, out startIndex, out endIndex);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return resultCount;
        }

        public int NextPathType(out byte pathType, out int startIndex, out int endIndex)
        {
            int resultCount = 0;
            int status = SafeNativeMethods.Gdip.GdipPathIterNextPathType(new HandleRef(this, this.nativeIter), out resultCount, out pathType, out startIndex, out endIndex);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return resultCount;
        }

        public int NextSubpath(GraphicsPath path, out bool isClosed)
        {
            int resultCount = 0;
            int status = SafeNativeMethods.Gdip.GdipPathIterNextSubpathPath(new HandleRef(this, this.nativeIter), out resultCount, new HandleRef(path, (path == null) ? IntPtr.Zero : path.nativePath), out isClosed);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return resultCount;
        }

        public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed)
        {
            int resultCount = 0;
            int num2 = 0;
            int num3 = 0;
            int status = SafeNativeMethods.Gdip.GdipPathIterNextSubpath(new HandleRef(this, this.nativeIter), out resultCount, out num2, out num3, out isClosed);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            startIndex = num2;
            endIndex = num3;
            return resultCount;
        }

        public void Rewind()
        {
            int status = SafeNativeMethods.Gdip.GdipPathIterRewind(new HandleRef(this, this.nativeIter));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                int status = SafeNativeMethods.Gdip.GdipPathIterGetCount(new HandleRef(this, this.nativeIter), out count);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return count;
            }
        }

        public int SubpathCount
        {
            get
            {
                int count = 0;
                int status = SafeNativeMethods.Gdip.GdipPathIterGetSubpathCount(new HandleRef(this, this.nativeIter), out count);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return count;
            }
        }
    }
}

