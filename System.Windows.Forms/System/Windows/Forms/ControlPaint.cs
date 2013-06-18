namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.Internal;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.VisualStyles;

    public sealed class ControlPaint
    {
        private static readonly System.Drawing.ContentAlignment anyBottom = (System.Drawing.ContentAlignment.BottomRight | System.Drawing.ContentAlignment.BottomCenter | System.Drawing.ContentAlignment.BottomLeft);
        private static readonly System.Drawing.ContentAlignment anyCenter = (System.Drawing.ContentAlignment.BottomCenter | System.Drawing.ContentAlignment.MiddleCenter | System.Drawing.ContentAlignment.TopCenter);
        private static readonly System.Drawing.ContentAlignment anyMiddle = (System.Drawing.ContentAlignment.MiddleRight | System.Drawing.ContentAlignment.MiddleCenter | System.Drawing.ContentAlignment.MiddleLeft);
        private static readonly System.Drawing.ContentAlignment anyRight = (System.Drawing.ContentAlignment.BottomRight | System.Drawing.ContentAlignment.MiddleRight | System.Drawing.ContentAlignment.TopRight);
        [ThreadStatic]
        private static Bitmap checkImage;
        [ThreadStatic]
        private static ImageAttributes disabledImageAttr;
        [ThreadStatic]
        private static Pen focusPen;
        private static Color focusPenColor;
        [ThreadStatic]
        private static Pen focusPenInvert;
        [ThreadStatic]
        private static Brush frameBrushActive;
        [ThreadStatic]
        private static Brush frameBrushSelected;
        private static Color frameColorActive;
        private static Color frameColorSelected;
        private static Brush grabBrushPrimary;
        private static Brush grabBrushSecondary;
        private static Pen grabPenPrimary;
        private static Pen grabPenSecondary;
        [ThreadStatic]
        private static Brush gridBrush;
        private static bool gridInvert;
        private static Size gridSize;

        private ControlPaint()
        {
        }

        private static DashStyle BorderStyleToDashStyle(ButtonBorderStyle borderStyle)
        {
            switch (borderStyle)
            {
                case ButtonBorderStyle.Dotted:
                    return DashStyle.Dot;

                case ButtonBorderStyle.Dashed:
                    return DashStyle.Dash;

                case ButtonBorderStyle.Solid:
                    return DashStyle.Solid;
            }
            return DashStyle.Solid;
        }

        internal static Rectangle CalculateBackgroundImageRectangle(Rectangle bounds, Image backgroundImage, ImageLayout imageLayout)
        {
            Rectangle rectangle = bounds;
            if (backgroundImage != null)
            {
                switch (imageLayout)
                {
                    case ImageLayout.None:
                        rectangle.Size = backgroundImage.Size;
                        return rectangle;

                    case ImageLayout.Tile:
                        return rectangle;

                    case ImageLayout.Center:
                    {
                        rectangle.Size = backgroundImage.Size;
                        Size size = bounds.Size;
                        if (size.Width > rectangle.Width)
                        {
                            rectangle.X = (size.Width - rectangle.Width) / 2;
                        }
                        if (size.Height > rectangle.Height)
                        {
                            rectangle.Y = (size.Height - rectangle.Height) / 2;
                        }
                        return rectangle;
                    }
                    case ImageLayout.Stretch:
                        rectangle.Size = bounds.Size;
                        return rectangle;

                    case ImageLayout.Zoom:
                    {
                        Size size2 = backgroundImage.Size;
                        float num = ((float) bounds.Width) / ((float) size2.Width);
                        float num2 = ((float) bounds.Height) / ((float) size2.Height);
                        if (num >= num2)
                        {
                            rectangle.Height = bounds.Height;
                            rectangle.Width = (int) ((size2.Width * num2) + 0.5);
                            if (bounds.X >= 0)
                            {
                                rectangle.X = (bounds.Width - rectangle.Width) / 2;
                            }
                            return rectangle;
                        }
                        rectangle.Width = bounds.Width;
                        rectangle.Height = (int) ((size2.Height * num) + 0.5);
                        if (bounds.Y >= 0)
                        {
                            rectangle.Y = (bounds.Height - rectangle.Height) / 2;
                        }
                        return rectangle;
                    }
                }
            }
            return rectangle;
        }

        internal static void CopyPixels(IntPtr sourceHwnd, IDeviceContext targetDC, Point sourceLocation, Point destinationLocation, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
        {
            int width = blockRegionSize.Width;
            int height = blockRegionSize.Height;
            DeviceContext context = DeviceContext.FromHwnd(sourceHwnd);
            HandleRef hDC = new HandleRef(null, targetDC.GetHdc());
            HandleRef hSrcDC = new HandleRef(null, context.Hdc);
            try
            {
                if (!System.Windows.Forms.SafeNativeMethods.BitBlt(hDC, destinationLocation.X, destinationLocation.Y, width, height, hSrcDC, sourceLocation.X, sourceLocation.Y, (int) copyPixelOperation))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                targetDC.ReleaseHdc();
                context.Dispose();
            }
        }

        private static IntPtr CreateBitmapInfo(Bitmap bitmap, IntPtr hdcS)
        {
            System.Windows.Forms.NativeMethods.BITMAPINFOHEADER bitmapinfoheader;
            bitmapinfoheader = new System.Windows.Forms.NativeMethods.BITMAPINFOHEADER {
                biSize = Marshal.SizeOf(bitmapinfoheader),
                biWidth = bitmap.Width,
                biHeight = bitmap.Height,
                biPlanes = 1,
                biBitCount = 0x10,
                biCompression = 0
            };
            int nEntries = 0;
            IntPtr handle = System.Windows.Forms.SafeNativeMethods.CreateHalftonePalette(new HandleRef(null, hdcS));
            System.Windows.Forms.UnsafeNativeMethods.GetObject(new HandleRef(null, handle), 2, ref nEntries);
            int[] lppe = new int[nEntries];
            System.Windows.Forms.SafeNativeMethods.GetPaletteEntries(new HandleRef(null, handle), 0, nEntries, lppe);
            int[] source = new int[nEntries];
            for (int i = 0; i < nEntries; i++)
            {
                int num3 = lppe[i];
                source[i] = (((num3 & -16777216) >> ((6 + (num3 & 0xff0000)) & 0x1f)) >> ((4 + (num3 & 0xff00)) & 0x1f)) >> 2;
            }
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(bitmapinfoheader) + (nEntries * 4));
            Marshal.StructureToPtr(bitmapinfoheader, ptr, false);
            Marshal.Copy(source, 0, (IntPtr) (((long) ptr) + Marshal.SizeOf(bitmapinfoheader)), nEntries);
            return ptr;
        }

        internal static IntPtr CreateHalftoneHBRUSH()
        {
            short[] lpvBits = new short[8];
            for (int i = 0; i < 8; i++)
            {
                lpvBits[i] = (short) (((int) 0x5555) << (i & 1));
            }
            IntPtr handle = System.Windows.Forms.SafeNativeMethods.CreateBitmap(8, 8, 1, 1, lpvBits);
            System.Windows.Forms.NativeMethods.LOGBRUSH lb = new System.Windows.Forms.NativeMethods.LOGBRUSH {
                lbColor = ColorTranslator.ToWin32(Color.Black),
                lbStyle = 3,
                lbHatch = handle
            };
            IntPtr ptr2 = System.Windows.Forms.SafeNativeMethods.CreateBrushIndirect(lb);
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
            return ptr2;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static IntPtr CreateHBitmap16Bit(Bitmap bitmap, Color background)
        {
            Size size = bitmap.Size;
            using (DeviceContext context = DeviceContext.ScreenDC)
            {
                IntPtr hdc = context.Hdc;
                using (DeviceContext context2 = DeviceContext.FromCompatibleDC(hdc))
                {
                    IntPtr handle = context2.Hdc;
                    byte[] ppvBits = new byte[bitmap.Width * bitmap.Height];
                    IntPtr ptr4 = CreateBitmapInfo(bitmap, hdc);
                    IntPtr ptr = System.Windows.Forms.SafeNativeMethods.CreateDIBSection(new HandleRef(null, hdc), new HandleRef(null, ptr4), 0, ppvBits, IntPtr.Zero, 0);
                    Marshal.FreeCoTaskMem(ptr4);
                    if (ptr == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                    try
                    {
                        IntPtr ptr5 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr));
                        if (ptr5 == IntPtr.Zero)
                        {
                            throw new Win32Exception();
                        }
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr5));
                        using (Graphics graphics = Graphics.FromHdcInternal(handle))
                        {
                            using (Brush brush = new SolidBrush(background))
                            {
                                graphics.FillRectangle(brush, 0, 0, size.Width, size.Height);
                            }
                            graphics.DrawImage(bitmap, 0, 0, size.Width, size.Height);
                        }
                        return ptr;
                    }
                    catch
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr));
                        throw;
                    }
                    return ptr;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static IntPtr CreateHBitmapColorMask(Bitmap bitmap, IntPtr monochromeMask)
        {
            Size size = bitmap.Size;
            IntPtr hbitmap = bitmap.GetHbitmap();
            IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, dC));
            IntPtr ptr4 = System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, dC));
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            IntPtr ptr5 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, monochromeMask));
            IntPtr ptr6 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, ptr4), new HandleRef(null, hbitmap));
            System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, ptr4), 0xffffff);
            System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(null, ptr4), 0);
            System.Windows.Forms.SafeNativeMethods.BitBlt(new HandleRef(null, ptr4), 0, 0, size.Width, size.Height, new HandleRef(null, handle), 0, 0, 0x220326);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr5));
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, ptr4), new HandleRef(null, ptr6));
            System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(new HandleRef(null, handle));
            System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(new HandleRef(null, ptr4));
            return System.Internal.HandleCollector.Add(hbitmap, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static IntPtr CreateHBitmapTransparencyMask(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }
            Size size = bitmap.Size;
            int width = bitmap.Width;
            int height = bitmap.Height;
            int num3 = width / 8;
            if ((width % 8) != 0)
            {
                num3++;
            }
            if ((num3 % 2) != 0)
            {
                num3++;
            }
            byte[] lpvBits = new byte[num3 * height];
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            for (int i = 0; i < height; i++)
            {
                IntPtr ptr = (IntPtr) (((long) bitmapdata.Scan0) + (i * bitmapdata.Stride));
                for (int j = 0; j < width; j++)
                {
                    if ((Marshal.ReadInt32(ptr, j * 4) >> 0x18) == 0)
                    {
                        int index = (num3 * i) + (j / 8);
                        lpvBits[index] = (byte) (lpvBits[index] | ((byte) (((int) 0x80) >> (j % 8))));
                    }
                }
            }
            bitmap.UnlockBits(bitmapdata);
            return System.Windows.Forms.SafeNativeMethods.CreateBitmap(size.Width, size.Height, 1, 1, lpvBits);
        }

        internal static StringFormat CreateStringFormat(Control ctl, System.Drawing.ContentAlignment textAlign, bool showEllipsis, bool useMnemonic)
        {
            StringFormat format = StringFormatForAlignment(textAlign);
            if (ctl.RightToLeft == RightToLeft.Yes)
            {
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }
            if (showEllipsis)
            {
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags |= StringFormatFlags.LineLimit;
            }
            if (!useMnemonic)
            {
                format.HotkeyPrefix = HotkeyPrefix.None;
            }
            else if (ctl.ShowKeyboardCues)
            {
                format.HotkeyPrefix = HotkeyPrefix.Show;
            }
            else
            {
                format.HotkeyPrefix = HotkeyPrefix.Hide;
            }
            if (ctl.AutoSize)
            {
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            }
            return format;
        }

        internal static TextFormatFlags CreateTextFormatFlags(Control ctl, System.Drawing.ContentAlignment textAlign, bool showEllipsis, bool useMnemonic)
        {
            textAlign = ctl.RtlTranslateContent(textAlign);
            TextFormatFlags flags = TextFormatFlagsForAlignmentGDI(textAlign) | (TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            if (showEllipsis)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }
            if (ctl.RightToLeft == RightToLeft.Yes)
            {
                flags |= TextFormatFlags.RightToLeft;
            }
            if (!useMnemonic)
            {
                return (flags | TextFormatFlags.NoPrefix);
            }
            if (!ctl.ShowKeyboardCues)
            {
                flags |= TextFormatFlags.HidePrefix;
            }
            return flags;
        }

        public static Color Dark(Color baseColor)
        {
            HLSColor color = new HLSColor(baseColor);
            return color.Darker(0.5f);
        }

        public static Color Dark(Color baseColor, float percOfDarkDark)
        {
            HLSColor color = new HLSColor(baseColor);
            return color.Darker(percOfDarkDark);
        }

        public static Color DarkDark(Color baseColor)
        {
            HLSColor color = new HLSColor(baseColor);
            return color.Darker(1f);
        }

        internal static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect)
        {
            DrawBackgroundImage(g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, Point.Empty, RightToLeft.No);
        }

        internal static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset)
        {
            DrawBackgroundImage(g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, RightToLeft.No);
        }

        internal static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            if (backgroundImageLayout == ImageLayout.Tile)
            {
                using (TextureBrush brush = new TextureBrush(backgroundImage, WrapMode.Tile))
                {
                    if (scrollOffset != Point.Empty)
                    {
                        Matrix transform = brush.Transform;
                        transform.Translate((float) scrollOffset.X, (float) scrollOffset.Y);
                        brush.Transform = transform;
                    }
                    g.FillRectangle(brush, clipRect);
                    return;
                }
            }
            Rectangle rect = CalculateBackgroundImageRectangle(bounds, backgroundImage, backgroundImageLayout);
            if ((rightToLeft == RightToLeft.Yes) && (backgroundImageLayout == ImageLayout.None))
            {
                rect.X += clipRect.Width - rect.Width;
            }
            using (SolidBrush brush2 = new SolidBrush(backColor))
            {
                g.FillRectangle(brush2, clipRect);
            }
            if (!clipRect.Contains(rect))
            {
                if ((backgroundImageLayout == ImageLayout.Stretch) || (backgroundImageLayout == ImageLayout.Zoom))
                {
                    rect.Intersect(clipRect);
                    g.DrawImage(backgroundImage, rect);
                }
                else if (backgroundImageLayout == ImageLayout.None)
                {
                    rect.Offset(clipRect.Location);
                    Rectangle destRect = rect;
                    destRect.Intersect(clipRect);
                    Rectangle rectangle3 = new Rectangle(Point.Empty, destRect.Size);
                    g.DrawImage(backgroundImage, destRect, rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height, GraphicsUnit.Pixel);
                }
                else
                {
                    Rectangle rectangle4 = rect;
                    rectangle4.Intersect(clipRect);
                    Rectangle rectangle5 = new Rectangle(new Point(rectangle4.X - rect.X, rectangle4.Y - rect.Y), rectangle4.Size);
                    g.DrawImage(backgroundImage, rectangle4, rectangle5.X, rectangle5.Y, rectangle5.Width, rectangle5.Height, GraphicsUnit.Pixel);
                }
            }
            else
            {
                ImageAttributes imageAttr = new ImageAttributes();
                imageAttr.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(backgroundImage, rect, 0, 0, backgroundImage.Width, backgroundImage.Height, GraphicsUnit.Pixel, imageAttr);
                imageAttr.Dispose();
            }
        }

        public static void DrawBorder(Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style)
        {
            switch (style)
            {
                case ButtonBorderStyle.None:
                    break;

                case ButtonBorderStyle.Dotted:
                case ButtonBorderStyle.Dashed:
                case ButtonBorderStyle.Solid:
                    DrawBorderSimple(graphics, bounds, color, style);
                    return;

                case ButtonBorderStyle.Inset:
                case ButtonBorderStyle.Outset:
                    DrawBorderComplex(graphics, bounds, color, style);
                    break;

                default:
                    return;
            }
        }

        public static void DrawBorder(Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth, ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle, Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor, int bottomWidth, ButtonBorderStyle bottomStyle)
        {
            Pen pen;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            int[] numArray = new int[topWidth];
            int[] numArray2 = new int[topWidth];
            int[] numArray3 = new int[leftWidth];
            int[] numArray4 = new int[leftWidth];
            int[] numArray5 = new int[bottomWidth];
            int[] numArray6 = new int[bottomWidth];
            int[] numArray7 = new int[rightWidth];
            int[] numArray8 = new int[rightWidth];
            float num = 0f;
            float num2 = 0f;
            if (leftWidth > 0)
            {
                num = ((float) topWidth) / ((float) leftWidth);
                num2 = ((float) bottomWidth) / ((float) leftWidth);
            }
            float num3 = 0f;
            float num4 = 0f;
            if (rightWidth > 0)
            {
                num3 = ((float) topWidth) / ((float) rightWidth);
                num4 = ((float) bottomWidth) / ((float) rightWidth);
            }
            HLSColor color = new HLSColor(topColor);
            HLSColor color2 = new HLSColor(leftColor);
            HLSColor color3 = new HLSColor(bottomColor);
            HLSColor color4 = new HLSColor(rightColor);
            if (topWidth > 0)
            {
                int index = 0;
                while (index < topWidth)
                {
                    int num6 = 0;
                    if (num > 0f)
                    {
                        num6 = (int) (((float) index) / num);
                    }
                    int num7 = 0;
                    if (num3 > 0f)
                    {
                        num7 = (int) (((float) index) / num3);
                    }
                    numArray[index] = bounds.X + num6;
                    numArray2[index] = ((bounds.X + bounds.Width) - num7) - 1;
                    if (leftWidth > 0)
                    {
                        numArray3[num6] = (bounds.Y + index) + 1;
                    }
                    if (rightWidth > 0)
                    {
                        numArray7[num7] = bounds.Y + index;
                    }
                    index++;
                }
                for (int i = index; i < leftWidth; i++)
                {
                    numArray3[i] = (bounds.Y + index) + 1;
                }
                for (int j = index; j < rightWidth; j++)
                {
                    numArray7[j] = bounds.Y + index;
                }
            }
            else
            {
                for (int k = 0; k < leftWidth; k++)
                {
                    numArray3[k] = bounds.Y;
                }
                for (int m = 0; m < rightWidth; m++)
                {
                    numArray7[m] = bounds.Y;
                }
            }
            if (bottomWidth > 0)
            {
                int num12 = 0;
                while (num12 < bottomWidth)
                {
                    int num13 = 0;
                    if (num2 > 0f)
                    {
                        num13 = (int) (((float) num12) / num2);
                    }
                    int num14 = 0;
                    if (num4 > 0f)
                    {
                        num14 = (int) (((float) num12) / num4);
                    }
                    numArray5[num12] = bounds.X + num13;
                    numArray6[num12] = ((bounds.X + bounds.Width) - num14) - 1;
                    if (leftWidth > 0)
                    {
                        numArray4[num13] = ((bounds.Y + bounds.Height) - num12) - 1;
                    }
                    if (rightWidth > 0)
                    {
                        numArray8[num14] = ((bounds.Y + bounds.Height) - num12) - 1;
                    }
                    num12++;
                }
                for (int n = num12; n < leftWidth; n++)
                {
                    numArray4[n] = ((bounds.Y + bounds.Height) - num12) - 1;
                }
                for (int num16 = num12; num16 < rightWidth; num16++)
                {
                    numArray8[num16] = ((bounds.Y + bounds.Height) - num12) - 1;
                }
            }
            else
            {
                for (int num17 = 0; num17 < leftWidth; num17++)
                {
                    numArray4[num17] = (bounds.Y + bounds.Height) - 1;
                }
                for (int num18 = 0; num18 < rightWidth; num18++)
                {
                    numArray8[num18] = (bounds.Y + bounds.Height) - 1;
                }
            }
            switch (topStyle)
            {
                case ButtonBorderStyle.Dotted:
                    pen = new Pen(topColor) {
                        DashStyle = DashStyle.Dot
                    };
                    for (int num19 = 0; num19 < topWidth; num19++)
                    {
                        graphics.DrawLine(pen, numArray[num19], bounds.Y + num19, numArray2[num19], bounds.Y + num19);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Dashed:
                    pen = new Pen(topColor) {
                        DashStyle = DashStyle.Dash
                    };
                    for (int num20 = 0; num20 < topWidth; num20++)
                    {
                        graphics.DrawLine(pen, numArray[num20], bounds.Y + num20, numArray2[num20], bounds.Y + num20);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Solid:
                    pen = new Pen(topColor) {
                        DashStyle = DashStyle.Solid
                    };
                    for (int num21 = 0; num21 < topWidth; num21++)
                    {
                        graphics.DrawLine(pen, numArray[num21], bounds.Y + num21, numArray2[num21], bounds.Y + num21);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Inset:
                {
                    float num22 = InfinityToOne(1f / ((float) (topWidth - 1)));
                    for (int num23 = 0; num23 < topWidth; num23++)
                    {
                        pen = new Pen(color.Darker(1f - (num23 * num22))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, numArray[num23], bounds.Y + num23, numArray2[num23], bounds.Y + num23);
                        pen.Dispose();
                    }
                    break;
                }
                case ButtonBorderStyle.Outset:
                {
                    float num24 = InfinityToOne(1f / ((float) (topWidth - 1)));
                    for (int num25 = 0; num25 < topWidth; num25++)
                    {
                        pen = new Pen(color.Lighter(1f - (num25 * num24))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, numArray[num25], bounds.Y + num25, numArray2[num25], bounds.Y + num25);
                        pen.Dispose();
                    }
                    break;
                }
            }
            pen = null;
            switch (leftStyle)
            {
                case ButtonBorderStyle.Dotted:
                    pen = new Pen(leftColor) {
                        DashStyle = DashStyle.Dot
                    };
                    for (int num26 = 0; num26 < leftWidth; num26++)
                    {
                        graphics.DrawLine(pen, bounds.X + num26, numArray3[num26], bounds.X + num26, numArray4[num26]);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Dashed:
                    pen = new Pen(leftColor) {
                        DashStyle = DashStyle.Dash
                    };
                    for (int num27 = 0; num27 < leftWidth; num27++)
                    {
                        graphics.DrawLine(pen, bounds.X + num27, numArray3[num27], bounds.X + num27, numArray4[num27]);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Solid:
                    pen = new Pen(leftColor) {
                        DashStyle = DashStyle.Solid
                    };
                    for (int num28 = 0; num28 < leftWidth; num28++)
                    {
                        graphics.DrawLine(pen, bounds.X + num28, numArray3[num28], bounds.X + num28, numArray4[num28]);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Inset:
                {
                    float num29 = InfinityToOne(1f / ((float) (leftWidth - 1)));
                    for (int num30 = 0; num30 < leftWidth; num30++)
                    {
                        pen = new Pen(color2.Darker(1f - (num30 * num29))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, bounds.X + num30, numArray3[num30], bounds.X + num30, numArray4[num30]);
                        pen.Dispose();
                    }
                    break;
                }
                case ButtonBorderStyle.Outset:
                {
                    float num31 = InfinityToOne(1f / ((float) (leftWidth - 1)));
                    for (int num32 = 0; num32 < leftWidth; num32++)
                    {
                        pen = new Pen(color2.Lighter(1f - (num32 * num31))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, bounds.X + num32, numArray3[num32], bounds.X + num32, numArray4[num32]);
                        pen.Dispose();
                    }
                    break;
                }
            }
            pen = null;
            switch (bottomStyle)
            {
                case ButtonBorderStyle.Dotted:
                    pen = new Pen(bottomColor) {
                        DashStyle = DashStyle.Dot
                    };
                    for (int num33 = 0; num33 < bottomWidth; num33++)
                    {
                        graphics.DrawLine(pen, numArray5[num33], ((bounds.Y + bounds.Height) - 1) - num33, numArray6[num33], ((bounds.Y + bounds.Height) - 1) - num33);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Dashed:
                    pen = new Pen(bottomColor) {
                        DashStyle = DashStyle.Dash
                    };
                    for (int num34 = 0; num34 < bottomWidth; num34++)
                    {
                        graphics.DrawLine(pen, numArray5[num34], ((bounds.Y + bounds.Height) - 1) - num34, numArray6[num34], ((bounds.Y + bounds.Height) - 1) - num34);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Solid:
                    pen = new Pen(bottomColor) {
                        DashStyle = DashStyle.Solid
                    };
                    for (int num35 = 0; num35 < bottomWidth; num35++)
                    {
                        graphics.DrawLine(pen, numArray5[num35], ((bounds.Y + bounds.Height) - 1) - num35, numArray6[num35], ((bounds.Y + bounds.Height) - 1) - num35);
                    }
                    pen.Dispose();
                    break;

                case ButtonBorderStyle.Inset:
                {
                    float num36 = InfinityToOne(1f / ((float) (bottomWidth - 1)));
                    for (int num37 = 0; num37 < bottomWidth; num37++)
                    {
                        pen = new Pen(color3.Lighter(1f - (num37 * num36))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, numArray5[num37], ((bounds.Y + bounds.Height) - 1) - num37, numArray6[num37], ((bounds.Y + bounds.Height) - 1) - num37);
                        pen.Dispose();
                    }
                    break;
                }
                case ButtonBorderStyle.Outset:
                {
                    float num38 = InfinityToOne(1f / ((float) (bottomWidth - 1)));
                    for (int num39 = 0; num39 < bottomWidth; num39++)
                    {
                        pen = new Pen(color3.Darker(1f - (num39 * num38))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, numArray5[num39], ((bounds.Y + bounds.Height) - 1) - num39, numArray6[num39], ((bounds.Y + bounds.Height) - 1) - num39);
                        pen.Dispose();
                    }
                    break;
                }
            }
            pen = null;
            switch (rightStyle)
            {
                case ButtonBorderStyle.None:
                    break;

                case ButtonBorderStyle.Dotted:
                    pen = new Pen(rightColor) {
                        DashStyle = DashStyle.Dot
                    };
                    for (int num40 = 0; num40 < rightWidth; num40++)
                    {
                        graphics.DrawLine(pen, ((bounds.X + bounds.Width) - 1) - num40, numArray7[num40], ((bounds.X + bounds.Width) - 1) - num40, numArray8[num40]);
                    }
                    pen.Dispose();
                    return;

                case ButtonBorderStyle.Dashed:
                    pen = new Pen(rightColor) {
                        DashStyle = DashStyle.Dash
                    };
                    for (int num41 = 0; num41 < rightWidth; num41++)
                    {
                        graphics.DrawLine(pen, ((bounds.X + bounds.Width) - 1) - num41, numArray7[num41], ((bounds.X + bounds.Width) - 1) - num41, numArray8[num41]);
                    }
                    pen.Dispose();
                    return;

                case ButtonBorderStyle.Solid:
                    pen = new Pen(rightColor) {
                        DashStyle = DashStyle.Solid
                    };
                    for (int num42 = 0; num42 < rightWidth; num42++)
                    {
                        graphics.DrawLine(pen, ((bounds.X + bounds.Width) - 1) - num42, numArray7[num42], ((bounds.X + bounds.Width) - 1) - num42, numArray8[num42]);
                    }
                    pen.Dispose();
                    return;

                case ButtonBorderStyle.Inset:
                {
                    float num43 = InfinityToOne(1f / ((float) (rightWidth - 1)));
                    for (int num44 = 0; num44 < rightWidth; num44++)
                    {
                        pen = new Pen(color4.Lighter(1f - (num44 * num43))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, ((bounds.X + bounds.Width) - 1) - num44, numArray7[num44], ((bounds.X + bounds.Width) - 1) - num44, numArray8[num44]);
                        pen.Dispose();
                    }
                    return;
                }
                case ButtonBorderStyle.Outset:
                {
                    float num45 = InfinityToOne(1f / ((float) (rightWidth - 1)));
                    for (int num46 = 0; num46 < rightWidth; num46++)
                    {
                        pen = new Pen(color4.Darker(1f - (num46 * num45))) {
                            DashStyle = DashStyle.Solid
                        };
                        graphics.DrawLine(pen, ((bounds.X + bounds.Width) - 1) - num46, numArray7[num46], ((bounds.X + bounds.Width) - 1) - num46, numArray8[num46]);
                        pen.Dispose();
                    }
                    break;
                }
                default:
                    return;
            }
        }

        public static void DrawBorder3D(Graphics graphics, Rectangle rectangle)
        {
            DrawBorder3D(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Border3DStyle.Etched, Border3DSide.Bottom | Border3DSide.Right | Border3DSide.Top | Border3DSide.Left);
        }

        public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style)
        {
            DrawBorder3D(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, style, Border3DSide.Bottom | Border3DSide.Right | Border3DSide.Top | Border3DSide.Left);
        }

        public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides)
        {
            DrawBorder3D(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, style, sides);
        }

        public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height)
        {
            DrawBorder3D(graphics, x, y, width, height, Border3DStyle.Etched, Border3DSide.Bottom | Border3DSide.Right | Border3DSide.Top | Border3DSide.Left);
        }

        public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height, Border3DStyle style)
        {
            DrawBorder3D(graphics, x, y, width, height, style, Border3DSide.Bottom | Border3DSide.Right | Border3DSide.Top | Border3DSide.Left);
        }

        public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height, Border3DStyle style, Border3DSide sides)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            int edge = ((int) style) & 15;
            int flags = (int) (sides | ((Border3DSide) ((int) (style & ~(Border3DStyle.Sunken | Border3DStyle.Raised)))));
            System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(x, y, width, height);
            if ((flags & 0x2000) == 0x2000)
            {
                Size size = SystemInformation.Border3DSize;
                rect.left -= size.Width;
                rect.right += size.Width;
                rect.top -= size.Height;
                rect.bottom += size.Height;
                flags &= -8193;
            }
            using (WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(graphics))
            {
                System.Windows.Forms.SafeNativeMethods.DrawEdge(new HandleRef(graphics2, graphics2.DeviceContext.Hdc), ref rect, edge, flags);
            }
        }

        private static void DrawBorderComplex(Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (style == ButtonBorderStyle.Inset)
            {
                HLSColor color2 = new HLSColor(color);
                Pen pen = new Pen(color2.Darker(1f));
                graphics.DrawLine(pen, bounds.X, bounds.Y, (bounds.X + bounds.Width) - 1, bounds.Y);
                graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X, (bounds.Y + bounds.Height) - 1);
                pen.Color = color2.Lighter(1f);
                graphics.DrawLine(pen, bounds.X, (bounds.Y + bounds.Height) - 1, (bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                graphics.DrawLine(pen, (bounds.X + bounds.Width) - 1, bounds.Y, (bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                pen.Color = color2.Lighter(0.5f);
                graphics.DrawLine(pen, (int) (bounds.X + 1), (int) (bounds.Y + 1), (int) ((bounds.X + bounds.Width) - 2), (int) (bounds.Y + 1));
                graphics.DrawLine(pen, (int) (bounds.X + 1), (int) (bounds.Y + 1), (int) (bounds.X + 1), (int) ((bounds.Y + bounds.Height) - 2));
                if (color.ToKnownColor() == SystemColors.Control.ToKnownColor())
                {
                    pen.Color = SystemColors.ControlLight;
                    graphics.DrawLine(pen, (int) (bounds.X + 1), (int) ((bounds.Y + bounds.Height) - 2), (int) ((bounds.X + bounds.Width) - 2), (int) ((bounds.Y + bounds.Height) - 2));
                    graphics.DrawLine(pen, (int) ((bounds.X + bounds.Width) - 2), (int) (bounds.Y + 1), (int) ((bounds.X + bounds.Width) - 2), (int) ((bounds.Y + bounds.Height) - 2));
                }
                pen.Dispose();
            }
            else
            {
                bool flag = color.ToKnownColor() == SystemColors.Control.ToKnownColor();
                HLSColor color3 = new HLSColor(color);
                Pen controlDarkDark = flag ? SystemPens.ControlLightLight : new Pen(color3.Lighter(1f));
                graphics.DrawLine(controlDarkDark, bounds.X, bounds.Y, (bounds.X + bounds.Width) - 1, bounds.Y);
                graphics.DrawLine(controlDarkDark, bounds.X, bounds.Y, bounds.X, (bounds.Y + bounds.Height) - 1);
                if (flag)
                {
                    controlDarkDark = SystemPens.ControlDarkDark;
                }
                else
                {
                    controlDarkDark.Color = color3.Darker(1f);
                }
                graphics.DrawLine(controlDarkDark, bounds.X, (bounds.Y + bounds.Height) - 1, (bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                graphics.DrawLine(controlDarkDark, (bounds.X + bounds.Width) - 1, bounds.Y, (bounds.X + bounds.Width) - 1, (bounds.Y + bounds.Height) - 1);
                if (flag)
                {
                    if (SystemInformation.HighContrast)
                    {
                        controlDarkDark = SystemPens.ControlLight;
                    }
                    else
                    {
                        controlDarkDark = SystemPens.Control;
                    }
                }
                else
                {
                    controlDarkDark.Color = color;
                }
                graphics.DrawLine(controlDarkDark, (int) (bounds.X + 1), (int) (bounds.Y + 1), (int) ((bounds.X + bounds.Width) - 2), (int) (bounds.Y + 1));
                graphics.DrawLine(controlDarkDark, (int) (bounds.X + 1), (int) (bounds.Y + 1), (int) (bounds.X + 1), (int) ((bounds.Y + bounds.Height) - 2));
                if (flag)
                {
                    controlDarkDark = SystemPens.ControlDark;
                }
                else
                {
                    controlDarkDark.Color = color3.Darker(0.5f);
                }
                graphics.DrawLine(controlDarkDark, (int) (bounds.X + 1), (int) ((bounds.Y + bounds.Height) - 2), (int) ((bounds.X + bounds.Width) - 2), (int) ((bounds.Y + bounds.Height) - 2));
                graphics.DrawLine(controlDarkDark, (int) ((bounds.X + bounds.Width) - 2), (int) (bounds.Y + 1), (int) ((bounds.X + bounds.Width) - 2), (int) ((bounds.Y + bounds.Height) - 2));
                if (!flag)
                {
                    controlDarkDark.Dispose();
                }
            }
        }

        private static void DrawBorderSimple(Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style)
        {
            Pen pen;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            bool flag = (style == ButtonBorderStyle.Solid) && color.IsSystemColor;
            if (flag)
            {
                pen = SystemPens.FromSystemColor(color);
            }
            else
            {
                pen = new Pen(color);
                if (style != ButtonBorderStyle.Solid)
                {
                    pen.DashStyle = BorderStyleToDashStyle(style);
                }
            }
            graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            if (!flag)
            {
                pen.Dispose();
            }
        }

        public static void DrawButton(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            DrawButton(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, state);
        }

        public static void DrawButton(Graphics graphics, int x, int y, int width, int height, ButtonState state)
        {
            DrawFrameControl(graphics, x, y, width, height, 4, 0x10 | state, Color.Empty, Color.Empty);
        }

        public static void DrawCaptionButton(Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state)
        {
            DrawCaptionButton(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, button, state);
        }

        public static void DrawCaptionButton(Graphics graphics, int x, int y, int width, int height, CaptionButton button, ButtonState state)
        {
            DrawFrameControl(graphics, x, y, width, height, 1, (int) (button | ((CaptionButton) ((int) state))), Color.Empty, Color.Empty);
        }

        public static void DrawCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            DrawCheckBox(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, state);
        }

        public static void DrawCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state)
        {
            if ((state & ButtonState.Flat) == ButtonState.Flat)
            {
                DrawFlatCheckBox(graphics, new Rectangle(x, y, width, height), state);
            }
            else
            {
                DrawFrameControl(graphics, x, y, width, height, 4, (int) state, Color.Empty, Color.Empty);
            }
        }

        public static void DrawComboButton(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            DrawComboButton(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, state);
        }

        public static void DrawComboButton(Graphics graphics, int x, int y, int width, int height, ButtonState state)
        {
            DrawFrameControl(graphics, x, y, width, height, 3, 5 | state, Color.Empty, Color.Empty);
        }

        public static void DrawContainerGrabHandle(Graphics graphics, Rectangle bounds)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            Brush white = Brushes.White;
            Pen black = Pens.Black;
            graphics.FillRectangle(white, (int) (bounds.Left + 1), (int) (bounds.Top + 1), (int) (bounds.Width - 2), (int) (bounds.Height - 2));
            graphics.DrawLine(black, bounds.X + 1, bounds.Y, bounds.Right - 2, bounds.Y);
            graphics.DrawLine(black, (int) (bounds.X + 1), (int) (bounds.Bottom - 1), (int) (bounds.Right - 2), (int) (bounds.Bottom - 1));
            graphics.DrawLine(black, bounds.X, bounds.Y + 1, bounds.X, bounds.Bottom - 2);
            graphics.DrawLine(black, (int) (bounds.Right - 1), (int) (bounds.Y + 1), (int) (bounds.Right - 1), (int) (bounds.Bottom - 2));
            int num = bounds.X + (bounds.Width / 2);
            int num2 = bounds.Y + (bounds.Height / 2);
            graphics.DrawLine(black, num, bounds.Y, num, bounds.Bottom - 2);
            graphics.DrawLine(black, bounds.X, num2, bounds.Right - 2, num2);
            graphics.DrawLine(black, (int) (num - 1), (int) (bounds.Y + 2), (int) (num + 1), (int) (bounds.Y + 2));
            graphics.DrawLine(black, (int) (num - 2), (int) (bounds.Y + 3), (int) (num + 2), (int) (bounds.Y + 3));
            graphics.DrawLine(black, (int) (bounds.X + 2), (int) (num2 - 1), (int) (bounds.X + 2), (int) (num2 + 1));
            graphics.DrawLine(black, (int) (bounds.X + 3), (int) (num2 - 2), (int) (bounds.X + 3), (int) (num2 + 2));
            graphics.DrawLine(black, (int) (bounds.Right - 3), (int) (num2 - 1), (int) (bounds.Right - 3), (int) (num2 + 1));
            graphics.DrawLine(black, (int) (bounds.Right - 4), (int) (num2 - 2), (int) (bounds.Right - 4), (int) (num2 + 2));
            graphics.DrawLine(black, (int) (num - 1), (int) (bounds.Bottom - 3), (int) (num + 1), (int) (bounds.Bottom - 3));
            graphics.DrawLine(black, (int) (num - 2), (int) (bounds.Bottom - 4), (int) (num + 2), (int) (bounds.Bottom - 4));
        }

        private static void DrawFlatCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            Brush background = ((state & ButtonState.Inactive) == ButtonState.Inactive) ? SystemBrushes.Control : SystemBrushes.Window;
            Color foreground = ((state & ButtonState.Inactive) == ButtonState.Inactive) ? SystemColors.ControlDark : SystemColors.ControlText;
            DrawFlatCheckBox(graphics, rectangle, foreground, background, state);
        }

        private static void DrawFlatCheckBox(Graphics graphics, Rectangle rectangle, Color foreground, Brush background, ButtonState state)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if ((rectangle.Width < 0) || (rectangle.Height < 0))
            {
                throw new ArgumentOutOfRangeException("rectangle");
            }
            Rectangle rectangle2 = new Rectangle(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2);
            graphics.FillRectangle(background, rectangle2);
            if ((state & ButtonState.Checked) == ButtonState.Checked)
            {
                if (((checkImage == null) || (checkImage.Width != rectangle.Width)) || (checkImage.Height != rectangle.Height))
                {
                    if (checkImage != null)
                    {
                        checkImage.Dispose();
                        checkImage = null;
                    }
                    System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(0, 0, rectangle.Width, rectangle.Height);
                    Bitmap image = new Bitmap(rectangle.Width, rectangle.Height);
                    using (Graphics graphics2 = Graphics.FromImage(image))
                    {
                        graphics2.Clear(Color.Transparent);
                        IntPtr hdc = graphics2.GetHdc();
                        try
                        {
                            System.Windows.Forms.SafeNativeMethods.DrawFrameControl(new HandleRef(null, hdc), ref rect, 2, 1);
                        }
                        finally
                        {
                            graphics2.ReleaseHdcInternal(hdc);
                        }
                    }
                    image.MakeTransparent();
                    checkImage = image;
                }
                rectangle.X++;
                DrawImageColorized(graphics, checkImage, rectangle, foreground);
                rectangle.X--;
            }
            Pen controlDark = SystemPens.ControlDark;
            graphics.DrawRectangle(controlDark, rectangle2.X, rectangle2.Y, rectangle2.Width - 1, rectangle2.Height - 1);
        }

        public static void DrawFocusRectangle(Graphics graphics, Rectangle rectangle)
        {
            DrawFocusRectangle(graphics, rectangle, SystemColors.ControlText, SystemColors.Control);
        }

        public static void DrawFocusRectangle(Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            rectangle.Width--;
            rectangle.Height--;
            graphics.DrawRectangle(GetFocusPen(backColor, ((rectangle.X + rectangle.Y) % 2) == 1), rectangle);
        }

        private static void DrawFrameControl(Graphics graphics, int x, int y, int width, int height, int kind, int state, Color foreColor, Color backColor)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width");
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height");
            }
            System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(0, 0, width, height);
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics graphics2 = Graphics.FromImage(bitmap))
                {
                    graphics2.Clear(Color.Transparent);
                    using (WindowsGraphics graphics3 = WindowsGraphics.FromGraphics(graphics2))
                    {
                        System.Windows.Forms.SafeNativeMethods.DrawFrameControl(new HandleRef(graphics3, graphics3.DeviceContext.Hdc), ref rect, kind, state);
                    }
                    if ((foreColor == Color.Empty) || (backColor == Color.Empty))
                    {
                        graphics.DrawImage(bitmap, x, y);
                    }
                    else
                    {
                        ImageAttributes imageAttrs = new ImageAttributes();
                        ColorMap map = new ColorMap {
                            OldColor = Color.Black,
                            NewColor = foreColor
                        };
                        ColorMap map2 = new ColorMap {
                            OldColor = Color.White,
                            NewColor = backColor
                        };
                        imageAttrs.SetRemapTable(new ColorMap[] { map, map2 }, ColorAdjustType.Bitmap);
                        graphics.DrawImage(bitmap, new Rectangle(x, y, width, height), 0, 0, width, height, GraphicsUnit.Pixel, imageAttrs, null, IntPtr.Zero);
                    }
                }
            }
        }

        public static void DrawGrabHandle(Graphics graphics, Rectangle rectangle, bool primary, bool enabled)
        {
            Pen grabPenPrimary;
            Brush grabBrushPrimary;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (primary)
            {
                if (ControlPaint.grabPenPrimary == null)
                {
                    ControlPaint.grabPenPrimary = Pens.Black;
                }
                grabPenPrimary = ControlPaint.grabPenPrimary;
                if (enabled)
                {
                    if (ControlPaint.grabBrushPrimary == null)
                    {
                        ControlPaint.grabBrushPrimary = Brushes.White;
                    }
                    grabBrushPrimary = ControlPaint.grabBrushPrimary;
                }
                else
                {
                    grabBrushPrimary = SystemBrushes.Control;
                }
            }
            else
            {
                if (grabPenSecondary == null)
                {
                    grabPenSecondary = Pens.White;
                }
                grabPenPrimary = grabPenSecondary;
                if (enabled)
                {
                    if (grabBrushSecondary == null)
                    {
                        grabBrushSecondary = Brushes.Black;
                    }
                    grabBrushPrimary = grabBrushSecondary;
                }
                else
                {
                    grabBrushPrimary = SystemBrushes.Control;
                }
            }
            Rectangle rect = new Rectangle(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 1, rectangle.Height - 1);
            graphics.FillRectangle(grabBrushPrimary, rect);
            rectangle.Width--;
            rectangle.Height--;
            graphics.DrawRectangle(grabPenPrimary, rectangle);
        }

        public static void DrawGrid(Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if ((pixelsBetweenDots.Width <= 0) || (pixelsBetweenDots.Height <= 0))
            {
                throw new ArgumentOutOfRangeException("pixelsBetweenDots");
            }
            bool flag = backColor.GetBrightness() < 0.5;
            if (((gridBrush == null) || (gridSize.Width != pixelsBetweenDots.Width)) || ((gridSize.Height != pixelsBetweenDots.Height) || (flag != gridInvert)))
            {
                if (gridBrush != null)
                {
                    gridBrush.Dispose();
                    gridBrush = null;
                }
                gridSize = pixelsBetweenDots;
                int num2 = 0x10;
                gridInvert = flag;
                Color color = gridInvert ? Color.White : Color.Black;
                int width = ((num2 / pixelsBetweenDots.Width) + 1) * pixelsBetweenDots.Width;
                int height = ((num2 / pixelsBetweenDots.Height) + 1) * pixelsBetweenDots.Height;
                Bitmap bitmap = new Bitmap(width, height);
                for (int i = 0; i < width; i += pixelsBetweenDots.Width)
                {
                    for (int j = 0; j < height; j += pixelsBetweenDots.Height)
                    {
                        bitmap.SetPixel(i, j, color);
                    }
                }
                gridBrush = new TextureBrush(bitmap);
                bitmap.Dispose();
            }
            graphics.FillRectangle(gridBrush, area);
        }

        internal static void DrawImageColorized(Graphics graphics, Image image, Rectangle destination, Color replaceBlack)
        {
            DrawImageColorized(graphics, image, destination, RemapBlackAndWhitePreserveTransparentMatrix(replaceBlack, Color.White));
        }

        private static void DrawImageColorized(Graphics graphics, Image image, Rectangle destination, ColorMatrix matrix)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            ImageAttributes imageAttrs = new ImageAttributes();
            imageAttrs.SetColorMatrix(matrix);
            graphics.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttrs, null, IntPtr.Zero);
            imageAttrs.Dispose();
        }

        internal static void DrawImageDisabled(Graphics graphics, Image image, Rectangle imageBounds, Color background, bool unscaledImage)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            Size size = image.Size;
            if (disabledImageAttr == null)
            {
                float[][] newColorMatrix = new float[5][];
                newColorMatrix[0] = new float[] { 0.2125f, 0.2125f, 0.2125f, 0f, 0f };
                newColorMatrix[1] = new float[] { 0.2577f, 0.2577f, 0.2577f, 0f, 0f };
                newColorMatrix[2] = new float[] { 0.0361f, 0.0361f, 0.0361f, 0f, 0f };
                float[] numArray2 = new float[5];
                numArray2[3] = 1f;
                newColorMatrix[3] = numArray2;
                newColorMatrix[4] = new float[] { 0.38f, 0.38f, 0.38f, 0f, 1f };
                ColorMatrix matrix = new ColorMatrix(newColorMatrix);
                disabledImageAttr = new ImageAttributes();
                disabledImageAttr.ClearColorKey();
                disabledImageAttr.SetColorMatrix(matrix);
            }
            if (unscaledImage)
            {
                using (Bitmap bitmap = new Bitmap(image.Width, image.Height))
                {
                    using (Graphics graphics2 = Graphics.FromImage(bitmap))
                    {
                        graphics2.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, disabledImageAttr);
                    }
                    graphics.DrawImageUnscaled(bitmap, imageBounds);
                    return;
                }
            }
            graphics.DrawImage(image, imageBounds, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, disabledImageAttr);
        }

        public static void DrawImageDisabled(Graphics graphics, Image image, int x, int y, Color background)
        {
            DrawImageDisabled(graphics, image, new Rectangle(x, y, image.Width, image.Height), background, false);
        }

        internal static void DrawImageReplaceColor(Graphics g, Image image, Rectangle dest, Color oldColor, Color newColor)
        {
            ImageAttributes imageAttrs = new ImageAttributes();
            ColorMap map = new ColorMap {
                OldColor = oldColor,
                NewColor = newColor
            };
            imageAttrs.SetRemapTable(new ColorMap[] { map }, ColorAdjustType.Bitmap);
            g.DrawImage(image, dest, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttrs, null, IntPtr.Zero);
            imageAttrs.Dispose();
        }

        public static void DrawLockedFrame(Graphics graphics, Rectangle rectangle, bool primary)
        {
            Pen white;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (primary)
            {
                white = Pens.White;
            }
            else
            {
                white = Pens.Black;
            }
            graphics.DrawRectangle(white, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
            rectangle.Inflate(-1, -1);
            graphics.DrawRectangle(white, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
            if (primary)
            {
                white = Pens.Black;
            }
            else
            {
                white = Pens.White;
            }
            rectangle.Inflate(-1, -1);
            graphics.DrawRectangle(white, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
        }

        public static void DrawMenuGlyph(Graphics graphics, Rectangle rectangle, MenuGlyph glyph)
        {
            DrawMenuGlyph(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, glyph);
        }

        public static void DrawMenuGlyph(Graphics graphics, Rectangle rectangle, MenuGlyph glyph, Color foreColor, Color backColor)
        {
            DrawMenuGlyph(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, glyph, foreColor, backColor);
        }

        public static void DrawMenuGlyph(Graphics graphics, int x, int y, int width, int height, MenuGlyph glyph)
        {
            DrawFrameControl(graphics, x, y, width, height, 2, (int) glyph, Color.Empty, Color.Empty);
        }

        public static void DrawMenuGlyph(Graphics graphics, int x, int y, int width, int height, MenuGlyph glyph, Color foreColor, Color backColor)
        {
            DrawFrameControl(graphics, x, y, width, height, 2, (int) glyph, foreColor, backColor);
        }

        public static void DrawMixedCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            DrawMixedCheckBox(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, state);
        }

        public static void DrawMixedCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state)
        {
            DrawFrameControl(graphics, x, y, width, height, 4, 8 | state, Color.Empty, Color.Empty);
        }

        public static void DrawRadioButton(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            DrawRadioButton(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, state);
        }

        public static void DrawRadioButton(Graphics graphics, int x, int y, int width, int height, ButtonState state)
        {
            DrawFrameControl(graphics, x, y, width, height, 4, 4 | state, Color.Empty, Color.Empty);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        public static void DrawReversibleFrame(Rectangle rectangle, Color backColor, FrameStyle style)
        {
            int num;
            Color white;
            IntPtr ptr2;
            if (backColor.GetBrightness() < 0.5)
            {
                num = 10;
                white = Color.White;
            }
            else
            {
                num = 7;
                white = Color.Black;
            }
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.GetDCEx(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetDesktopWindow()), System.Windows.Forms.NativeMethods.NullHandleRef, 0x403);
            switch (style)
            {
                case FrameStyle.Dashed:
                    ptr2 = System.Windows.Forms.SafeNativeMethods.CreatePen(2, 1, ColorTranslator.ToWin32(backColor));
                    break;

                default:
                    ptr2 = System.Windows.Forms.SafeNativeMethods.CreatePen(0, 2, ColorTranslator.ToWin32(backColor));
                    break;
            }
            int nDrawMode = System.Windows.Forms.SafeNativeMethods.SetROP2(new HandleRef(null, handle), num);
            IntPtr ptr3 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetStockObject(5)));
            IntPtr ptr4 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr2));
            System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, handle), ColorTranslator.ToWin32(white));
            System.Windows.Forms.SafeNativeMethods.Rectangle(new HandleRef(null, handle), rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
            System.Windows.Forms.SafeNativeMethods.SetROP2(new HandleRef(null, handle), nDrawMode);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr3));
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr4));
            if (ptr2 != IntPtr.Zero)
            {
                System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr2));
            }
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, handle));
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        public static void DrawReversibleLine(Point start, Point end, Color backColor)
        {
            int nDrawMode = GetColorRop(backColor, 10, 7);
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.GetDCEx(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetDesktopWindow()), System.Windows.Forms.NativeMethods.NullHandleRef, 0x403);
            IntPtr ptr2 = System.Windows.Forms.SafeNativeMethods.CreatePen(0, 1, ColorTranslator.ToWin32(backColor));
            int num2 = System.Windows.Forms.SafeNativeMethods.SetROP2(new HandleRef(null, handle), nDrawMode);
            IntPtr ptr3 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetStockObject(5)));
            IntPtr ptr4 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr2));
            System.Windows.Forms.SafeNativeMethods.MoveToEx(new HandleRef(null, handle), start.X, start.Y, null);
            System.Windows.Forms.SafeNativeMethods.LineTo(new HandleRef(null, handle), end.X, end.Y);
            System.Windows.Forms.SafeNativeMethods.SetROP2(new HandleRef(null, handle), num2);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr3));
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr4));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr2));
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, handle));
        }

        public static void DrawScrollButton(Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state)
        {
            DrawScrollButton(graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, button, state);
        }

        public static void DrawScrollButton(Graphics graphics, int x, int y, int width, int height, ScrollButton button, ButtonState state)
        {
            DrawFrameControl(graphics, x, y, width, height, 3, (int) (button | ((ScrollButton) ((int) state))), Color.Empty, Color.Empty);
        }

        public static void DrawSelectionFrame(Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect, Color backColor)
        {
            Brush activeBrush;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (active)
            {
                activeBrush = GetActiveBrush(backColor);
            }
            else
            {
                activeBrush = GetSelectedBrush(backColor);
            }
            Region clip = graphics.Clip;
            graphics.ExcludeClip(insideRect);
            graphics.FillRectangle(activeBrush, outsideRect);
            graphics.Clip = clip;
        }

        public static void DrawSizeGrip(Graphics graphics, Color backColor, Rectangle bounds)
        {
            DrawSizeGrip(graphics, backColor, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public static void DrawSizeGrip(Graphics graphics, Color backColor, int x, int y, int width, int height)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            using (Pen pen = new Pen(LightLight(backColor)))
            {
                using (Pen pen2 = new Pen(Dark(backColor)))
                {
                    int num = Math.Min(width, height);
                    int num2 = (x + width) - 1;
                    int num3 = (y + height) - 2;
                    for (int i = 0; i < (num - 4); i += 4)
                    {
                        graphics.DrawLine(pen2, (num2 - (i + 1)) - 2, num3, num2, (num3 - (i + 1)) - 2);
                        graphics.DrawLine(pen2, (num2 - (i + 2)) - 2, num3, num2, (num3 - (i + 2)) - 2);
                        graphics.DrawLine(pen, (num2 - (i + 3)) - 2, num3, num2, (num3 - (i + 3)) - 2);
                    }
                }
            }
        }

        public static void DrawStringDisabled(Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            layoutRectangle.Offset(1f, 1f);
            using (SolidBrush brush = new SolidBrush(LightLight(color)))
            {
                graphics.DrawString(s, font, brush, layoutRectangle, format);
                layoutRectangle.Offset(-1f, -1f);
                color = Dark(color);
                brush.Color = color;
                graphics.DrawString(s, font, brush, layoutRectangle, format);
            }
        }

        public static void DrawStringDisabled(IDeviceContext dc, string s, Font font, Color color, Rectangle layoutRectangle, TextFormatFlags format)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            layoutRectangle.Offset(1, 1);
            Color foreColor = LightLight(color);
            TextRenderer.DrawText(dc, s, font, layoutRectangle, foreColor, format);
            layoutRectangle.Offset(-1, -1);
            foreColor = Dark(color);
            TextRenderer.DrawText(dc, s, font, layoutRectangle, foreColor, format);
        }

        public static void DrawVisualStyleBorder(Graphics graphics, Rectangle bounds)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            using (Pen pen = new Pen(VisualStyleInformation.TextControlBorder))
            {
                graphics.DrawRectangle(pen, bounds);
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        public static void FillReversibleRectangle(Rectangle rectangle, Color backColor)
        {
            int rop = GetColorRop(backColor, 0xa50065, 0x5a0049);
            int nDrawMode = GetColorRop(backColor, 6, 6);
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.GetDCEx(new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetDesktopWindow()), System.Windows.Forms.NativeMethods.NullHandleRef, 0x403);
            IntPtr ptr2 = System.Windows.Forms.SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(backColor));
            int num3 = System.Windows.Forms.SafeNativeMethods.SetROP2(new HandleRef(null, handle), nDrawMode);
            IntPtr ptr3 = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr2));
            System.Windows.Forms.SafeNativeMethods.PatBlt(new HandleRef(null, handle), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rop);
            System.Windows.Forms.SafeNativeMethods.SetROP2(new HandleRef(null, handle), num3);
            System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr3));
            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr2));
            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, handle));
        }

        internal static Font FontInPoints(Font font)
        {
            return new Font(font.FontFamily, font.SizeInPoints, font.Style, GraphicsUnit.Point, font.GdiCharSet, font.GdiVerticalFont);
        }

        internal static bool FontToIFont(Font source, System.Windows.Forms.UnsafeNativeMethods.IFont target)
        {
            bool flag = false;
            string name = target.GetName();
            if (!source.Name.Equals(name))
            {
                target.SetName(source.Name);
                flag = true;
            }
            float num = ((float) target.GetSize()) / 10000f;
            float sizeInPoints = source.SizeInPoints;
            if (sizeInPoints != num)
            {
                target.SetSize((long) (sizeInPoints * 10000f));
                flag = true;
            }
            System.Windows.Forms.NativeMethods.LOGFONT logFont = new System.Windows.Forms.NativeMethods.LOGFONT();
            System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
            try
            {
                source.ToLogFont(logFont);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            if (target.GetWeight() != logFont.lfWeight)
            {
                target.SetWeight((short) logFont.lfWeight);
                flag = true;
            }
            if (target.GetBold() != (logFont.lfWeight >= 700))
            {
                target.SetBold(logFont.lfWeight >= 700);
                flag = true;
            }
            if (target.GetItalic() != (0 != logFont.lfItalic))
            {
                target.SetItalic(0 != logFont.lfItalic);
                flag = true;
            }
            if (target.GetUnderline() != (0 != logFont.lfUnderline))
            {
                target.SetUnderline(0 != logFont.lfUnderline);
                flag = true;
            }
            if (target.GetStrikethrough() != (0 != logFont.lfStrikeOut))
            {
                target.SetStrikethrough(0 != logFont.lfStrikeOut);
                flag = true;
            }
            if (target.GetCharset() != logFont.lfCharSet)
            {
                target.SetCharset(logFont.lfCharSet);
                flag = true;
            }
            return flag;
        }

        private static Brush GetActiveBrush(Color backColor)
        {
            Color controlLight;
            if (backColor.GetBrightness() <= 0.5)
            {
                controlLight = SystemColors.ControlLight;
            }
            else
            {
                controlLight = SystemColors.ControlDark;
            }
            if ((frameBrushActive == null) || !frameColorActive.Equals(controlLight))
            {
                if (frameBrushActive != null)
                {
                    frameBrushActive.Dispose();
                    frameBrushActive = null;
                }
                frameColorActive = controlLight;
                int width = 8;
                Bitmap bitmap = new Bitmap(width, width);
                for (int i = 0; i < width; i++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        bitmap.SetPixel(i, k, Color.Transparent);
                    }
                }
                for (int j = 0; j < width; j++)
                {
                    for (int m = -j; m < width; m += 4)
                    {
                        if (m >= 0)
                        {
                            bitmap.SetPixel(m, j, controlLight);
                        }
                    }
                }
                frameBrushActive = new TextureBrush(bitmap);
                bitmap.Dispose();
            }
            return frameBrushActive;
        }

        private static int GetColorRop(Color color, int darkROP, int lightROP)
        {
            if (color.GetBrightness() < 0.5)
            {
                return darkROP;
            }
            return lightROP;
        }

        private static Pen GetFocusPen(Color backColor, bool odds)
        {
            if (((focusPen == null) || ((focusPenColor.GetBrightness() <= 0.5) && (backColor.GetBrightness() <= 0.5))) || !focusPenColor.Equals(backColor))
            {
                if (focusPen != null)
                {
                    focusPen.Dispose();
                    focusPen = null;
                    focusPenInvert.Dispose();
                    focusPenInvert = null;
                }
                focusPenColor = backColor;
                Bitmap bitmap = new Bitmap(2, 2);
                Color transparent = Color.Transparent;
                Color black = Color.Black;
                if (backColor.GetBrightness() <= 0.5)
                {
                    transparent = black;
                    black = InvertColor(backColor);
                }
                else if (backColor == Color.Transparent)
                {
                    transparent = Color.White;
                }
                bitmap.SetPixel(1, 0, black);
                bitmap.SetPixel(0, 1, black);
                bitmap.SetPixel(0, 0, transparent);
                bitmap.SetPixel(1, 1, transparent);
                Brush brush = new TextureBrush(bitmap);
                focusPen = new Pen(brush, 1f);
                brush.Dispose();
                bitmap.SetPixel(1, 0, transparent);
                bitmap.SetPixel(0, 1, transparent);
                bitmap.SetPixel(0, 0, black);
                bitmap.SetPixel(1, 1, black);
                brush = new TextureBrush(bitmap);
                focusPenInvert = new Pen(brush, 1f);
                brush.Dispose();
                bitmap.Dispose();
            }
            if (!odds)
            {
                return focusPenInvert;
            }
            return focusPen;
        }

        private static Brush GetSelectedBrush(Color backColor)
        {
            Color controlLight;
            if (backColor.GetBrightness() <= 0.5)
            {
                controlLight = SystemColors.ControlLight;
            }
            else
            {
                controlLight = SystemColors.ControlDark;
            }
            if ((frameBrushSelected == null) || !frameColorSelected.Equals(controlLight))
            {
                if (frameBrushSelected != null)
                {
                    frameBrushSelected.Dispose();
                    frameBrushSelected = null;
                }
                frameColorSelected = controlLight;
                int width = 8;
                Bitmap bitmap = new Bitmap(width, width);
                for (int i = 0; i < width; i++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        bitmap.SetPixel(i, k, Color.Transparent);
                    }
                }
                int num4 = 0;
                for (int j = 0; j < width; j += 2)
                {
                    for (int m = num4; m < width; m += 2)
                    {
                        bitmap.SetPixel(j, m, controlLight);
                    }
                    num4 ^= 1;
                }
                frameBrushSelected = new TextureBrush(bitmap);
                bitmap.Dispose();
            }
            return frameBrushSelected;
        }

        private static float InfinityToOne(float value)
        {
            if ((value != float.NegativeInfinity) && (value != float.PositiveInfinity))
            {
                return value;
            }
            return 1f;
        }

        private static Color InvertColor(Color color)
        {
            return Color.FromArgb(color.A, ~color.R, ~color.G, ~color.B);
        }

        internal static bool IsDarker(Color c1, Color c2)
        {
            HLSColor color = new HLSColor(c1);
            HLSColor color2 = new HLSColor(c2);
            return (color.Luminosity < color2.Luminosity);
        }

        internal static bool IsImageTransparent(Image backgroundImage)
        {
            return ((backgroundImage != null) && ((backgroundImage.Flags & 2) > 0));
        }

        public static Color Light(Color baseColor)
        {
            HLSColor color = new HLSColor(baseColor);
            return color.Lighter(0.5f);
        }

        public static Color Light(Color baseColor, float percOfLightLight)
        {
            HLSColor color = new HLSColor(baseColor);
            return color.Lighter(percOfLightLight);
        }

        public static Color LightLight(Color baseColor)
        {
            HLSColor color = new HLSColor(baseColor);
            return color.Lighter(1f);
        }

        internal static ColorMatrix MultiplyColorMatrix(float[][] matrix1, float[][] matrix2)
        {
            int num = 5;
            float[][] newColorMatrix = new float[num][];
            for (int i = 0; i < num; i++)
            {
                newColorMatrix[i] = new float[num];
            }
            float[] numArray2 = new float[num];
            for (int j = 0; j < num; j++)
            {
                for (int k = 0; k < num; k++)
                {
                    numArray2[k] = matrix1[k][j];
                }
                for (int m = 0; m < num; m++)
                {
                    float[] numArray3 = matrix2[m];
                    float num6 = 0f;
                    for (int n = 0; n < num; n++)
                    {
                        num6 += numArray3[n] * numArray2[n];
                    }
                    newColorMatrix[m][j] = num6;
                }
            }
            return new ColorMatrix(newColorMatrix);
        }

        internal static void PaintTableCellBorder(TableLayoutPanelCellBorderStyle borderStyle, Graphics g, Rectangle bound)
        {
            switch (borderStyle)
            {
                case TableLayoutPanelCellBorderStyle.None:
                    break;

                case TableLayoutPanelCellBorderStyle.Single:
                    g.DrawRectangle(SystemPens.ControlDark, bound);
                    return;

                case TableLayoutPanelCellBorderStyle.Inset:
                    using (Pen pen = new Pen(SystemColors.Window))
                    {
                        g.DrawLine(pen, bound.X, bound.Y, (bound.X + bound.Width) - 1, bound.Y);
                        g.DrawLine(pen, bound.X, bound.Y, bound.X, (bound.Y + bound.Height) - 1);
                    }
                    g.DrawLine(SystemPens.ControlDark, (bound.X + bound.Width) - 1, bound.Y, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                    g.DrawLine(SystemPens.ControlDark, bound.X, (bound.Y + bound.Height) - 1, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                    return;

                case TableLayoutPanelCellBorderStyle.InsetDouble:
                    g.DrawRectangle(SystemPens.Control, bound);
                    bound = new Rectangle(bound.X + 1, bound.Y + 1, bound.Width - 1, bound.Height - 1);
                    using (Pen pen2 = new Pen(SystemColors.Window))
                    {
                        g.DrawLine(pen2, bound.X, bound.Y, (bound.X + bound.Width) - 1, bound.Y);
                        g.DrawLine(pen2, bound.X, bound.Y, bound.X, (bound.Y + bound.Height) - 1);
                    }
                    g.DrawLine(SystemPens.ControlDark, (bound.X + bound.Width) - 1, bound.Y, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                    g.DrawLine(SystemPens.ControlDark, bound.X, (bound.Y + bound.Height) - 1, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                    return;

                case TableLayoutPanelCellBorderStyle.Outset:
                {
                    g.DrawLine(SystemPens.ControlDark, bound.X, bound.Y, (bound.X + bound.Width) - 1, bound.Y);
                    g.DrawLine(SystemPens.ControlDark, bound.X, bound.Y, bound.X, (bound.Y + bound.Height) - 1);
                    using (Pen pen3 = new Pen(SystemColors.Window))
                    {
                        g.DrawLine(pen3, (bound.X + bound.Width) - 1, bound.Y, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                        g.DrawLine(pen3, bound.X, (bound.Y + bound.Height) - 1, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                        break;
                    }
                }
                case TableLayoutPanelCellBorderStyle.OutsetDouble:
                case TableLayoutPanelCellBorderStyle.OutsetPartial:
                    g.DrawRectangle(SystemPens.Control, bound);
                    bound = new Rectangle(bound.X + 1, bound.Y + 1, bound.Width - 1, bound.Height - 1);
                    g.DrawLine(SystemPens.ControlDark, bound.X, bound.Y, (bound.X + bound.Width) - 1, bound.Y);
                    g.DrawLine(SystemPens.ControlDark, bound.X, bound.Y, bound.X, (bound.Y + bound.Height) - 1);
                    using (Pen pen4 = new Pen(SystemColors.Window))
                    {
                        g.DrawLine(pen4, (bound.X + bound.Width) - 1, bound.Y, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                        g.DrawLine(pen4, bound.X, (bound.Y + bound.Height) - 1, (bound.X + bound.Width) - 1, (bound.Y + bound.Height) - 1);
                    }
                    break;

                default:
                    return;
            }
        }

        internal static void PaintTableControlBorder(TableLayoutPanelCellBorderStyle borderStyle, Graphics g, Rectangle bound)
        {
            int x = bound.X;
            int y = bound.Y;
            int right = bound.Right;
            int bottom = bound.Bottom;
            switch (borderStyle)
            {
                case TableLayoutPanelCellBorderStyle.None:
                case TableLayoutPanelCellBorderStyle.Single:
                    break;

                case TableLayoutPanelCellBorderStyle.Inset:
                case TableLayoutPanelCellBorderStyle.InsetDouble:
                {
                    g.DrawLine(SystemPens.ControlDark, x, y, right - 1, y);
                    g.DrawLine(SystemPens.ControlDark, x, y, x, bottom - 1);
                    using (Pen pen = new Pen(SystemColors.Window))
                    {
                        g.DrawLine(pen, right - 1, y, right - 1, bottom - 1);
                        g.DrawLine(pen, x, bottom - 1, right - 1, bottom - 1);
                        break;
                    }
                }
                case TableLayoutPanelCellBorderStyle.Outset:
                case TableLayoutPanelCellBorderStyle.OutsetDouble:
                case TableLayoutPanelCellBorderStyle.OutsetPartial:
                    using (Pen pen2 = new Pen(SystemColors.Window))
                    {
                        g.DrawLine(pen2, x, y, right - 1, y);
                        g.DrawLine(pen2, x, y, x, bottom - 1);
                    }
                    g.DrawLine(SystemPens.ControlDark, right - 1, y, right - 1, bottom - 1);
                    g.DrawLine(SystemPens.ControlDark, x, bottom - 1, right - 1, bottom - 1);
                    break;

                default:
                    return;
            }
        }

        internal static void PrintBorder(Graphics graphics, Rectangle bounds, BorderStyle style, Border3DStyle b3dStyle)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            switch (style)
            {
                case BorderStyle.None:
                    break;

                case BorderStyle.FixedSingle:
                    DrawBorder(graphics, bounds, Color.FromKnownColor(KnownColor.WindowFrame), ButtonBorderStyle.Solid);
                    return;

                case BorderStyle.Fixed3D:
                    DrawBorder3D(graphics, bounds, b3dStyle);
                    break;

                default:
                    return;
            }
        }

        private static ColorMatrix RemapBlackAndWhitePreserveTransparentMatrix(Color replaceBlack, Color replaceWhite)
        {
            float num = ((float) replaceBlack.R) / 255f;
            float num2 = ((float) replaceBlack.G) / 255f;
            float num3 = ((float) replaceBlack.B) / 255f;
            float single1 = ((float) replaceBlack.A) / 255f;
            float num4 = ((float) replaceWhite.R) / 255f;
            float num5 = ((float) replaceWhite.G) / 255f;
            float num6 = ((float) replaceWhite.B) / 255f;
            float single2 = ((float) replaceWhite.A) / 255f;
            return new ColorMatrix { Matrix00 = -num, Matrix01 = -num2, Matrix02 = -num3, Matrix10 = num4, Matrix11 = num5, Matrix12 = num6, Matrix33 = 1f, Matrix40 = num, Matrix41 = num2, Matrix42 = num3, Matrix44 = 1f };
        }

        internal static StringFormat StringFormatForAlignment(System.Drawing.ContentAlignment align)
        {
            return new StringFormat { Alignment = TranslateAlignment(align), LineAlignment = TranslateLineAlignment(align) };
        }

        internal static TextFormatFlags TextFormatFlagsForAlignmentGDI(System.Drawing.ContentAlignment align)
        {
            TextFormatFlags flags = TextFormatFlags.Default;
            flags |= TranslateAlignmentForGDI(align);
            return (flags | TranslateLineAlignmentForGDI(align));
        }

        internal static StringAlignment TranslateAlignment(System.Drawing.ContentAlignment align)
        {
            if ((align & anyRight) != ((System.Drawing.ContentAlignment) 0))
            {
                return StringAlignment.Far;
            }
            if ((align & anyCenter) != ((System.Drawing.ContentAlignment) 0))
            {
                return StringAlignment.Center;
            }
            return StringAlignment.Near;
        }

        internal static TextFormatFlags TranslateAlignmentForGDI(System.Drawing.ContentAlignment align)
        {
            if ((align & anyBottom) != ((System.Drawing.ContentAlignment) 0))
            {
                return TextFormatFlags.Bottom;
            }
            if ((align & anyMiddle) != ((System.Drawing.ContentAlignment) 0))
            {
                return TextFormatFlags.VerticalCenter;
            }
            return TextFormatFlags.Default;
        }

        internal static StringAlignment TranslateLineAlignment(System.Drawing.ContentAlignment align)
        {
            if ((align & anyBottom) != ((System.Drawing.ContentAlignment) 0))
            {
                return StringAlignment.Far;
            }
            if ((align & anyMiddle) != ((System.Drawing.ContentAlignment) 0))
            {
                return StringAlignment.Center;
            }
            return StringAlignment.Near;
        }

        internal static TextFormatFlags TranslateLineAlignmentForGDI(System.Drawing.ContentAlignment align)
        {
            if ((align & anyRight) != ((System.Drawing.ContentAlignment) 0))
            {
                return TextFormatFlags.Right;
            }
            if ((align & anyCenter) != ((System.Drawing.ContentAlignment) 0))
            {
                return TextFormatFlags.HorizontalCenter;
            }
            return TextFormatFlags.Default;
        }

        public static Color ContrastControlDark
        {
            get
            {
                if (!SystemInformation.HighContrast)
                {
                    return SystemColors.ControlDark;
                }
                return SystemColors.WindowFrame;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HLSColor
        {
            private const int ShadowAdj = -333;
            private const int HilightAdj = 500;
            private const int WatermarkAdj = -50;
            private const int Range = 240;
            private const int HLSMax = 240;
            private const int RGBMax = 0xff;
            private const int Undefined = 160;
            private int hue;
            private int saturation;
            private int luminosity;
            private bool isSystemColors_Control;
            public HLSColor(Color color)
            {
                this.isSystemColors_Control = color.ToKnownColor() == SystemColors.Control.ToKnownColor();
                int r = color.R;
                int g = color.G;
                int b = color.B;
                int num4 = Math.Max(Math.Max(r, g), b);
                int num5 = Math.Min(Math.Min(r, g), b);
                int num6 = num4 + num5;
                this.luminosity = ((num6 * 240) + 0xff) / 510;
                int num7 = num4 - num5;
                if (num7 == 0)
                {
                    this.saturation = 0;
                    this.hue = 160;
                }
                else
                {
                    if (this.luminosity <= 120)
                    {
                        this.saturation = ((num7 * 240) + (num6 / 2)) / num6;
                    }
                    else
                    {
                        this.saturation = ((num7 * 240) + ((510 - num6) / 2)) / (510 - num6);
                    }
                    int num8 = (((num4 - r) * 40) + (num7 / 2)) / num7;
                    int num9 = (((num4 - g) * 40) + (num7 / 2)) / num7;
                    int num10 = (((num4 - b) * 40) + (num7 / 2)) / num7;
                    if (r == num4)
                    {
                        this.hue = num10 - num9;
                    }
                    else if (g == num4)
                    {
                        this.hue = (80 + num8) - num10;
                    }
                    else
                    {
                        this.hue = (160 + num9) - num8;
                    }
                    if (this.hue < 0)
                    {
                        this.hue += 240;
                    }
                    if (this.hue > 240)
                    {
                        this.hue -= 240;
                    }
                }
            }

            public int Luminosity
            {
                get
                {
                    return this.luminosity;
                }
            }
            public Color Darker(float percDarker)
            {
                if (this.isSystemColors_Control)
                {
                    if (percDarker == 0f)
                    {
                        return SystemColors.ControlDark;
                    }
                    if (percDarker == 1f)
                    {
                        return SystemColors.ControlDarkDark;
                    }
                    Color controlDark = SystemColors.ControlDark;
                    Color controlDarkDark = SystemColors.ControlDarkDark;
                    int num = controlDark.R - controlDarkDark.R;
                    int num2 = controlDark.G - controlDarkDark.G;
                    int num3 = controlDark.B - controlDarkDark.B;
                    return Color.FromArgb((byte) (controlDark.R - ((byte) (num * percDarker))), (byte) (controlDark.G - ((byte) (num2 * percDarker))), (byte) (controlDark.B - ((byte) (num3 * percDarker))));
                }
                int num4 = 0;
                int num5 = this.NewLuma(-333, true);
                return this.ColorFromHLS(this.hue, num5 - ((int) ((num5 - num4) * percDarker)), this.saturation);
            }

            public override bool Equals(object o)
            {
                if (!(o is ControlPaint.HLSColor))
                {
                    return false;
                }
                ControlPaint.HLSColor color = (ControlPaint.HLSColor) o;
                return ((((this.hue == color.hue) && (this.saturation == color.saturation)) && (this.luminosity == color.luminosity)) && (this.isSystemColors_Control == color.isSystemColors_Control));
            }

            public static bool operator ==(ControlPaint.HLSColor a, ControlPaint.HLSColor b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(ControlPaint.HLSColor a, ControlPaint.HLSColor b)
            {
                return !a.Equals(b);
            }

            public override int GetHashCode()
            {
                return (((this.hue << 6) | (this.saturation << 2)) | this.luminosity);
            }

            public Color Lighter(float percLighter)
            {
                if (this.isSystemColors_Control)
                {
                    if (percLighter == 0f)
                    {
                        return SystemColors.ControlLight;
                    }
                    if (percLighter == 1f)
                    {
                        return SystemColors.ControlLightLight;
                    }
                    Color controlLight = SystemColors.ControlLight;
                    Color controlLightLight = SystemColors.ControlLightLight;
                    int num = controlLight.R - controlLightLight.R;
                    int num2 = controlLight.G - controlLightLight.G;
                    int num3 = controlLight.B - controlLightLight.B;
                    return Color.FromArgb((byte) (controlLight.R - ((byte) (num * percLighter))), (byte) (controlLight.G - ((byte) (num2 * percLighter))), (byte) (controlLight.B - ((byte) (num3 * percLighter))));
                }
                int luminosity = this.luminosity;
                int num5 = this.NewLuma(500, true);
                return this.ColorFromHLS(this.hue, luminosity + ((int) ((num5 - luminosity) * percLighter)), this.saturation);
            }

            private int NewLuma(int n, bool scale)
            {
                return this.NewLuma(this.luminosity, n, scale);
            }

            private int NewLuma(int luminosity, int n, bool scale)
            {
                if (n == 0)
                {
                    return luminosity;
                }
                if (scale)
                {
                    if (n > 0)
                    {
                        return (int) (((luminosity * (0x3e8 - n)) + (0xf1L * n)) / 0x3e8L);
                    }
                    return ((luminosity * (n + 0x3e8)) / 0x3e8);
                }
                int num = luminosity;
                num += (int) ((n * 240L) / 0x3e8L);
                if (num < 0)
                {
                    num = 0;
                }
                if (num > 240)
                {
                    num = 240;
                }
                return num;
            }

            private Color ColorFromHLS(int hue, int luminosity, int saturation)
            {
                byte num;
                byte num2;
                byte num3;
                if (saturation == 0)
                {
                    num = num2 = num3 = (byte) ((luminosity * 0xff) / 240);
                    if (hue == 160)
                    {
                    }
                }
                else
                {
                    int num5;
                    if (luminosity <= 120)
                    {
                        num5 = ((luminosity * (240 + saturation)) + 120) / 240;
                    }
                    else
                    {
                        num5 = (luminosity + saturation) - (((luminosity * saturation) + 120) / 240);
                    }
                    int num4 = (2 * luminosity) - num5;
                    num = (byte) (((this.HueToRGB(num4, num5, hue + 80) * 0xff) + 120) / 240);
                    num2 = (byte) (((this.HueToRGB(num4, num5, hue) * 0xff) + 120) / 240);
                    num3 = (byte) (((this.HueToRGB(num4, num5, hue - 80) * 0xff) + 120) / 240);
                }
                return Color.FromArgb(num, num2, num3);
            }

            private int HueToRGB(int n1, int n2, int hue)
            {
                if (hue < 0)
                {
                    hue += 240;
                }
                if (hue > 240)
                {
                    hue -= 240;
                }
                if (hue < 40)
                {
                    return (n1 + ((((n2 - n1) * hue) + 20) / 40));
                }
                if (hue < 120)
                {
                    return n2;
                }
                if (hue < 160)
                {
                    return (n1 + ((((n2 - n1) * (160 - hue)) + 20) / 40));
                }
                return n1;
            }
        }
    }
}

