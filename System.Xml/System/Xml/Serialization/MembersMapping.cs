namespace System.Xml.Serialization
{
    using System;

    internal class MembersMapping : TypeMapping
    {
        private bool hasWrapperElement = true;
        private MemberMapping[] members;
        private bool validateRpcWrapperElement;
        private bool writeAccessors = true;
        private MemberMapping xmlnsMember;

        internal bool HasWrapperElement
        {
            get
            {
                return this.hasWrapperElement;
            }
            set
            {
                this.hasWrapperElement = value;
            }
        }

        internal MemberMapping[] Members
        {
            get
            {
                return this.members;
            }
            set
            {
                this.members = value;
            }
        }

        internal bool ValidateRpcWrapperElement
        {
            get
            {
                return this.validateRpcWrapperElement;
            }
            set
            {
                this.validateRpcWrapperElement = value;
            }
        }

        internal bool WriteAccessors
        {
            get
            {
                return this.writeAccessors;
            }
            set
            {
                this.writeAccessors = value;
            }
        }

        internal MemberMapping XmlnsMember
        {
            get
            {
                return this.xmlnsMember;
            }
            set
            {
                this.xmlnsMember = value;
            }
        }
    }
}

