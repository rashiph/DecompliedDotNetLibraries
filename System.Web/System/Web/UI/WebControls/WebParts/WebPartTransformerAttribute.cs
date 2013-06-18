namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Web;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WebPartTransformerAttribute : Attribute
    {
        private Type _consumerType;
        private Type _providerType;
        private static readonly Hashtable transformerCache = Hashtable.Synchronized(new Hashtable());

        public WebPartTransformerAttribute(Type consumerType, Type providerType)
        {
            if (consumerType == null)
            {
                throw new ArgumentNullException("consumerType");
            }
            if (providerType == null)
            {
                throw new ArgumentNullException("providerType");
            }
            this._consumerType = consumerType;
            this._providerType = providerType;
        }

        public static Type GetConsumerType(Type transformerType)
        {
            return GetTransformerTypes(transformerType)[0];
        }

        public static Type GetProviderType(Type transformerType)
        {
            return GetTransformerTypes(transformerType)[1];
        }

        private static Type[] GetTransformerTypes(Type transformerType)
        {
            if (transformerType == null)
            {
                throw new ArgumentNullException("transformerType");
            }
            if (!transformerType.IsSubclassOf(typeof(WebPartTransformer)))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerAttribute_NotTransformer", new object[] { transformerType.FullName }));
            }
            Type[] transformerTypesFromAttribute = (Type[]) transformerCache[transformerType];
            if (transformerTypesFromAttribute == null)
            {
                transformerTypesFromAttribute = GetTransformerTypesFromAttribute(transformerType);
                transformerCache[transformerType] = transformerTypesFromAttribute;
            }
            return transformerTypesFromAttribute;
        }

        private static Type[] GetTransformerTypesFromAttribute(Type transformerType)
        {
            Type[] typeArray = new Type[2];
            object[] customAttributes = transformerType.GetCustomAttributes(typeof(WebPartTransformerAttribute), true);
            if (customAttributes.Length != 1)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerAttribute_Missing", new object[] { transformerType.FullName }));
            }
            WebPartTransformerAttribute attribute = (WebPartTransformerAttribute) customAttributes[0];
            if (attribute.ConsumerType == attribute.ProviderType)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerAttribute_SameTypes"));
            }
            typeArray[0] = attribute.ConsumerType;
            typeArray[1] = attribute.ProviderType;
            return typeArray;
        }

        public Type ConsumerType
        {
            get
            {
                return this._consumerType;
            }
        }

        public Type ProviderType
        {
            get
            {
                return this._providerType;
            }
        }
    }
}

