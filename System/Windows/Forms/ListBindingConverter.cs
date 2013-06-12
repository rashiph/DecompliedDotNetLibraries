namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    public class ListBindingConverter : TypeConverter
    {
        private static string[] ctorParamProps;
        private static System.Type[] ctorTypes;

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is Binding))
            {
                Binding b = (Binding) value;
                return this.GetInstanceDescriptorFromValues(b);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            object obj2;
            try
            {
                obj2 = new Binding((string) propertyValues["PropertyName"], propertyValues["DataSource"], (string) propertyValues["DataMember"]);
            }
            catch (InvalidCastException exception)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyValueInvalidEntry"), exception);
            }
            catch (NullReferenceException exception2)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyValueInvalidEntry"), exception2);
            }
            return obj2;
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private InstanceDescriptor GetInstanceDescriptorFromValues(Binding b)
        {
            b.FormattingEnabled = true;
            bool isComplete = true;
            int index = ConstructorParameterProperties.Length - 1;
            while (index >= 0)
            {
                if (ConstructorParameterProperties[index] == null)
                {
                    break;
                }
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(b)[ConstructorParameterProperties[index]];
                if ((descriptor != null) && descriptor.ShouldSerializeValue(b))
                {
                    break;
                }
                index--;
            }
            System.Type[] destinationArray = new System.Type[index + 1];
            Array.Copy(ConstructorParamaterTypes, 0, destinationArray, 0, destinationArray.Length);
            ConstructorInfo constructor = typeof(Binding).GetConstructor(destinationArray);
            if (constructor == null)
            {
                isComplete = false;
                constructor = typeof(Binding).GetConstructor(new System.Type[] { typeof(string), typeof(object), typeof(string) });
            }
            object[] arguments = new object[destinationArray.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                object propertyName = null;
                switch (i)
                {
                    case 0:
                        propertyName = b.PropertyName;
                        break;

                    case 1:
                        propertyName = b.BindToObject.DataSource;
                        break;

                    case 2:
                        propertyName = b.BindToObject.BindingMemberInfo.BindingMember;
                        break;

                    default:
                        propertyName = TypeDescriptor.GetProperties(b)[ConstructorParameterProperties[i]].GetValue(b);
                        break;
                }
                arguments[i] = propertyName;
            }
            return new InstanceDescriptor(constructor, arguments, isComplete);
        }

        private static System.Type[] ConstructorParamaterTypes
        {
            get
            {
                if (ctorTypes == null)
                {
                    ctorTypes = new System.Type[] { typeof(string), typeof(object), typeof(string), typeof(bool), typeof(DataSourceUpdateMode), typeof(object), typeof(string), typeof(IFormatProvider) };
                }
                return ctorTypes;
            }
        }

        private static string[] ConstructorParameterProperties
        {
            get
            {
                if (ctorParamProps == null)
                {
                    string[] strArray = new string[8];
                    strArray[3] = "FormattingEnabled";
                    strArray[4] = "DataSourceUpdateMode";
                    strArray[5] = "NullValue";
                    strArray[6] = "FormatString";
                    strArray[7] = "FormatInfo";
                    ctorParamProps = strArray;
                }
                return ctorParamProps;
            }
        }
    }
}

