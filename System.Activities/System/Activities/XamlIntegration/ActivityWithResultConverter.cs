namespace System.Activities.XamlIntegration
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Xaml;

    public sealed class ActivityWithResultConverter : TypeConverterBase
    {
        public ActivityWithResultConverter() : base(typeof(Activity<>), typeof(ExpressionConverterHelper))
        {
        }

        public ActivityWithResultConverter(Type type) : base(type, typeof(Activity<>), typeof(ExpressionConverterHelper))
        {
        }

        internal static object GetRootTemplatedActivity(IServiceProvider serviceProvider)
        {
            IRootObjectProvider service = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (service == null)
            {
                return null;
            }
            IAmbientProvider provider2 = serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider;
            if (provider2 == null)
            {
                return null;
            }
            IXamlSchemaContextProvider provider3 = serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;
            if (provider3 == null)
            {
                return null;
            }
            XamlMember member = GetXamlMember(provider3.SchemaContext, typeof(Activity), "Implementation");
            XamlMember member2 = GetXamlMember(provider3.SchemaContext, typeof(DynamicActivity), "Implementation");
            if ((member == null) || (member2 == null))
            {
                return null;
            }
            if (provider2.GetFirstAmbientValue(null, new XamlMember[] { member, member2 }) == null)
            {
                return null;
            }
            return (service.RootObject as Activity);
        }

        private static XamlMember GetXamlMember(XamlSchemaContext schemaContext, Type type, string memberName)
        {
            XamlType xamlType = schemaContext.GetXamlType(type);
            if (xamlType == null)
            {
                return null;
            }
            return xamlType.GetMember(memberName);
        }

        internal sealed class ExpressionConverterHelper<T> : TypeConverterBase.TypeConverterHelper<Activity<T>>
        {
            private TypeConverter baseConverter;
            private static Regex LiteralEscapeRegex;
            private LocationHelper<T> locationHelper;
            private static Type LocationHelperType;
            private Type valueType;

            static ExpressionConverterHelper()
            {
                ActivityWithResultConverter.ExpressionConverterHelper<T>.LiteralEscapeRegex = new Regex(@"^(%+\[)");
                ActivityWithResultConverter.ExpressionConverterHelper<T>.LocationHelperType = typeof(LocationHelper);
            }

            public ExpressionConverterHelper() : this(TypeHelper.AreTypesCompatible(typeof(T), typeof(Location)))
            {
            }

            public ExpressionConverterHelper(bool isLocationType)
            {
                this.valueType = typeof(T);
                if (isLocationType)
                {
                    this.valueType = this.valueType.GetGenericArguments()[0];
                    Type type = ActivityWithResultConverter.ExpressionConverterHelper<T>.LocationHelperType.MakeGenericType(new Type[] { typeof(T), this.valueType });
                    this.locationHelper = (LocationHelper<T>) Activator.CreateInstance(type);
                }
            }

            public override Activity<T> ConvertFromString(string text, ITypeDescriptorContext context)
            {
                T local;
                if (ActivityWithResultConverter.ExpressionConverterHelper<T>.IsExpression(text))
                {
                    string expressionText = text.Substring(1, text.Length - 2);
                    if (this.locationHelper != null)
                    {
                        return (Activity<T>) this.locationHelper.CreateExpression(expressionText);
                    }
                    return new VisualBasicValue<T> { ExpressionText = expressionText };
                }
                if (this.locationHelper != null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidLocationExpression));
                }
                if (text.EndsWith("]", StringComparison.Ordinal) && ActivityWithResultConverter.ExpressionConverterHelper<T>.LiteralEscapeRegex.IsMatch(text))
                {
                    text = text.Substring(1, text.Length - 1);
                }
                if (text is T)
                {
                    local = (T) text;
                }
                else if (text == string.Empty)
                {
                    local = default(T);
                }
                else
                {
                    local = (T) this.BaseConverter.ConvertFromString(context, text);
                }
                return new Literal<T> { Value = local };
            }

            private static bool IsExpression(string text)
            {
                return (text.StartsWith("[", StringComparison.Ordinal) && text.EndsWith("]", StringComparison.Ordinal));
            }

            private TypeConverter BaseConverter
            {
                get
                {
                    if (this.baseConverter == null)
                    {
                        this.baseConverter = TypeDescriptor.GetConverter(this.valueType);
                    }
                    return this.baseConverter;
                }
            }

            private abstract class LocationHelper
            {
                protected LocationHelper()
                {
                }

                public abstract Activity CreateExpression(string expressionText);
            }

            private class LocationHelper<TLocationValue> : ActivityWithResultConverter.ExpressionConverterHelper<T>.LocationHelper
            {
                public override Activity CreateExpression(string expressionText)
                {
                    return new VisualBasicReference<TLocationValue> { ExpressionText = expressionText };
                }
            }
        }
    }
}

