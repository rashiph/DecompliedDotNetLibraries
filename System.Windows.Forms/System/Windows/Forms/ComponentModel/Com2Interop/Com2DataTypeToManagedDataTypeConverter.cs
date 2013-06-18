namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;

    internal abstract class Com2DataTypeToManagedDataTypeConverter
    {
        protected Com2DataTypeToManagedDataTypeConverter()
        {
        }

        public abstract object ConvertManagedToNative(object managedValue, Com2PropertyDescriptor pd, ref bool cancelSet);
        public abstract object ConvertNativeToManaged(object nativeValue, Com2PropertyDescriptor pd);

        public virtual bool AllowExpand
        {
            get
            {
                return false;
            }
        }

        public abstract Type ManagedType { get; }
    }
}

