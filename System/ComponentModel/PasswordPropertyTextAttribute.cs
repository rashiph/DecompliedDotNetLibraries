namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class PasswordPropertyTextAttribute : Attribute
    {
        private bool _password;
        public static readonly PasswordPropertyTextAttribute Default = No;
        public static readonly PasswordPropertyTextAttribute No = new PasswordPropertyTextAttribute(false);
        public static readonly PasswordPropertyTextAttribute Yes = new PasswordPropertyTextAttribute(true);

        public PasswordPropertyTextAttribute() : this(false)
        {
        }

        public PasswordPropertyTextAttribute(bool password)
        {
            this._password = password;
        }

        public override bool Equals(object o)
        {
            return ((o is PasswordPropertyTextAttribute) && (((PasswordPropertyTextAttribute) o).Password == this._password));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool Password
        {
            get
            {
                return this._password;
            }
        }
    }
}

