namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.CodeDom;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct SerializableMemberAttributes
    {
        private int memberAttributes;
        public SerializableMemberAttributes(MemberAttributes memberAttributes)
        {
            this.memberAttributes = Convert.ToInt32(memberAttributes);
        }

        public MemberAttributes ToMemberAttributes()
        {
            return (MemberAttributes) this.memberAttributes;
        }
    }
}

