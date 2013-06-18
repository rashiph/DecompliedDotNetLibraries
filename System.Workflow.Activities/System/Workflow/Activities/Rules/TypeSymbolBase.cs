namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    internal abstract class TypeSymbolBase : Symbol
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TypeSymbolBase()
        {
        }

        internal abstract OverloadedTypeSymbol OverloadType(TypeSymbolBase typeSymBase);
    }
}

