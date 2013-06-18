namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.UI;

    [ExpressionEditor("System.Web.UI.Design.ResourceExpressionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ExpressionPrefix("Resources")]
    public class ResourceExpressionBuilder : System.Web.Compilation.ExpressionBuilder
    {
        private static ResourceProviderFactory s_resourceProviderFactory;

        private static void EnsureResourceProviderFactory()
        {
            if (s_resourceProviderFactory == null)
            {
                Type resourceProviderFactoryTypeInternal = null;
                resourceProviderFactoryTypeInternal = RuntimeConfig.GetAppConfig().Globalization.ResourceProviderFactoryTypeInternal;
                if (resourceProviderFactoryTypeInternal == null)
                {
                    s_resourceProviderFactory = new ResXResourceProviderFactory();
                }
                else
                {
                    s_resourceProviderFactory = (ResourceProviderFactory) HttpRuntime.CreatePublicInstance(resourceProviderFactoryTypeInternal);
                }
            }
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            ResourceExpressionFields fields = (ResourceExpressionFields) parsedData;
            IResourceProvider resourceProvider = GetResourceProvider(fields, context.VirtualPathObject);
            if (entry.Type == typeof(string))
            {
                return GetResourceObject(resourceProvider, fields.ResourceKey, null);
            }
            return GetResourceObject(resourceProvider, fields.ResourceKey, null, entry.DeclaringType, entry.PropertyInfo.Name);
        }

        private CodeExpression GetAppResCodeExpression(string classKey, string resourceKey, BoundPropertyEntry entry)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "GetGlobalResourceObject" }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(classKey));
            expression.Parameters.Add(new CodePrimitiveExpression(resourceKey));
            if ((entry.Type != typeof(string)) && (entry.Type != null))
            {
                expression.Parameters.Add(new CodeTypeOfExpression(entry.DeclaringType));
                expression.Parameters.Add(new CodePrimitiveExpression(entry.PropertyInfo.Name));
            }
            return expression;
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            ResourceExpressionFields fields = (ResourceExpressionFields) parsedData;
            if (fields.ClassKey.Length == 0)
            {
                return this.GetPageResCodeExpression(fields.ResourceKey, entry);
            }
            return this.GetAppResCodeExpression(fields.ClassKey, fields.ResourceKey, entry);
        }

        internal static object GetGlobalResourceObject(string classKey, string resourceKey)
        {
            return GetGlobalResourceObject(classKey, resourceKey, null, null, null);
        }

        internal static object GetGlobalResourceObject(string classKey, string resourceKey, Type objType, string propName, CultureInfo culture)
        {
            return GetResourceObject(GetGlobalResourceProvider(classKey), resourceKey, culture, objType, propName);
        }

        private static IResourceProvider GetGlobalResourceProvider(string classKey)
        {
            string str = "Resources." + classKey;
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            string key = "A" + str;
            IResourceProvider provider = cacheInternal[key] as IResourceProvider;
            if (provider == null)
            {
                EnsureResourceProviderFactory();
                provider = s_resourceProviderFactory.CreateGlobalResourceProvider(classKey);
                cacheInternal.UtcInsert(key, provider);
            }
            return provider;
        }

        internal static IResourceProvider GetLocalResourceProvider(TemplateControl templateControl)
        {
            return GetLocalResourceProvider(templateControl.VirtualPath);
        }

        internal static IResourceProvider GetLocalResourceProvider(VirtualPath virtualPath)
        {
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            string key = "A" + virtualPath.VirtualPathString;
            IResourceProvider provider = cacheInternal[key] as IResourceProvider;
            if (provider == null)
            {
                EnsureResourceProviderFactory();
                provider = s_resourceProviderFactory.CreateLocalResourceProvider(virtualPath.VirtualPathString);
                cacheInternal.UtcInsert(key, provider);
            }
            return provider;
        }

        private CodeExpression GetPageResCodeExpression(string resourceKey, BoundPropertyEntry entry)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "GetLocalResourceObject" }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(resourceKey));
            if ((entry.Type != typeof(string)) && (entry.Type != null))
            {
                expression.Parameters.Add(new CodeTypeOfExpression(entry.DeclaringType));
                expression.Parameters.Add(new CodePrimitiveExpression(entry.PropertyInfo.Name));
            }
            return expression;
        }

        internal static object GetParsedData(string resourceKey)
        {
            return new ResourceExpressionFields(string.Empty, resourceKey);
        }

        internal static object GetResourceObject(IResourceProvider resourceProvider, string resourceKey, CultureInfo culture)
        {
            return GetResourceObject(resourceProvider, resourceKey, culture, null, null);
        }

        internal static object GetResourceObject(IResourceProvider resourceProvider, string resourceKey, CultureInfo culture, Type objType, string propName)
        {
            if (resourceProvider == null)
            {
                return null;
            }
            object obj2 = resourceProvider.GetObject(resourceKey, culture);
            if (objType == null)
            {
                return obj2;
            }
            string str = obj2 as string;
            if (str == null)
            {
                return obj2;
            }
            return ObjectFromString(str, objType, propName);
        }

        private static IResourceProvider GetResourceProvider(ResourceExpressionFields fields, VirtualPath virtualPath)
        {
            if (fields.ClassKey.Length == 0)
            {
                return GetLocalResourceProvider(virtualPath);
            }
            return GetGlobalResourceProvider(fields.ClassKey);
        }

        private static object ObjectFromString(string value, Type objType, string propName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(objType)[propName];
            if (descriptor == null)
            {
                return null;
            }
            TypeConverter converter = descriptor.Converter;
            if (converter == null)
            {
                return null;
            }
            return converter.ConvertFromInvariantString(value);
        }

        public static ResourceExpressionFields ParseExpression(string expression)
        {
            return ParseExpressionInternal(expression);
        }

        public override object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context)
        {
            ResourceExpressionFields fields = null;
            try
            {
                fields = ParseExpressionInternal(expression);
            }
            catch
            {
            }
            if (fields == null)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_res_expr", new object[] { expression }));
            }
            if (context.VirtualPathObject != null)
            {
                IResourceProvider resourceProvider = GetResourceProvider(fields, VirtualPath.Create(context.VirtualPath));
                object obj2 = null;
                if (resourceProvider != null)
                {
                    try
                    {
                        obj2 = resourceProvider.GetObject(fields.ResourceKey, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                    }
                }
                if (obj2 == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Res_not_found", new object[] { fields.ResourceKey }));
                }
            }
            return fields;
        }

        private static ResourceExpressionFields ParseExpressionInternal(string expression)
        {
            string classKey = null;
            string resourceKey = null;
            if (expression.Length != 0)
            {
                string[] strArray = expression.Split(new char[] { ',' });
                int length = strArray.Length;
                if (length > 2)
                {
                    return null;
                }
                if (length == 1)
                {
                    resourceKey = strArray[0].Trim();
                }
                else
                {
                    classKey = strArray[0].Trim();
                    resourceKey = strArray[1].Trim();
                }
            }
            return new ResourceExpressionFields(classKey, resourceKey);
        }

        public override bool SupportsEvaluate
        {
            get
            {
                return true;
            }
        }
    }
}

