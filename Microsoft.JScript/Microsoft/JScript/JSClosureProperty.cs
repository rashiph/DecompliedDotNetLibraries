namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class JSClosureProperty : JSWrappedProperty
    {
        private MethodInfo getMeth;
        private MethodInfo setMeth;

        internal JSClosureProperty(PropertyInfo property, MethodInfo getMeth, MethodInfo setMeth) : base(property, null)
        {
            this.getMeth = getMeth;
            this.setMeth = setMeth;
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (!nonPublic && ((this.getMeth == null) || !this.getMeth.IsPublic))
            {
                return null;
            }
            return this.getMeth;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (!nonPublic && ((this.setMeth == null) || !this.setMeth.IsPublic))
            {
                return null;
            }
            return this.setMeth;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.getMeth == null)
            {
                throw new MissingMethodException();
            }
            return this.getMeth.Invoke(obj, invokeAttr, binder, index, culture);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.setMeth == null)
            {
                throw new MissingMethodException();
            }
            int n = (index == null) ? 0 : index.Length;
            object[] target = new object[n + 1];
            target[0] = value;
            if (n > 0)
            {
                ArrayObject.Copy(index, 0, target, 1, n);
            }
            this.setMeth.Invoke(obj, invokeAttr, binder, target, culture);
        }
    }
}

