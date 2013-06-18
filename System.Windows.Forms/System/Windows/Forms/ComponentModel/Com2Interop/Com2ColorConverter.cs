namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Drawing;

    internal class Com2ColorConverter : Com2DataTypeToManagedDataTypeConverter
    {
        public override object ConvertManagedToNative(object managedValue, Com2PropertyDescriptor pd, ref bool cancelSet)
        {
            cancelSet = false;
            if (managedValue == null)
            {
                managedValue = Color.Black;
            }
            if (managedValue is Color)
            {
                return ColorTranslator.ToOle((Color) managedValue);
            }
            return 0;
        }

        public override object ConvertNativeToManaged(object nativeValue, Com2PropertyDescriptor pd)
        {
            int oleColor = 0;
            if (nativeValue is uint)
            {
                oleColor = (int) ((uint) nativeValue);
            }
            else if (nativeValue is int)
            {
                oleColor = (int) nativeValue;
            }
            return ColorTranslator.FromOle(oleColor);
        }

        public override Type ManagedType
        {
            get
            {
                return typeof(Color);
            }
        }
    }
}

