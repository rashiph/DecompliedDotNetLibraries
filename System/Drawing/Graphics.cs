namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.Drawing.Text;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public sealed class Graphics : MarshalByRefObject, IDeviceContext, IDisposable
    {
        private Image backingImage;
        private static IntPtr halftonePalette;
        private IntPtr nativeGraphics;
        private IntPtr nativeHdc;
        private GraphicsContext previousContext;
        private object printingHelper;
        private static object syncObject = new object();

        private Graphics(IntPtr gdipNativeGraphics)
        {
            if (gdipNativeGraphics == IntPtr.Zero)
            {
                throw new ArgumentNullException("gdipNativeGraphics");
            }
            this.nativeGraphics = gdipNativeGraphics;
        }

        public void AddMetafileComment(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            int status = SafeNativeMethods.Gdip.GdipComment(new HandleRef(this, this.NativeGraphics), data.Length, data);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public GraphicsContainer BeginContainer()
        {
            GraphicsContext context = new GraphicsContext(this);
            int state = 0;
            int status = SafeNativeMethods.Gdip.GdipBeginContainer2(new HandleRef(this, this.NativeGraphics), out state);
            if (status != 0)
            {
                context.Dispose();
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            context.State = state;
            this.PushContext(context);
            return new GraphicsContainer(state);
        }

        public GraphicsContainer BeginContainer(Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
        {
            GraphicsContext context = new GraphicsContext(this);
            int state = 0;
            GPRECT dstRect = new GPRECT(dstrect);
            GPRECT srcRect = new GPRECT(srcrect);
            int status = SafeNativeMethods.Gdip.GdipBeginContainerI(new HandleRef(this, this.NativeGraphics), ref dstRect, ref srcRect, (int) unit, out state);
            if (status != 0)
            {
                context.Dispose();
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            context.State = state;
            this.PushContext(context);
            return new GraphicsContainer(state);
        }

        public GraphicsContainer BeginContainer(RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
        {
            GraphicsContext context = new GraphicsContext(this);
            int state = 0;
            GPRECTF dstRect = dstrect.ToGPRECTF();
            GPRECTF srcRect = srcrect.ToGPRECTF();
            int status = SafeNativeMethods.Gdip.GdipBeginContainer(new HandleRef(this, this.NativeGraphics), ref dstRect, ref srcRect, (int) unit, out state);
            if (status != 0)
            {
                context.Dispose();
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            context.State = state;
            this.PushContext(context);
            return new GraphicsContainer(state);
        }

        private void CheckErrorStatus(int status)
        {
            if (status != 0)
            {
                if ((status == 1) || (status == 7))
                {
                    int num = Marshal.GetLastWin32Error();
                    if (((num == 5) || (num == 0x7f)) || (((System.Drawing.UnsafeNativeMethods.GetSystemMetrics(0x1000) & 1) != 0) && (num == 0)))
                    {
                        return;
                    }
                }
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Clear(Color color)
        {
            int status = SafeNativeMethods.Gdip.GdipGraphicsClear(new HandleRef(this, this.NativeGraphics), color.ToArgb());
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize)
        {
            this.CopyFromScreen(upperLeftSource.X, upperLeftSource.Y, upperLeftDestination.X, upperLeftDestination.Y, blockRegionSize);
        }

        public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
        {
            this.CopyFromScreen(upperLeftSource.X, upperLeftSource.Y, upperLeftDestination.X, upperLeftDestination.Y, blockRegionSize, copyPixelOperation);
        }

        public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize)
        {
            this.CopyFromScreen(sourceX, sourceY, destinationX, destinationY, blockRegionSize, CopyPixelOperation.SourceCopy);
        }

        public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
        {
            switch (copyPixelOperation)
            {
                case CopyPixelOperation.NotSourceErase:
                case CopyPixelOperation.NotSourceCopy:
                case CopyPixelOperation.NoMirrorBitmap:
                case CopyPixelOperation.Blackness:
                case CopyPixelOperation.SourceErase:
                case CopyPixelOperation.DestinationInvert:
                case CopyPixelOperation.PatInvert:
                case CopyPixelOperation.SourceInvert:
                case CopyPixelOperation.MergeCopy:
                case CopyPixelOperation.SourceCopy:
                case CopyPixelOperation.SourceAnd:
                case CopyPixelOperation.MergePaint:
                case CopyPixelOperation.SourcePaint:
                case CopyPixelOperation.PatCopy:
                case CopyPixelOperation.PatPaint:
                case CopyPixelOperation.Whiteness:
                case CopyPixelOperation.CaptureBlt:
                {
                    new UIPermission(UIPermissionWindow.AllWindows).Demand();
                    int width = blockRegionSize.Width;
                    int height = blockRegionSize.Height;
                    using (DeviceContext context = DeviceContext.FromHwnd(IntPtr.Zero))
                    {
                        HandleRef hSrcDC = new HandleRef(null, context.Hdc);
                        HandleRef hDC = new HandleRef(null, this.GetHdc());
                        try
                        {
                            if (SafeNativeMethods.BitBlt(hDC, destinationX, destinationY, width, height, hSrcDC, sourceX, sourceY, (int) copyPixelOperation) == 0)
                            {
                                throw new Win32Exception();
                            }
                        }
                        finally
                        {
                            this.ReleaseHdc();
                        }
                    }
                    return;
                }
            }
            throw new InvalidEnumArgumentException("value", (int) copyPixelOperation, typeof(CopyPixelOperation));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            while (this.previousContext != null)
            {
                GraphicsContext previous = this.previousContext.Previous;
                this.previousContext.Dispose();
                this.previousContext = previous;
            }
            if (this.nativeGraphics != IntPtr.Zero)
            {
                try
                {
                    if (this.nativeHdc != IntPtr.Zero)
                    {
                        this.ReleaseHdc();
                    }
                    if (this.PrintingHelper != null)
                    {
                        DeviceContext printingHelper = this.PrintingHelper as DeviceContext;
                        if (printingHelper != null)
                        {
                            printingHelper.Dispose();
                            this.printingHelper = null;
                        }
                    }
                    SafeNativeMethods.Gdip.GdipDeleteGraphics(new HandleRef(this, this.nativeGraphics));
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
                    this.nativeGraphics = IntPtr.Zero;
                }
            }
        }

        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            this.DrawArc(pen, (float) rect.X, (float) rect.Y, (float) rect.Width, (float) rect.Height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            this.DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawArcI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height, (float) startAngle, (float) sweepAngle);
            this.CheckErrorStatus(status);
        }

        public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawArc(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height, startAngle, sweepAngle);
            this.CheckErrorStatus(status);
        }

        public void DrawBezier(Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
        {
            this.DrawBezier(pen, (float) pt1.X, (float) pt1.Y, (float) pt2.X, (float) pt2.Y, (float) pt3.X, (float) pt3.Y, (float) pt4.X, (float) pt4.Y);
        }

        public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            this.DrawBezier(pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        public void DrawBezier(Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawBezier(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x1, y1, x2, y2, x3, y3, x4, y4);
            this.CheckErrorStatus(status);
        }

        public void DrawBeziers(Pen pen, Point[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawBeziersI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawBeziers(Pen pen, PointF[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawBeziers(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawClosedCurve(Pen pen, Point[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawClosedCurveI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawClosedCurve(Pen pen, PointF[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawClosedCurve(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawClosedCurve(Pen pen, Point[] points, float tension, FillMode fillmode)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawClosedCurve2I(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length, tension);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawClosedCurve(Pen pen, PointF[] points, float tension, FillMode fillmode)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawClosedCurve2(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length, tension);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawCurve(Pen pen, Point[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawCurveI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawCurve(Pen pen, PointF[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawCurve(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawCurve(Pen pen, Point[] points, float tension)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawCurve2I(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length, tension);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawCurve(Pen pen, PointF[] points, float tension)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawCurve2(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length, tension);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments)
        {
            this.DrawCurve(pen, points, offset, numberOfSegments, 0.5f);
        }

        public void DrawCurve(Pen pen, Point[] points, int offset, int numberOfSegments, float tension)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawCurve3I(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length, offset, numberOfSegments, tension);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments, float tension)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawCurve3(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length, offset, numberOfSegments, tension);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            this.DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawEllipse(Pen pen, RectangleF rect)
        {
            this.DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawEllipseI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void DrawEllipse(Pen pen, float x, float y, float width, float height)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawEllipse(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void DrawIcon(Icon icon, Rectangle targetRect)
        {
            if (icon == null)
            {
                throw new ArgumentNullException("icon");
            }
            if (this.backingImage != null)
            {
                this.DrawImage(icon.ToBitmap(), targetRect);
            }
            else
            {
                icon.Draw(this, targetRect);
            }
        }

        public void DrawIcon(Icon icon, int x, int y)
        {
            if (icon == null)
            {
                throw new ArgumentNullException("icon");
            }
            if (this.backingImage != null)
            {
                this.DrawImage(icon.ToBitmap(), x, y);
            }
            else
            {
                icon.Draw(this, x, y);
            }
        }

        public void DrawIconUnstretched(Icon icon, Rectangle targetRect)
        {
            if (icon == null)
            {
                throw new ArgumentNullException("icon");
            }
            if (this.backingImage != null)
            {
                this.DrawImageUnscaled(icon.ToBitmap(), targetRect);
            }
            else
            {
                icon.DrawUnstretched(this, targetRect);
            }
        }

        public void DrawImage(Image image, Point point)
        {
            this.DrawImage(image, point.X, point.Y);
        }

        public void DrawImage(Image image, PointF point)
        {
            this.DrawImage(image, point.X, point.Y);
        }

        public void DrawImage(Image image, Rectangle rect)
        {
            this.DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawImage(Image image, RectangleF rect)
        {
            this.DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawImage(Image image, Point[] destPoints)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int length = destPoints.Length;
            if ((length != 3) && (length != 4))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidLength"));
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePointsI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), new HandleRef(this, handle), length);
            Marshal.FreeHGlobal(handle);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, PointF[] destPoints)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int length = destPoints.Length;
            if ((length != 3) && (length != 4))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidLength"));
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePoints(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), new HandleRef(this, handle), length);
            Marshal.FreeHGlobal(handle);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, int x, int y)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), x, y);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, float x, float y)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImage(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), x, y);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageRectRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), destRect.X, destRect.Y, destRect.Width, destRect.Height, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit, System.Drawing.NativeMethods.NullHandleRef, null, System.Drawing.NativeMethods.NullHandleRef);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            this.DrawImage(image, destPoints, srcRect, srcUnit, null, null, 0);
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageRectRect(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), destRect.X, destRect.Y, destRect.Width, destRect.Height, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit, System.Drawing.NativeMethods.NullHandleRef, null, System.Drawing.NativeMethods.NullHandleRef);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int length = destPoints.Length;
            if ((length != 3) && (length != 4))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidLength"));
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePointsRect(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), new HandleRef(this, handle), destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit, System.Drawing.NativeMethods.NullHandleRef, null, System.Drawing.NativeMethods.NullHandleRef);
            Marshal.FreeHGlobal(handle);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
        {
            this.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr, null, 0);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
        {
            this.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr, null, 0);
        }

        public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePointRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, int x, int y, int width, int height)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), x, y, width, height);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePointRect(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, float x, float y, float width, float height)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageRect(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), x, y, width, height);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
        {
            this.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr, callback, 0);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
        {
            this.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr, callback, 0);
        }

        public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int length = destPoints.Length;
            if ((length != 3) && (length != 4))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidLength"));
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePointsRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), new HandleRef(this, handle), destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit, new HandleRef(imageAttr, (imageAttr != null) ? imageAttr.nativeImageAttributes : IntPtr.Zero), callback, new HandleRef(null, (IntPtr) callbackData));
            Marshal.FreeHGlobal(handle);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int length = destPoints.Length;
            if ((length != 3) && (length != 4))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidLength"));
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImagePointsRect(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), new HandleRef(this, handle), destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, (int) srcUnit, new HandleRef(imageAttr, (imageAttr != null) ? imageAttr.nativeImageAttributes : IntPtr.Zero), callback, new HandleRef(null, (IntPtr) callbackData));
            Marshal.FreeHGlobal(handle);
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
        {
            this.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, null);
        }

        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
        {
            this.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, null);
        }

        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
        {
            this.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr, null);
        }

        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
        {
            this.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs, null);
        }

        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
        {
            this.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr, callback, IntPtr.Zero);
        }

        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback)
        {
            this.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs, callback, IntPtr.Zero);
        }

        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageRectRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, (int) srcUnit, new HandleRef(imageAttrs, (imageAttrs != null) ? imageAttrs.nativeImageAttributes : IntPtr.Zero), callback, new HandleRef(null, callbackData));
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int errorStatus = SafeNativeMethods.Gdip.GdipDrawImageRectRect(new HandleRef(this, this.NativeGraphics), new HandleRef(image, image.nativeImage), (float) destRect.X, (float) destRect.Y, (float) destRect.Width, (float) destRect.Height, srcX, srcY, srcWidth, srcHeight, (int) srcUnit, new HandleRef(imageAttrs, (imageAttrs != null) ? imageAttrs.nativeImageAttributes : IntPtr.Zero), callback, new HandleRef(null, callbackData));
            this.IgnoreMetafileErrors(image, ref errorStatus);
            this.CheckErrorStatus(errorStatus);
        }

        public void DrawImageUnscaled(Image image, Point point)
        {
            this.DrawImage(image, point.X, point.Y);
        }

        public void DrawImageUnscaled(Image image, Rectangle rect)
        {
            this.DrawImage(image, rect.X, rect.Y);
        }

        public void DrawImageUnscaled(Image image, int x, int y)
        {
            this.DrawImage(image, x, y);
        }

        public void DrawImageUnscaled(Image image, int x, int y, int width, int height)
        {
            this.DrawImage(image, x, y);
        }

        public void DrawImageUnscaledAndClipped(Image image, Rectangle rect)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            int srcWidth = Math.Min(rect.Width, image.Width);
            int srcHeight = Math.Min(rect.Height, image.Height);
            this.DrawImage(image, rect, 0, 0, srcWidth, srcHeight, GraphicsUnit.Pixel);
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            this.DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        public void DrawLine(Pen pen, PointF pt1, PointF pt2)
        {
            this.DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawLineI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x1, y1, x2, y2);
            this.CheckErrorStatus(status);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawLine(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x1, y1, x2, y2);
            this.CheckErrorStatus(status);
        }

        public void DrawLines(Pen pen, Point[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawLinesI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawLines(Pen pen, PointF[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawLines(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawPath(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(path, path.nativePath));
            this.CheckErrorStatus(status);
        }

        public void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            this.DrawPie(pen, (float) rect.X, (float) rect.Y, (float) rect.Width, (float) rect.Height, startAngle, sweepAngle);
        }

        public void DrawPie(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            this.DrawPie(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        public void DrawPie(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawPieI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height, (float) startAngle, (float) sweepAngle);
            this.CheckErrorStatus(status);
        }

        public void DrawPie(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawPie(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height, startAngle, sweepAngle);
            this.CheckErrorStatus(status);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawPolygonI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawPolygon(Pen pen, PointF[] points)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipDrawPolygon(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            this.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawRectangleI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            int status = SafeNativeMethods.Gdip.GdipDrawRectangle(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void DrawRectangles(Pen pen, Rectangle[] rects)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (rects == null)
            {
                throw new ArgumentNullException("rects");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertRectangleToMemory(rects);
            int status = SafeNativeMethods.Gdip.GdipDrawRectanglesI(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), rects.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawRectangles(Pen pen, RectangleF[] rects)
        {
            if (pen == null)
            {
                throw new ArgumentNullException("pen");
            }
            if (rects == null)
            {
                throw new ArgumentNullException("rects");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertRectangleToMemory(rects);
            int status = SafeNativeMethods.Gdip.GdipDrawRectangles(new HandleRef(this, this.NativeGraphics), new HandleRef(pen, pen.NativePen), new HandleRef(this, handle), rects.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void DrawString(string s, Font font, Brush brush, PointF point)
        {
            this.DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0f, 0f), null);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle)
        {
            this.DrawString(s, font, brush, layoutRectangle, null);
        }

        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
        {
            this.DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0f, 0f), format);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if ((s != null) && (s.Length != 0))
            {
                if (font == null)
                {
                    throw new ArgumentNullException("font");
                }
                GPRECTF layoutRect = new GPRECTF(layoutRectangle);
                IntPtr handle = (format == null) ? IntPtr.Zero : format.nativeFormat;
                int status = SafeNativeMethods.Gdip.GdipDrawString(new HandleRef(this, this.NativeGraphics), s, s.Length, new HandleRef(font, font.NativeFont), ref layoutRect, new HandleRef(format, handle), new HandleRef(brush, brush.NativeBrush));
                this.CheckErrorStatus(status);
            }
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y)
        {
            this.DrawString(s, font, brush, new RectangleF(x, y, 0f, 0f), null);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format)
        {
            this.DrawString(s, font, brush, new RectangleF(x, y, 0f, 0f), format);
        }

        public void EndContainer(GraphicsContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            int status = SafeNativeMethods.Gdip.GdipEndContainer(new HandleRef(this, this.NativeGraphics), container.nativeGraphicsContainer);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.PopContext(container.nativeGraphicsContainer);
        }

        public void EnumerateMetafile(Metafile metafile, Point destPoint, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoint, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, PointF destPoint, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoint, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destRect, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destRect, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoints, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoints, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoint, callback, callbackData, (ImageAttributes) null);
        }

        public void EnumerateMetafile(Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoint, callback, callbackData, (ImageAttributes) null);
        }

        public void EnumerateMetafile(Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destRect, callback, callbackData, (ImageAttributes) null);
        }

        public void EnumerateMetafile(Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destRect, callback, callbackData, (ImageAttributes) null);
        }

        public void EnumerateMetafile(Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoints, callback, callbackData, (ImageAttributes) null);
        }

        public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoints, callback, IntPtr.Zero, (ImageAttributes) null);
        }

        public void EnumerateMetafile(Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileDestPointI(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), new GPPOINT(destPoint), callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (destPoints.Length != 3)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidParallelogram"));
            }
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            IntPtr ptr3 = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileDestPointsI(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ptr3, destPoints.Length, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            Marshal.FreeHGlobal(ptr3);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (destPoints.Length != 3)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidParallelogram"));
            }
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            IntPtr ptr3 = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileDestPoints(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ptr3, destPoints.Length, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            Marshal.FreeHGlobal(ptr3);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoint, srcRect, srcUnit, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileDestPoint(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), new GPPOINTF(destPoint), callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoint, srcRect, srcUnit, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            GPRECT gprect = new GPRECT(destRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileDestRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ref gprect, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destRect, srcRect, srcUnit, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            GPRECTF gprectf = new GPRECTF(destRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileDestRect(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ref gprectf, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoints, srcRect, srcUnit, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destRect, srcRect, srcUnit, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
        {
            this.EnumerateMetafile(metafile, destPoints, srcRect, srcUnit, callback, IntPtr.Zero);
        }

        public void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoint, srcRect, srcUnit, callback, callbackData, null);
        }

        public void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoints, srcRect, srcUnit, callback, callbackData, null);
        }

        public void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoint, srcRect, srcUnit, callback, callbackData, null);
        }

        public void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destRect, srcRect, srcUnit, callback, callbackData, null);
        }

        public void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destRect, srcRect, srcUnit, callback, callbackData, null);
        }

        public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
        {
            this.EnumerateMetafile(metafile, destPoints, srcRect, srcUnit, callback, callbackData, null);
        }

        public void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            GPPOINT gppoint = new GPPOINT(destPoint);
            GPRECT gprect = new GPRECT(srcRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileSrcRectDestPointI(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), gppoint, ref gprect, (int) unit, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (destPoints.Length != 3)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidParallelogram"));
            }
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            IntPtr ptr3 = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            GPRECT gprect = new GPRECT(srcRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileSrcRectDestPointsI(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ptr3, destPoints.Length, ref gprect, (int) unit, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            Marshal.FreeHGlobal(ptr3);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            GPRECTF gprectf = new GPRECTF(srcRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileSrcRectDestPoint(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), new GPPOINTF(destPoint), ref gprectf, (int) unit, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            GPRECT gprect = new GPRECT(destRect);
            GPRECT gprect2 = new GPRECT(srcRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileSrcRectDestRectI(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ref gprect, ref gprect2, (int) unit, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            GPRECTF gprectf = new GPRECTF(destRect);
            GPRECTF gprectf2 = new GPRECTF(srcRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileSrcRectDestRect(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ref gprectf, ref gprectf2, (int) unit, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
        {
            if (destPoints == null)
            {
                throw new ArgumentNullException("destPoints");
            }
            if (destPoints.Length != 3)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusDestPointsInvalidParallelogram"));
            }
            IntPtr handle = (metafile == null) ? IntPtr.Zero : metafile.nativeImage;
            IntPtr ptr2 = (imageAttr == null) ? IntPtr.Zero : imageAttr.nativeImageAttributes;
            IntPtr ptr3 = SafeNativeMethods.Gdip.ConvertPointToMemory(destPoints);
            GPRECTF gprectf = new GPRECTF(srcRect);
            int status = SafeNativeMethods.Gdip.GdipEnumerateMetafileSrcRectDestPoints(new HandleRef(this, this.NativeGraphics), new HandleRef(metafile, handle), ptr3, destPoints.Length, ref gprectf, (int) unit, callback, new HandleRef(null, callbackData), new HandleRef(imageAttr, ptr2));
            Marshal.FreeHGlobal(ptr3);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ExcludeClip(Rectangle rect)
        {
            int status = SafeNativeMethods.Gdip.GdipSetClipRectI(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Exclude);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ExcludeClip(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipSetClipRegion(new HandleRef(this, this.NativeGraphics), new HandleRef(region, region.nativeRegion), CombineMode.Exclude);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void FillClosedCurve(Brush brush, Point[] points)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipFillClosedCurveI(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillClosedCurve(Brush brush, PointF[] points)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipFillClosedCurve(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), points.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode)
        {
            this.FillClosedCurve(brush, points, fillmode, 0.5f);
        }

        public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode)
        {
            this.FillClosedCurve(brush, points, fillmode, 0.5f);
        }

        public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode, float tension)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipFillClosedCurve2I(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), points.Length, tension, (int) fillmode);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode, float tension)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipFillClosedCurve2(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), points.Length, tension, (int) fillmode);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillEllipse(Brush brush, Rectangle rect)
        {
            this.FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillEllipse(Brush brush, RectangleF rect)
        {
            this.FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipFillEllipseI(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void FillEllipse(Brush brush, float x, float y, float width, float height)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipFillEllipse(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void FillPath(Brush brush, GraphicsPath path)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipFillPath(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(path, path.nativePath));
            this.CheckErrorStatus(status);
        }

        public void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle)
        {
            this.FillPie(brush, (float) rect.X, (float) rect.Y, (float) rect.Width, (float) rect.Height, startAngle, sweepAngle);
        }

        public void FillPie(Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipFillPieI(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), x, y, width, height, (float) startAngle, (float) sweepAngle);
            this.CheckErrorStatus(status);
        }

        public void FillPie(Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipFillPie(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), x, y, width, height, startAngle, sweepAngle);
            this.CheckErrorStatus(status);
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            this.FillPolygon(brush, points, FillMode.Alternate);
        }

        public void FillPolygon(Brush brush, PointF[] points)
        {
            this.FillPolygon(brush, points, FillMode.Alternate);
        }

        public void FillPolygon(Brush brush, Point[] points, FillMode fillMode)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipFillPolygonI(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), points.Length, (int) fillMode);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillPolygon(Brush brush, PointF[] points, FillMode fillMode)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertPointToMemory(points);
            int status = SafeNativeMethods.Gdip.GdipFillPolygon(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), points.Length, (int) fillMode);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillRectangle(Brush brush, Rectangle rect)
        {
            this.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRectangle(Brush brush, RectangleF rect)
        {
            this.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipFillRectangleI(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            int status = SafeNativeMethods.Gdip.GdipFillRectangle(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), x, y, width, height);
            this.CheckErrorStatus(status);
        }

        public void FillRectangles(Brush brush, Rectangle[] rects)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (rects == null)
            {
                throw new ArgumentNullException("rects");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertRectangleToMemory(rects);
            int status = SafeNativeMethods.Gdip.GdipFillRectanglesI(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), rects.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillRectangles(Brush brush, RectangleF[] rects)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (rects == null)
            {
                throw new ArgumentNullException("rects");
            }
            IntPtr handle = SafeNativeMethods.Gdip.ConvertRectangleToMemory(rects);
            int status = SafeNativeMethods.Gdip.GdipFillRectangles(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(this, handle), rects.Length);
            Marshal.FreeHGlobal(handle);
            this.CheckErrorStatus(status);
        }

        public void FillRegion(Brush brush, Region region)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipFillRegion(new HandleRef(this, this.NativeGraphics), new HandleRef(brush, brush.NativeBrush), new HandleRef(region, region.nativeRegion));
            this.CheckErrorStatus(status);
        }

        ~Graphics()
        {
            this.Dispose(false);
        }

        public void Flush()
        {
            this.Flush(FlushIntention.Flush);
        }

        public void Flush(FlushIntention intention)
        {
            int status = SafeNativeMethods.Gdip.GdipFlush(new HandleRef(this, this.NativeGraphics), intention);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Graphics FromHdc(IntPtr hdc)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            if (hdc == IntPtr.Zero)
            {
                throw new ArgumentNullException("hdc");
            }
            return FromHdcInternal(hdc);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Graphics FromHdc(IntPtr hdc, IntPtr hdevice)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateFromHDC2(new HandleRef(null, hdc), new HandleRef(null, hdevice), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Graphics(zero);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static Graphics FromHdcInternal(IntPtr hdc)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateFromHDC(new HandleRef(null, hdc), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Graphics(zero);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Graphics FromHwnd(IntPtr hwnd)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            return FromHwndInternal(hwnd);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static Graphics FromHwndInternal(IntPtr hwnd)
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateFromHWND(new HandleRef(null, hwnd), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Graphics(zero);
        }

        public static Graphics FromImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if ((image.PixelFormat & PixelFormat.Indexed) != PixelFormat.Undefined)
            {
                throw new Exception(System.Drawing.SR.GetString("GdiplusCannotCreateGraphicsFromIndexedPixelFormat"));
            }
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipGetImageGraphicsContext(new HandleRef(image, image.nativeImage), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Graphics(zero) { backingImage = image };
        }

        [EditorBrowsable(EditorBrowsableState.Never), StrongNameIdentityPermission(SecurityAction.LinkDemand, Name="System.Windows.Forms", PublicKey="0x00000000000000000400000000000000")]
        public object GetContextInfo()
        {
            Region clip = this.Clip;
            Matrix transform = this.Transform;
            PointF empty = PointF.Empty;
            PointF tf2 = PointF.Empty;
            if (!transform.IsIdentity)
            {
                float[] elements = transform.Elements;
                empty.X = elements[4];
                empty.Y = elements[5];
            }
            GraphicsContext previousContext = this.previousContext;
            while (previousContext != null)
            {
                if (!previousContext.TransformOffset.IsEmpty)
                {
                    transform.Translate(previousContext.TransformOffset.X, previousContext.TransformOffset.Y);
                }
                if (!empty.IsEmpty)
                {
                    clip.Translate(empty.X, empty.Y);
                    tf2.X += empty.X;
                    tf2.Y += empty.Y;
                }
                if (previousContext.Clip != null)
                {
                    clip.Intersect(previousContext.Clip);
                }
                empty = previousContext.TransformOffset;
                do
                {
                    previousContext = previousContext.Previous;
                }
                while (((previousContext != null) && previousContext.Next.IsCumulative) && previousContext.IsCumulative);
            }
            if (!tf2.IsEmpty)
            {
                clip.Translate(-tf2.X, -tf2.Y);
            }
            return new object[] { clip, transform };
        }

        public static IntPtr GetHalftonePalette()
        {
            if (halftonePalette == IntPtr.Zero)
            {
                lock (syncObject)
                {
                    if (halftonePalette == IntPtr.Zero)
                    {
                        if (Environment.OSVersion.Platform != PlatformID.Win32Windows)
                        {
                            AppDomain.CurrentDomain.DomainUnload += new EventHandler(Graphics.OnDomainUnload);
                        }
                        AppDomain.CurrentDomain.ProcessExit += new EventHandler(Graphics.OnDomainUnload);
                        halftonePalette = SafeNativeMethods.Gdip.GdipCreateHalftonePalette();
                    }
                }
            }
            return halftonePalette;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public IntPtr GetHdc()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipGetDC(new HandleRef(this, this.NativeGraphics), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.nativeHdc = zero;
            return this.nativeHdc;
        }

        public Color GetNearestColor(Color color)
        {
            int num = color.ToArgb();
            int status = SafeNativeMethods.Gdip.GdipGetNearestColor(new HandleRef(this, this.NativeGraphics), ref num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return Color.FromArgb(num);
        }

        private void IgnoreMetafileErrors(Image image, ref int errorStatus)
        {
            if ((errorStatus != 0) && image.RawFormat.Equals(ImageFormat.Emf))
            {
                errorStatus = 0;
            }
        }

        public void IntersectClip(Rectangle rect)
        {
            int status = SafeNativeMethods.Gdip.GdipSetClipRectI(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void IntersectClip(RectangleF rect)
        {
            int status = SafeNativeMethods.Gdip.GdipSetClipRect(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void IntersectClip(Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipSetClipRegion(new HandleRef(this, this.NativeGraphics), new HandleRef(region, region.nativeRegion), CombineMode.Intersect);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public bool IsVisible(Point point)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisiblePointI(new HandleRef(this, this.NativeGraphics), point.X, point.Y, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(PointF point)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisiblePoint(new HandleRef(this, this.NativeGraphics), point.X, point.Y, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(Rectangle rect)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisibleRectI(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(RectangleF rect)
        {
            int num;
            int status = SafeNativeMethods.Gdip.GdipIsVisibleRect(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return (num != 0);
        }

        public bool IsVisible(int x, int y)
        {
            return this.IsVisible(new Point(x, y));
        }

        public bool IsVisible(float x, float y)
        {
            return this.IsVisible(new PointF(x, y));
        }

        public bool IsVisible(int x, int y, int width, int height)
        {
            return this.IsVisible(new Rectangle(x, y, width, height));
        }

        public bool IsVisible(float x, float y, float width, float height)
        {
            return this.IsVisible(new RectangleF(x, y, width, height));
        }

        public Region[] MeasureCharacterRanges(string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
        {
            int num;
            if ((text == null) || (text.Length == 0))
            {
                return new Region[0];
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            int status = SafeNativeMethods.Gdip.GdipGetStringFormatMeasurableCharacterRangeCount(new HandleRef(stringFormat, (stringFormat == null) ? IntPtr.Zero : stringFormat.nativeFormat), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            IntPtr[] region = new IntPtr[num];
            GPRECTF gprectf = new GPRECTF(layoutRect);
            Region[] regionArray = new Region[num];
            for (int i = 0; i < num; i++)
            {
                regionArray[i] = new Region();
                region[i] = regionArray[i].nativeRegion;
            }
            status = SafeNativeMethods.Gdip.GdipMeasureCharacterRanges(new HandleRef(this, this.NativeGraphics), text, text.Length, new HandleRef(font, font.NativeFont), ref gprectf, new HandleRef(stringFormat, (stringFormat == null) ? IntPtr.Zero : stringFormat.nativeFormat), num, region);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return regionArray;
        }

        public SizeF MeasureString(string text, Font font)
        {
            return this.MeasureString(text, font, new SizeF(0f, 0f));
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea)
        {
            return this.MeasureString(text, font, layoutArea, null);
        }

        public SizeF MeasureString(string text, Font font, int width)
        {
            return this.MeasureString(text, font, new SizeF((float) width, 999999f));
        }

        public SizeF MeasureString(string text, Font font, PointF origin, StringFormat stringFormat)
        {
            int num;
            int num2;
            if ((text == null) || (text.Length == 0))
            {
                return new SizeF(0f, 0f);
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            GPRECTF layoutRect = new GPRECTF();
            GPRECTF boundingBox = new GPRECTF();
            layoutRect.X = origin.X;
            layoutRect.Y = origin.Y;
            layoutRect.Width = 0f;
            layoutRect.Height = 0f;
            int status = SafeNativeMethods.Gdip.GdipMeasureString(new HandleRef(this, this.NativeGraphics), text, text.Length, new HandleRef(font, font.NativeFont), ref layoutRect, new HandleRef(stringFormat, (stringFormat == null) ? IntPtr.Zero : stringFormat.nativeFormat), ref boundingBox, out num, out num2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return boundingBox.SizeF;
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
        {
            int num;
            int num2;
            if ((text == null) || (text.Length == 0))
            {
                return new SizeF(0f, 0f);
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            GPRECTF layoutRect = new GPRECTF(0f, 0f, layoutArea.Width, layoutArea.Height);
            GPRECTF boundingBox = new GPRECTF();
            int status = SafeNativeMethods.Gdip.GdipMeasureString(new HandleRef(this, this.NativeGraphics), text, text.Length, new HandleRef(font, font.NativeFont), ref layoutRect, new HandleRef(stringFormat, (stringFormat == null) ? IntPtr.Zero : stringFormat.nativeFormat), ref boundingBox, out num, out num2);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return boundingBox.SizeF;
        }

        public SizeF MeasureString(string text, Font font, int width, StringFormat format)
        {
            return this.MeasureString(text, font, new SizeF((float) width, 999999f), format);
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
        {
            if ((text == null) || (text.Length == 0))
            {
                charactersFitted = 0;
                linesFilled = 0;
                return new SizeF(0f, 0f);
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            GPRECTF layoutRect = new GPRECTF(0f, 0f, layoutArea.Width, layoutArea.Height);
            GPRECTF boundingBox = new GPRECTF();
            int status = SafeNativeMethods.Gdip.GdipMeasureString(new HandleRef(this, this.NativeGraphics), text, text.Length, new HandleRef(font, font.NativeFont), ref layoutRect, new HandleRef(stringFormat, (stringFormat == null) ? IntPtr.Zero : stringFormat.nativeFormat), ref boundingBox, out charactersFitted, out linesFilled);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return boundingBox.SizeF;
        }

        public void MultiplyTransform(Matrix matrix)
        {
            this.MultiplyTransform(matrix, MatrixOrder.Prepend);
        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }
            int status = SafeNativeMethods.Gdip.GdipMultiplyWorldTransform(new HandleRef(this, this.NativeGraphics), new HandleRef(matrix, matrix.nativeMatrix), order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        [PrePrepareMethod]
        private static void OnDomainUnload(object sender, EventArgs e)
        {
            if (halftonePalette != IntPtr.Zero)
            {
                SafeNativeMethods.IntDeleteObject(new HandleRef(null, halftonePalette));
                halftonePalette = IntPtr.Zero;
            }
        }

        private void PopContext(int currentContextState)
        {
            for (GraphicsContext context = this.previousContext; context != null; context = context.Previous)
            {
                if (context.State == currentContextState)
                {
                    this.previousContext = context.Previous;
                    context.Dispose();
                    return;
                }
            }
        }

        private void PushContext(GraphicsContext context)
        {
            if (this.previousContext != null)
            {
                context.Previous = this.previousContext;
                this.previousContext.Next = context;
            }
            this.previousContext = context;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public void ReleaseHdc()
        {
            this.ReleaseHdcInternal(this.nativeHdc);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void ReleaseHdc(IntPtr hdc)
        {
            System.Drawing.IntSecurity.Win32HandleManipulation.Demand();
            this.ReleaseHdcInternal(hdc);
        }

        [EditorBrowsable(EditorBrowsableState.Never), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public void ReleaseHdcInternal(IntPtr hdc)
        {
            int status = SafeNativeMethods.Gdip.GdipReleaseDC(new HandleRef(this, this.NativeGraphics), new HandleRef(null, hdc));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.nativeHdc = IntPtr.Zero;
        }

        public void ResetClip()
        {
            int status = SafeNativeMethods.Gdip.GdipResetClip(new HandleRef(this, this.NativeGraphics));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void ResetTransform()
        {
            int status = SafeNativeMethods.Gdip.GdipResetWorldTransform(new HandleRef(this, this.NativeGraphics));
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void Restore(System.Drawing.Drawing2D.GraphicsState gstate)
        {
            int status = SafeNativeMethods.Gdip.GdipRestoreGraphics(new HandleRef(this, this.NativeGraphics), gstate.nativeState);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.PopContext(gstate.nativeState);
        }

        public void RotateTransform(float angle)
        {
            this.RotateTransform(angle, MatrixOrder.Prepend);
        }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipRotateWorldTransform(new HandleRef(this, this.NativeGraphics), angle, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public System.Drawing.Drawing2D.GraphicsState Save()
        {
            GraphicsContext context = new GraphicsContext(this);
            int state = 0;
            int status = SafeNativeMethods.Gdip.GdipSaveGraphics(new HandleRef(this, this.NativeGraphics), out state);
            if (status != 0)
            {
                context.Dispose();
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            context.State = state;
            context.IsCumulative = true;
            this.PushContext(context);
            return new System.Drawing.Drawing2D.GraphicsState(state);
        }

        public void ScaleTransform(float sx, float sy)
        {
            this.ScaleTransform(sx, sy, MatrixOrder.Prepend);
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipScaleWorldTransform(new HandleRef(this, this.NativeGraphics), sx, sy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetClip(GraphicsPath path)
        {
            this.SetClip(path, CombineMode.Replace);
        }

        public void SetClip(Graphics g)
        {
            this.SetClip(g, CombineMode.Replace);
        }

        public void SetClip(Rectangle rect)
        {
            this.SetClip(rect, CombineMode.Replace);
        }

        public void SetClip(RectangleF rect)
        {
            this.SetClip(rect, CombineMode.Replace);
        }

        public void SetClip(GraphicsPath path, CombineMode combineMode)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            int status = SafeNativeMethods.Gdip.GdipSetClipPath(new HandleRef(this, this.NativeGraphics), new HandleRef(path, path.nativePath), combineMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetClip(Graphics g, CombineMode combineMode)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            int status = SafeNativeMethods.Gdip.GdipSetClipGraphics(new HandleRef(this, this.NativeGraphics), new HandleRef(g, g.NativeGraphics), combineMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetClip(Rectangle rect, CombineMode combineMode)
        {
            int status = SafeNativeMethods.Gdip.GdipSetClipRectI(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, combineMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetClip(RectangleF rect, CombineMode combineMode)
        {
            int status = SafeNativeMethods.Gdip.GdipSetClipRect(new HandleRef(this, this.NativeGraphics), rect.X, rect.Y, rect.Width, rect.Height, combineMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetClip(Region region, CombineMode combineMode)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            int status = SafeNativeMethods.Gdip.GdipSetClipRegion(new HandleRef(this, this.NativeGraphics), new HandleRef(region, region.nativeRegion), combineMode);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, Point[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr points = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            int status = SafeNativeMethods.Gdip.GdipTransformPointsI(new HandleRef(this, this.NativeGraphics), (int) destSpace, (int) srcSpace, points, pts.Length);
            try
            {
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                Point[] pointArray = SafeNativeMethods.Gdip.ConvertGPPOINTArray(points, pts.Length);
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = pointArray[i];
                }
            }
            finally
            {
                Marshal.FreeHGlobal(points);
            }
        }

        public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }
            IntPtr points = SafeNativeMethods.Gdip.ConvertPointToMemory(pts);
            int status = SafeNativeMethods.Gdip.GdipTransformPoints(new HandleRef(this, this.NativeGraphics), (int) destSpace, (int) srcSpace, points, pts.Length);
            try
            {
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                PointF[] tfArray = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(points, pts.Length);
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = tfArray[i];
                }
            }
            finally
            {
                Marshal.FreeHGlobal(points);
            }
        }

        public void TranslateClip(int dx, int dy)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateClip(new HandleRef(this, this.NativeGraphics), (float) dx, (float) dy);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void TranslateClip(float dx, float dy)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateClip(new HandleRef(this, this.NativeGraphics), dx, dy);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void TranslateTransform(float dx, float dy)
        {
            this.TranslateTransform(dx, dy, MatrixOrder.Prepend);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            int status = SafeNativeMethods.Gdip.GdipTranslateWorldTransform(new HandleRef(this, this.NativeGraphics), dx, dy, order);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public Region Clip
        {
            get
            {
                Region wrapper = new Region();
                int status = SafeNativeMethods.Gdip.GdipGetClip(new HandleRef(this, this.NativeGraphics), new HandleRef(wrapper, wrapper.nativeRegion));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return wrapper;
            }
            set
            {
                this.SetClip(value, CombineMode.Replace);
            }
        }

        public RectangleF ClipBounds
        {
            get
            {
                GPRECTF rect = new GPRECTF();
                int status = SafeNativeMethods.Gdip.GdipGetClipBounds(new HandleRef(this, this.NativeGraphics), ref rect);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return rect.ToRectangleF();
            }
        }

        public System.Drawing.Drawing2D.CompositingMode CompositingMode
        {
            get
            {
                int compositeMode = 0;
                int status = SafeNativeMethods.Gdip.GdipGetCompositingMode(new HandleRef(this, this.NativeGraphics), out compositeMode);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.CompositingMode) compositeMode;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.CompositingMode));
                }
                int status = SafeNativeMethods.Gdip.GdipSetCompositingMode(new HandleRef(this, this.NativeGraphics), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Drawing2D.CompositingQuality CompositingQuality
        {
            get
            {
                System.Drawing.Drawing2D.CompositingQuality quality;
                int status = SafeNativeMethods.Gdip.GdipGetCompositingQuality(new HandleRef(this, this.NativeGraphics), out quality);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return quality;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, -1, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.CompositingQuality));
                }
                int status = SafeNativeMethods.Gdip.GdipSetCompositingQuality(new HandleRef(this, this.NativeGraphics), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public float DpiX
        {
            get
            {
                float[] dpi = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetDpiX(new HandleRef(this, this.NativeGraphics), dpi);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return dpi[0];
            }
        }

        public float DpiY
        {
            get
            {
                float[] dpi = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetDpiY(new HandleRef(this, this.NativeGraphics), dpi);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return dpi[0];
            }
        }

        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
        {
            get
            {
                int mode = 0;
                int status = SafeNativeMethods.Gdip.GdipGetInterpolationMode(new HandleRef(this, this.NativeGraphics), out mode);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (System.Drawing.Drawing2D.InterpolationMode) mode;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, -1, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.InterpolationMode));
                }
                int status = SafeNativeMethods.Gdip.GdipSetInterpolationMode(new HandleRef(this, this.NativeGraphics), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public bool IsClipEmpty
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipIsClipEmpty(new HandleRef(this, this.NativeGraphics), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (num != 0);
            }
        }

        public bool IsVisibleClipEmpty
        {
            get
            {
                int num;
                int status = SafeNativeMethods.Gdip.GdipIsVisibleClipEmpty(new HandleRef(this, this.NativeGraphics), out num);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (num != 0);
            }
        }

        internal IntPtr NativeGraphics
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.nativeGraphics;
            }
        }

        public float PageScale
        {
            get
            {
                float[] scale = new float[1];
                int status = SafeNativeMethods.Gdip.GdipGetPageScale(new HandleRef(this, this.NativeGraphics), scale);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return scale[0];
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetPageScale(new HandleRef(this, this.NativeGraphics), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public GraphicsUnit PageUnit
        {
            get
            {
                int unit = 0;
                int status = SafeNativeMethods.Gdip.GdipGetPageUnit(new HandleRef(this, this.NativeGraphics), out unit);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return (GraphicsUnit) unit;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 6))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(GraphicsUnit));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPageUnit(new HandleRef(this, this.NativeGraphics), (int) value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Drawing2D.PixelOffsetMode PixelOffsetMode
        {
            get
            {
                System.Drawing.Drawing2D.PixelOffsetMode pixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                int status = SafeNativeMethods.Gdip.GdipGetPixelOffsetMode(new HandleRef(this, this.NativeGraphics), out pixelOffsetMode);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return pixelOffsetMode;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, -1, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.PixelOffsetMode));
                }
                int status = SafeNativeMethods.Gdip.GdipSetPixelOffsetMode(new HandleRef(this, this.NativeGraphics), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        internal object PrintingHelper
        {
            get
            {
                return this.printingHelper;
            }
            set
            {
                this.printingHelper = value;
            }
        }

        public Point RenderingOrigin
        {
            get
            {
                int num;
                int num2;
                int status = SafeNativeMethods.Gdip.GdipGetRenderingOrigin(new HandleRef(this, this.NativeGraphics), out num, out num2);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return new Point(num, num2);
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetRenderingOrigin(new HandleRef(this, this.NativeGraphics), value.X, value.Y);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
        {
            get
            {
                System.Drawing.Drawing2D.SmoothingMode smoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                int status = SafeNativeMethods.Gdip.GdipGetSmoothingMode(new HandleRef(this, this.NativeGraphics), out smoothingMode);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return smoothingMode;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, -1, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Drawing2D.SmoothingMode));
                }
                int status = SafeNativeMethods.Gdip.GdipSetSmoothingMode(new HandleRef(this, this.NativeGraphics), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public int TextContrast
        {
            get
            {
                int textContrast = 0;
                int status = SafeNativeMethods.Gdip.GdipGetTextContrast(new HandleRef(this, this.NativeGraphics), out textContrast);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return textContrast;
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetTextContrast(new HandleRef(this, this.NativeGraphics), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public System.Drawing.Text.TextRenderingHint TextRenderingHint
        {
            get
            {
                System.Drawing.Text.TextRenderingHint systemDefault = System.Drawing.Text.TextRenderingHint.SystemDefault;
                int status = SafeNativeMethods.Gdip.GdipGetTextRenderingHint(new HandleRef(this, this.NativeGraphics), out systemDefault);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return systemDefault;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Text.TextRenderingHint));
                }
                int status = SafeNativeMethods.Gdip.GdipSetTextRenderingHint(new HandleRef(this, this.NativeGraphics), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public Matrix Transform
        {
            get
            {
                Matrix wrapper = new Matrix();
                int status = SafeNativeMethods.Gdip.GdipGetWorldTransform(new HandleRef(this, this.NativeGraphics), new HandleRef(wrapper, wrapper.nativeMatrix));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return wrapper;
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetWorldTransform(new HandleRef(this, this.NativeGraphics), new HandleRef(value, value.nativeMatrix));
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public RectangleF VisibleClipBounds
        {
            get
            {
                if (this.PrintingHelper != null)
                {
                    PrintPreviewGraphics printingHelper = this.PrintingHelper as PrintPreviewGraphics;
                    if (printingHelper != null)
                    {
                        return printingHelper.VisibleClipBounds;
                    }
                }
                GPRECTF rect = new GPRECTF();
                int status = SafeNativeMethods.Gdip.GdipGetVisibleClipBounds(new HandleRef(this, this.NativeGraphics), ref rect);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return rect.ToRectangleF();
            }
        }

        public delegate bool DrawImageAbort(IntPtr callbackdata);

        public delegate bool EnumerateMetafileProc(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr data, PlayRecordCallback callbackData);
    }
}

