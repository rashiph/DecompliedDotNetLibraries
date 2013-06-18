namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class Com2FontConverter : Com2DataTypeToManagedDataTypeConverter
    {
        private Font lastFont;
        private IntPtr lastHandle = IntPtr.Zero;

        public override object ConvertManagedToNative(object managedValue, Com2PropertyDescriptor pd, ref bool cancelSet)
        {
            if (managedValue == null)
            {
                managedValue = Control.DefaultFont;
            }
            cancelSet = true;
            if ((this.lastFont == null) || !this.lastFont.Equals(managedValue))
            {
                this.lastFont = (Font) managedValue;
                System.Windows.Forms.UnsafeNativeMethods.IFont nativeValue = (System.Windows.Forms.UnsafeNativeMethods.IFont) pd.GetNativeValue(pd.TargetObject);
                if ((nativeValue != null) && ControlPaint.FontToIFont(this.lastFont, nativeValue))
                {
                    this.lastFont = null;
                    this.ConvertNativeToManaged(nativeValue, pd);
                }
            }
            return null;
        }

        public override object ConvertNativeToManaged(object nativeValue, Com2PropertyDescriptor pd)
        {
            System.Windows.Forms.UnsafeNativeMethods.IFont font = nativeValue as System.Windows.Forms.UnsafeNativeMethods.IFont;
            if (font == null)
            {
                this.lastHandle = IntPtr.Zero;
                this.lastFont = Control.DefaultFont;
                return this.lastFont;
            }
            IntPtr hFont = font.GetHFont();
            if ((hFont != this.lastHandle) || (this.lastFont == null))
            {
                this.lastHandle = hFont;
                try
                {
                    using (Font font2 = Font.FromHfont(this.lastHandle))
                    {
                        this.lastFont = ControlPaint.FontInPoints(font2);
                    }
                }
                catch (ArgumentException)
                {
                    this.lastFont = Control.DefaultFont;
                }
            }
            return this.lastFont;
        }

        public override bool AllowExpand
        {
            get
            {
                return true;
            }
        }

        public override System.Type ManagedType
        {
            get
            {
                return typeof(Font);
            }
        }
    }
}

