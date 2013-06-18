namespace System.Drawing.Drawing2D
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    public sealed class GraphicsPath : MarshalByRefObject, ICloneable, IDisposable
    {
        internal IntPtr nativePath;

        public GraphicsPath() : this(System.Drawing.Drawing2D.FillMode.Alternate)
        {
        }

        public GraphicsPath(System.Drawing.Drawing2D.FillMode fillMode)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreatePath((int) fillMode, out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.nativePath = zero;
        }

        public GraphicsPath(Point[] pts, byte[] types) : this(pts, types, System.Drawing.Drawing2D.FillMode.Alternate)
        {
        }

        public GraphicsPath(PointF[] pts, byte[] types) : this(pts, types, System.Drawing.Drawing2D.FillMode.Alternate)
        {
        }

        private GraphicsPath(IntPtr nativePath, int extra)
        {
            if (nativePath == IntPtr.Zero)
            {
                throw new ArgumentNullException("nativePath");
            }
            this.nativePath = nativePath;
        }

        public GraphicsPath(Point[] pts, byte[] types, System.Drawing.Drawing2D.FillMode fillMode)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr zero = IntPtr.Zero;
            if (pts.Length != types.Length)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            int length = types.Length;
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            IntPtr destination = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.Copy(types, 0, destination, length);
                int status = SafeNativeMethods.Gdip.GdipCreatePath2I(new HandleRef(null, handle), new HandleRef(null, destination), length, (int) fillMode, out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
                Marshal.FreeHGlobal(destination);
            }
            this.nativePath = zero;
        }

        public GraphicsPath(PointF[] pts, byte[] types, System.Drawing.Drawing2D.FillMode fillMode)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr zero = IntPtr.Zero;
            if (pts.Length != types.Length)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            int length = types.Length;
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            IntPtr destination = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.Copy(types, 0, destination, length);
                int status = SafeNativeMethods.Gdip.GdipCreatePath2(new HandleRef(null, handle), new HandleRef(null, destination), length, (int) fillMode, out zero);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
                Marshal.FreeHGlobal(destination);
            }
            this.nativePath = zero;
        }

        private System.Drawing.Drawing2D.PathData _GetPathData()
        {
            int num = Marshal.SizeOf(typeof(GPPOINTF));
            int pointCount = this.PointCount;
            System.Drawing.Drawing2D.PathData data = new System.Drawing.Drawing2D.PathData {
                Types = new byte[pointCount]
            };
            IntPtr ptr = Marshal.AllocHGlobal((int) (3 * IntPtr.Size));
            IntPtr structure = Marshal.AllocHGlobal((int) (num * pointCount));
            try
            {
                GCHandle handle = GCHandle.Alloc(data.Types, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr3 = handle.AddrOfPinnedObject();
                    Marshal.StructureToPtr(pointCount, ptr, false);
                    Marshal.StructureToPtr(structure, (IntPtr) (((long) ptr) + IntPtr.Size), false);
                    Marshal.StructureToPtr(ptr3, (IntPtr) (((long) ptr) + (2 * IntPtr.Size)), false);
                    int status = SafeNativeMethods.Gdip.GdipGetPathData(new HandleRef(this, this.nativePath), ptr);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    data.Points = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(structure, pointCount);
                }
                finally
                {
                    handle.Free();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                Marshal.FreeHGlobal(structure);
            }
            return data;
        }

        public void AddArc(Rectangle rect, float startAngle, float sweepAngle)
        {
            this.AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void AddArc(RectangleF rect, float startAngle, float sweepAngle)
        {
            this.AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void AddArc(int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathArcI(new HandleRef(this, this.nativePath), x, y, width, height, startAngle, sweepAngle);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathArc(new HandleRef(this, this.nativePath), x, y, width, height, startAngle, sweepAngle);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddBezier(Point pt1, Point pt2, Point pt3, Point pt4)
        {
            this.AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            this.AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        public void AddBezier(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathBezierI(new HandleRef(this, this.nativePath), x1, y1, x2, y2, x3, y3, x4, y4);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathBezier(new HandleRef(this, this.nativePath), x1, y1, x2, y2, x3, y3, x4, y4);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddBeziers(params Point[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathBeziersI(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddBeziers(PointF[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathBeziers(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddClosedCurve(Point[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathClosedCurveI(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddClosedCurve(PointF[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathClosedCurve(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddClosedCurve(Point[] points, float tension)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathClosedCurve2I(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length, tension);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddClosedCurve(PointF[] points, float tension)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathClosedCurve2(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length, tension);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddCurve(Point[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathCurveI(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddCurve(PointF[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathCurve(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddCurve(Point[] points, float tension)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathCurve2I(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length, tension);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddCurve(PointF[] points, float tension)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathCurve2(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length, tension);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddCurve(Point[] points, int offset, int numberOfSegments, float tension)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathCurve3I(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length, offset, numberOfSegments, tension);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathCurve3(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length, offset, numberOfSegments, tension);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddEllipse(Rectangle rect)
        {
            this.AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void AddEllipse(RectangleF rect)
        {
            this.AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void AddEllipse(int x, int y, int width, int height)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathEllipseI(new HandleRef(this, this.nativePath), x, y, width, height);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddEllipse(float x, float y, float width, float height)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathEllipse(new HandleRef(this, this.nativePath), x, y, width, height);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddLine(Point pt1, Point pt2)
        {
            this.AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        public void AddLine(PointF pt1, PointF pt2)
        {
            this.AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        public void AddLine(int x1, int y1, int x2, int y2)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathLineI(new HandleRef(this, this.nativePath), x1, y1, x2, y2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddLine(float x1, float y1, float x2, float y2)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathLine(new HandleRef(this, this.nativePath), x1, y1, x2, y2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddLines(Point[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathLine2I(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddLines(PointF[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathLine2(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddPath(GraphicsPath addingPath, bool connect)
        {
            if (addingPath == null)
            {
                throw new ArgumentNullException("addingPath");
            }
            int status = SafeNativeMethods.Gdip.GdipAddPathPath(new HandleRef(this, this.nativePath), new HandleRef(addingPath, addingPath.nativePath), connect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddPie(Rectangle rect, float startAngle, float sweepAngle)
        {
            this.AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void AddPie(int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathPieI(new HandleRef(this, this.nativePath), x, y, width, height, startAngle, sweepAngle);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathPie(new HandleRef(this, this.nativePath), x, y, width, height, startAngle, sweepAngle);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddPolygon(Point[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathPolygonI(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddPolygon(PointF[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathPolygon(new HandleRef(this, this.nativePath), new HandleRef(null, handle), points.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddRectangle(Rectangle rect)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathRectangleI(new HandleRef(this, this.nativePath), rect.X, rect.Y, rect.Width, rect.Height);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddRectangle(RectangleF rect)
        {
            int status = SafeNativeMethods.Gdip.GdipAddPathRectangle(new HandleRef(this, this.nativePath), rect.X, rect.Y, rect.Width, rect.Height);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddRectangles(Rectangle[] rects)
        {
            if (rects == null)
            {
                throw new ArgumentNullException("rects");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertRectangleToMemory(rects);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathRectanglesI(new HandleRef(this, this.nativePath), new HandleRef(null, handle), rects.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddRectangles(RectangleF[] rects)
        {
            if (rects == null)
            {
                throw new ArgumentNullException("rects");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertRectangleToMemory(rects);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipAddPathRectangles(new HandleRef(this, this.nativePath), new HandleRef(null, handle), rects.Length);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void AddString(string s, FontFamily family, int style, float emSize, Point origin, StringFormat format)
        {
            GPRECT layoutRect = new GPRECT(origin.X, origin.Y, 0, 0);
            int status = SafeNativeMethods.Gdip.GdipAddPathStringI(new HandleRef(this, this.nativePath), s, s.Length, new HandleRef(family, (family != null) ? family.NativeFamily : IntPtr.Zero), style, emSize, ref layoutRect, new HandleRef(format, (format != null) ? format.nativeFormat : IntPtr.Zero));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddString(string s, FontFamily family, int style, float emSize, PointF origin, StringFormat format)
        {
            GPRECTF layoutRect = new GPRECTF(origin.X, origin.Y, 0f, 0f);
            int status = SafeNativeMethods.Gdip.GdipAddPathString(new HandleRef(this, this.nativePath), s, s.Length, new HandleRef(family, (family != null) ? family.NativeFamily : IntPtr.Zero), style, emSize, ref layoutRect, new HandleRef(format, (format != null) ? format.nativeFormat : IntPtr.Zero));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddString(string s, FontFamily family, int style, float emSize, Rectangle layoutRect, StringFormat format)
        {
            GPRECT gprect = new GPRECT(layoutRect);
            int status = SafeNativeMethods.Gdip.GdipAddPathStringI(new HandleRef(this, this.nativePath), s, s.Length, new HandleRef(family, (family != null) ? family.NativeFamily : IntPtr.Zero), style, emSize, ref gprect, new HandleRef(format, (format != null) ? format.nativeFormat : IntPtr.Zero));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void AddString(string s, FontFamily family, int style, float emSize, RectangleF layoutRect, StringFormat format)
        {
            GPRECTF gprectf = new GPRECTF(layoutRect);
            int status = SafeNativeMethods.Gdip.GdipAddPathString(new HandleRef(this, this.nativePath), s, s.Length, new HandleRef(family, (family != null) ? family.NativeFamily : IntPtr.Zero), style, emSize, ref gprectf, new HandleRef(format, (format != null) ? format.nativeFormat : IntPtr.Zero));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ClearMarkers()
        {
            int status = SafeNativeMethods.Gdip.GdipClearPathMarkers(new HandleRef(this, this.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipClonePath(new HandleRef(this, this.nativePath), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new GraphicsPath(zero, 0);
        }

        public void CloseAllFigures()
        {
            int status = SafeNativeMethods.Gdip.GdipClosePathFigures(new HandleRef(this, this.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void CloseFigure()
        {
            int status = SafeNativeMethods.Gdip.GdipClosePathFigure(new HandleRef(this, this.nativePath));
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
            if (this.nativePath != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeletePath(new HandleRef(this, this.nativePath));
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
                    this.nativePath = IntPtr.Zero;
                }
            }
        }

        ~GraphicsPath()
        {
            this.Dispose(false);
        }

        public void Flatten()
        {
            this.Flatten(null);
        }

        public void Flatten(Matrix matrix)
        {
            this.Flatten(matrix, 0.25f);
        }

        public void Flatten(Matrix matrix, float flatness)
        {
            int status = SafeNativeMethods.Gdip.GdipFlattenPath(new HandleRef(this, this.nativePath), new HandleRef(matrix, (matrix == null) ? IntPtr.Zero : matrix.nativeMatrix), flatness);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public RectangleF GetBounds()
        {
            return this.GetBounds(null);
        }

        public RectangleF GetBounds(Matrix matrix)
        {
            return this.GetBounds(matrix, null);
        }

        public RectangleF GetBounds(Matrix matrix, Pen pen)
        {
            GPRECTF gprectf = new GPRECTF();
            IntPtr zero = IntPtr.Zero;
            IntPtr handle = IntPtr.Zero;
            if (matrix != null)
            {
                zero = matrix.nativeMatrix;
            }
            if (pen != null)
            {
                handle = pen.NativePen;
            }
            int status = SafeNativeMethods.Gdip.GdipGetPathWorldBounds(new HandleRef(this, this.nativePath), ref gprectf, new HandleRef(matrix, zero), new HandleRef(pen, handle));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return gprectf.ToRectangleF();
        }

        public PointF GetLastPoint()
        {
            GPPOINTF lastPoint = new GPPOINTF();
            int status = SafeNativeMethods.Gdip.GdipGetPathLastPoint(new HandleRef(this, this.nativePath), lastPoint);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return lastPoint.ToPoint();
        }

        public bool IsOutlineVisible(Point point, Pen pen)
        {
            return this.IsOutlineVisible(point, pen, null);
        }

        public bool IsOutlineVisible(PointF point, Pen pen)
        {
            return this.IsOutlineVisible(point, pen, null);
        }

        public bool IsOutlineVisible(Point pt, Pen pen, Graphics graphics)
        {
            int num;
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipIsOutlineVisiblePathPointI(new HandleRef(this, this.nativePath), pt.X, pt.Y, new HandleRef(pen, pen.NativePen), new HandleRef(graphics, (graphics != null) ? graphics.NativeGraphics : IntPtr.Zero), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsOutlineVisible(PointF pt, Pen pen, Graphics graphics)
        {
            int num;
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipIsOutlineVisiblePathPoint(new HandleRef(this, this.nativePath), pt.X, pt.Y, new HandleRef(pen, pen.NativePen), new HandleRef(graphics, (graphics != null) ? graphics.NativeGraphics : IntPtr.Zero), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsOutlineVisible(int x, int y, Pen pen)
        {
            return this.IsOutlineVisible(new Point(x, y), pen, null);
        }

        public bool IsOutlineVisible(float x, float y, Pen pen)
        {
            return this.IsOutlineVisible(new PointF(x, y), pen, null);
        }

        public bool IsOutlineVisible(int x, int y, Pen pen, Graphics graphics)
        {
            return this.IsOutlineVisible(new Point(x, y), pen, graphics);
        }

        public bool IsOutlineVisible(float x, float y, Pen pen, Graphics graphics)
        {
            return this.IsOutlineVisible(new PointF(x, y), pen, graphics);
        }

        public bool IsVisible(Point point)
        {
            return this.IsVisible(point, null);
        }

        public bool IsVisible(PointF point)
        {
            return this.IsVisible(point, null);
        }

        public bool IsVisible(Point pt, Graphics graphics)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisiblePathPointI(new HandleRef(this, this.nativePath), pt.X, pt.Y, new HandleRef(graphics, (graphics != null) ? graphics.NativeGraphics : IntPtr.Zero), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(PointF pt, Graphics graphics)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisiblePathPoint(new HandleRef(this, this.nativePath), pt.X, pt.Y, new HandleRef(graphics, (graphics != null) ? graphics.NativeGraphics : IntPtr.Zero), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(int x, int y)
        {
            return this.IsVisible(new Point(x, y), null);
        }

        public bool IsVisible(float x, float y)
        {
            return this.IsVisible(new PointF(x, y), null);
        }

        public bool IsVisible(int x, int y, Graphics graphics)
        {
            return this.IsVisible(new Point(x, y), graphics);
        }

        public bool IsVisible(float x, float y, Graphics graphics)
        {
            return this.IsVisible(new PointF(x, y), graphics);
        }

        public void Reset()
        {
            int status = SafeNativeMethods.Gdip.GdipResetPath(new HandleRef(this, this.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Reverse()
        {
            int status = SafeNativeMethods.Gdip.GdipReversePath(new HandleRef(this, this.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetMarkers()
        {
            int status = SafeNativeMethods.Gdip.GdipSetPathMarker(new HandleRef(this, this.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void StartFigure()
        {
            int status = SafeNativeMethods.Gdip.GdipStartPathFigure(new HandleRef(this, this.nativePath));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Transform(Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            if (matrix.nativeMatrix != IntPtr.Zero)
            {
                int status = SafeNativeMethods.Gdip.GdipTransformPath(new HandleRef(this, this.nativePath), new HandleRef(matrix, matrix.nativeMatrix));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect)
        {
            this.Warp(destPoints, srcRect, null);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix matrix)
        {
            this.Warp(destPoints, srcRect, matrix, WarpMode.Perspective);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode)
        {
            this.Warp(destPoints, srcRect, matrix, warpMode, 0.25f);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode, float flatness)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            try
            {
                int status = SafeNativeMethods.Gdip.GdipWarpPath(new HandleRef(this, this.nativePath), new HandleRef(matrix, (matrix == null) ? IntPtr.Zero : matrix.nativeMatrix), new HandleRef(null, handle), destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, warpMode, flatness);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        public void Widen(Pen pen)
        {
            float flatness = 0.6666667f;
            this.Widen(pen, null, flatness);
        }

        public void Widen(Pen pen, Matrix matrix)
        {
            float flatness = 0.6666667f;
            this.Widen(pen, matrix, flatness);
        }

        public void Widen(Pen pen, Matrix matrix, float flatness)
        {
            IntPtr zero;
            int num;
            if (matrix == null)
            {
                zero = IntPtr.Zero;
            }
            else
            {
                zero = matrix.nativeMatrix;
            }
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            SafeNativeMethods.Gdip.GdipGetPointCount(new HandleRef(this, this.nativePath), out num);
            if (num != 0)
            {
                int status = SafeNativeMethods.Gdip.GdipWidenPath(new HandleRef(this, this.nativePath), new HandleRef(pen, pen.NativePen), new HandleRef(matrix, zero), flatness);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Drawing2D.FillMode FillMode
        {
            get
            {
                int fillmode = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPathFillMode(new HandleRef(this, this.nativePath), out fillmode);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.FillMode) fillmode;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.FillMode));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPathFillMode(new HandleRef(this, this.nativePath), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Drawing2D.PathData PathData
        {
            get
            {
                return this._GetPathData();
            }
        }

        public PointF[] PathPoints
        {
            get
            {
                PointF[] tfArray2;
                int pointCount = this.PointCount;
                int num2 = Marshal.SizeOf(typeof(GPPOINTF));
                IntPtr handle = Marshal.AllocHGlobal((int) (pointCount * num2));
                try
                {
                    int status = SafeNativeMethods.Gdip.GdipGetPathPoints(new HandleRef(this, this.nativePath), new HandleRef(null, handle), pointCount);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    tfArray2 = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(handle, pointCount);
                }
                finally
                {
                    Marshal.FreeHGlobal(handle);
                }
                return tfArray2;
            }
        }

        public byte[] PathTypes
        {
            get
            {
                int pointCount = this.PointCount;
                byte[] types = new byte[pointCount];
                int status = SafeNativeMethods.Gdip.GdipGetPathTypes(new HandleRef(this, this.nativePath), types, pointCount);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return types;
            }
        }

        public int PointCount
        {
            get
            {
                int count = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPointCount(new HandleRef(this, this.nativePath), out count);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return count;
            }
        }
    }
}

