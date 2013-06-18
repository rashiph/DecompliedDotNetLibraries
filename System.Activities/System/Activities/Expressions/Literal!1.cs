namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;

    [ContentProperty("Value")]
    public sealed class Literal<T> : CodeActivity<T>, IExpressionContainer, IValueSerializableExpression
    {
        private static Regex ExpressionEscapeRegex;

        static Literal()
        {
            Literal<T>.ExpressionEscapeRegex = new Regex(@"^(%*\[)");
        }

        public Literal()
        {
        }

        public Literal(T value) : this()
        {
            this.Value = value;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Type type = typeof(T);
            if (!type.IsValueType && (type != TypeHelper.StringType))
            {
                metadata.AddValidationError(System.Activities.SR.LiteralsMustBeValueTypesOrImmutableTypes(TypeHelper.StringType, type));
            }
        }

        public bool CanConvertToString(IValueSerializerContext context)
        {
            if (this.Value == null)
            {
                return true;
            }
            Type type = typeof(T);
            Type type2 = this.Value.GetType();
            if (type2 == TypeHelper.StringType)
            {
                string str = this.Value as string;
                if (string.IsNullOrEmpty(str))
                {
                    return false;
                }
            }
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            return (((type == type2) && (converter != null)) && (converter.CanConvertTo(TypeHelper.StringType) && converter.CanConvertFrom(TypeHelper.StringType)));
        }

        public string ConvertToString(IValueSerializerContext context)
        {
            if (this.Value == null)
            {
                return "[Nothing]";
            }
            Type type = typeof(T);
            this.Value.GetType();
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (type == TypeHelper.StringType)
            {
                string input = Convert.ToString(this.Value);
                if (input.EndsWith("]", StringComparison.Ordinal) && Literal<T>.ExpressionEscapeRegex.IsMatch(input))
                {
                    return ("%" + input);
                }
            }
            return converter.ConvertToString(context, this.Value);
        }

        protected override T Execute(CodeActivityContext context)
        {
            return base.ExecuteWithTryGetValue(context);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeValue()
        {
            return !object.Equals(this.Value, default(T));
        }

        public override string ToString()
        {
            if (this.Value != null)
            {
                return this.Value.ToString();
            }
            return "null";
        }

        internal override bool TryGetValue(ActivityContext context, out T value)
        {
            value = this.Value;
            return true;
        }

        Expression IExpressionContainer.Expression
        {
            get
            {
                return Expression.Lambda<Func<ActivityContext, T>>(Expression.Constant(this.Value, typeof(T)), new ParameterExpression[] { ExpressionUtilities.RuntimeContextParameter });
            }
        }

        public T Value { get; set; }
    }
}

