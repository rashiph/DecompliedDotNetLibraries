namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Guid("CA0F511A-FAF2-4942-B9A8-17D5E46514E8"), ComVisible(true)]
    public class COMFieldInfo : FieldInfo, MemberInfoInitializer
    {
        private COMMemberInfo _comObject = null;
        private string _name = null;

        public COMMemberInfo GetCOMMemberInfo()
        {
            return this._comObject;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new FieldInfo[0];
        }

        public override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return new FieldInfo[0];
        }

        public override object GetValue(object obj)
        {
            return this._comObject.GetValue(BindingFlags.Default, null, new object[0], null);
        }

        public virtual void Initialize(string name, COMMemberInfo dispatch)
        {
            this._name = name;
            this._comObject = dispatch;
        }

        public override bool IsDefined(Type t, bool inherit)
        {
            return false;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            this._comObject.SetValue(value, invokeAttr, binder, new object[0], culture);
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return FieldAttributes.Public;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return null;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                throw new JScriptException(JSError.InternalError);
            }
        }

        public override Type FieldType
        {
            get
            {
                return typeof(object);
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Field;
            }
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return null;
            }
        }
    }
}

