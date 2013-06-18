namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;

    internal class MemberSymbol : Symbol
    {
        private MemberInfo member;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal MemberSymbol(MemberInfo member)
        {
            this.member = member;
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseUnadornedMemberIdentifier(parserContext, this, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            list.Add(this.member);
        }

        internal override string Name
        {
            get
            {
                return this.member.Name;
            }
        }
    }
}

