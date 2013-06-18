namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms.Internal;

    internal sealed class WindowsFormsUtils
    {
        public static readonly ContentAlignment AnyBottomAlign = (ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft);
        public static readonly ContentAlignment AnyCenterAlign = (ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter);
        public static readonly ContentAlignment AnyLeftAlign = (ContentAlignment.BottomLeft | ContentAlignment.MiddleLeft | ContentAlignment.TopLeft);
        public static readonly ContentAlignment AnyMiddleAlign = (ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft);
        public static readonly ContentAlignment AnyRightAlign = (ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight);
        public static readonly ContentAlignment AnyTopAlign = (ContentAlignment.TopRight | ContentAlignment.TopCenter | ContentAlignment.TopLeft);
        public static readonly Size UninitializedSize = new Size(-7199369, -5999471);

        internal static string AssertControlInformation(bool condition, Control control)
        {
            if (condition)
            {
                return string.Empty;
            }
            return GetControlInformation(control.Handle);
        }

        internal static Rectangle ConstrainToBounds(Rectangle constrainingBounds, Rectangle bounds)
        {
            if (!constrainingBounds.Contains(bounds))
            {
                bounds.Size = new Size(Math.Min(constrainingBounds.Width - 2, bounds.Width), Math.Min(constrainingBounds.Height - 2, bounds.Height));
                if (bounds.Right > constrainingBounds.Right)
                {
                    bounds.X = constrainingBounds.Right - bounds.Width;
                }
                else if (bounds.Left < constrainingBounds.Left)
                {
                    bounds.X = constrainingBounds.Left;
                }
                if (bounds.Bottom > constrainingBounds.Bottom)
                {
                    bounds.Y = (constrainingBounds.Bottom - 1) - bounds.Height;
                    return bounds;
                }
                if (bounds.Top < constrainingBounds.Top)
                {
                    bounds.Y = constrainingBounds.Top;
                }
            }
            return bounds;
        }

        internal static Rectangle ConstrainToScreenBounds(Rectangle bounds)
        {
            return ConstrainToBounds(Screen.FromRectangle(bounds).Bounds, bounds);
        }

        internal static Rectangle ConstrainToScreenWorkingAreaBounds(Rectangle bounds)
        {
            return ConstrainToBounds(Screen.GetWorkingArea(bounds), bounds);
        }

        public static bool ContainsMnemonic(string text)
        {
            if (text != null)
            {
                int length = text.Length;
                int index = text.IndexOf('&', 0);
                if (((index >= 0) && (index <= (length - 2))) && (text.IndexOf('&', index + 1) == -1))
                {
                    return true;
                }
            }
            return false;
        }

        public static Graphics CreateMeasurementGraphics()
        {
            return Graphics.FromHdcInternal(WindowsGraphicsCacheManager.MeasurementGraphics.DeviceContext.Hdc);
        }

        internal static string EscapeTextWithAmpersands(string text)
        {
            if (text == null)
            {
                return null;
            }
            int index = text.IndexOf('&');
            if (index == -1)
            {
                return text;
            }
            StringBuilder builder = new StringBuilder(text.Substring(0, index));
            while (index < text.Length)
            {
                if (text[index] == '&')
                {
                    builder.Append("&");
                }
                if (index < text.Length)
                {
                    builder.Append(text[index]);
                }
                index++;
            }
            return builder.ToString();
        }

        internal static int GetCombinedHashCodes(params int[] args)
        {
            int num = -757577119;
            for (int i = 0; i < args.Length; i++)
            {
                num = (args[i] ^ num) * -1640531535;
            }
            return num;
        }

        public static string GetComponentName(IComponent component, string defaultNameValue)
        {
            string name = string.Empty;
            if (string.IsNullOrEmpty(defaultNameValue))
            {
                if (component.Site != null)
                {
                    name = component.Site.Name;
                }
                if (name == null)
                {
                    name = string.Empty;
                }
                return name;
            }
            return defaultNameValue;
        }

        internal static string GetControlInformation(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return "Handle is IntPtr.Zero";
            }
            return "";
        }

        public static char GetMnemonic(string text, bool bConvertToUpperCase)
        {
            if (text != null)
            {
                int length = text.Length;
                for (int i = 0; i < (length - 1); i++)
                {
                    if (text[i] == '&')
                    {
                        if (text[i + 1] != '&')
                        {
                            if (bConvertToUpperCase)
                            {
                                return char.ToUpper(text[i + 1], CultureInfo.CurrentCulture);
                            }
                            return char.ToLower(text[i + 1], CultureInfo.CurrentCulture);
                        }
                        i++;
                    }
                }
            }
            return '\0';
        }

        public static HandleRef GetRootHWnd(HandleRef hwnd)
        {
            return new HandleRef(hwnd.Wrapper, System.Windows.Forms.UnsafeNativeMethods.GetAncestor(new HandleRef(hwnd, hwnd.Handle), 2));
        }

        public static HandleRef GetRootHWnd(Control control)
        {
            return GetRootHWnd(new HandleRef(control, control.Handle));
        }

        public static int RotateLeft(int value, int nBits)
        {
            nBits = nBits % 0x20;
            return ((value << nBits) | (value >> (0x20 - nBits)));
        }

        public static bool SafeCompareStrings(string string1, string string2, bool ignoreCase)
        {
            if ((string1 == null) || (string2 == null))
            {
                return false;
            }
            if (string1.Length != string2.Length)
            {
                return false;
            }
            return (string.Compare(string1, string2, ignoreCase, CultureInfo.InvariantCulture) == 0);
        }

        public static string TextWithoutMnemonics(string text)
        {
            if (text == null)
            {
                return null;
            }
            int index = text.IndexOf('&');
            if (index == -1)
            {
                return text;
            }
            StringBuilder builder = new StringBuilder(text.Substring(0, index));
            while (index < text.Length)
            {
                if (text[index] == '&')
                {
                    index++;
                }
                if (index < text.Length)
                {
                    builder.Append(text[index]);
                }
                index++;
            }
            return builder.ToString();
        }

        public static Point TranslatePoint(Point point, Control fromControl, Control toControl)
        {
            System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(point.X, point.Y);
            System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(fromControl, fromControl.Handle), new HandleRef(toControl, toControl.Handle), pt, 1);
            return new Point(pt.x, pt.y);
        }

        public static Point LastCursorPoint
        {
            get
            {
                int messagePos = System.Windows.Forms.SafeNativeMethods.GetMessagePos();
                return new Point(System.Windows.Forms.NativeMethods.Util.SignedLOWORD(messagePos), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(messagePos));
            }
        }

        public class ArraySubsetEnumerator : IEnumerator
        {
            private object[] array;
            private int current;
            private int total;

            public ArraySubsetEnumerator(object[] array, int count)
            {
                this.array = array;
                this.total = count;
                this.current = -1;
            }

            public bool MoveNext()
            {
                if (this.current < (this.total - 1))
                {
                    this.current++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.current = -1;
            }

            public object Current
            {
                get
                {
                    if (this.current == -1)
                    {
                        return null;
                    }
                    return this.array[this.current];
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DCMapping : IDisposable
        {
            private DeviceContext dc;
            private System.Drawing.Graphics graphics;
            private Rectangle translatedBounds;
            public DCMapping(HandleRef hDC, Rectangle bounds)
            {
                if (hDC.Handle == IntPtr.Zero)
                {
                    throw new ArgumentNullException("hDC");
                }
                System.Windows.Forms.NativeMethods.POINT point = new System.Windows.Forms.NativeMethods.POINT();
                HandleRef nullHandleRef = System.Windows.Forms.NativeMethods.NullHandleRef;
                System.Windows.Forms.NativeMethods.RegionFlags nULLREGION = System.Windows.Forms.NativeMethods.RegionFlags.NULLREGION;
                this.translatedBounds = bounds;
                this.graphics = null;
                this.dc = DeviceContext.FromHdc(hDC.Handle);
                this.dc.SaveHdc();
                System.Windows.Forms.SafeNativeMethods.GetViewportOrgEx(hDC, point);
                HandleRef hRgn = new HandleRef(null, System.Windows.Forms.SafeNativeMethods.CreateRectRgn(point.x + bounds.Left, point.y + bounds.Top, point.x + bounds.Right, point.y + bounds.Bottom));
                try
                {
                    nullHandleRef = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateRectRgn(0, 0, 0, 0));
                    int clipRgn = System.Windows.Forms.SafeNativeMethods.GetClipRgn(hDC, nullHandleRef);
                    System.Windows.Forms.NativeMethods.POINT point2 = new System.Windows.Forms.NativeMethods.POINT();
                    System.Windows.Forms.SafeNativeMethods.SetViewportOrgEx(hDC, point.x + bounds.Left, point.y + bounds.Top, point2);
                    if (clipRgn != 0)
                    {
                        System.Windows.Forms.NativeMethods.RECT clipRect = new System.Windows.Forms.NativeMethods.RECT();
                        if (System.Windows.Forms.SafeNativeMethods.GetRgnBox(nullHandleRef, ref clipRect) == 2)
                        {
                            System.Windows.Forms.SafeNativeMethods.CombineRgn(hRgn, hRgn, nullHandleRef, 1);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(nullHandleRef);
                        nullHandleRef = new HandleRef(null, IntPtr.Zero);
                        nULLREGION = System.Windows.Forms.NativeMethods.RegionFlags.SIMPLEREGION;
                    }
                    System.Windows.Forms.SafeNativeMethods.SelectClipRgn(hDC, hRgn);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    this.dc.RestoreHdc();
                    this.dc.Dispose();
                }
                finally
                {
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(hRgn);
                    if (nullHandleRef.Handle != IntPtr.Zero)
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(nullHandleRef);
                    }
                }
            }

            public void Dispose()
            {
                if (this.graphics != null)
                {
                    this.graphics.Dispose();
                    this.graphics = null;
                }
                if (this.dc != null)
                {
                    this.dc.RestoreHdc();
                    this.dc.Dispose();
                    this.dc = null;
                }
            }

            public System.Drawing.Graphics Graphics
            {
                get
                {
                    if (this.graphics == null)
                    {
                        this.graphics = System.Drawing.Graphics.FromHdcInternal(this.dc.Hdc);
                        this.graphics.SetClip(new Rectangle(Point.Empty, this.translatedBounds.Size));
                    }
                    return this.graphics;
                }
            }
        }

        public static class EnumValidator
        {
            public static bool IsEnumWithinShiftedRange(Enum enumValue, int numBitsToShift, int minValAfterShift, int maxValAfterShift)
            {
                int num = Convert.ToInt32(enumValue, CultureInfo.InvariantCulture);
                int num2 = num >> numBitsToShift;
                if ((num2 << numBitsToShift) != num)
                {
                    return false;
                }
                return ((num2 >= minValAfterShift) && (num2 <= maxValAfterShift));
            }

            public static bool IsValidArrowDirection(ArrowDirection direction)
            {
                switch (direction)
                {
                    case ArrowDirection.Left:
                    case ArrowDirection.Up:
                    case ArrowDirection.Right:
                    case ArrowDirection.Down:
                        return true;
                }
                return false;
            }

            public static bool IsValidContentAlignment(ContentAlignment contentAlign)
            {
                if (System.Windows.Forms.ClientUtils.GetBitCount((uint) contentAlign) != 1)
                {
                    return false;
                }
                int num = 0x777;
                return ((num & contentAlign) != 0);
            }

            public static bool IsValidTextImageRelation(TextImageRelation relation)
            {
                return System.Windows.Forms.ClientUtils.IsEnumValid(relation, (int) relation, 0, 8, 1);
            }
        }

        internal class ReadOnlyControlCollection : Control.ControlCollection
        {
            private readonly bool _isReadOnly;

            public ReadOnlyControlCollection(Control owner, bool isReadOnly) : base(owner)
            {
                this._isReadOnly = isReadOnly;
            }

            public override void Add(Control value)
            {
                if (this.IsReadOnly)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                }
                this.AddInternal(value);
            }

            internal virtual void AddInternal(Control value)
            {
                base.Add(value);
            }

            public override void Clear()
            {
                if (this.IsReadOnly)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                }
                base.Clear();
            }

            public override void RemoveByKey(string key)
            {
                if (this.IsReadOnly)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                }
                base.RemoveByKey(key);
            }

            internal virtual void RemoveInternal(Control value)
            {
                base.Remove(value);
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this._isReadOnly;
                }
            }
        }

        internal class TypedControlCollection : WindowsFormsUtils.ReadOnlyControlCollection
        {
            private Control ownerControl;
            private System.Type typeOfControl;

            public TypedControlCollection(Control owner, System.Type typeOfControl) : base(owner, false)
            {
                this.typeOfControl = typeOfControl;
                this.ownerControl = owner;
            }

            public TypedControlCollection(Control owner, System.Type typeOfControl, bool isReadOnly) : base(owner, isReadOnly)
            {
                this.typeOfControl = typeOfControl;
                this.ownerControl = owner;
            }

            public override void Add(Control value)
            {
                Control.CheckParentingCycle(this.ownerControl, value);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.IsReadOnly)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                }
                if (!this.typeOfControl.IsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TypedControlCollectionShouldBeOfType", new object[] { this.typeOfControl.Name }), new object[0]), value.GetType().Name);
                }
                base.Add(value);
            }
        }
    }
}

