using System;
using System.Diagnostics;

[Conditional("CODE_ANALYSIS"), AttributeUsage(AttributeTargets.Parameter)]
internal sealed class BidArgumentTypeAttribute : Attribute
{
    public readonly Type ArgumentType;

    internal BidArgumentTypeAttribute(Type bidArgumentType)
    {
        this.ArgumentType = bidArgumentType;
    }
}

