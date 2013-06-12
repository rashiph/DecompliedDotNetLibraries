namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, TypeConverter(typeof(CursorConverter)), Editor("System.Drawing.Design.CursorEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class Cursor : IDisposable, ISerializable
    {
        private byte[] cursorData;
        private static System.Drawing.Size cursorSize = System.Drawing.Size.Empty;
        private IntPtr handle;
        private bool ownHandle;
        private int resourceId;
        private object userData;

        public Cursor(IntPtr handle)
        {
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidGDIHandle", new object[] { typeof(Cursor).Name }));
            }
            this.handle = handle;
            this.ownHandle = false;
        }

        public Cursor(Stream stream)
        {
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            this.cursorData = new byte[stream.Length];
            stream.Read(this.cursorData, 0, Convert.ToInt32(stream.Length));
            this.LoadPicture(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(this.cursorData)));
        }

        public Cursor(string fileName)
        {
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                this.cursorData = new byte[stream.Length];
                stream.Read(this.cursorData, 0, Convert.ToInt32(stream.Length));
            }
            finally
            {
                stream.Close();
            }
            this.LoadPicture(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(this.cursorData)));
        }

        internal Cursor(int nResourceId, int dummy)
        {
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            this.LoadFromResourceId(nResourceId);
        }

        internal Cursor(SerializationInfo info, StreamingContext context)
        {
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    if (string.Equals(enumerator.Name, "CursorData", StringComparison.OrdinalIgnoreCase))
                    {
                        this.cursorData = (byte[]) enumerator.Value;
                        if (this.cursorData != null)
                        {
                            this.LoadPicture(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(this.cursorData)));
                        }
                    }
                    else if (string.Compare(enumerator.Name, "CursorResourceId", true, CultureInfo.InvariantCulture) == 0)
                    {
                        this.LoadFromResourceId((int) enumerator.Value);
                    }
                }
            }
        }

        internal Cursor(string resource, int dummy)
        {
            this.handle = IntPtr.Zero;
            this.ownHandle = true;
            Stream manifestResourceStream = typeof(Cursor).Module.Assembly.GetManifestResourceStream(typeof(Cursor), resource);
            this.cursorData = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(this.cursorData, 0, Convert.ToInt32(manifestResourceStream.Length));
            this.LoadPicture(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(this.cursorData)));
        }

        public Cursor(System.Type type, string resource) : this(type.Module.Assembly.GetManifestResourceStream(type, resource))
        {
        }

        public IntPtr CopyHandle()
        {
            System.Drawing.Size size = this.Size;
            return System.Windows.Forms.SafeNativeMethods.CopyImage(new HandleRef(this, this.Handle), 2, size.Width, size.Height, 0);
        }

        private void DestroyHandle()
        {
            if (this.ownHandle)
            {
                System.Windows.Forms.UnsafeNativeMethods.DestroyCursor(new HandleRef(this, this.handle));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.handle != IntPtr.Zero)
            {
                this.DestroyHandle();
                this.handle = IntPtr.Zero;
            }
        }

        public void Draw(Graphics g, Rectangle targetRect)
        {
            this.DrawImageCore(g, Rectangle.Empty, targetRect, false);
        }

        private void DrawImageCore(Graphics graphics, Rectangle imageRect, Rectangle targetRect, bool stretch)
        {
            targetRect.X += (int) graphics.Transform.OffsetX;
            targetRect.Y += (int) graphics.Transform.OffsetY;
            int num = 0xcc0020;
            IntPtr hdc = graphics.GetHdc();
            try
            {
                int width;
                int height;
                int num10;
                int num11;
                int num12;
                int num13;
                int num2 = 0;
                int num3 = 0;
                int x = 0;
                int y = 0;
                int num8 = 0;
                int num9 = 0;
                System.Drawing.Size size = this.Size;
                if (!imageRect.IsEmpty)
                {
                    num2 = imageRect.X;
                    num3 = imageRect.Y;
                    width = imageRect.Width;
                    height = imageRect.Height;
                }
                else
                {
                    width = size.Width;
                    height = size.Height;
                }
                if (!targetRect.IsEmpty)
                {
                    x = targetRect.X;
                    y = targetRect.Y;
                    num8 = targetRect.Width;
                    num9 = targetRect.Height;
                }
                else
                {
                    num8 = size.Width;
                    num9 = size.Height;
                }
                if (stretch)
                {
                    if ((((num8 == width) && (num9 == height)) && ((num2 == 0) && (num3 == 0))) && (((num == 0xcc0020) && (width == size.Width)) && (height == size.Height)))
                    {
                        System.Windows.Forms.SafeNativeMethods.DrawIcon(new HandleRef(graphics, hdc), x, y, new HandleRef(this, this.handle));
                        return;
                    }
                    num10 = (size.Width * num8) / width;
                    num11 = (size.Height * num9) / height;
                    num12 = num8;
                    num13 = num9;
                }
                else
                {
                    if ((((num2 == 0) && (num3 == 0)) && ((num == 0xcc0020) && (size.Width <= num8))) && (((size.Height <= num9) && (size.Width == width)) && (size.Height == height)))
                    {
                        System.Windows.Forms.SafeNativeMethods.DrawIcon(new HandleRef(graphics, hdc), x, y, new HandleRef(this, this.handle));
                        return;
                    }
                    num10 = size.Width;
                    num11 = size.Height;
                    num12 = (num8 < width) ? num8 : width;
                    num13 = (num9 < height) ? num9 : height;
                }
                if (num == 0xcc0020)
                {
                    System.Windows.Forms.SafeNativeMethods.IntersectClipRect(new HandleRef(this, this.Handle), x, y, x + num12, y + num13);
                    System.Windows.Forms.SafeNativeMethods.DrawIconEx(new HandleRef(graphics, hdc), x - num2, y - num3, new HandleRef(this, this.handle), num10, num11, 0, System.Windows.Forms.NativeMethods.NullHandleRef, 3);
                }
            }
            finally
            {
                graphics.ReleaseHdcInternal(hdc);
            }
        }

        public void DrawStretched(Graphics g, Rectangle targetRect)
        {
            this.DrawImageCore(g, Rectangle.Empty, targetRect, true);
        }

        public override bool Equals(object obj)
        {
            return ((obj is Cursor) && (this == ((Cursor) obj)));
        }

        ~Cursor()
        {
            this.Dispose(false);
        }

        public override int GetHashCode()
        {
            return (int) this.handle;
        }

        private System.Drawing.Size GetIconSize(IntPtr iconHandle)
        {
            System.Drawing.Size size = this.Size;
            System.Windows.Forms.NativeMethods.ICONINFO info = new System.Windows.Forms.NativeMethods.ICONINFO();
            System.Windows.Forms.SafeNativeMethods.GetIconInfo(new HandleRef(this, iconHandle), info);
            System.Windows.Forms.NativeMethods.BITMAP bm = new System.Windows.Forms.NativeMethods.BITMAP();
            if (info.hbmColor != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.GetObject(new HandleRef(null, info.hbmColor), Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.BITMAP)), bm);
                System.Windows.Forms.SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmColor));
                size = new System.Drawing.Size(bm.bmWidth, bm.bmHeight);
            }
            else if (info.hbmMask != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.GetObject(new HandleRef(null, info.hbmMask), Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.BITMAP)), bm);
                size = new System.Drawing.Size(bm.bmWidth, bm.bmHeight / 2);
            }
            if (info.hbmMask != IntPtr.Zero)
            {
                System.Windows.Forms.SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmMask));
            }
            return size;
        }

        public static void Hide()
        {
            System.Windows.Forms.IntSecurity.AdjustCursorClip.Demand();
            System.Windows.Forms.UnsafeNativeMethods.ShowCursor(false);
        }

        private void LoadFromResourceId(int nResourceId)
        {
            this.ownHandle = false;
            try
            {
                this.resourceId = nResourceId;
                this.handle = System.Windows.Forms.SafeNativeMethods.LoadCursor(System.Windows.Forms.NativeMethods.NullHandleRef, nResourceId);
            }
            catch (Exception)
            {
                this.handle = IntPtr.Zero;
            }
        }

        private void LoadPicture(System.Windows.Forms.UnsafeNativeMethods.IStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            try
            {
                Guid gUID = typeof(System.Windows.Forms.UnsafeNativeMethods.IPicture).GUID;
                System.Windows.Forms.UnsafeNativeMethods.IPicture o = null;
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    o = System.Windows.Forms.UnsafeNativeMethods.OleCreateIPictureIndirect(null, ref gUID, true);
                    ((System.Windows.Forms.UnsafeNativeMethods.IPersistStream) o).Load(stream);
                    if ((o == null) || (o.GetPictureType() != 3))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidPictureType", new object[] { "picture", "Cursor" }), "picture");
                    }
                    IntPtr handle = o.GetHandle();
                    System.Drawing.Size iconSize = this.GetIconSize(handle);
                    this.handle = System.Windows.Forms.SafeNativeMethods.CopyImageAsCursor(new HandleRef(this, handle), 2, iconSize.Width, iconSize.Height, 0);
                    this.ownHandle = true;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                    if (o != null)
                    {
                        Marshal.ReleaseComObject(o);
                    }
                }
            }
            catch (COMException exception)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidPictureFormat"), "stream", exception);
            }
        }

        public static bool operator ==(Cursor left, Cursor right)
        {
            if (object.ReferenceEquals(left, null) != object.ReferenceEquals(right, null))
            {
                return false;
            }
            if (!object.ReferenceEquals(left, null))
            {
                return (left.handle == right.handle);
            }
            return true;
        }

        public static bool operator !=(Cursor left, Cursor right)
        {
            return !(left == right);
        }

        internal void SavePicture(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.resourceId != 0)
            {
                throw new FormatException(System.Windows.Forms.SR.GetString("CursorCannotCovertToBytes"));
            }
            try
            {
                stream.Write(this.cursorData, 0, this.cursorData.Length);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("InvalidPictureFormat"));
            }
        }

        public static void Show()
        {
            System.Windows.Forms.UnsafeNativeMethods.ShowCursor(true);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (this.cursorData != null)
            {
                si.AddValue("CursorData", this.cursorData, typeof(byte[]));
            }
            else
            {
                if (this.resourceId == 0)
                {
                    throw new SerializationException(System.Windows.Forms.SR.GetString("CursorNonSerializableHandle"));
                }
                si.AddValue("CursorResourceId", this.resourceId, typeof(int));
            }
        }

        public override string ToString()
        {
            string str = null;
            if (!this.ownHandle)
            {
                str = TypeDescriptor.GetConverter(typeof(Cursor)).ConvertToString(this);
            }
            else
            {
                str = base.ToString();
            }
            return ("[Cursor: " + str + "]");
        }

        public static Rectangle Clip
        {
            get
            {
                return ClipInternal;
            }
            set
            {
                if (!value.IsEmpty)
                {
                    System.Windows.Forms.IntSecurity.AdjustCursorClip.Demand();
                }
                ClipInternal = value;
            }
        }

        internal static Rectangle ClipInternal
        {
            get
            {
                System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.SafeNativeMethods.GetClipCursor(ref lpRect);
                return Rectangle.FromLTRB(lpRect.left, lpRect.top, lpRect.right, lpRect.bottom);
            }
            set
            {
                if (value.IsEmpty)
                {
                    System.Windows.Forms.UnsafeNativeMethods.ClipCursor((System.Windows.Forms.NativeMethods.COMRECT) null);
                }
                else
                {
                    System.Windows.Forms.UnsafeNativeMethods.ClipCursor(ref System.Windows.Forms.NativeMethods.RECT.FromXYWH(value.X, value.Y, value.Width, value.Height));
                }
            }
        }

        public static Cursor Current
        {
            get
            {
                return CurrentInternal;
            }
            set
            {
                System.Windows.Forms.IntSecurity.ModifyCursor.Demand();
                CurrentInternal = value;
            }
        }

        internal static Cursor CurrentInternal
        {
            get
            {
                IntPtr cursor = System.Windows.Forms.SafeNativeMethods.GetCursor();
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                return Cursors.KnownCursorFromHCursor(cursor);
            }
            set
            {
                IntPtr handle = (value == null) ? IntPtr.Zero : value.handle;
                System.Windows.Forms.UnsafeNativeMethods.SetCursor(new HandleRef(value, handle));
            }
        }

        public IntPtr Handle
        {
            get
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException(System.Windows.Forms.SR.GetString("ObjectDisposed", new object[] { base.GetType().Name }));
                }
                return this.handle;
            }
        }

        public Point HotSpot
        {
            get
            {
                Point empty = Point.Empty;
                System.Windows.Forms.NativeMethods.ICONINFO info = new System.Windows.Forms.NativeMethods.ICONINFO();
                Icon icon = null;
                System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                try
                {
                    icon = Icon.FromHandle(this.Handle);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                try
                {
                    System.Windows.Forms.SafeNativeMethods.GetIconInfo(new HandleRef(this, icon.Handle), info);
                    empty = new Point(info.xHotspot, info.yHotspot);
                }
                finally
                {
                    if (info.hbmMask != IntPtr.Zero)
                    {
                        System.Windows.Forms.SafeNativeMethods.ExternalDeleteObject(new HandleRef(null, info.hbmMask));
                        info.hbmMask = IntPtr.Zero;
                    }
                    if (info.hbmColor != IntPtr.Zero)
                    {
                        System.Windows.Forms.SafeNativeMethods.ExternalDeleteObject(new HandleRef(null, info.hbmColor));
                        info.hbmColor = IntPtr.Zero;
                    }
                    icon.Dispose();
                }
                return empty;
            }
        }

        public static Point Position
        {
            get
            {
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                return new Point(pt.x, pt.y);
            }
            set
            {
                System.Windows.Forms.IntSecurity.AdjustCursorPosition.Demand();
                System.Windows.Forms.UnsafeNativeMethods.SetCursorPos(value.X, value.Y);
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                if (cursorSize.IsEmpty)
                {
                    cursorSize = new System.Drawing.Size(System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(13), System.Windows.Forms.UnsafeNativeMethods.GetSystemMetrics(14));
                }
                return cursorSize;
            }
        }

        [DefaultValue((string) null), Localizable(false), System.Windows.Forms.SRDescription("ControlTagDescr"), Bindable(true), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRCategory("CatData")]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }
    }
}

