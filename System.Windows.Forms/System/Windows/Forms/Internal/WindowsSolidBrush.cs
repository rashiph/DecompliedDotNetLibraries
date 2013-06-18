namespace System.Windows.Forms.Internal
{
    using System;
    using System.Drawing;
    using System.Globalization;

    internal sealed class WindowsSolidBrush : WindowsBrush
    {
        public WindowsSolidBrush(DeviceContext dc) : base(dc)
        {
        }

        public WindowsSolidBrush(DeviceContext dc, Color color) : base(dc, color)
        {
        }

        public override object Clone()
        {
            return new WindowsSolidBrush(base.DC, base.Color);
        }

        protected override void CreateBrush()
        {
            IntPtr ptr = IntSafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(base.Color));
            bool flag1 = ptr == IntPtr.Zero;
            base.NativeHandle = ptr;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: Color={1}", new object[] { base.GetType().Name, base.Color });
        }
    }
}

