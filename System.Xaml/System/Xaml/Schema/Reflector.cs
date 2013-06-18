namespace System.Xaml.Schema
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Markup;
    using System.Xaml;

    internal abstract class Reflector
    {
        protected IList<CustomAttributeData> _attributeData;
        protected NullableReference<ICustomAttributeProvider> _attributeProvider;

        protected Reflector()
        {
        }

        protected void EnsureAttributeData()
        {
            if (this._attributeData == null)
            {
                this._attributeData = CustomAttributeData.GetCustomAttributes(this.Member);
            }
        }

        private T Extract<T>(CustomAttributeData cad)
        {
            CustomAttributeTypedArgument argument2;
            if (cad.ConstructorArguments.Count == 0)
            {
                return default(T);
            }
            if (cad.ConstructorArguments.Count <= 1)
            {
                CustomAttributeTypedArgument argument = cad.ConstructorArguments[0];
                if (TypesAreEqual(argument.ArgumentType, typeof(T)))
                {
                    goto Label_005C;
                }
            }
            this.ThrowInvalidMetadata(cad, 1, typeof(T));
        Label_005C:
            argument2 = cad.ConstructorArguments[0];
            return (T) argument2.Value;
        }

        private Type ExtractType(CustomAttributeData cad)
        {
            Type type = null;
            if (cad.ConstructorArguments.Count == 1)
            {
                type = this.ExtractType(cad.ConstructorArguments[0]);
            }
            if (type == null)
            {
                this.ThrowInvalidMetadata(cad, 1, typeof(Type));
            }
            return type;
        }

        private Type ExtractType(CustomAttributeTypedArgument arg)
        {
            if (arg.ArgumentType == typeof(Type))
            {
                return (Type) arg.Value;
            }
            if (arg.ArgumentType == typeof(string))
            {
                string fullName = (string) arg.Value;
                return XamlNamespace.GetTypeFromFullTypeName(fullName);
            }
            return null;
        }

        private Type[] ExtractTypes(CustomAttributeData cad, int count)
        {
            if (cad.ConstructorArguments.Count != count)
            {
                this.ThrowInvalidMetadata(cad, count, typeof(Type));
            }
            Type[] typeArray = new Type[count];
            for (int i = 0; i < count; i++)
            {
                typeArray[i] = this.ExtractType(cad.ConstructorArguments[i]);
                if (typeArray[i] == null)
                {
                    this.ThrowInvalidMetadata(cad, count, typeof(Type));
                }
            }
            return typeArray;
        }

        public List<T> GetAllAttributeContents<T>(Type attributeType)
        {
            if (this.CustomAttributeProvider != null)
            {
                object[] customAttributes = this.CustomAttributeProvider.GetCustomAttributes(attributeType, false);
                if (customAttributes.Length == 0)
                {
                    return null;
                }
                List<T> list = new List<T>();
                if (attributeType == typeof(ContentWrapperAttribute))
                {
                    foreach (ContentWrapperAttribute attribute in customAttributes)
                    {
                        list.Add((T) attribute.ContentWrapper);
                    }
                    return list;
                }
                if (!(attributeType == typeof(DependsOnAttribute)))
                {
                    return null;
                }
                foreach (DependsOnAttribute attribute2 in customAttributes)
                {
                    list.Add((T) attribute2.Name);
                }
                return list;
            }
            try
            {
                List<CustomAttributeData> cads = new List<CustomAttributeData>();
                this.GetAttributes(attributeType, cads);
                if (cads.Count == 0)
                {
                    return null;
                }
                List<T> list3 = new List<T>();
                foreach (CustomAttributeData data in cads)
                {
                    T item = this.Extract<T>(data);
                    list3.Add(item);
                }
                return list3;
            }
            catch (CustomAttributeFormatException)
            {
                this.CustomAttributeProvider = this.Member;
                return this.GetAllAttributeContents<T>(attributeType);
            }
        }

        private CustomAttributeData GetAttribute(Type attributeType)
        {
            this.EnsureAttributeData();
            for (int i = 0; i < this._attributeData.Count; i++)
            {
                if (TypesAreEqual(this._attributeData[i].Constructor.DeclaringType, attributeType))
                {
                    return this._attributeData[i];
                }
            }
            return null;
        }

        private void GetAttributes(Type attributeType, IList<CustomAttributeData> cads)
        {
            this.EnsureAttributeData();
            for (int i = 0; i < this._attributeData.Count; i++)
            {
                if (TypesAreEqual(this._attributeData[i].Constructor.DeclaringType, attributeType))
                {
                    cads.Add(this._attributeData[i]);
                }
            }
        }

        public string GetAttributeString(Type attributeType, out bool checkedInherited)
        {
            if (this.CustomAttributeProvider != null)
            {
                checkedInherited = true;
                object[] customAttributes = this.CustomAttributeProvider.GetCustomAttributes(attributeType, true);
                if (customAttributes.Length != 0)
                {
                    if (attributeType == typeof(ContentPropertyAttribute))
                    {
                        return ((ContentPropertyAttribute) customAttributes[0]).Name;
                    }
                    if (attributeType == typeof(RuntimeNamePropertyAttribute))
                    {
                        return ((RuntimeNamePropertyAttribute) customAttributes[0]).Name;
                    }
                    if (attributeType == typeof(DictionaryKeyPropertyAttribute))
                    {
                        return ((DictionaryKeyPropertyAttribute) customAttributes[0]).Name;
                    }
                    if (attributeType == typeof(XamlSetMarkupExtensionAttribute))
                    {
                        return ((XamlSetMarkupExtensionAttribute) customAttributes[0]).XamlSetMarkupExtensionHandler;
                    }
                    if (attributeType == typeof(XamlSetTypeConverterAttribute))
                    {
                        return ((XamlSetTypeConverterAttribute) customAttributes[0]).XamlSetTypeConverterHandler;
                    }
                    if (attributeType == typeof(UidPropertyAttribute))
                    {
                        return ((UidPropertyAttribute) customAttributes[0]).Name;
                    }
                    if (attributeType == typeof(XmlLangPropertyAttribute))
                    {
                        return ((XmlLangPropertyAttribute) customAttributes[0]).Name;
                    }
                    if (attributeType == typeof(ConstructorArgumentAttribute))
                    {
                        return ((ConstructorArgumentAttribute) customAttributes[0]).ArgumentName;
                    }
                }
                return null;
            }
            try
            {
                checkedInherited = false;
                CustomAttributeData attribute = this.GetAttribute(attributeType);
                if (attribute == null)
                {
                    return null;
                }
                return (this.Extract<string>(attribute) ?? string.Empty);
            }
            catch (CustomAttributeFormatException)
            {
                this.CustomAttributeProvider = this.Member;
                return this.GetAttributeString(attributeType, out checkedInherited);
            }
        }

        public Type GetAttributeType(Type attributeType)
        {
            if (this.CustomAttributeProvider != null)
            {
                object[] customAttributes = this.CustomAttributeProvider.GetCustomAttributes(attributeType, false);
                if (customAttributes.Length != 0)
                {
                    if (attributeType == typeof(TypeConverterAttribute))
                    {
                        return XamlNamespace.GetTypeFromFullTypeName(((TypeConverterAttribute) customAttributes[0]).ConverterTypeName);
                    }
                    if (attributeType == typeof(MarkupExtensionReturnTypeAttribute))
                    {
                        return ((MarkupExtensionReturnTypeAttribute) customAttributes[0]).ReturnType;
                    }
                    if (attributeType == typeof(ValueSerializerAttribute))
                    {
                        return ((ValueSerializerAttribute) customAttributes[0]).ValueSerializerType;
                    }
                }
                return null;
            }
            try
            {
                CustomAttributeData attribute = this.GetAttribute(attributeType);
                if (attribute == null)
                {
                    return null;
                }
                return this.ExtractType(attribute);
            }
            catch (CustomAttributeFormatException)
            {
                this.CustomAttributeProvider = this.Member;
                return this.GetAttributeType(attributeType);
            }
        }

        public Type[] GetAttributeTypes(Type attributeType, int count)
        {
            if (this.CustomAttributeProvider != null)
            {
                object[] customAttributes = this.CustomAttributeProvider.GetCustomAttributes(attributeType, false);
                if (customAttributes.Length == 0)
                {
                    return null;
                }
                XamlDeferLoadAttribute attribute = (XamlDeferLoadAttribute) customAttributes[0];
                Type typeFromFullTypeName = XamlNamespace.GetTypeFromFullTypeName(attribute.LoaderTypeName);
                Type type2 = XamlNamespace.GetTypeFromFullTypeName(attribute.ContentTypeName);
                return new Type[] { typeFromFullTypeName, type2 };
            }
            try
            {
                CustomAttributeData cad = this.GetAttribute(attributeType);
                if (cad == null)
                {
                    return null;
                }
                return this.ExtractTypes(cad, count);
            }
            catch (CustomAttributeFormatException)
            {
                this.CustomAttributeProvider = this.Member;
                return this.GetAttributeTypes(attributeType, count);
            }
        }

        public T? GetAttributeValue<T>(Type attributeType) where T: struct
        {
            if (this.CustomAttributeProvider != null)
            {
                object[] customAttributes = this.CustomAttributeProvider.GetCustomAttributes(attributeType, false);
                if (customAttributes.Length != 0)
                {
                    if (attributeType == typeof(DesignerSerializationVisibilityAttribute))
                    {
                        return new T?((T) ((DesignerSerializationVisibilityAttribute) customAttributes[0]).Visibility);
                    }
                    if (attributeType == typeof(UsableDuringInitializationAttribute))
                    {
                        return new T?((T) ((UsableDuringInitializationAttribute) customAttributes[0]).Usable);
                    }
                }
                return null;
            }
            try
            {
                CustomAttributeData attribute = this.GetAttribute(attributeType);
                if (attribute == null)
                {
                    return null;
                }
                return new T?(this.Extract<T>(attribute));
            }
            catch (CustomAttributeFormatException)
            {
                this.CustomAttributeProvider = this.Member;
                return this.GetAttributeValue<T>(attributeType);
            }
        }

        protected static bool? GetFlag(int bitMask, int bitToCheck)
        {
            int validMask = GetValidMask(bitToCheck);
            if ((bitMask & validMask) != 0)
            {
                return new bool?(0 != (bitMask & bitToCheck));
            }
            return null;
        }

        protected static int GetValidMask(int flagMask)
        {
            return (flagMask << 0x10);
        }

        public bool IsAttributePresent(Type attributeType)
        {
            if (this.CustomAttributeProvider != null)
            {
                return this.CustomAttributeProvider.IsDefined(attributeType, false);
            }
            try
            {
                return (this.GetAttribute(attributeType) != null);
            }
            catch (CustomAttributeFormatException)
            {
                this.CustomAttributeProvider = this.Member;
                return this.IsAttributePresent(attributeType);
            }
        }

        protected static void SetBit(ref int flags, int mask)
        {
            int num;
            int num2;
            do
            {
                num = flags;
                num2 = num | mask;
            }
            while (num != Interlocked.CompareExchange(ref flags, num2, num));
        }

        internal void SetCustomAttributeProviderVolatile(ICustomAttributeProvider value)
        {
            this._attributeProvider.SetVolatile(value);
        }

        protected static void SetFlag(ref int bitMask, int bitToSet, bool value)
        {
            int mask = GetValidMask(bitToSet) + (value ? bitToSet : 0);
            SetBit(ref bitMask, mask);
        }

        protected void ThrowInvalidMetadata(CustomAttributeData cad, int expectedCount, Type expectedType)
        {
            throw new XamlSchemaException(System.Xaml.SR.Get("UnexpectedConstructorArg", new object[] { cad.Constructor.DeclaringType, this.Member, expectedCount, expectedType }));
        }

        private static bool TypesAreEqual(Type userType, Type builtInType)
        {
            if (userType.Assembly.ReflectionOnly)
            {
                return LooseTypeExtensions.AssemblyQualifiedNameEquals(userType, builtInType);
            }
            return (userType == builtInType);
        }

        internal ICustomAttributeProvider CustomAttributeProvider
        {
            get
            {
                return this._attributeProvider.Value;
            }
            set
            {
                this._attributeProvider.Value = value;
            }
        }

        internal bool CustomAttributeProviderIsSet
        {
            get
            {
                return this._attributeProvider.IsSet;
            }
        }

        internal bool CustomAttributeProviderIsSetVolatile
        {
            get
            {
                return this._attributeProvider.IsSetVolatile;
            }
        }

        protected abstract MemberInfo Member { get; }
    }
}

