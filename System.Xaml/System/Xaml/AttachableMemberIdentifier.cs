namespace System.Xaml
{
    using System;

    public class AttachableMemberIdentifier : IEquatable<AttachableMemberIdentifier>
    {
        private Type declaringType;
        private string memberName;

        public AttachableMemberIdentifier(Type declaringType, string memberName)
        {
            this.declaringType = declaringType;
            this.memberName = memberName;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as AttachableMemberIdentifier);
        }

        public bool Equals(AttachableMemberIdentifier other)
        {
            if (other == null)
            {
                return false;
            }
            return ((this.declaringType == other.declaringType) && (this.memberName == other.memberName));
        }

        public override int GetHashCode()
        {
            int num = (this.declaringType == null) ? 0 : this.declaringType.GetHashCode();
            int num2 = (this.memberName == null) ? 0 : this.memberName.GetHashCode();
            return (((num << 5) + num) ^ num2);
        }

        public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (this.declaringType == null)
            {
                return this.memberName;
            }
            return (this.declaringType.ToString() + "." + this.memberName);
        }

        public Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }

        public string MemberName
        {
            get
            {
                return this.memberName;
            }
        }
    }
}

