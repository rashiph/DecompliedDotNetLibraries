namespace System.Web.Compilation
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ExpressionPrefixAttribute : Attribute
    {
        private string _expressionPrefix;

        public ExpressionPrefixAttribute(string expressionPrefix)
        {
            if (string.IsNullOrEmpty(expressionPrefix))
            {
                throw new ArgumentNullException("expressionPrefix");
            }
            this._expressionPrefix = expressionPrefix;
        }

        public string ExpressionPrefix
        {
            get
            {
                return this._expressionPrefix;
            }
        }
    }
}

