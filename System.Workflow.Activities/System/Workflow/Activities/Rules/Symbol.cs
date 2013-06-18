namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Runtime;

    internal abstract class Symbol
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected Symbol()
        {
        }

        internal abstract CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality);
        internal abstract void RecordSymbol(ArrayList list);

        internal abstract string Name { get; }
    }
}

