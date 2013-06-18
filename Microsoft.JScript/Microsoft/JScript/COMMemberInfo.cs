namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Guid("84BCEB62-16EB-4e1c-975C-FCB40D331043"), ComVisible(true)]
    public interface COMMemberInfo
    {
        object Call(BindingFlags invokeAttr, Binder binder, object[] arguments, CultureInfo culture);
        object GetValue(BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
        void SetValue(object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
    }
}

