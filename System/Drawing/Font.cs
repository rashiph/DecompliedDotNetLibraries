namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Drawing.Internal;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, Editor("System.Drawing.Design.FontEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(FontConverter)), ComVisible(true)]
    public sealed class Font : MarshalByRefObject, ICloneable, ISerializable, IDisposable
    {
        private System.Drawing.FontFamily fontFamily;
        private float fontSize;
        private FontStyle fontStyle;
        private GraphicsUnit fontUnit;
        private byte gdiCharSet;
        private bool gdiVerticalFont;
        private const int LogFontCharSetOffset = 0x17;
        private const int LogFontNameOffset = 0x1c;
        private IntPtr nativeFont;
        private string originalFontName;
        private string systemFontName;

        public Font(Font prototype, FontStyle newStyle)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.originalFontName = prototype.OriginalFontName;
            this.Initialize(prototype.FontFamily, prototype.Size, newStyle, prototype.Unit, 1, false);
        }

        public Font(System.Drawing.FontFamily family, float emSize)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(family, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, false);
        }

        private Font(SerializationInfo info, StreamingContext context)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            string familyName = null;
            float emSize = -1f;
            FontStyle regular = FontStyle.Regular;
            GraphicsUnit point = GraphicsUnit.Point;
            SingleConverter converter = new SingleConverter();
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (string.Equals(enumerator.Name, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    familyName = (string) enumerator.Value;
                }
                else
                {
                    if (string.Equals(enumerator.Name, "Size", StringComparison.OrdinalIgnoreCase))
                    {
                        if (enumerator.Value is string)
                        {
                            emSize = (float) converter.ConvertFrom(enumerator.Value);
                        }
                        else
                        {
                            emSize = (float) enumerator.Value;
                        }
                        continue;
                    }
                    if (string.Compare(enumerator.Name, "Style", true, CultureInfo.InvariantCulture) == 0)
                    {
                        regular = (FontStyle) enumerator.Value;
                    }
                    else if (string.Compare(enumerator.Name, "Unit", true, CultureInfo.InvariantCulture) == 0)
                    {
                        point = (GraphicsUnit) enumerator.Value;
                    }
                }
            }
            this.Initialize(familyName, emSize, regular, point, 1, IsVerticalName(familyName));
        }

        public Font(string familyName, float emSize)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, IsVerticalName(familyName));
        }

        public Font(System.Drawing.FontFamily family, float emSize, FontStyle style)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(family, emSize, style, GraphicsUnit.Point, 1, false);
        }

        public Font(System.Drawing.FontFamily family, float emSize, GraphicsUnit unit)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(family, emSize, FontStyle.Regular, unit, 1, false);
        }

        private Font(IntPtr nativeFont, byte gdiCharSet, bool gdiVerticalFont)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            int status = 0;
            float size = 0f;
            GraphicsUnit point = GraphicsUnit.Point;
            FontStyle regular = FontStyle.Regular;
            IntPtr zero = IntPtr.Zero;
            this.nativeFont = nativeFont;
            status = SafeNativeMethods.Gdip.GdipGetFontUnit(new HandleRef(this, nativeFont), out point);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            status = SafeNativeMethods.Gdip.GdipGetFontSize(new HandleRef(this, nativeFont), out size);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            status = SafeNativeMethods.Gdip.GdipGetFontStyle(new HandleRef(this, nativeFont), out regular);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            status = SafeNativeMethods.Gdip.GdipGetFamily(new HandleRef(this, nativeFont), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.SetFontFamily(new System.Drawing.FontFamily(zero));
            this.Initialize(this.fontFamily, size, regular, point, gdiCharSet, gdiVerticalFont);
        }

        public Font(string familyName, float emSize, FontStyle style)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(familyName, emSize, style, GraphicsUnit.Point, 1, IsVerticalName(familyName));
        }

        public Font(string familyName, float emSize, GraphicsUnit unit)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(familyName, emSize, FontStyle.Regular, unit, 1, IsVerticalName(familyName));
        }

        public Font(System.Drawing.FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(family, emSize, style, unit, 1, false);
        }

        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(familyName, emSize, style, unit, 1, IsVerticalName(familyName));
        }

        public Font(System.Drawing.FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(family, emSize, style, unit, gdiCharSet, false);
        }

        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(familyName, emSize, style, unit, gdiCharSet, IsVerticalName(familyName));
        }

        public Font(System.Drawing.FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            this.Initialize(family, emSize, style, unit, gdiCharSet, gdiVerticalFont);
        }

        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
        {
            this.gdiCharSet = 1;
            this.systemFontName = "";
            if ((float.IsNaN(emSize) || float.IsInfinity(emSize)) || (emSize <= 0f))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidBoundArgument", new object[] { "emSize", emSize, 0, "System.Single.MaxValue" }), "emSize");
            }
            this.Initialize(familyName, emSize, style, unit, gdiCharSet, gdiVerticalFont);
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneFont(new HandleRef(this, this.nativeFont), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Font(zero, this.gdiCharSet, this.gdiVerticalFont);
        }

        private void CreateNativeFont()
        {
            int status = SafeNativeMethods.Gdip.GdipCreateFont(new HandleRef(this, this.fontFamily.NativeFamily), this.fontSize, this.fontStyle, this.fontUnit, out this.nativeFont);
            if (status == 15)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusFontStyleNotFound", new object[] { this.fontFamily.Name, this.fontStyle.ToString() }));
            }
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
            if (this.nativeFont != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeleteFont(new HandleRef(this, this.nativeFont));
                }
                catch (Exception exception)
                {
                    if (System.Drawing.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.nativeFont = IntPtr.Zero;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            Font font = obj as Font;
            if (font == null)
            {
                return false;
            }
            return ((((font.FontFamily.Equals(this.FontFamily) && (font.GdiVerticalFont == this.GdiVerticalFont)) && ((font.GdiCharSet == this.GdiCharSet) && (font.Style == this.Style))) && (font.Size == this.Size)) && (font.Unit == this.Unit));
        }

        ~Font()
        {
            this.Dispose(false);
        }

        public static Font FromHdc(IntPtr hdc)
        {
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateFontFromDC(new HandleRef(null, hdc), ref zero);
            if (status == 0x10)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusNotTrueTypeFont_NoName"));
            }
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new Font(zero, 0, false);
        }

        public static Font FromHfont(IntPtr hfont)
        {
            Font font;
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            SafeNativeMethods.LOGFONT lp = new SafeNativeMethods.LOGFONT();
            SafeNativeMethods.GetObject(new HandleRef(null, hfont), lp);
            IntPtr dC = System.Drawing.UnsafeNativeMethods.GetDC(System.Drawing.NativeMethods.NullHandleRef);
            try
            {
                font = FromLogFont(lp, dC);
            }
            finally
            {
                System.Drawing.UnsafeNativeMethods.ReleaseDC(System.Drawing.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            }
            return font;
        }

        public static Font FromLogFont(object lf)
        {
            Font font;
            IntPtr dC = System.Drawing.UnsafeNativeMethods.GetDC(System.Drawing.NativeMethods.NullHandleRef);
            try
            {
                font = FromLogFont(lf, dC);
            }
            finally
            {
                System.Drawing.UnsafeNativeMethods.ReleaseDC(System.Drawing.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            }
            return font;
        }

        public static Font FromLogFont(object lf, IntPtr hdc)
        {
            int num;
            bool flag;
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            IntPtr zero = IntPtr.Zero;
            if (Marshal.SystemDefaultCharSize == 1)
            {
                num = SafeNativeMethods.Gdip.GdipCreateFontFromLogfontA(new HandleRef(null, hdc), lf, out zero);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipCreateFontFromLogfontW(new HandleRef(null, hdc), lf, out zero);
            }
            if (num == 0x10)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusNotTrueTypeFont_NoName"));
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
            if (zero == IntPtr.Zero)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("GdiplusNotTrueTypeFont", new object[] { lf.ToString() }));
            }
            if (Marshal.SystemDefaultCharSize == 1)
            {
                flag = Marshal.ReadByte(lf, 0x1c) == 0x40;
            }
            else
            {
                flag = Marshal.ReadInt16(lf, 0x1c) == 0x40;
            }
            return new Font(zero, Marshal.ReadByte(lf, 0x17), flag);
        }

        public override int GetHashCode()
        {
            return ((((((int) this.fontStyle) << 13) | (((int) this.fontStyle) >> 0x13)) ^ ((((int) this.fontUnit) << 0x1a) | (((int) this.fontUnit) >> 6))) ^ ((int) ((((uint) this.fontSize) << 7) | (((uint) this.fontSize) >> 0x19))));
        }

        public float GetHeight()
        {
            IntPtr dC = System.Drawing.UnsafeNativeMethods.GetDC(System.Drawing.NativeMethods.NullHandleRef);
            float height = 0f;
            try
            {
                using (Graphics graphics = Graphics.FromHdcInternal(dC))
                {
                    height = this.GetHeight(graphics);
                }
            }
            finally
            {
                System.Drawing.UnsafeNativeMethods.ReleaseDC(System.Drawing.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            }
            return height;
        }

        public float GetHeight(Graphics graphics)
        {
            float num;
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            int status = SafeNativeMethods.Gdip.GdipGetFontHeight(new HandleRef(this, this.NativeFont), new HandleRef(graphics, graphics.NativeGraphics), out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        public float GetHeight(float dpi)
        {
            float num;
            int status = SafeNativeMethods.Gdip.GdipGetFontHeightGivenDPI(new HandleRef(this, this.NativeFont), dpi, out num);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return num;
        }

        private void Initialize(System.Drawing.FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
        {
            if (family == null)
            {
                throw new ArgumentNullException("family");
            }
            if ((float.IsNaN(emSize) || float.IsInfinity(emSize)) || (emSize <= 0f))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidBoundArgument", new object[] { "emSize", emSize, 0, "System.Single.MaxValue" }), "emSize");
            }
            this.fontSize = emSize;
            this.fontStyle = style;
            this.fontUnit = unit;
            this.gdiCharSet = gdiCharSet;
            this.gdiVerticalFont = gdiVerticalFont;
            if (this.fontFamily == null)
            {
                this.SetFontFamily(new System.Drawing.FontFamily(family.NativeFamily));
            }
            if (this.nativeFont == IntPtr.Zero)
            {
                this.CreateNativeFont();
            }
            int status = SafeNativeMethods.Gdip.GdipGetFontSize(new HandleRef(this, this.nativeFont), out this.fontSize);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        private void Initialize(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
        {
            this.originalFontName = familyName;
            this.SetFontFamily(new System.Drawing.FontFamily(StripVerticalName(familyName), true));
            this.Initialize(this.fontFamily, emSize, style, unit, gdiCharSet, gdiVerticalFont);
        }

        private static bool IsVerticalName(string familyName)
        {
            return (((familyName != null) && (familyName.Length > 0)) && (familyName[0] == '@'));
        }

        private void SetFontFamily(System.Drawing.FontFamily family)
        {
            this.fontFamily = family;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            GC.SuppressFinalize(this.fontFamily);
        }

        internal void SetSystemFontName(string systemFontName)
        {
            this.systemFontName = systemFontName;
        }

        private static string StripVerticalName(string familyName)
        {
            if (((familyName != null) && (familyName.Length > 1)) && (familyName[0] == '@'))
            {
                return familyName.Substring(1);
            }
            return familyName;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            si.AddValue("Name", string.IsNullOrEmpty(this.OriginalFontName) ? this.Name : this.OriginalFontName);
            si.AddValue("Size", this.Size);
            si.AddValue("Style", this.Style);
            si.AddValue("Unit", this.Unit);
        }

        public IntPtr ToHfont()
        {
            SafeNativeMethods.LOGFONT logFont = new SafeNativeMethods.LOGFONT();
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Assert();
            try
            {
                this.ToLogFont(logFont);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            IntPtr ptr = IntUnsafeNativeMethods.IntCreateFontIndirect(logFont);
            if (ptr == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            return ptr;
        }

        public void ToLogFont(object logFont)
        {
            IntPtr dC = System.Drawing.UnsafeNativeMethods.GetDC(System.Drawing.NativeMethods.NullHandleRef);
            try
            {
                using (Graphics graphics = Graphics.FromHdcInternal(dC))
                {
                    this.ToLogFont(logFont, graphics);
                }
            }
            finally
            {
                System.Drawing.UnsafeNativeMethods.ReleaseDC(System.Drawing.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            }
        }

        public void ToLogFont(object logFont, Graphics graphics)
        {
            int num;
            System.Drawing.IntSecurity.ObjectFromWin32Handle.Demand();
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (Marshal.SystemDefaultCharSize == 1)
            {
                num = SafeNativeMethods.Gdip.GdipGetLogFontA(new HandleRef(this, this.NativeFont), new HandleRef(graphics, graphics.NativeGraphics), logFont);
            }
            else
            {
                num = SafeNativeMethods.Gdip.GdipGetLogFontW(new HandleRef(this, this.NativeFont), new HandleRef(graphics, graphics.NativeGraphics), logFont);
            }
            if (this.gdiVerticalFont)
            {
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    for (int i = 30; i >= 0; i--)
                    {
                        Marshal.WriteByte(logFont, (0x1c + i) + 1, Marshal.ReadByte(logFont, 0x1c + i));
                    }
                    Marshal.WriteByte(logFont, 0x1c, 0x40);
                }
                else
                {
                    for (int j = 60; j >= 0; j -= 2)
                    {
                        Marshal.WriteInt16(logFont, (0x1c + j) + 2, Marshal.ReadInt16(logFont, 0x1c + j));
                    }
                    Marshal.WriteInt16(logFont, 0x1c, (short) 0x40);
                }
            }
            if (Marshal.ReadByte(logFont, 0x17) == 0)
            {
                Marshal.WriteByte(logFont, 0x17, this.gdiCharSet);
            }
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}: Name={1}, Size={2}, Units={3}, GdiCharSet={4}, GdiVerticalFont={5}]", new object[] { base.GetType().Name, this.FontFamily.Name, this.fontSize, (int) this.fontUnit, this.gdiCharSet, this.gdiVerticalFont });
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Bold
        {
            get
            {
                return ((this.Style & FontStyle.Bold) != FontStyle.Regular);
            }
        }

        [Browsable(false)]
        public System.Drawing.FontFamily FontFamily
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.fontFamily;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte GdiCharSet
        {
            get
            {
                return this.gdiCharSet;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool GdiVerticalFont
        {
            get
            {
                return this.gdiVerticalFont;
            }
        }

        [Browsable(false)]
        public int Height
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (int) Math.Ceiling((double) this.GetHeight());
            }
        }

        [Browsable(false)]
        public bool IsSystemFont
        {
            get
            {
                return !string.IsNullOrEmpty(this.systemFontName);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Italic
        {
            get
            {
                return ((this.Style & FontStyle.Italic) != FontStyle.Regular);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Editor("System.Drawing.Design.FontNameEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(FontConverter.FontNameConverter))]
        public string Name
        {
            get
            {
                return this.FontFamily.Name;
            }
        }

        internal IntPtr NativeFont
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.nativeFont;
            }
        }

        [Browsable(false)]
        public string OriginalFontName
        {
            get
            {
                return this.originalFontName;
            }
        }

        public float Size
        {
            get
            {
                return this.fontSize;
            }
        }

        [Browsable(false)]
        public float SizeInPoints
        {
            get
            {
                float num;
                if (this.Unit == GraphicsUnit.Point)
                {
                    return this.Size;
                }
                IntPtr dC = System.Drawing.UnsafeNativeMethods.GetDC(System.Drawing.NativeMethods.NullHandleRef);
                try
                {
                    using (Graphics graphics = Graphics.FromHdcInternal(dC))
                    {
                        float num2 = (float) (((double) graphics.DpiY) / 72.0);
                        float num4 = (this.GetHeight(graphics) * this.FontFamily.GetEmHeight(this.Style)) / ((float) this.FontFamily.GetLineSpacing(this.Style));
                        num = num4 / num2;
                    }
                }
                finally
                {
                    System.Drawing.UnsafeNativeMethods.ReleaseDC(System.Drawing.NativeMethods.NullHandleRef, new HandleRef(null, dC));
                }
                return num;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Strikeout
        {
            get
            {
                return ((this.Style & FontStyle.Strikeout) != FontStyle.Regular);
            }
        }

        [Browsable(false)]
        public FontStyle Style
        {
            get
            {
                return this.fontStyle;
            }
        }

        [Browsable(false)]
        public string SystemFontName
        {
            get
            {
                return this.systemFontName;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Underline
        {
            get
            {
                return ((this.Style & FontStyle.Underline) != FontStyle.Regular);
            }
        }

        [TypeConverter(typeof(FontConverter.FontUnitConverter))]
        public GraphicsUnit Unit
        {
            get
            {
                return this.fontUnit;
            }
        }
    }
}

