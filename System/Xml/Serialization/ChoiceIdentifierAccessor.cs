namespace System.Xml.Serialization
{
    using System;

    internal class ChoiceIdentifierAccessor : Accessor
    {
        private string[] memberIds;
        private string memberName;

        internal string[] MemberIds
        {
            get
            {
                return this.memberIds;
            }
            set
            {
                this.memberIds = value;
            }
        }

        internal string MemberName
        {
            get
            {
                return this.memberName;
            }
            set
            {
                this.memberName = value;
            }
        }
    }
}

