namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.ComponentModel;

    public sealed class SerializeAbsoluteContext
    {
        private MemberDescriptor _member;

        public SerializeAbsoluteContext()
        {
        }

        public SerializeAbsoluteContext(MemberDescriptor member)
        {
            this._member = member;
        }

        public bool ShouldSerialize(MemberDescriptor member)
        {
            if (this._member != null)
            {
                return (this._member == member);
            }
            return true;
        }

        public MemberDescriptor Member
        {
            get
            {
                return this._member;
            }
        }
    }
}

