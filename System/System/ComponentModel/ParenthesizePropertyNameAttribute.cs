namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ParenthesizePropertyNameAttribute : Attribute
    {
        public static readonly ParenthesizePropertyNameAttribute Default = new ParenthesizePropertyNameAttribute();
        private bool needParenthesis;

        public ParenthesizePropertyNameAttribute() : this(false)
        {
        }

        public ParenthesizePropertyNameAttribute(bool needParenthesis)
        {
            this.needParenthesis = needParenthesis;
        }

        public override bool Equals(object o)
        {
            return ((o is ParenthesizePropertyNameAttribute) && (((ParenthesizePropertyNameAttribute) o).NeedParenthesis == this.needParenthesis));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool NeedParenthesis
        {
            get
            {
                return this.needParenthesis;
            }
        }
    }
}

