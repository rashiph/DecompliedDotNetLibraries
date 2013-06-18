namespace System.Drawing
{
    using System;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;

    public sealed class SolidBrush : Brush, ISystemColorTracker
    {
        private System.Drawing.Color color;
        private bool immutable;

        public SolidBrush(System.Drawing.Color color)
        {
            this.color = System.Drawing.Color.Empty;
            this.color = color;
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCreateSolidFill(this.color.ToArgb(), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            base.SetNativeBrushInternal(zero);
            if (color.IsSystemColor)
            {
                SystemColorTracker.Add(this);
            }
        }

        internal SolidBrush(IntPtr nativeBrush)
        {
            this.color = System.Drawing.Color.Empty;
            base.SetNativeBrushInternal(nativeBrush);
        }

        internal SolidBrush(System.Drawing.Color color, bool immutable) : this(color)
        {
            this.immutable = immutable;
        }

        public override object Clone()
        {
            IntPtr zero = IntPtr.Zero;
            int status = SafeNativeMethods.Gdip.GdipCloneBrush(new HandleRef(this, base.NativeBrush), out zero);
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            return new SolidBrush(zero);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                this.immutable = false;
            }
            else if (this.immutable)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Brush" }));
            }
            base.Dispose(disposing);
        }

        private void InternalSetColor(System.Drawing.Color value)
        {
            int status = SafeNativeMethods.Gdip.GdipSetSolidFillColor(new HandleRef(this, base.NativeBrush), value.ToArgb());
            if (status != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(status);
            }
            this.color = value;
        }

        void ISystemColorTracker.OnSystemColorChanged()
        {
            if (base.NativeBrush != IntPtr.Zero)
            {
                this.InternalSetColor(this.color);
            }
        }

        public System.Drawing.Color Color
        {
            get
            {
                if (this.color == System.Drawing.Color.Empty)
                {
                    int color = 0;
                    int status = SafeNativeMethods.Gdip.GdipGetSolidFillColor(new HandleRef(this, base.NativeBrush), out color);
                    if (status != 0)
                    {
                        throw SafeNativeMethods.Gdip.StatusException(status);
                    }
                    this.color = System.Drawing.Color.FromArgb(color);
                }
                return this.color;
            }
            set
            {
                if (this.immutable)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("CantChangeImmutableObjects", new object[] { "Brush" }));
                }
                if (this.color != value)
                {
                    System.Drawing.Color color = this.color;
                    this.InternalSetColor(value);
                    if (value.IsSystemColor && !color.IsSystemColor)
                    {
                        SystemColorTracker.Add(this);
                    }
                }
            }
        }
    }
}

