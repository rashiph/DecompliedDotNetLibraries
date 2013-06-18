namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class COMSetterMethod : COMMethodInfo
    {
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            int index = parameters.Length - 1;
            object obj2 = parameters[index];
            object[] target = null;
            if (index > 0)
            {
                target = new object[index];
                ArrayObject.Copy(parameters, 0, target, 0, index);
            }
            else
            {
                target = new object[0];
            }
            base._comObject.SetValue(obj2, invokeAttr, binder, target, culture);
            return null;
        }
    }
}

