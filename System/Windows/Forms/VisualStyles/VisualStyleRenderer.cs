namespace System.Windows.Forms.VisualStyles
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Windows.Forms;

    public sealed class VisualStyleRenderer
    {
        private string _class;
        private const TextFormatFlags AllGraphicsProperties = (TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping);
        internal const int EdgeAdjust = 0x2000;
        private static long globalCacheVersion = 0L;
        private int lastHResult;
        private static int numberOfPossibleClasses = VisualStyleElement.Count;
        private int part;
        private int state;
        [ThreadStatic]
        private static Hashtable themeHandles = null;
        [ThreadStatic]
        private static long threadCacheVersion = 0L;

        static VisualStyleRenderer()
        {
            SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(VisualStyleRenderer.OnUserPreferenceChanging);
        }

        public VisualStyleRenderer(VisualStyleElement element) : this(element.ClassName, element.Part, element.State)
        {
        }

        public VisualStyleRenderer(string className, int part, int state)
        {
            if (!IsCombinationDefined(className, part))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("VisualStylesInvalidCombination"));
            }
            this._class = className;
            this.part = part;
            this.state = state;
        }

        private static void CreateThemeHandleHashtable()
        {
            themeHandles = new Hashtable(numberOfPossibleClasses);
        }

        public void DrawBackground(IDeviceContext dc, Rectangle bounds)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if ((bounds.Width >= 0) && (bounds.Height >= 0))
            {
                using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
                {
                    HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                    this.lastHResult = System.Windows.Forms.SafeNativeMethods.DrawThemeBackground(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(bounds), null);
                }
            }
        }

        public void DrawBackground(IDeviceContext dc, Rectangle bounds, Rectangle clipRectangle)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (((bounds.Width >= 0) && (bounds.Height >= 0)) && ((clipRectangle.Width >= 0) && (clipRectangle.Height >= 0)))
            {
                using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
                {
                    HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                    this.lastHResult = System.Windows.Forms.SafeNativeMethods.DrawThemeBackground(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(bounds), new System.Windows.Forms.NativeMethods.COMRECT(clipRectangle));
                }
            }
        }

        public Rectangle DrawEdge(IDeviceContext dc, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid_Masked(edges, (int) edges, 0x1f))
            {
                throw new InvalidEnumArgumentException("edges", (int) edges, typeof(Edges));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(style, (int) style, new int[] { 5, 10, 6, 9 }))
            {
                throw new InvalidEnumArgumentException("style", (int) style, typeof(EdgeStyle));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid_Masked(effects, (int) effects, 0xd800))
            {
                throw new InvalidEnumArgumentException("effects", (int) effects, typeof(EdgeEffects));
            }
            System.Windows.Forms.NativeMethods.COMRECT pContentRect = new System.Windows.Forms.NativeMethods.COMRECT();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.DrawThemeEdge(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(bounds), (int) style, ((int) (edges | ((Edges) ((int) effects)))) | 0x2000, pContentRect);
            }
            return Rectangle.FromLTRB(pContentRect.left, pContentRect.top, pContentRect.right, pContentRect.bottom);
        }

        public void DrawImage(Graphics g, Rectangle bounds, Image image)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if ((bounds.Width >= 0) && (bounds.Height >= 0))
            {
                ImageList imageList = new ImageList();
                try
                {
                    imageList.Images.Add(image);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    g.DrawImage(image, bounds);
                    return;
                }
                this.DrawImage(g, bounds, imageList, 0);
            }
        }

        public void DrawImage(Graphics g, Rectangle bounds, ImageList imageList, int imageIndex)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            if (imageList == null)
            {
                throw new ArgumentNullException("imageList");
            }
            if ((imageIndex < 0) || (imageIndex >= imageList.Images.Count))
            {
                throw new ArgumentOutOfRangeException("imageIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "imageIndex", imageIndex.ToString(CultureInfo.CurrentCulture) }));
            }
            if ((bounds.Width >= 0) && (bounds.Height >= 0))
            {
                g.DrawImage(imageList.Images[imageIndex], bounds);
            }
        }

        public void DrawParentBackground(IDeviceContext dc, Rectangle bounds, Control childControl)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (childControl == null)
            {
                throw new ArgumentNullException("childControl");
            }
            if (((bounds.Width >= 0) && (bounds.Height >= 0)) && (childControl.Handle != IntPtr.Zero))
            {
                using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
                {
                    HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                    this.lastHResult = System.Windows.Forms.SafeNativeMethods.DrawThemeParentBackground(new HandleRef(this, childControl.Handle), hdc, new System.Windows.Forms.NativeMethods.COMRECT(bounds));
                }
            }
        }

        public void DrawText(IDeviceContext dc, Rectangle bounds, string textToDraw)
        {
            this.DrawText(dc, bounds, textToDraw, false);
        }

        public void DrawText(IDeviceContext dc, Rectangle bounds, string textToDraw, bool drawDisabled)
        {
            this.DrawText(dc, bounds, textToDraw, drawDisabled, TextFormatFlags.HorizontalCenter);
        }

        public void DrawText(IDeviceContext dc, Rectangle bounds, string textToDraw, bool drawDisabled, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if ((bounds.Width >= 0) && (bounds.Height >= 0))
            {
                int num = drawDisabled ? 1 : 0;
                if (!string.IsNullOrEmpty(textToDraw))
                {
                    using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
                    {
                        HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                        this.lastHResult = System.Windows.Forms.SafeNativeMethods.DrawThemeText(new HandleRef(this, this.Handle), hdc, this.part, this.state, textToDraw, textToDraw.Length, (int) flags, num, new System.Windows.Forms.NativeMethods.COMRECT(bounds));
                    }
                }
            }
        }

        public Rectangle GetBackgroundContentRectangle(IDeviceContext dc, Rectangle bounds)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if ((bounds.Width < 0) || (bounds.Height < 0))
            {
                return Rectangle.Empty;
            }
            System.Windows.Forms.NativeMethods.COMRECT pContentRect = new System.Windows.Forms.NativeMethods.COMRECT();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeBackgroundContentRect(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(bounds), pContentRect);
            }
            return Rectangle.FromLTRB(pContentRect.left, pContentRect.top, pContentRect.right, pContentRect.bottom);
        }

        public Rectangle GetBackgroundExtent(IDeviceContext dc, Rectangle contentBounds)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if ((contentBounds.Width < 0) || (contentBounds.Height < 0))
            {
                return Rectangle.Empty;
            }
            System.Windows.Forms.NativeMethods.COMRECT pExtentRect = new System.Windows.Forms.NativeMethods.COMRECT();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeBackgroundExtent(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(contentBounds), pExtentRect);
            }
            return Rectangle.FromLTRB(pExtentRect.left, pExtentRect.top, pExtentRect.right, pExtentRect.bottom);
        }

        [SuppressUnmanagedCodeSecurity]
        public Region GetBackgroundRegion(IDeviceContext dc, Rectangle bounds)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if ((bounds.Width < 0) || (bounds.Height < 0))
            {
                return null;
            }
            IntPtr zero = IntPtr.Zero;
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeBackgroundRegion(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(bounds), ref zero);
            }
            if (zero == IntPtr.Zero)
            {
                return null;
            }
            Region region = Region.FromHrgn(zero);
            System.Windows.Forms.SafeNativeMethods.ExternalDeleteObject(new HandleRef(null, zero));
            return region;
        }

        public bool GetBoolean(BooleanProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0x899, 0x8a5))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(BooleanProperty));
            }
            bool pfVal = false;
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeBool(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, ref pfVal);
            return pfVal;
        }

        public System.Drawing.Color GetColor(ColorProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xed9, 0xeef))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(ColorProperty));
            }
            int pColor = 0;
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeColor(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, ref pColor);
            return ColorTranslator.FromWin32(pColor);
        }

        public int GetEnumValue(EnumProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xfa1, 0xfaf))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(EnumProperty));
            }
            int piVal = 0;
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeEnumValue(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, ref piVal);
            return piVal;
        }

        public string GetFilename(FilenameProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xbb9, 0xbc0))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(FilenameProperty));
            }
            StringBuilder pszThemeFilename = new StringBuilder(0x200);
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeFilename(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, pszThemeFilename, pszThemeFilename.Capacity);
            return pszThemeFilename.ToString();
        }

        public Font GetFont(IDeviceContext dc, FontProperty prop)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xa29, 0xa29))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(FontProperty));
            }
            System.Windows.Forms.NativeMethods.LOGFONT pFont = new System.Windows.Forms.NativeMethods.LOGFONT();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeFont(new HandleRef(this, this.Handle), hdc, this.part, this.state, (int) prop, pFont);
            }
            Font font = null;
            if (!System.Windows.Forms.NativeMethods.Succeeded(this.lastHResult))
            {
                return font;
            }
            System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
            try
            {
                return Font.FromLogFont(pFont);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
                return null;
            }
        }

        private static IntPtr GetHandle(string className)
        {
            return GetHandle(className, true);
        }

        private static IntPtr GetHandle(string className, bool throwExceptionOnFail)
        {
            ThemeHandle handle;
            if (themeHandles == null)
            {
                CreateThemeHandleHashtable();
            }
            if (threadCacheVersion != globalCacheVersion)
            {
                RefreshCache();
                threadCacheVersion = globalCacheVersion;
            }
            if (!themeHandles.Contains(className))
            {
                handle = ThemeHandle.Create(className, throwExceptionOnFail);
                if (handle == null)
                {
                    return IntPtr.Zero;
                }
                themeHandles.Add(className, handle);
            }
            else
            {
                handle = (ThemeHandle) themeHandles[className];
            }
            return handle.NativeHandle;
        }

        public int GetInteger(IntegerProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0x961, 0x978))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(IntegerProperty));
            }
            int piVal = 0;
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeInt(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, ref piVal);
            return piVal;
        }

        public Padding GetMargins(IDeviceContext dc, MarginProperty prop)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xe11, 0xe13))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(MarginProperty));
            }
            System.Windows.Forms.NativeMethods.MARGINS margins = new System.Windows.Forms.NativeMethods.MARGINS();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hDC = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeMargins(new HandleRef(this, this.Handle), hDC, this.part, this.state, (int) prop, ref margins);
            }
            return new Padding(margins.cxLeftWidth, margins.cyTopHeight, margins.cxRightWidth, margins.cyBottomHeight);
        }

        public Size GetPartSize(IDeviceContext dc, ThemeSizeType type)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(type, (int) type, 0, 2))
            {
                throw new InvalidEnumArgumentException("type", (int) type, typeof(ThemeSizeType));
            }
            System.Windows.Forms.NativeMethods.SIZE psz = new System.Windows.Forms.NativeMethods.SIZE();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemePartSize(new HandleRef(this, this.Handle), hdc, this.part, this.state, null, type, psz);
            }
            return new Size(psz.cx, psz.cy);
        }

        public Size GetPartSize(IDeviceContext dc, Rectangle bounds, ThemeSizeType type)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(type, (int) type, 0, 2))
            {
                throw new InvalidEnumArgumentException("type", (int) type, typeof(ThemeSizeType));
            }
            System.Windows.Forms.NativeMethods.SIZE psz = new System.Windows.Forms.NativeMethods.SIZE();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemePartSize(new HandleRef(this, this.Handle), hdc, this.part, this.state, new System.Windows.Forms.NativeMethods.COMRECT(bounds), type, psz);
            }
            return new Size(psz.cx, psz.cy);
        }

        public Point GetPoint(PointProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xd49, 0xd50))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(PointProperty));
            }
            System.Windows.Forms.NativeMethods.POINT pPoint = new System.Windows.Forms.NativeMethods.POINT();
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemePosition(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, pPoint);
            return new Point(pPoint.x, pPoint.y);
        }

        public string GetString(StringProperty prop)
        {
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(prop, (int) prop, 0xc81, 0xc81))
            {
                throw new InvalidEnumArgumentException("prop", (int) prop, typeof(StringProperty));
            }
            StringBuilder pszBuff = new StringBuilder(0x200);
            this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeString(new HandleRef(this, this.Handle), this.part, this.state, (int) prop, pszBuff, pszBuff.Capacity);
            return pszBuff.ToString();
        }

        public Rectangle GetTextExtent(IDeviceContext dc, string textToDraw, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (string.IsNullOrEmpty(textToDraw))
            {
                throw new ArgumentNullException("textToDraw");
            }
            System.Windows.Forms.NativeMethods.COMRECT pExtentRect = new System.Windows.Forms.NativeMethods.COMRECT();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeTextExtent(new HandleRef(this, this.Handle), hdc, this.part, this.state, textToDraw, textToDraw.Length, (int) flags, null, pExtentRect);
            }
            return Rectangle.FromLTRB(pExtentRect.left, pExtentRect.top, pExtentRect.right, pExtentRect.bottom);
        }

        public Rectangle GetTextExtent(IDeviceContext dc, Rectangle bounds, string textToDraw, TextFormatFlags flags)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            if (string.IsNullOrEmpty(textToDraw))
            {
                throw new ArgumentNullException("textToDraw");
            }
            System.Windows.Forms.NativeMethods.COMRECT pExtentRect = new System.Windows.Forms.NativeMethods.COMRECT();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeTextExtent(new HandleRef(this, this.Handle), hdc, this.part, this.state, textToDraw, textToDraw.Length, (int) flags, new System.Windows.Forms.NativeMethods.COMRECT(bounds), pExtentRect);
            }
            return Rectangle.FromLTRB(pExtentRect.left, pExtentRect.top, pExtentRect.right, pExtentRect.bottom);
        }

        public TextMetrics GetTextMetrics(IDeviceContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            TextMetrics ptm = new TextMetrics();
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.GetThemeTextMetrics(new HandleRef(this, this.Handle), hdc, this.part, this.state, ref ptm);
            }
            return ptm;
        }

        public HitTestCode HitTestBackground(IDeviceContext dc, Rectangle backgroundRectangle, Point pt, HitTestOptions options)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            int pwHitTestCode = 0;
            System.Windows.Forms.NativeMethods.POINTSTRUCT ptTest = new System.Windows.Forms.NativeMethods.POINTSTRUCT(pt.X, pt.Y);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.HitTestThemeBackground(new HandleRef(this, this.Handle), hdc, this.part, this.state, (int) options, new System.Windows.Forms.NativeMethods.COMRECT(backgroundRectangle), System.Windows.Forms.NativeMethods.NullHandleRef, ptTest, ref pwHitTestCode);
            }
            return (HitTestCode) pwHitTestCode;
        }

        public HitTestCode HitTestBackground(Graphics g, Rectangle backgroundRectangle, Region region, Point pt, HitTestOptions options)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            IntPtr hrgn = region.GetHrgn(g);
            return this.HitTestBackground(g, backgroundRectangle, hrgn, pt, options);
        }

        public HitTestCode HitTestBackground(IDeviceContext dc, Rectangle backgroundRectangle, IntPtr hRgn, Point pt, HitTestOptions options)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            int pwHitTestCode = 0;
            System.Windows.Forms.NativeMethods.POINTSTRUCT ptTest = new System.Windows.Forms.NativeMethods.POINTSTRUCT(pt.X, pt.Y);
            using (WindowsGraphicsWrapper wrapper = new WindowsGraphicsWrapper(dc, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping))
            {
                HandleRef hdc = new HandleRef(wrapper, wrapper.WindowsGraphics.DeviceContext.Hdc);
                this.lastHResult = System.Windows.Forms.SafeNativeMethods.HitTestThemeBackground(new HandleRef(this, this.Handle), hdc, this.part, this.state, (int) options, new System.Windows.Forms.NativeMethods.COMRECT(backgroundRectangle), new HandleRef(this, hRgn), ptTest, ref pwHitTestCode);
            }
            return (HitTestCode) pwHitTestCode;
        }

        public bool IsBackgroundPartiallyTransparent()
        {
            return System.Windows.Forms.SafeNativeMethods.IsThemeBackgroundPartiallyTransparent(new HandleRef(this, this.Handle), this.part, this.state);
        }

        private static bool IsCombinationDefined(string className, int part)
        {
            bool flag = false;
            if (!IsSupported)
            {
                if (!VisualStyleInformation.IsEnabledByUser)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("VisualStyleNotActive"));
                }
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("VisualStylesDisabledInClientArea"));
            }
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            IntPtr ptr = GetHandle(className, false);
            if (ptr != IntPtr.Zero)
            {
                if (part == 0)
                {
                    flag = true;
                }
                else
                {
                    flag = System.Windows.Forms.SafeNativeMethods.IsThemePartDefined(new HandleRef(null, ptr), part, 0);
                }
            }
            if (!flag)
            {
                using (ThemeHandle handle = ThemeHandle.Create(className, false))
                {
                    if (handle != null)
                    {
                        flag = System.Windows.Forms.SafeNativeMethods.IsThemePartDefined(new HandleRef(null, handle.NativeHandle), part, 0);
                    }
                    if (flag)
                    {
                        RefreshCache();
                    }
                }
            }
            return flag;
        }

        public static bool IsElementDefined(VisualStyleElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return IsCombinationDefined(element.ClassName, element.Part);
        }

        private static void OnUserPreferenceChanging(object sender, UserPreferenceChangingEventArgs ea)
        {
            if (ea.Category == UserPreferenceCategory.VisualStyle)
            {
                globalCacheVersion += 1L;
            }
        }

        private static void RefreshCache()
        {
            ThemeHandle handle = null;
            if (themeHandles != null)
            {
                string[] array = new string[themeHandles.Keys.Count];
                themeHandles.Keys.CopyTo(array, 0);
                bool flag = VisualStyleInformation.IsEnabledByUser && ((Application.VisualStyleState == VisualStyleState.ClientAreaEnabled) || (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled));
                foreach (string str in array)
                {
                    handle = (ThemeHandle) themeHandles[str];
                    if (handle != null)
                    {
                        handle.Dispose();
                    }
                    if (flag)
                    {
                        handle = ThemeHandle.Create(str, false);
                        if (handle != null)
                        {
                            themeHandles[str] = handle;
                        }
                    }
                }
            }
        }

        public void SetParameters(VisualStyleElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            this.SetParameters(element.ClassName, element.Part, element.State);
        }

        public void SetParameters(string className, int part, int state)
        {
            if (!IsCombinationDefined(className, part))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("VisualStylesInvalidCombination"));
            }
            this._class = className;
            this.part = part;
            this.state = state;
        }

        public string Class
        {
            get
            {
                return this._class;
            }
        }

        public IntPtr Handle
        {
            get
            {
                if (IsSupported)
                {
                    return GetHandle(this._class);
                }
                if (!VisualStyleInformation.IsEnabledByUser)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("VisualStyleNotActive"));
                }
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("VisualStylesDisabledInClientArea"));
            }
        }

        public static bool IsSupported
        {
            get
            {
                bool flag = VisualStyleInformation.IsEnabledByUser && ((Application.VisualStyleState == VisualStyleState.ClientAreaEnabled) || (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled));
                if (flag)
                {
                    flag = GetHandle("BUTTON", false) != IntPtr.Zero;
                }
                return flag;
            }
        }

        public int LastHResult
        {
            get
            {
                return this.lastHResult;
            }
        }

        public int Part
        {
            get
            {
                return this.part;
            }
        }

        public int State
        {
            get
            {
                return this.state;
            }
        }

        private class ThemeHandle : IDisposable
        {
            private IntPtr _hTheme = IntPtr.Zero;

            private ThemeHandle(IntPtr hTheme)
            {
                this._hTheme = hTheme;
            }

            public static VisualStyleRenderer.ThemeHandle Create(string className, bool throwExceptionOnFail)
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    zero = System.Windows.Forms.SafeNativeMethods.OpenThemeData(new HandleRef(null, IntPtr.Zero), className);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    if (throwExceptionOnFail)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("VisualStyleHandleCreationFailed"), exception);
                    }
                    return null;
                }
                if (!(zero == IntPtr.Zero))
                {
                    return new VisualStyleRenderer.ThemeHandle(zero);
                }
                if (throwExceptionOnFail)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("VisualStyleHandleCreationFailed"));
                }
                return null;
            }

            public void Dispose()
            {
                if (this._hTheme != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.CloseThemeData(new HandleRef(null, this._hTheme));
                    this._hTheme = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
            }

            ~ThemeHandle()
            {
                this.Dispose();
            }

            public IntPtr NativeHandle
            {
                get
                {
                    return this._hTheme;
                }
            }
        }
    }
}

