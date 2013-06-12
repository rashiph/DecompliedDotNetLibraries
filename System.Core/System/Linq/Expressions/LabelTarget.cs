namespace System.Linq.Expressions
{
    using System;

    public sealed class LabelTarget
    {
        private readonly string _name;
        private readonly System.Type _type;

        internal LabelTarget(System.Type type, string name)
        {
            this._type = type;
            this._name = name;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return this.Name;
            }
            return "UnamedLabel";
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

