namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Resources;
    using System.Web.Compilation;

    public class ResourceExpressionEditorSheet : ExpressionEditorSheet
    {
        private string _classKey;
        private string _resourceKey;

        public ResourceExpressionEditorSheet(string expression, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                ResourceExpressionFields fields = ParseExpressionInternal(expression);
                this.ClassKey = fields.ClassKey;
                this.ResourceKey = fields.ResourceKey;
            }
        }

        public override string GetExpression()
        {
            if (!string.IsNullOrEmpty(this._classKey))
            {
                return (this._classKey + ", " + this._resourceKey);
            }
            return this._resourceKey;
        }

        private static ResourceExpressionFields ParseExpressionInternal(string expression)
        {
            ResourceExpressionFields fields = new ResourceExpressionFields();
            int length = expression.Length;
            string[] strArray = expression.Split(new char[] { ',' });
            int num = strArray.Length;
            if (num > 2)
            {
                return null;
            }
            if (num == 1)
            {
                fields.ResourceKey = strArray[0].Trim();
                return fields;
            }
            fields.ClassKey = strArray[0].Trim();
            fields.ResourceKey = strArray[1].Trim();
            return fields;
        }

        [DefaultValue(""), System.Design.SRDescription("ResourceExpressionEditorSheet_ClassKey")]
        public string ClassKey
        {
            get
            {
                if (this._classKey == null)
                {
                    return string.Empty;
                }
                return this._classKey;
            }
            set
            {
                this._classKey = value;
            }
        }

        public override bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.ResourceKey);
            }
        }

        [DefaultValue(""), TypeConverter(typeof(ResourceKeyTypeConverter)), System.Design.SRDescription("ResourceExpressionEditorSheet_ResourceKey")]
        public string ResourceKey
        {
            get
            {
                if (this._resourceKey == null)
                {
                    return string.Empty;
                }
                return this._resourceKey;
            }
            set
            {
                this._resourceKey = value;
            }
        }

        internal class ResourceExpressionFields
        {
            internal string ClassKey;
            internal string ResourceKey;
        }

        private class ResourceKeyTypeConverter : StringConverter
        {
            private static ICollection GetResourceKeys(IServiceProvider serviceProvider, string classKey)
            {
                IResourceProvider provider;
                DesignTimeResourceProviderFactory designTimeResourceProviderFactory = ControlDesigner.GetDesignTimeResourceProviderFactory(serviceProvider);
                if (string.IsNullOrEmpty(classKey))
                {
                    provider = designTimeResourceProviderFactory.CreateDesignTimeLocalResourceProvider(serviceProvider);
                }
                else
                {
                    provider = designTimeResourceProviderFactory.CreateDesignTimeGlobalResourceProvider(serviceProvider, classKey);
                }
                if (provider != null)
                {
                    IResourceReader resourceReader = provider.ResourceReader;
                    if (resourceReader != null)
                    {
                        ArrayList list = new ArrayList();
                        foreach (DictionaryEntry entry in resourceReader)
                        {
                            list.Add(entry.Key);
                        }
                        list.Sort(StringComparer.CurrentCultureIgnoreCase);
                        return list;
                    }
                }
                return null;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if ((context != null) && (context.Instance != null))
                {
                    ResourceExpressionEditorSheet instance = (ResourceExpressionEditorSheet) context.Instance;
                    ICollection resourceKeys = GetResourceKeys(instance.ServiceProvider, instance.ClassKey);
                    if ((resourceKeys != null) && (resourceKeys.Count > 0))
                    {
                        return new TypeConverter.StandardValuesCollection(resourceKeys);
                    }
                }
                return base.GetStandardValues(context);
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                if ((context != null) && (context.Instance != null))
                {
                    ResourceExpressionEditorSheet instance = (ResourceExpressionEditorSheet) context.Instance;
                    ICollection resourceKeys = GetResourceKeys(instance.ServiceProvider, instance.ClassKey);
                    if ((resourceKeys != null) && (resourceKeys.Count > 0))
                    {
                        return true;
                    }
                }
                return base.GetStandardValuesSupported(context);
            }
        }
    }
}

