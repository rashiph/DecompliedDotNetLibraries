namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Text;
    using System.Runtime.InteropServices;

    public sealed class StringFormat : MarshalByRefObject, ICloneable, IDisposable
    {
        internal IntPtr nativeFormat;

        public StringFormat() : this(0, 0)
        {
        }

        public StringFormat(StringFormat format)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            int status = SafeNativeMethods.Gdip.GdipCloneStringFormat(new HandleRef(format, format.nativeFormat), out this.nativeFormat);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public StringFormat(StringFormatFlags options) : this(options, 0)
        {
        }

        private StringFormat(IntPtr format)
        {
            this.nativeFormat = format;
        }

        public StringFormat(StringFormatFlags options, int language)
        {
            int status = SafeNativeMethods.Gdip.GdipCreateStringFormat(options, language, out this.nativeFormat);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneStringFormat(new HandleRef(this, this.nativeFormat), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new StringFormat(zero);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.nativeFormat != IntPtr.Zero)
            {
                try
                {
                    SafeNativeMethods.Gdip.GdipDeleteStringFormat(new HandleRef(this, this.nativeFormat));
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
                    this.nativeFormat = IntPtr.Zero;
                }
            }
        }

        ~StringFormat()
        {
            this.Dispose(false);
        }

        public float[] GetTabStops(out float firstTabOffset)
        {
            int count = 0;
            int status = SafeNativeMethods.Gdip.GdipGetStringFormatTabStopCount(new HandleRef(this, this.nativeFormat), out count);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            float[] tabStops = new float[count];
            status = SafeNativeMethods.Gdip.GdipGetStringFormatTabStops(new HandleRef(this, this.nativeFormat), count, out firstTabOffset, tabStops);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return tabStops;
        }

        public void SetDigitSubstitution(int language, StringDigitSubstitute substitute)
        {
            int status = SafeNativeMethods.Gdip.GdipSetStringFormatDigitSubstitution(new HandleRef(this, this.nativeFormat), language, substitute);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetMeasurableCharacterRanges(CharacterRange[] ranges)
        {
            int status = SafeNativeMethods.Gdip.GdipSetStringFormatMeasurableCharacterRanges(new HandleRef(this, this.nativeFormat), ranges.Length, ranges);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public void SetTabStops(float firstTabOffset, float[] tabStops)
        {
            if (firstTabOffset < 0f)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidArgument", new object[] { "firstTabOffset", firstTabOffset }));
            }
            int status = SafeNativeMethods.Gdip.GdipSetStringFormatTabStops(new HandleRef(this, this.nativeFormat), firstTabOffset, tabStops.Length, tabStops);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
        }

        public override string ToString()
        {
            return ("[StringFormat, FormatFlags=" + this.FormatFlags.ToString() + "]");
        }

        public StringAlignment Alignment
        {
            get
            {
                StringAlignment near = StringAlignment.Near;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatAlign(new HandleRef(this, this.nativeFormat), out near);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return near;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(StringAlignment));
                }
                int status = SafeNativeMethods.Gdip.GdipSetStringFormatAlign(new HandleRef(this, this.nativeFormat), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public int DigitSubstitutionLanguage
        {
            get
            {
                StringDigitSubstitute substitute;
                int langID = 0;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatDigitSubstitution(new HandleRef(this, this.nativeFormat), out langID, out substitute);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return langID;
            }
        }

        public StringDigitSubstitute DigitSubstitutionMethod
        {
            get
            {
                StringDigitSubstitute substitute;
                int langID = 0;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatDigitSubstitution(new HandleRef(this, this.nativeFormat), out langID, out substitute);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return substitute;
            }
        }

        public StringFormatFlags FormatFlags
        {
            get
            {
                StringFormatFlags flags;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatFlags(new HandleRef(this, this.nativeFormat), out flags);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return flags;
            }
            set
            {
                int status = SafeNativeMethods.Gdip.GdipSetStringFormatFlags(new HandleRef(this, this.nativeFormat), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public static StringFormat GenericDefault
        {
            get
            {
                IntPtr ptr;
                int status = SafeNativeMethods.Gdip.GdipStringFormatGetGenericDefault(out ptr);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return new StringFormat(ptr);
            }
        }

        public static StringFormat GenericTypographic
        {
            get
            {
                IntPtr ptr;
                int status = SafeNativeMethods.Gdip.GdipStringFormatGetGenericTypographic(out ptr);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return new StringFormat(ptr);
            }
        }

        public System.Drawing.Text.HotkeyPrefix HotkeyPrefix
        {
            get
            {
                System.Drawing.Text.HotkeyPrefix prefix;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatHotkeyPrefix(new HandleRef(this, this.nativeFormat), out prefix);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return prefix;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Drawing.Text.HotkeyPrefix));
                }
                int status = SafeNativeMethods.Gdip.GdipSetStringFormatHotkeyPrefix(new HandleRef(this, this.nativeFormat), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public StringAlignment LineAlignment
        {
            get
            {
                StringAlignment near = StringAlignment.Near;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatLineAlign(new HandleRef(this, this.nativeFormat), out near);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return near;
            }
            set
            {
                if ((value < StringAlignment.Near) || (value > StringAlignment.Far))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(StringAlignment));
                }
                int status = SafeNativeMethods.Gdip.GdipSetStringFormatLineAlign(new HandleRef(this, this.nativeFormat), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }

        public StringTrimming Trimming
        {
            get
            {
                StringTrimming trimming;
                int status = SafeNativeMethods.Gdip.GdipGetStringFormatTrimming(new HandleRef(this, this.nativeFormat), out trimming);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
                return trimming;
            }
            set
            {
                if (!System.Drawing.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(StringTrimming));
                }
                int status = SafeNativeMethods.Gdip.GdipSetStringFormatTrimming(new HandleRef(this, this.nativeFormat), value);
                if (status != 0)
                {
                    throw SafeNativeMethods.Gdip.StatusException(status);
                }
            }
        }
    }
}

