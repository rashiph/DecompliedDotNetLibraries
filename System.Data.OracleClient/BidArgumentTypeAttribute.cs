using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Parameter), Conditional("CODE_ANALYSIS")]
internal sealed class BidArgumentTypeAttribute : Attribute
{
    public readonly Type ArgumentType;

    internal BidArgumentTypeAttribute(Type bidArgumentType)
    {
        this.ArgumentType = bidArgumentType;
    }
}

