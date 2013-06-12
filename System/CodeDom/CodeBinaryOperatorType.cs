namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum CodeBinaryOperatorType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulus,
        Assign,
        IdentityInequality,
        IdentityEquality,
        ValueEquality,
        BitwiseOr,
        BitwiseAnd,
        BooleanOr,
        BooleanAnd,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }
}

