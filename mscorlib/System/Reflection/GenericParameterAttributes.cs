namespace System.Reflection
{
    using System;

    [Flags]
    public enum GenericParameterAttributes
    {
        Contravariant = 2,
        Covariant = 1,
        DefaultConstructorConstraint = 0x10,
        None = 0,
        NotNullableValueTypeConstraint = 8,
        ReferenceTypeConstraint = 4,
        SpecialConstraintMask = 0x1c,
        VarianceMask = 3
    }
}

