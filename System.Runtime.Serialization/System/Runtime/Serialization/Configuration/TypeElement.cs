namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.Serialization;

    public sealed class TypeElement : ConfigurationElement
    {
        private string key;
        private ConfigurationPropertyCollection properties;

        public TypeElement()
        {
            this.key = Guid.NewGuid().ToString();
        }

        public TypeElement(string typeName) : this()
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            this.Type = typeName;
        }

        internal System.Type GetType(string rootType, System.Type[] typeArgs)
        {
            return GetType(rootType, typeArgs, this.Type, this.Index, this.Parameters);
        }

        internal static System.Type GetType(string rootType, System.Type[] typeArgs, string type, int index, ParameterElementCollection parameters)
        {
            if (string.IsNullOrEmpty(type))
            {
                if ((typeArgs != null) && (index < typeArgs.Length))
                {
                    return typeArgs[index];
                }
                int num = (typeArgs == null) ? 0 : typeArgs.Length;
                if (num == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.Runtime.Serialization.SR.GetString("KnownTypeConfigIndexOutOfBoundsZero", new object[] { rootType, num, index }));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.Runtime.Serialization.SR.GetString("KnownTypeConfigIndexOutOfBounds", new object[] { rootType, num, index }));
            }
            System.Type type2 = System.Type.GetType(type, true);
            if (!type2.IsGenericTypeDefinition)
            {
                return type2;
            }
            if (parameters.Count != type2.GetGenericArguments().Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.Runtime.Serialization.SR.GetString("KnownTypeConfigGenericParamMismatch", new object[] { type, type2.GetGenericArguments().Length, parameters.Count }));
            }
            System.Type[] typeArguments = new System.Type[parameters.Count];
            for (int i = 0; i < typeArguments.Length; i++)
            {
                typeArguments[i] = parameters[i].GetType(rootType, typeArgs);
            }
            return type2.MakeGenericType(typeArguments);
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            TypeElement element = (TypeElement) parentElement;
            this.key = element.key;
            base.Reset(parentElement);
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("index", DefaultValue=0)]
        public int Index
        {
            get
            {
                return (int) base["index"];
            }
            set
            {
                base["index"] = value;
            }
        }

        internal string Key
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.key;
            }
        }

        [ConfigurationProperty("", DefaultValue=null, Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ParameterElementCollection Parameters
        {
            get
            {
                return (ParameterElementCollection) base[""];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(ParameterElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    propertys.Add(new ConfigurationProperty("type", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("index", typeof(int), 0, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("type", DefaultValue=""), StringValidator(MinLength=0)]
        public string Type
        {
            get
            {
                return (string) base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }
    }
}

