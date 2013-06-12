namespace System.Linq.Expressions
{
    using System;
    using System.Collections.ObjectModel;

    internal class NewValueTypeExpression : NewExpression
    {
        private readonly System.Type _valueType;

        internal NewValueTypeExpression(System.Type type, ReadOnlyCollection<Expression> arguments, ReadOnlyCollection<MemberInfo> members) : base(null, arguments, members)
        {
            this._valueType = type;
        }

        public sealed override System.Type Type
        {
            get
            {
                return this._valueType;
            }
        }
    }
}

