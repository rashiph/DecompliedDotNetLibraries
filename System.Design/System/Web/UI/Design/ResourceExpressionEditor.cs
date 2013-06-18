namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.Compilation;

    public class ResourceExpressionEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            System.Web.Compilation.ResourceExpressionFields fields;
            IResourceProvider provider;
            if (parseTimeData is System.Web.Compilation.ResourceExpressionFields)
            {
                fields = (System.Web.Compilation.ResourceExpressionFields) parseTimeData;
            }
            else
            {
                fields = ResourceExpressionBuilder.ParseExpression(expression);
            }
            if (string.IsNullOrEmpty(fields.ResourceKey))
            {
                return null;
            }
            object obj2 = null;
            DesignTimeResourceProviderFactory designTimeResourceProviderFactory = ControlDesigner.GetDesignTimeResourceProviderFactory(serviceProvider);
            if (string.IsNullOrEmpty(fields.ClassKey))
            {
                provider = designTimeResourceProviderFactory.CreateDesignTimeLocalResourceProvider(serviceProvider);
            }
            else
            {
                provider = designTimeResourceProviderFactory.CreateDesignTimeGlobalResourceProvider(serviceProvider, fields.ClassKey);
            }
            if (provider != null)
            {
                obj2 = provider.GetObject(fields.ResourceKey, CultureInfo.InvariantCulture);
            }
            if (obj2 != null)
            {
                Type c = obj2.GetType();
                if (!propertyType.IsAssignableFrom(c))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(propertyType);
                    if ((converter != null) && converter.CanConvertFrom(c))
                    {
                        return converter.ConvertFrom(obj2);
                    }
                }
            }
            return obj2;
        }

        public override ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new ResourceExpressionEditorSheet(expression, serviceProvider);
        }
    }
}

