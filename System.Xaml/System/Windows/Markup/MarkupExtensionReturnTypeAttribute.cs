namespace System.Windows.Markup
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class MarkupExtensionReturnTypeAttribute : Attribute
    {
        private Type _expressionType;
        private Type _returnType;

        public MarkupExtensionReturnTypeAttribute()
        {
        }

        public MarkupExtensionReturnTypeAttribute(Type returnType)
        {
            this._returnType = returnType;
        }

        [Obsolete("The expressionType argument is not used by the XAML parser. To specify the expected return type, use MarkupExtensionReturnTypeAttribute(Type). To specify custom handling for expression types, use XamlSetMarkupExtensionAttribute.")]
        public MarkupExtensionReturnTypeAttribute(Type returnType, Type expressionType)
        {
            this._returnType = returnType;
            this._expressionType = expressionType;
        }

        [Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute.")]
        public Type ExpressionType
        {
            get
            {
                return this._expressionType;
            }
        }

        public Type ReturnType
        {
            get
            {
                return this._returnType;
            }
        }
    }
}

