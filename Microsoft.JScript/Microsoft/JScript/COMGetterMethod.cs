namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class COMGetterMethod : COMMethodInfo
    {
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return base._comObject.GetValue(invokeAttr, binder, (parameters != null) ? parameters : new object[0], culture);
        }
    }
}

