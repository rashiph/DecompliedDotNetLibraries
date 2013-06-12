namespace System.Drawing
{
    using System;
    using System.Drawing.Drawing2D;
    using System.Drawing.Internal;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public sealed class Region : MarshalByRefObject, IDisposable
    {
        internal IntPtr nativeRegion;

        public Region()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateRegion(out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeRegion(zero);
        }

        public Region(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateRegionPath(new HandleRef(path, path.nativePath), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeRegion(zero);
        }

        public Region(RegionData rgnData)
        {
            if (rgnData == null)
            {
                throw new ArgumentNullException("rgnData");
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateRegionRgnData(rgnData.Data, rgnData.Data.Length, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeRegion(zero);
        }

        public Region(Rectangle rect)
        {
            IntPtr zero = IntPtr.Zero;
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCreateRegionRectI(ref gprect, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeRegion(zero);
        }

        public Region(RectangleF rect)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateRegionRect(ref rect.ToGPRECTF(), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetNativeRegion(zero);
        }

        internal Region(IntPtr nativeRegion)
        {
            this.SetNativeRegion(nativeRegion);
        }

        public Region Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneRegion(new HandleRef(this, this.nativeRegion), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Region(zero);
        }

        public void Complement(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionPath(new HandleRef(this, this.nativeRegion), new HandleRef(path, path.nativePath), CombineMode.Complement);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Complement(Rectangle rect)
        {
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRectI(new HandleRef(this, this.nativeRegion), ref gprect, CombineMode.Complement);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Complement(RectangleF rect)
        {
            GPRECTF gprectf = rect.ToGPRECTF();
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRect(new HandleRef(this, this.nativeRegion), ref gprectf, CombineMode.Complement);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Complement(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRegion(new HandleRef(this, this.nativeRegion), new HandleRef(region, region.nativeRegion), CombineMode.Complement);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.nativeRegion != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeleteRegion(new HandleRef(this, this.nativeRegion));
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
                    this.nativeRegion = IntPtr.Zero;
                }
            }
        }

        public bool Equals(Region region, Graphics g)
        {
            int num;
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipIsEqualRegion(new HandleRef(this, this.nativeRegion), new HandleRef(region, region.nativeRegion), new HandleRef(g, g.NativeGraphics), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public void Exclude(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionPath(new HandleRef(this, this.nativeRegion), new HandleRef(path, path.nativePath), CombineMode.Exclude);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Exclude(Rectangle rect)
        {
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRectI(new HandleRef(this, this.nativeRegion), ref gprect, CombineMode.Exclude);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Exclude(RectangleF rect)
        {
            GPRECTF gprectf = new GPRECTF(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRect(new HandleRef(this, this.nativeRegion), ref gprectf, CombineMode.Exclude);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Exclude(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRegion(new HandleRef(this, this.nativeRegion), new HandleRef(region, region.nativeRegion), CombineMode.Exclude);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        ~Region()
        {
            this.Dispose(false);
        }

        public static Region FromHrgn(IntPtr hrgn)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateRegionHrgn(new HandleRef(null, hrgn), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Region(zero);
        }

        public RectangleF GetBounds(Graphics g)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            GPRECTF gprectf = new GPRECTF();
            int status = SafeNativeMethods.Gdip.GdipGetRegionBounds(new HandleRef(this, this.nativeRegion), new HandleRef(g, g.NativeGraphics), ref gprectf);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return gprectf.ToRectangleF();
        }

        public IntPtr GetHrgn(Graphics g)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipGetRegionHRgn(new HandleRef(this, this.nativeRegion), new HandleRef(g, g.NativeGraphics), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return zero;
        }

        public RegionData GetRegionData()
        {
            int bufferSize = 0;
            int status = SafeNativeMethods.Gdip.GdipGetRegionDataSize(new HandleRef(this, this.nativeRegion), out bufferSize);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            if (bufferSize == 0)
            {
                return null;
            }
            byte[] regionData = new byte[bufferSize];
            status = SafeNativeMethods.Gdip.GdipGetRegionData(new HandleRef(this, this.nativeRegion), regionData, bufferSize, out bufferSize);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new RegionData(regionData);
        }

        public RectangleF[] GetRegionScans(Matrix matrix)
        {
            RectangleF[] efArray;
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            int count = 0;
            int status = SafeNativeMethods.Gdip.GdipGetRegionScansCount(new HandleRef(this, this.nativeRegion), out count, new HandleRef(matrix, matrix.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            int num3 = Marshal.SizeOf(typeof(GPRECTF));
            IntPtr rects = Marshal.AllocHGlobal((int) (num3 * count));
            try
            {
                status = SafeNativeMethods.Gdip.GdipGetRegionScans(new HandleRef(this, this.nativeRegion), rects, out count, new HandleRef(matrix, matrix.nativeMatrix));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                GPRECTF gprectf = new GPRECTF();
                efArray = new RectangleF[count];
                for (int i = 0; i < count; i++)
                {
                    efArray[i] = ((GPRECTF) UnsafeNativeMethods.PtrToStructure((IntPtr) (((long) rects) + (num3 * i)), typeof(GPRECTF))).ToRectangleF();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(rects);
            }
            return efArray;
        }

        public void Intersect(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionPath(new HandleRef(this, this.nativeRegion), new HandleRef(path, path.nativePath), CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Intersect(Rectangle rect)
        {
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRectI(new HandleRef(this, this.nativeRegion), ref gprect, CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Intersect(RectangleF rect)
        {
            GPRECTF gprectf = rect.ToGPRECTF();
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRect(new HandleRef(this, this.nativeRegion), ref gprectf, CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Intersect(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRegion(new HandleRef(this, this.nativeRegion), new HandleRef(region, region.nativeRegion), CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public bool IsEmpty(Graphics g)
        {
            int num;
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            int status = SafeNativeMethods.Gdip.GdipIsEmptyRegion(new HandleRef(this, this.nativeRegion), new HandleRef(g, g.NativeGraphics), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsInfinite(Graphics g)
        {
            int num;
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            int status = SafeNativeMethods.Gdip.GdipIsInfiniteRegion(new HandleRef(this, this.nativeRegion), new HandleRef(g, g.NativeGraphics), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(Point point)
        {
            return this.IsVisible(point, null);
        }

        public bool IsVisible(PointF point)
        {
            return this.IsVisible(point, null);
        }

        public bool IsVisible(Rectangle rect)
        {
            return this.IsVisible(rect, null);
        }

        public bool IsVisible(RectangleF rect)
        {
            return this.IsVisible(rect, null);
        }

        public bool IsVisible(Point point, Graphics g)
        {
            int boolean = 0;
            int status = SafeNativeMethods.Gdip.GdipIsVisibleRegionPointI(new HandleRef(this, this.nativeRegion), point.X, point.Y, new HandleRef(g, (g == null) ? IntPtr.Zero : g.NativeGraphics), out boolean);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (boolean != 0);
        }

        public bool IsVisible(PointF point, Graphics g)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisibleRegionPoint(new HandleRef(this, this.nativeRegion), point.X, point.Y, new HandleRef(g, (g == null) ? IntPtr.Zero : g.NativeGraphics), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(Rectangle rect, Graphics g)
        {
            int boolean = 0;
            int status = SafeNativeMethods.Gdip.GdipIsVisibleRegionRectI(new HandleRef(this, this.nativeRegion), rect.X, rect.Y, rect.Width, rect.Height, new HandleRef(g, (g == null) ? IntPtr.Zero : g.NativeGraphics), out boolean);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (boolean != 0);
        }

        public bool IsVisible(RectangleF rect, Graphics g)
        {
            int boolean = 0;
            int status = SafeNativeMethods.Gdip.GdipIsVisibleRegionRect(new HandleRef(this, this.nativeRegion), rect.X, rect.Y, rect.Width, rect.Height, new HandleRef(g, (g == null) ? IntPtr.Zero : g.NativeGraphics), out boolean);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (boolean != 0);
        }

        public bool IsVisible(float x, float y)
        {
            return this.IsVisible(new PointF(x, y), null);
        }

        public bool IsVisible(int x, int y, Graphics g)
        {
            return this.IsVisible(new Point(x, y), g);
        }

        public bool IsVisible(float x, float y, Graphics g)
        {
            return this.IsVisible(new PointF(x, y), g);
        }

        public bool IsVisible(int x, int y, int width, int height)
        {
            return this.IsVisible(new Rectangle(x, y, width, height), null);
        }

        public bool IsVisible(float x, float y, float width, float height)
        {
            return this.IsVisible(new RectangleF(x, y, width, height), null);
        }

        public bool IsVisible(int x, int y, int width, int height, Graphics g)
        {
            return this.IsVisible(new Rectangle(x, y, width, height), g);
        }

        public bool IsVisible(float x, float y, float width, float height, Graphics g)
        {
            return this.IsVisible(new RectangleF(x, y, width, height), g);
        }

        public void MakeEmpty()
        {
            int status = SafeNativeMethods.Gdip.GdipSetEmpty(new HandleRef(this, this.nativeRegion));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void MakeInfinite()
        {
            int status = SafeNativeMethods.Gdip.GdipSetInfinite(new HandleRef(this, this.nativeRegion));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ReleaseHrgn(IntPtr regionHandle)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();
            if (regionHandle == IntPtr.Zero)
            {
                throw new ArgumentNullException("regionHandle");
            }
            SafeNativeMethods.IntDeleteObject(new HandleRef(this, regionHandle));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private void SetNativeRegion(IntPtr nativeRegion)
        {
            if (nativeRegion == IntPtr.Zero)
            {
                throw new ArgumentNullException("nativeRegion");
            }
            this.nativeRegion = nativeRegion;
        }

        public void Transform(Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            int status = SafeNativeMethods.Gdip.GdipTransformRegion(new HandleRef(this, this.nativeRegion), new HandleRef(matrix, matrix.nativeMatrix));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Translate(int dx, int dy)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateRegionI(new HandleRef(this, this.nativeRegion), dx, dy);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Translate(float dx, float dy)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateRegion(new HandleRef(this, this.nativeRegion), dx, dy);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Union(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionPath(new HandleRef(this, this.nativeRegion), new HandleRef(path, path.nativePath), CombineMode.Union);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Union(Rectangle rect)
        {
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRectI(new HandleRef(this, this.nativeRegion), ref gprect, CombineMode.Union);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Union(RectangleF rect)
        {
            GPRECTF gprectf = new GPRECTF(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRect(new HandleRef(this, this.nativeRegion), ref gprectf, CombineMode.Union);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Union(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRegion(new HandleRef(this, this.nativeRegion), new HandleRef(region, region.nativeRegion), CombineMode.Union);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Xor(GraphicsPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionPath(new HandleRef(this, this.nativeRegion), new HandleRef(path, path.nativePath), CombineMode.Xor);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Xor(Rectangle rect)
        {
            GPRECT gprect = new GPRECT(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRectI(new HandleRef(this, this.nativeRegion), ref gprect, CombineMode.Xor);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Xor(RectangleF rect)
        {
            GPRECTF gprectf = new GPRECTF(rect);
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRect(new HandleRef(this, this.nativeRegion), ref gprectf, CombineMode.Xor);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Xor(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipCombineRegionRegion(new HandleRef(this, this.nativeRegion), new HandleRef(region, region.nativeRegion), CombineMode.Xor);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }
    }
}

