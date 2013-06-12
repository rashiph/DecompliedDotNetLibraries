namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class EnumConverter : TypeConverter
    {
        private Type type;
        private TypeConverter.StandardValuesCollection values;

        public EnumConverter(Type type)
        {
            this.type = type;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (!(sourceType == typeof(string)) && !(sourceType == typeof(Enum[])))
            {
                return base.CanConvertFrom(context, sourceType);
            }
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (!(destinationType == typeof(InstanceDescriptor)) && !(destinationType == typeof(Enum[])))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string str = (string) value;
                    if (str.IndexOf(',') != -1)
                    {
                        long num = 0L;
                        foreach (string str2 in str.Split(new char[] { ',' }))
                        {
                            num |= Convert.ToInt64((Enum) Enum.Parse(this.type, str2, true), culture);
                        }
                        return Enum.ToObject(this.type, num);
                    }
                    return Enum.Parse(this.type, str, true);
                }
                catch (Exception exception)
                {
                    throw new FormatException(SR.GetString("ConvertInvalidPrimitive", new object[] { (string) value, this.type.Name }), exception);
                }
            }
            if (!(value is Enum[]))
            {
                return base.ConvertFrom(context, culture, value);
            }
            long num2 = 0L;
            foreach (Enum enum2 in (Enum[]) value)
            {
                num2 |= Convert.ToInt64(enum2, culture);
            }
            return Enum.ToObject(this.type, num2);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(string)) && (value != null))
            {
                Type underlyingType = Enum.GetUnderlyingType(this.type);
                if ((value is IConvertible) && (value.GetType() != underlyingType))
                {
                    value = ((IConvertible) value).ToType(underlyingType, culture);
                }
                if (!this.type.IsDefined(typeof(FlagsAttribute), false) && !Enum.IsDefined(this.type, value))
                {
                    throw new ArgumentException(SR.GetString("EnumConverterInvalidValue", new object[] { value.ToString(), this.type.Name }));
                }
                return Enum.Format(this.type, value, "G");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value != null))
            {
                string name = base.ConvertToInvariantString(context, value);
                if (this.type.IsDefined(typeof(FlagsAttribute), false) && (name.IndexOf(',') != -1))
                {
                    Type conversionType = Enum.GetUnderlyingType(this.type);
                    if (value is IConvertible)
                    {
                        object obj2 = ((IConvertible) value).ToType(conversionType, culture);
                        MethodInfo method = typeof(Enum).GetMethod("ToObject", new Type[] { typeof(Type), conversionType });
                        if (method != null)
                        {
                            return new InstanceDescriptor(method, new object[] { this.type, obj2 });
                        }
                    }
                }
                else
                {
                    FieldInfo field = this.type.GetField(name);
                    if (field != null)
                    {
                        return new InstanceDescriptor(field, null);
                    }
                }
            }
            if (!(destinationType == typeof(Enum[])) || (value == null))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (!this.type.IsDefined(typeof(FlagsAttribute), false))
            {
                return new Enum[] { ((Enum) Enum.ToObject(this.type, value)) };
            }
            List<Enum> list = new List<Enum>();
            Array values = Enum.GetValues(this.type);
            long[] numArray = new long[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                numArray[i] = Convert.ToInt64((Enum) values.GetValue(i), culture);
            }
            long num2 = Convert.ToInt64((Enum) value, culture);
            bool flag = true;
            while (flag)
            {
                flag = false;
                foreach (long num3 in numArray)
                {
                    if (((num3 != 0L) && ((num3 & num2) == num3)) || (num3 == num2))
                    {
                        list.Add((Enum) Enum.ToObject(this.type, num3));
                        flag = true;
                        num2 &= ~num3;
                        break;
                    }
                }
                if (num2 == 0L)
                {
                    break;
                }
            }
            if (!flag && (num2 != 0L))
            {
                list.Add((Enum) Enum.ToObject(this.type, num2));
            }
            return list.ToArray();
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                Type reflectionType = TypeDescriptor.GetReflectionType(this.type);
                if (reflectionType == null)
                {
                    reflectionType = this.type;
                }
                FieldInfo[] fields = reflectionType.GetFields(BindingFlags.Public | BindingFlags.Static);
                ArrayList list = null;
                if ((fields != null) && (fields.Length > 0))
                {
                    list = new ArrayList(fields.Length);
                }
                if (list != null)
                {
                    foreach (FieldInfo info in fields)
                    {
                        BrowsableAttribute attribute = null;
                        foreach (Attribute attribute2 in info.GetCustomAttributes(typeof(BrowsableAttribute), false))
                        {
                            attribute = attribute2 as BrowsableAttribute;
                        }
                        if ((attribute == null) || attribute.Browsable)
                        {
                            object obj2 = null;
                            try
                            {
                                if (info.Name != null)
                                {
                                    obj2 = Enum.Parse(this.type, info.Name);
                                }
                            }
                            catch (ArgumentException)
                            {
                            }
                            if (obj2 != null)
                            {
                                list.Add(obj2);
                            }
                        }
                    }
                    IComparer comparer = this.Comparer;
                    if (comparer != null)
                    {
                        list.Sort(comparer);
                    }
                }
                Array values = (list != null) ? ((Array) list.ToArray()) : null;
                this.values = new TypeConverter.StandardValuesCollection(values);
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return !this.type.IsDefined(typeof(FlagsAttribute), false);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return Enum.IsDefined(this.type, value);
        }

        protected virtual IComparer Comparer
        {
            get
            {
                return System.InvariantComparer.Default;
            }
        }

        protected Type EnumType
        {
            get
            {
                return this.type;
            }
        }

        protected TypeConverter.StandardValuesCollection Values
        {
            get
            {
                return this.values;
            }
            set
            {
                this.values = value;
            }
        }
    }
}

