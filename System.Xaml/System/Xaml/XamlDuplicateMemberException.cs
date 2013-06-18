namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public class XamlDuplicateMemberException : XamlException
    {
        public XamlDuplicateMemberException()
        {
        }

        public XamlDuplicateMemberException(string message) : base(message)
        {
        }

        protected XamlDuplicateMemberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.DuplicateMember = (XamlMember) info.GetValue("DuplicateMember", typeof(XamlMember));
            this.ParentType = (XamlType) info.GetValue("ParentType", typeof(XamlType));
        }

        public XamlDuplicateMemberException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public XamlDuplicateMemberException(XamlMember member, XamlType type) : base(System.Xaml.SR.Get("DuplicateMemberSet", new object[] { (member != null) ? member.Name : null, (type != null) ? type.Name : null }))
        {
            this.DuplicateMember = member;
            this.ParentType = type;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("DuplicateMember", this.DuplicateMember);
            info.AddValue("ParentType", this.ParentType);
            base.GetObjectData(info, context);
        }

        public XamlMember DuplicateMember { get; set; }

        public XamlType ParentType { get; set; }
    }
}

